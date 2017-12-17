using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using Microsoft.Azure.Relay;
using Microsoft.Extensions.ObjectPool;

namespace Ricciolo.AspNetCore.AzureRelay
{
    public class RelayClientPooledObjectPolicy : IPooledObjectPolicy<RelayClient>
    {
        private readonly HybridConnectionClient _client;
        private readonly FeatureSerializerManager _featureSerializerManager;

        public RelayClientPooledObjectPolicy(HybridConnectionClient client, FeatureSerializerManager featureSerializerManager)
        {
            _client = client;
            _featureSerializerManager = featureSerializerManager;
        }

        public RelayClient Create()
        {
            return new RelayClient(_client, _featureSerializerManager);
        }

        public bool Return(RelayClient obj)
        {
            if (!obj.GoodStatus)
            {
                obj.Dispose();
                return false;
            }

            return true;
        }
    }
}
