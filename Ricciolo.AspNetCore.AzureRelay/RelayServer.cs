using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.Options;

namespace Ricciolo.AspNetCore.AzureRelay
{
    internal class RelayServer : IServer
    {
        private readonly RelayListener _relayListener;
        private RelayServerApplication _relayServerApplication;

        public RelayServer(RelayListener relayListener)
        {
            _relayListener = relayListener;
        }

        public void Dispose()
        {

        }

        public async Task StartAsync<TContext>(IHttpApplication<TContext> application, CancellationToken cancellationToken)
        {
            _relayServerApplication = new RelayServerApplication<TContext>(application);

            await _relayListener.OpenAsync(_relayServerApplication.ReceiveRequestAsync, cancellationToken);
            cancellationToken.ThrowIfCancellationRequested();
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            await _relayListener.CloseAsync(cancellationToken);
            cancellationToken.ThrowIfCancellationRequested();
        }

        public IFeatureCollection Features { get; } = new FeatureCollection();

        private abstract class RelayServerApplication
        {
            public abstract Task ReceiveRequestAsync(IFeatureCollection features, CancellationToken token);
        }

        private class RelayServerApplication<TContext> : RelayServerApplication
        {
            private readonly IHttpApplication<TContext> _application;

            public RelayServerApplication(IHttpApplication<TContext> application)
            {
                _application = application;
            }

            public override async Task ReceiveRequestAsync(IFeatureCollection features, CancellationToken token)
            {
                features.Set(new HttpRequestLifetimeFeature { RequestAborted = token });

                TContext context = _application.CreateContext(features);
                try
                {
                    await _application.ProcessRequestAsync(context);

                    _application.DisposeContext(context, null);
                }
                catch (Exception ex)
                {
                    _application.DisposeContext(context, ex);
                }
            }
        }
    }
}
