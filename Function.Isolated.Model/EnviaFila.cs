using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using System.Net;

namespace Function.Isolated.Model
{
    /// <summary>
    /// Rodar o azurite no cmd para debugar local
    /// </summary>
    /// https://learn.microsoft.com/en-us/azure/azure-functions/dotnet-isolated-process-guide?tabs=windows
    public class MyOutputType
    {
        [QueueOutput("minha-fila")]
        public string Message { get; set; } // marca essa propriedade como a saida da fila

        public HttpResponseData HttpResponse { get; set; } // marca essa propriedade como a saida da funcao
    }

    public class EnviaFila
    {
        private readonly ILogger<EnviaFila> _logger;

        public EnviaFila(ILogger<EnviaFila> logger)
        {
            _logger = logger;
        }

        [Function(nameof(EnviaFila))]
        public async Task<MyOutputType> Run([HttpTrigger(AuthorizationLevel.Anonymous, "post")] HttpRequestData req,
        FunctionContext context)
        {
            _logger.LogInformation("\n\n");
            _logger.LogInformation($"C# Queue trigger function processed");
            _logger.LogInformation("\n\n");

            var response = req.CreateResponse(HttpStatusCode.OK);

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();

            return new MyOutputType()
            {
                Message = requestBody,
                HttpResponse = response
            };
        }
    }
}
