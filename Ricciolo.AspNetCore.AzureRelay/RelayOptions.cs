using System;
using System.Collections.Generic;
using System.Text;

namespace Ricciolo.AspNetCore.AzureRelay
{
    public class RelayOptions
    {
        public string ConnectionString { get; set; }

        public int ClientsPoolSize { get; set; } = Environment.ProcessorCount * 25;
    }
}
