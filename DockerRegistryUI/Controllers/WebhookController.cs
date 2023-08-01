using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MySqlX.XDevAPI.Relational;
using Newtonsoft.Json;

namespace DockerRegistryUI.Controllers
{
    [Route("api/webhook")]
    [ApiController]
    public class WebhookController : ControllerBase
    {
        public WebhookController(WebhooksQueue webhooksQueue)
        {
            WebhooksQueue = webhooksQueue;
        }

        public WebhooksQueue WebhooksQueue { get; }

        [HttpPost]
        public async Task<IActionResult> HandleWebhookAsync()
        {
            using var reader = new StreamReader(Request.Body);
            var requestBody = await reader.ReadToEndAsync();

            WebhooksQueue.HandleWebhook(requestBody);

            return NoContent();
        }
    }
}
