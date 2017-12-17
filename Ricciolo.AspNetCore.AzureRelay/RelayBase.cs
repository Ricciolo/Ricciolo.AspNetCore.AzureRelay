using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Azure.Relay;
using Microsoft.Extensions.Options;

namespace Ricciolo.AspNetCore.AzureRelay
{
    public abstract class RelayBase : IDisposable
    {
        public IOptions<RelayOptions> RelayOptions { get; }

        protected RelayBase(IOptions<RelayOptions> relayOptions)
        {
            RelayOptions = relayOptions ?? throw new ArgumentNullException(nameof(relayOptions));
        }

        protected virtual void Dispose(bool disposing)
        {
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

    }
}
