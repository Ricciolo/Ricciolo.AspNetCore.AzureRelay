using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Azure.Relay;
using Microsoft.Extensions.Primitives;
using Ricciolo.AspNetCore.AzureRelay.FeatureSerializer;

namespace Ricciolo.AspNetCore.AzureRelay
{
    public class RelayClient : IDisposable
    {
        private readonly HybridConnectionClient _client;
        private readonly FeatureSerializerManager _featureSerializerManager;
        private HybridConnectionStream _connection;

        public RelayClient(HybridConnectionClient client, FeatureSerializerManager featureSerializerManager)
        {
            _client = client;
            _featureSerializerManager = featureSerializerManager;
            GoodStatus = true;
        }

        public async Task OpenAsync(CancellationToken token)
        {
            if (_connection != null) throw new InvalidOperationException("Connection already open");

            _connection = await _client.CreateConnectionAsync();
            token.ThrowIfCancellationRequested();

            IsOpen = true;
        }

        public async Task SendRequestAsync(IFeatureCollection features, CancellationToken token)
        {
            await _featureSerializerManager.WriteAsync(_connection, features, new[] { typeof(IHttpRequestFeature) }, token);

            IFeatureCollection resultFeatures = await _featureSerializerManager.ReadAsync(_connection, new[] { typeof(IHttpResponseFeature) }, token);

            var resultHttpResponseFeature = resultFeatures.Get<IHttpResponseFeature>();
            var httpResponseFeature = features.Get<IHttpResponseFeature>();

            await CopyHttpResponseFeature(resultHttpResponseFeature, httpResponseFeature, token);
        }

        private static async Task CopyHttpResponseFeature(IHttpResponseFeature fromHttpResponseFeature, IHttpResponseFeature toHttpResponseFeature, CancellationToken token)
        {
            toHttpResponseFeature.StatusCode = fromHttpResponseFeature.StatusCode;
            toHttpResponseFeature.ReasonPhrase = fromHttpResponseFeature.ReasonPhrase;
            toHttpResponseFeature.Headers.Clear();
            foreach (var pair in fromHttpResponseFeature.Headers)
            {
                toHttpResponseFeature.Headers[pair.Key] = pair.Value;
            }

            if (toHttpResponseFeature.Headers.ContentLength.GetValueOrDefault(1) > 0)
            {
                await fromHttpResponseFeature.Body.CopyToAsync(toHttpResponseFeature.Body, 81920, token);
            }
        }

        public async Task CloseAsync(CancellationToken token)
        {
            try
            {
                if (_connection == null) return;
                await _connection.ShutdownAsync(token);

                await _connection.CloseAsync(token);
            }
            finally
            {
                IsOpen = false;
                GoodStatus = false;
                _connection = null;
            }
        }

        public bool GoodStatus { get; private set; }

        public bool IsOpen { get; private set; }

        public void Dispose()
        {
        }
    }
}
