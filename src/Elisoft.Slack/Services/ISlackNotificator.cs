namespace Elisoft.Slack
{
  public interface ISlackNotificator
  {
    Task<bool> SendMessageAsync(string webhookUrl, string messageText);
  }
}