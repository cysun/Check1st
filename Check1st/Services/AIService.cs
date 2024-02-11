using Azure.AI.OpenAI;
using Check1st.Models;
using Microsoft.Extensions.Options;

namespace Check1st.Services;

public class AISettings
{
    public string ApiKey { get; set; }
    public string Model { get; set; }
}

public class AIService
{
    private readonly AISettings _settings;
    private readonly OpenAIClient _client;

    public AIService(IOptions<AISettings> settings)
    {
        _settings = settings.Value;
        _client = new OpenAIClient(_settings.ApiKey);
    }

    public async Task<string> ConsultAsync(Consultation consultation)
    {
        var chatCompletionOptions = new ChatCompletionsOptions
        {
            DeploymentName = _settings.Model,
            Messages =
            {
                new ChatRequestSystemMessage(consultation.Assignment.Prompt)
            }
        };

        foreach (var file in consultation.Files)
            chatCompletionOptions.Messages.Add(new ChatRequestUserMessage(file.Content.Text));

        var response = await _client.GetChatCompletionsAsync(chatCompletionOptions);
        return response.Value.Choices[0].Message.Content;
    }
}
