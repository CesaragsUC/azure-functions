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
    public class MessageProcessor
    {

        /// <summary>
        /// Essa triqgger é acionada quando uma mensagem é adicionada a fila "myqueue-items"
        /// Assim que é processado a mensagem é removida da fila
        /// </summary>
        /// <param name="queueItem"></param>
        /// <param name="log"></param>
        [FunctionName("MessageProcessor")]
        [StorageAccount("AzureWebJobsStorage")]
        public void Run([QueueTrigger("myqueue-items")] string queueItem, ILogger log)
        {
            // Ideias do que fazer com a mensagem
            // 1. Salvar no banco de dados
            // 2. Enviar um email
            // 3. Salvar em um arquivo
            // 4. Enviar para outro serviço
            // 5. Enviar para outro storage
            // 6. Enviar para outro queue
            // 7. Enviar para outro topico
            // Etc...    
            log.LogInformation("\n\n");
            log.LogInformation($"### Queue Trigger function processed: {queueItem} ###");
            log.LogInformation("\n\n");
        }
    }
}
