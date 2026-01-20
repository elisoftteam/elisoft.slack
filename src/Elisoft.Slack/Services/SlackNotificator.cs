using Microsoft.Extensions.Logging;
using System.Text;
using System.Text.Json;

namespace Elisoft.Slack
{
  public class SlackNotificator : ISlackNotificator
  {
    private readonly HttpClient _httpClient;
    private readonly ILogger<SlackNotificator> _logger;

    public SlackNotificator(HttpClient httpClient, ILogger<SlackNotificator> logger)
    {
      _httpClient = httpClient;
      _logger = logger;
    }

    public async Task<bool> SendMessageAsync(string webhookUrl, string messageText)
    {
      if (string.IsNullOrWhiteSpace(webhookUrl))
      {
        _logger.LogError("WebhookUrl is required.");
        throw new ArgumentNullException(nameof(webhookUrl));
      }

      if (!Uri.TryCreate(webhookUrl, UriKind.Absolute, out _))
      {
        _logger.LogError("Incorrect format WebhookUrl.");
        throw new ArgumentException(nameof(webhookUrl));
      }

      if (string.IsNullOrWhiteSpace(messageText))
      {
        _logger.LogError("The message content is required.");
        throw new ArgumentException(nameof(messageText));
      }

      var slackPayloadObject = new
      {
        text = messageText,
      };

      var jsonPayload = JsonSerializer.Serialize(slackPayloadObject);
      var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

      try
      {
        var response = await _httpClient.PostAsync(webhookUrl, content);

        if (!response.IsSuccessStatusCode)
        {
          var error = await response.Content.ReadAsStringAsync();
          _logger.LogError($"Error Slack Api ({response.StatusCode}): {error}");
          return false;
        }

        _logger.LogInformation("Notification sent successfully.");
        return true;
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "Exception when communicating with Slack.");
        return false;
      }
    }
  }
}