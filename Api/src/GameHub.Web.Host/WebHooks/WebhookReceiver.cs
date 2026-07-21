using Eaf.WebHooks;
using System.Threading.Tasks;

namespace GameHub.Web.WebHooks
{
    public class WebHookReceiver : EafWebHookReceiver
    {
        public override async Task ProcessRequest(string requestBody)
        {
            Logger.InfoFormat("WebHook '{0}' Body: {1}", ReceiverName, requestBody);
            await Task.CompletedTask;
        }
    }
}