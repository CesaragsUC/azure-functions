using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;

namespace Azure.Function.Demo
{

    /// <summary>
    /// Rodar o azurite no cmd para debugar local
    /// </summary>
    //https://learn.microsoft.com/pt-br/azure/azure-functions/functions-add-output-binding-storage-queue-vs?tabs=in-process
    public static class MessageReceiver
    {

        /// <summary>
        /// Quando a funcão é assincrona, o binding de saida da queue é feito com ICollector ex: [Queue("myqueue-items")] ICollector<string> msg
        /// Caso seja sincrona, o binding de saida da queue poderia ser somente com string ex: [Queue("myqueue-items")] string msg
        /// </summary>
        /// <param name="req"></param>
        /// <param name="msg"></param>
        /// <param name="log"></param>
        /// <returns></returns>

        [FunctionName("MessageReceiver")]
        [StorageAccount("AzureWebJobsStorage")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)] HttpRequest req,
            [Queue("myqueue-items")] ICollector<string> msg,
            ILogger log)
        {
            log.LogInformation("\n\n");
            log.LogInformation("*** HTTP trigger function processed a request. ***");
            log.LogInformation("\n\n");

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();

            //Envia a mensagem para o storage queue
            msg.Add(requestBody);

            return new OkResult();
        }
    }
}
