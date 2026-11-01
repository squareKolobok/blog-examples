namespace OpenAiApi.Models.Request;

public class Message
{
    public Roles Role { get; set; }
    public string? Content { get; set; }
}
