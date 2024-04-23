using System;
using System.Net.Http;
using Bogus;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;

namespace Function.InProcess.Mode
{
    public class Usuario
    {
        public Guid Id { get; set; }
        public string Nome { get; set; }
        public DateTime DataNascimento { get; set; }
        public int Idade { get; set; }

    }

    public class MessageSender
    {
        /// <summary>
        /// Chama a cada 5 segundo a api da function  MessageReceiver
        /// </summary>
        /// <param name="myTimer"></param>
        /// <param name="log"></param>
        [FunctionName("MessageSender")]
        public void Run([TimerTrigger("*/5 * * * * *")]TimerInfo myTimer, ILogger log)
        {
            log.LogInformation("\n\n");
            log.LogInformation($"C# Timer trigger function executed at: {DateTime.Now}");

            // cria um usuario com bogus
            var faker = new Faker("pt_BR");
            var usuario = new Usuario
            {
                Id = Guid.NewGuid(),
                Nome = faker.Person.FullName,
                DataNascimento = faker.Person.DateOfBirth,
                Idade = faker.Random.Int(18, 80)
            };

            HttpClient client = new();
            HttpRequestMessage request = new(HttpMethod.Post, "http://localhost:7006/api/MessageReceiver");

            request.Content = new StringContent(Newtonsoft.Json.JsonConvert.SerializeObject(usuario), 
                                                System.Text.Encoding.UTF8, "application/json");

            client.Send(request);    

            log.LogInformation($"--- Usuario {usuario.Nome} enviado para a fila. ---");
            log.LogInformation("\n\n");
        }
    }
}
