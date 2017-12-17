using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Http.Internal;
using Microsoft.Extensions.Primitives;

namespace Ricciolo.AspNetCore.AzureRelay.FeatureSerializer
{
    public abstract class HttpFeatureSerializerBase
    {

        protected void WriteHeaders(BinaryWriter writer, IHeaderDictionary headers)
        {
            writer.Write(headers.ContentLength.GetValueOrDefault());
            writer.Write(headers.Count);
            foreach (var pair in headers)
            {
                writer.Write(pair.Key);
                writer.Write(pair.Value.Count);
                for (int i = 0; i < pair.Value.Count; i++)
                {
                    writer.Write(pair.Value[i]);
                }
            }
        }

        protected void ReadHeaders(BinaryReader reader, IHeaderDictionary headers)
        {
            headers.ContentLength = reader.ReadInt64();
            int headersCount = reader.ReadInt32();
            for (int x = 0; x < headersCount; x++)
            {
                string key = reader.ReadString();
                int valuesCount = reader.ReadInt32();
                string[] values = new string[valuesCount];
                for (int c = 0; c < valuesCount; c++)
                {
                    values[c] = reader.ReadString();
                }
                headers[key] = new StringValues(values);
            }
        }

        protected async Task ReadStreamAsync(Stream fromStream, Stream toStream, long length, CancellationToken token)
        {
            byte[] buffer = new byte[1024];
            int i;
            long count = 0;
            while (count < length && (i = await fromStream.ReadAsync(buffer, 0, 1024, token)) > 0)
            {
                await toStream.WriteAsync(buffer, 0, i, token);

                count += i;
            }
            toStream.Seek(0, SeekOrigin.Begin);
        }
    }
}
