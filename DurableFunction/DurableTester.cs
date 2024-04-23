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
        //2 - Ponto intermedi�rio, chama as atividades
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

//id: Este � o identificador �nico da inst�ncia da fun��o dur�vel. Voc� pode usar esse ID para consultar o status da inst�ncia ou interagir com ela usando os URIs fornecidos abaixo.
//statusQueryGetUri: Este URI permite que voc� consulte o status da inst�ncia da fun��o dur�vel. Voc� pode fazer uma solicita��o GET para este URI para obter informa��es sobre o estado atual da inst�ncia, como se ela est� em execu��o, conclu�da ou falhou.
//sendEventPostUri: Este URI permite que voc� envie eventos para a inst�ncia da fun��o dur�vel. Voc� pode fazer uma solicita��o POST para este URI para enviar um evento personalizado para a inst�ncia, que pode ser usado para controlar o fluxo de trabalho.
//terminatePostUri: Este URI permite que voc� termine a inst�ncia da fun��o dur�vel. Voc� pode fazer uma solicita��o POST para este URI para encerrar a execu��o da inst�ncia com um motivo opcional fornecido no par�metro de consulta reason.
//purgeHistoryDeleteUri: Este URI permite que voc� remova permanentemente o hist�rico de execu��o da inst�ncia da fun��o dur�vel. Voc� pode fazer uma solicita��o DELETE para este URI para limpar o hist�rico de execu��o da inst�ncia.
//restartPostUri: Este URI permite que voc� reinicie a inst�ncia da fun��o dur�vel. Voc� pode fazer uma solicita��o POST para este URI para reiniciar a execu��o da inst�ncia.
//suspendPostUri: Este URI permite que voc� suspenda a inst�ncia da fun��o dur�vel. Voc� pode fazer uma solicita��o POST para este URI para pausar a execu��o da inst�ncia com um motivo opcional fornecido no par�metro de consulta reason.
//resumePostUri: Este URI permite que voc� retome a execu��o de uma inst�ncia da fun��o dur�vel que foi suspensa anteriormente. Voc� pode fazer uma solicita��o POST para este URI para retomar a execu��o da inst�ncia com um motivo opcional fornecido no par�metro de consulta reason.