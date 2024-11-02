using Aspire.Kafka.Api.MessageModels;
using MassTransit;

namespace Aspire.Kafka.Api.Consumers;

public class UserLoggedInConsumer(ILogger<UserLoggedInConsumer> logger):IConsumer<UserLoggedInMessageModel>
{
    public async  Task Consume(ConsumeContext<UserLoggedInMessageModel> context)
    {
        logger.LogWarning("User With ID: {userId} Logged In At Time {time}",context.Message.UserId,context.Message.LoggedDate);
        await Task.Delay(1_000);
    }
}