namespace Aspire.Kafka.Api.MessageModels;

public class UserLoggedInMessageModel
{
    public Guid UserId { get; set; }
    public DateTime LoggedDate => DateTime.Now;
}