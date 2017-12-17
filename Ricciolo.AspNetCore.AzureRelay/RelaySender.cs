using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Azure.Relay;
using Microsoft.Extensions.ObjectPool;
using Microsoft.Extensions.Options;

namespace Ricciolo.AspNetCore.AzureRelay
{
    public class RelaySender : RelayBase
    {
        private readonly ObjectPool<RelayClient> _clientsPool;

        public RelaySender(IOptions<RelayOptions> relayOptions, FeatureSerializerManager featureSerializerManager) : base(relayOptions)
        {
            var clientsPoolProvider = new DefaultObjectPoolProvider
            {
                MaximumRetained = relayOptions.Value.ClientsPoolSize
            };
            var hybridConnectionClient = new HybridConnectionClient(relayOptions.Value.ConnectionString);
            _clientsPool = clientsPoolProvider.Create(new RelayClientPooledObjectPolicy(hybridConnectionClient, featureSerializerManager));
        }

        public async Task SendRequestAsync(IFeatureCollection features, CancellationToken token)
        {
            RelayClient client = null;
            try
            {
                try
                {
                    client = _clientsPool.Get();
                    if (!client.IsOpen)
                    {
                        await client.OpenAsync(token);
                    }

                    await client.SendRequestAsync(features, token);
                }
                catch
                {
                    // TODO
                    if (client != null)
                    {
                        await client.CloseAsync(CancellationToken.None);
                    }
                }
            }
            finally
            {
                if (client != null)
                {
                    _clientsPool.Return(client);
                }
            }
        }


    }
}
