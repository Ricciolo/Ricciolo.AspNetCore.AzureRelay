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
    public class HttpResponseFeatureSerializer : HttpFeatureSerializerBase, IFeatureSerializer
    {
        public Type FeatureType => typeof(IHttpResponseFeature);

        public async Task WriteAsync(Stream stream, object feature, CancellationToken token)
        {
            IHttpResponseFeature hrf = (IHttpResponseFeature)feature;
            if (hrf.Body != Stream.Null && !hrf.Headers.ContentLength.HasValue)
            {
                hrf.Headers.ContentLength = hrf.Body.Length;
            }

            var buffer = new BufferedStream(stream);
            using (var writer = new BinaryWriter(buffer, Encoding.UTF8, true))
            {
                writer.Write(hrf.StatusCode);
                writer.Write(hrf.ReasonPhrase ?? String.Empty);

                WriteHeaders(writer, hrf.Headers);
            }
            await stream.FlushAsync(token);
            if (hrf.Body != Stream.Null && hrf.Body.Length > 0)
            {
                hrf.Body.Seek(0, SeekOrigin.Begin);
                await hrf.Body.CopyToAsync(stream, 81920, token);
                await stream.FlushAsync(token);
            }
        }

        public async Task<object> ReadAsync(Stream stream, CancellationToken token)
        {
            var hrf = new HttpResponseFeature();

            using (var reader = new BinaryReader(stream, Encoding.UTF8, true))
            {
                hrf.StatusCode = reader.ReadInt32();
                hrf.ReasonPhrase = reader.ReadString();

                ReadHeaders(reader, hrf.Headers);
            }

            hrf.Body = new MemoryStream();
            await ReadStreamAsync(stream, hrf.Body, hrf.Headers.ContentLength.GetValueOrDefault(), token);

            return hrf;
        }

    }
}
