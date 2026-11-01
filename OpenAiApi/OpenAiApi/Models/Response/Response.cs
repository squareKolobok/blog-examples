using System.Collections.ObjectModel;

namespace OpenAiApi.Models.Response;

internal class Response
{
    public string Id { get; set; } = string.Empty;
    public string Object { get; set; } = string.Empty;
    public string Model { get; set; } = string.Empty;
    public long Created { get; set; }
    public Collection<Choice> Choices { get; set; } = [];
    public Usage? Usage { get; set; }
}
