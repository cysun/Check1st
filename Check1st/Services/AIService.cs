using Azure.AI.OpenAI;
using Check1st.Models;
using Microsoft.Extensions.Options;

namespace Check1st.Services;

public class AISettings
{
    public string ApiKey { get; set; }
    public string Model { get; set; }

    // The maximum # of consultations per assignment a student can use
    public int PerAssignmentLimit { get; set; }
}

public class AIService
{
    private readonly AISettings _settings;
    private readonly OpenAIClient _client;

    private readonly ILogger<AIService> _logger;

    public AIService(IOptions<AISettings> settings, ILogger<AIService> logger)
    {
        _settings = settings.Value;
        _client = new OpenAIClient(_settings.ApiKey);
        _logger = logger;
    }

    public int PerAssignmentLimit => _settings.PerAssignmentLimit;

    public async Task<bool> ConsultAsync(Consultation consultation)
    {
        var chatCompletionOptions = new ChatCompletionsOptions
        {
            DeploymentName = _settings.Model,
            Messages =
            {
                new ChatRequestSystemMessage("You are a teaching assistant reviewing my solution to an assignment."),
                new ChatRequestSystemMessage("Format response in Markdown."),
                new ChatRequestSystemMessage("The following is the assignment:"),
                new ChatRequestSystemMessage(consultation.Assignment.Description),
                new ChatRequestSystemMessage("My solution is as follows:"),
            }
        };

        foreach (var file in consultation.Files)
            chatCompletionOptions.Messages.Add(new ChatRequestUserMessage(file.Content.Text));

        var response = await _client.GetChatCompletionsAsync(chatCompletionOptions);
        consultation.Feedback = response.Value.Choices[0].Message.Content;
        consultation.Model = _settings.Model;
        consultation.PromptTokens = response.Value.Usage.PromptTokens;
        consultation.CompletionTokens = response.Value.Usage.CompletionTokens;
        consultation.TimeCompleted = DateTime.UtcNow;

        bool success = response.Value.Choices[0].FinishReason == CompletionsFinishReason.Stopped;
        if (success)
        {
            _logger.LogInformation("Consultation {id} finished successfully", consultation.Id);
        }
        else
        {
            _logger.LogWarning("Consultation {id} did not finish successfully: {reason}\n{details}", consultation.Id,
                response.Value.Choices[0].FinishReason, response.Value.Choices[0].FinishDetails);
        }

        return success;
    }
}
