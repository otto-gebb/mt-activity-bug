using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace GettingStarted
{
    public class Message
    {
        public string Text { get; set; }
    }

    public class MessageConsumer :
        IConsumer<Message>
    {
        readonly ILogger<MessageConsumer> _logger;

        public MessageConsumer(ILogger<MessageConsumer> logger)
        {
            _logger = logger;
        }

        public Task Consume(ConsumeContext<Message> context)
        {
            bool isCurrentActivityGlobal = Program.globalActivity == Activity.Current;
            var baggage = string.Join(';', Activity.Current.Baggage.Select(x => $"{x.Key}, {x.Value}"));
            _logger.LogInformation("Received Text: {Text}", context.Message.Text);
            _logger.LogInformation($"inGlobalActivity: {isCurrentActivityGlobal}, baggage: {baggage}");
            return Task.CompletedTask;
        }
    }
}
