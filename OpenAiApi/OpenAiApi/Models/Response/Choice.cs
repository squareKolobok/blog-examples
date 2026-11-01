namespace OpenAiApi.Models.Response;

public class Choice
{
    public int Index { get; set; }
    public Message? Message { get; set; }
    public Message? Delta { get; set; }
    public string? FinishReason { get; set; }
}