using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Azure.Relay;
using Microsoft.Extensions.Options;
using Ricciolo.AspNetCore.AzureRelay.FeatureSerializer;

namespace Ricciolo.AspNetCore.AzureRelay
{
    public class RelayListener
    {
        private readonly IOptions<RelayOptions> _relayOptions;
        private readonly FeatureSerializerManager _featureSerializerManager;
        private static CancellationTokenSource _loopCancellationTokenSource;
        private HybridConnectionListener _listener;
        private Func<IFeatureCollection, CancellationToken, Task> _requestReceivedCallback;

        public RelayListener(IOptions<RelayOptions> relayOptions, FeatureSerializerManager featureSerializerManager)
        {
            _relayOptions = relayOptions;
            _featureSerializerManager = featureSerializerManager;
        }

        public async Task OpenAsync(Func<IFeatureCollection, CancellationToken, Task> requestReceivedCallback, CancellationToken token)
        {
            if (_listener != null) throw new InvalidOperationException("Already open");

            _requestReceivedCallback = requestReceivedCallback;
            _loopCancellationTokenSource = new CancellationTokenSource();
            CancellationToken cancellationToken = _loopCancellationTokenSource.Token;

            _listener = new HybridConnectionListener(_relayOptions.Value.ConnectionString);

            await _listener.OpenAsync(token);

            Task.Factory.StartNew(() => StartInternalAsync(cancellationToken), cancellationToken).GetAwaiter();
        }

        public async Task CloseAsync(CancellationToken token)
        {
            _loopCancellationTokenSource?.Cancel();

            // Close the listener after we exit the processing loop
            if (_listener != null)
            {
                await _listener.CloseAsync(token);
                _listener = null;
            }
        }

        private async Task StartInternalAsync(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                var relayConnection = await _listener.AcceptConnectionAsync().ConfigureAwait(false);
                if (relayConnection == null)
                {
                    continue;
                }

                ProcessMessagesOnConnection(relayConnection, token);
                //Task.Factory.StartNew(() => ProcessMessagesOnConnection(relayConnection, token), token, TaskCreationOptions.LongRunning, TaskScheduler.Current).GetAwaiter();
            }
        }

        private async void ProcessMessagesOnConnection(HybridConnectionStream connection, CancellationToken token)
        {
            try
            {
                try
                {
                    try
                    {
                        while (!token.IsCancellationRequested)
                        {
                            IFeatureCollection features = await _featureSerializerManager.ReadAsync(connection, new[] { typeof(IHttpRequestFeature) }, token);
                            PrepareDefaultFeatures(features);

                            await _requestReceivedCallback(features, token);

                            await _featureSerializerManager.WriteAsync(connection, features, new[] { typeof(IHttpResponseFeature) }, token);
                        }
                    }
                    finally
                    {
                        await connection.ShutdownAsync(token);
                    }
                }
                catch (IOException)
                { }
                catch (RelayException)
                { }
            }
            finally
            {
                await connection.CloseAsync(token);
            }
        }

        private static void PrepareDefaultFeatures(IFeatureCollection features)
        {
            features.Set<IHttpResponseFeature>(new HttpResponseFeature { Body = new MemoryStream() });
        }
    }
}
