namespace OpenAiApi.Models.Response;

public class Message
{
    public Roles Role { get; set; }
    public string? ReasoningContent { get; set; }
    public string? Content { get; set; }
    public string? Refusal { get; set; }
}