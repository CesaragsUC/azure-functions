using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;

namespace DurableFunction
{
    public static class DurableTester
    {
        //2 - Ponto intermediário, chama as atividades
        [FunctionName("DurableTester")]
        public static async Task<List<string>> RunOrchestrator(
            [OrchestrationTrigger] IDurableOrchestrationContext context)
        {
            var outputs = new List<string>();
            var input = context.GetInput<string>();

            // Replace "hello" with the name of your Durable Activity Function.
            outputs.Add(await context.CallActivityAsync<string>(nameof(SayHello), "Tokyo"));
            outputs.Add(await context.CallActivityAsync<string>(nameof(SayHello), "Seattle"));
            outputs.Add(await context.CallActivityAsync<string>(nameof(SayHello), "London"));

            outputs.Add(await context.CallActivityAsync<string>(nameof(AddToQueue), input));

            // returns ["Hello Tokyo!", "Hello Seattle!", "Hello London!"]
            return outputs;
        }

        [FunctionName(nameof(SayHello))]
        public static string SayHello([ActivityTrigger] string name, ILogger log)
        {
            log.LogInformation("Saying hello to {name}.", name);
            return $"Hello {name}!";
        }


        [FunctionName(nameof(AddToQueue))]
        public static string AddToQueue([ActivityTrigger] string messageToAdd, ILogger log,
        [Queue("queue-durable-function"), StorageAccount("AzureWebJobsStorage")] ICollector<string> msg)
        {
            if (!string.IsNullOrEmpty(messageToAdd))
            {
                log.LogInformation("Message added to Queue", messageToAdd);
                msg.Add(messageToAdd);
                return $"{messageToAdd} has been added";
                
            }
            return "Please pass a message to add to the queue";
        }

        //1 - Ponto inicial, chama o Orquestrador
        [FunctionName("DurableTester_HttpStart")]
        public static async Task<HttpResponseMessage> HttpStart(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post")] HttpRequestMessage req,
            [DurableClient] IDurableOrchestrationClient starter,
            ILogger log)
        {
            string requestBody = await req.Content.ReadAsStringAsync();
            // Function input comes from the request content.
            string instanceId = await starter.StartNewAsync("DurableTester", null, requestBody);

            log.LogInformation($"Started orchestration with ID = '{instanceId}'.");

            return starter.CreateCheckStatusResponse(req, instanceId);
        }
    }
}

//id: Este é o identificador único da instância da função durável. Você pode usar esse ID para consultar o status da instância ou interagir com ela usando os URIs fornecidos abaixo.
//statusQueryGetUri: Este URI permite que você consulte o status da instância da função durável. Você pode fazer uma solicitação GET para este URI para obter informações sobre o estado atual da instância, como se ela está em execução, concluída ou falhou.
//sendEventPostUri: Este URI permite que você envie eventos para a instância da função durável. Você pode fazer uma solicitação POST para este URI para enviar um evento personalizado para a instância, que pode ser usado para controlar o fluxo de trabalho.
//terminatePostUri: Este URI permite que você termine a instância da função durável. Você pode fazer uma solicitação POST para este URI para encerrar a execução da instância com um motivo opcional fornecido no parâmetro de consulta reason.
//purgeHistoryDeleteUri: Este URI permite que você remova permanentemente o histórico de execução da instância da função durável. Você pode fazer uma solicitação DELETE para este URI para limpar o histórico de execução da instância.
//restartPostUri: Este URI permite que você reinicie a instância da função durável. Você pode fazer uma solicitação POST para este URI para reiniciar a execução da instância.
//suspendPostUri: Este URI permite que você suspenda a instância da função durável. Você pode fazer uma solicitação POST para este URI para pausar a execução da instância com um motivo opcional fornecido no parâmetro de consulta reason.
//resumePostUri: Este URI permite que você retome a execução de uma instância da função durável que foi suspensa anteriormente. Você pode fazer uma solicitação POST para este URI para retomar a execução da instância com um motivo opcional fornecido no parâmetro de consulta reason.