using MassTransit;
using NetCoreApplication.ConsoleApp.Contracts;
using System;
using System.Threading.Tasks;

namespace NetCoreApplication.ConsoleApp.RabbitMQ
{
    public class TimeConsumer : IConsumer<IsItTime>
    {
        public Task Consume(ConsumeContext<IsItTime> context)
        {
            //var now = DateTimeOffset.Now;
            //if (now.DayOfWeek == DayOfWeek.Friday && now.Hour >= 17)
            //{
            //    return context.RespondAsync<YesItIs>(new { });
            //}

            //return context.RespondAsync<NoNotYet>(new { });

            Console.WriteLine($"Received message: {DateTime.Now}");

            return Task.CompletedTask;
        }
    }
}
