using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Ricciolo.AspNetCore.AzureRelay
{
    public class RelaySenderMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly RelaySender _relaySender;

        public RelaySenderMiddleware(RequestDelegate next, RelaySender relaySender)
        {
            _next = next;
            _relaySender = relaySender;
        }

        public async Task Invoke(HttpContext context)
        {
            await _relaySender.SendRequestAsync(context.Features, context.RequestAborted);
        }
    }
}
