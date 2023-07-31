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
        public WebhookController(WebhooksService webhooksService)
        {
            WebhooksService = webhooksService;
        }

        public WebhooksService WebhooksService { get; }

        [HttpPost]
        public async Task<IActionResult> HandleWebhookAsync()
        {
            var now = DateTimeOffset.Now.ToOffset(TimeSpan.FromHours(10));
            Console.WriteLine($"{now} Received webhook POST. Request body is below.");
            Console.WriteLine("-----------------------------------");

            using var reader = new StreamReader(Request.Body);
            var requestBody = await reader.ReadToEndAsync();

            Console.WriteLine(requestBody);
            Console.WriteLine("-----------------------------------");
            Console.WriteLine();

            try
            {
                var request = JsonConvert.DeserializeObject<DockerRegistryEventsRequest>(requestBody);
                if (request is not null)
                {
                    foreach (var evt in request.Events)
                    {
                        await WebhooksService.HandleEventAsync(evt);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }

            return NoContent();
        }
    }
}
