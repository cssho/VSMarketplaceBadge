using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http.ExceptionHandling;

namespace VSMarketplaceBadge
{
    public class VSMBExceptionLogger : ExceptionLogger
    {
        public override void Log(ExceptionLoggerContext context)
        {
            Logging(context);
        }

        public async override Task LogAsync(ExceptionLoggerContext context, CancellationToken cancellationToken)
        {
            await Task.Run(() => Logging(context));
        }

        private void Logging(ExceptionLoggerContext context)
        {
            Trace.TraceError(JsonConvert.SerializeObject(new
            {
                Url = context.Request.RequestUri,
                Exception = context.Exception
            }));
        }
    }
}