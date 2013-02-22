using System;
using RabbitMQ.Client;

namespace Mike.AmqpSpike
{
    public class ModelEventSpike
    {
        public void ModelShutdownEventShouldBeModelSpecific()
        {
            var connectionFactory = new ConnectionFactory
            {
                HostName = "localhost",

            };
            using (var connection = connectionFactory.CreateConnection())
            {
                var model1 = connection.CreateModel();
                var model2 = connection.CreateModel();
                model1.ModelShutdown += (model, reason) => Console.WriteLine("Model 1 Shutdown");
                model2.ModelShutdown += (model, reason) => Console.WriteLine("Model 2 Shutdown");


                Console.WriteLine("Model 1 Closing");
                model1.Close();
                Console.WriteLine("Model 1 Closed");
            }            
            
        } 
    }
}