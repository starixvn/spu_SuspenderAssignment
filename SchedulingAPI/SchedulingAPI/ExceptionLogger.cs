using Fanex.Logging;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http.ExceptionHandling;

namespace SchedulingAPI
{
    public class ExceptionLogger : IExceptionLogger
    {
        public Task LogAsync(ExceptionLoggerContext context, CancellationToken cancellationToken)
        {
            Logger.Log.Error(context.Exception.Message, context.Exception);

            return Task.CompletedTask;
        }
    }
}