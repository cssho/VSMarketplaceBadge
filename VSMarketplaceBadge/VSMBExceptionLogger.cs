using System.Threading;
using System.Threading.Tasks;
using System.Web.Http.ExceptionHandling;

namespace VSMarketplaceBadge
{
    public class VSMBExceptionLogger : ExceptionLogger
    {
        public override void Log(ExceptionLoggerContext context)
        {
            Loggly.SendError(context.Exception).FireAndForget();
        }

        public async override Task LogAsync(ExceptionLoggerContext context, CancellationToken cancellationToken)
        {
            await Loggly.SendError(context.Exception);
        }
    }
}