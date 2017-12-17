using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Http.Internal;
using Microsoft.Extensions.Primitives;

namespace Ricciolo.AspNetCore.AzureRelay.FeatureSerializer
{
    public class HttpRequestFeatureSerializer : HttpFeatureSerializerBase, IFeatureSerializer
    {
        public Type FeatureType => typeof(IHttpRequestFeature);

        public async Task WriteAsync(Stream stream, object feature, CancellationToken token)
        {
            IHttpRequestFeature hrf = (IHttpRequestFeature)feature;

            var buffer = new BufferedStream(stream);
            using (var writer = new BinaryWriter(buffer, Encoding.UTF8, true))
            {
                writer.Write(hrf.Method);
                writer.Write(hrf.Path);
                writer.Write(hrf.PathBase);
                writer.Write(hrf.Protocol);
                writer.Write(hrf.QueryString);
                writer.Write(hrf.RawTarget);
                writer.Write(hrf.Scheme);

                WriteHeaders(writer, hrf.Headers);
            }
            await stream.FlushAsync(token);

            await hrf.Body.CopyToAsync(stream);
            await stream.FlushAsync(token);
        }

        public async Task<object> ReadAsync(Stream stream, CancellationToken token)
        {
            var hrf = new HttpRequestFeature();

            using (var reader = new BinaryReader(stream, Encoding.UTF8, true))
            {
                hrf.Method = reader.ReadString();
                hrf.Path = reader.ReadString();
                hrf.PathBase = reader.ReadString();
                hrf.Protocol = reader.ReadString();
                hrf.QueryString = reader.ReadString();
                hrf.RawTarget = reader.ReadString();
                hrf.Scheme = reader.ReadString();

                ReadHeaders(reader, hrf.Headers);
            }

            await ReadStreamAsync(stream, hrf.Body, hrf.Headers.ContentLength.GetValueOrDefault(), token);

            return hrf;
        }
    }

}
