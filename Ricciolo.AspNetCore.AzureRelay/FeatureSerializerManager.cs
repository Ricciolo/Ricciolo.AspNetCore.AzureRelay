using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http.Features;
using Ricciolo.AspNetCore.AzureRelay.FeatureSerializer;

namespace Ricciolo.AspNetCore.AzureRelay
{
    public class FeatureSerializerManager
    {
        private readonly Dictionary<Type, IFeatureSerializer> _featureSerializers;

        public FeatureSerializerManager(IEnumerable<IFeatureSerializer> featureSerializers)
        {
            _featureSerializers = featureSerializers
                .OrderBy(d => d.FeatureType.Name)
                .ToDictionary(d => d.FeatureType);
        }

        public async Task WriteAsync(Stream stream, IFeatureCollection features, Type[] featureTypes, CancellationToken token)
        {
            foreach (Type type in featureTypes)
            {
                if (!_featureSerializers.TryGetValue(type, out IFeatureSerializer serializer)) continue;
                token.ThrowIfCancellationRequested();

                object feature = features[type];
                if (feature == null) continue;

                await serializer.WriteAsync(stream, feature, token);
            }
        }

        public async Task<IFeatureCollection> ReadAsync(Stream stream, Type[] featureTypes, CancellationToken token)
        {
            FeatureCollection features = new FeatureCollection();

            foreach (Type type in featureTypes)
            {
                if (!_featureSerializers.TryGetValue(type, out IFeatureSerializer serializer)) continue;
                token.ThrowIfCancellationRequested();

                object feature = await serializer.ReadAsync(stream, token);
                features[type] = feature;
            }

            return features;
        }
    }
}
