using System.Threading.Tasks;
using Enqueuer.Bot;
using Microsoft.AspNetCore.Mvc;
using Telegram.Bot.Types;

namespace Enqueuer.Web.Controller
{
    public class BotController : ControllerBase
    {
        [HttpPost]
        public async Task<IActionResult> Post([FromServices] IUpdateHandler updateHandler, [FromBody] Update update)
        {
            await updateHandler.HandleUpdateAsync(update);
            return Ok();
        }
    }
}
