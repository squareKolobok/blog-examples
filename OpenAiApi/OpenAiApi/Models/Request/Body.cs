using System.Collections.ObjectModel;

namespace OpenAiApi.Models.Request;

public class Body
{
    public required string Model { get; set; }
    public required Collection<Message> Messages { get; set; }
    public bool Stream { get; set; }
}
