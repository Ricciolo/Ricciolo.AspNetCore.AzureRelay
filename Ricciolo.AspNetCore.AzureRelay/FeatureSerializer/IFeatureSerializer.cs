using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Ricciolo.AspNetCore.AzureRelay.FeatureSerializer
{
    public interface IFeatureSerializer
    {
        Type FeatureType { get; }

        Task WriteAsync(Stream stream, object feature, CancellationToken token);

        Task<object> ReadAsync(Stream stream, CancellationToken token);
    }

}
