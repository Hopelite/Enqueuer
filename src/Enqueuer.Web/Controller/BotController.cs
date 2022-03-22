using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Telegram.Bot.Types;

namespace Enqueuer.Web.Controller
{
    /// <summary>
    /// Controller, that handles bot requests.
    /// </summary>
    public class BotController : ControllerBase
    {
        private readonly IUpdateHandler updateHandler;

        /// <summary>
        /// Initializes a new instance of the <see cref="BotController"/> class.
        /// </summary>
        /// <param name="updateHandler"><see cref="IUpdateHandler"/> to handle incoming updates by.</param>
        public BotController(IUpdateHandler updateHandler)
        {
            this.updateHandler = updateHandler;
        }

        /// <summary>
        /// Handles incoming telegram <see cref="Update"/>.
        /// </summary>
        /// <param name="update">Incoming telegram <see cref="Update"/> to handle/</param>
        /// <returns><see cref="OkResult"/> in response.</returns>
        [HttpPost]
        public async Task<IActionResult> Post([FromBody] Update update)
        {
            await this.updateHandler.HandleUpdateAsync(update);
            return Ok();
        }
    }
}
