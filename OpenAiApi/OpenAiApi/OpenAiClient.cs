using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using OpenAiApi.Models;
using OpenAiApi.Models.Request;
using OpenAiApi.Models.Response;

namespace OpenAiApi;

public class OpenAiClient : IDisposable
{
    private const float START_COMRESS_PERCENT = 0.7F;
    private const string CHAT_URL = "v1/chat/completions";
    private const string START_DATA_LINE = "data: ";
    private const string LAST_DATA_LINE = "data: [DONE]";
    private readonly HttpClient client;
    private readonly JsonSerializerOptions options;
    private readonly string model;
    private readonly string compressPromt;
    private readonly int maxWindowSize;
    private int currenWindowSize = 0;
    private List<Models.Request.Message> messages = new();

    public OpenAiClient(Settings settings)
    {
        client = new();
        client.BaseAddress = new Uri(settings.BaseAddress);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", settings.Token);

        options = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
            Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
        };

        model = settings.Model;
        maxWindowSize = settings.MaxWindowSize;
        compressPromt = File.ReadAllText(settings.CompressPromtPath);
    }

    public int CurrentContextSize { get { return currenWindowSize; } }

    public async Task<string> GetAiResponseAsync(string message)
    {
        await TryCompressContextAsync();
        var apiResponse = await GetHttpResponseMessageAsync(message, stream: false);

        apiResponse.EnsureSuccessStatusCode();

        var response = await apiResponse.Content.ReadFromJsonAsync<Models.Response.Response>(options);
        SaveMessage(message, Roles.User);
        var responseMessage = response?.Choices.FirstOrDefault()?.Message?.Content ?? string.Empty;
        SaveMessage(responseMessage, Roles.Assistant);

        return responseMessage;
    }

    public async Task SendAiStreamResponseAsync(string message, Action<string> consumer)
    {
        await TryCompressContextAsync();
        var apiResponse = await GetHttpResponseMessageAsync(message, stream: true);

        apiResponse.EnsureSuccessStatusCode();

        using var responseStream = await apiResponse.Content.ReadAsStreamAsync();
        using var reader = new StreamReader(responseStream);
        var stringBuilder = new StringBuilder();
        Usage? usage = null;

        while (!reader.EndOfStream)
        {
            var line = await reader.ReadLineAsync();

            if (string.IsNullOrEmpty(line) ||
                !line.StartsWith(START_DATA_LINE, StringComparison.CurrentCultureIgnoreCase) ||
                line.StartsWith(LAST_DATA_LINE, StringComparison.InvariantCultureIgnoreCase))
            {
                continue;
            }

            var data = line.Substring(START_DATA_LINE.Length);
            var response = JsonSerializer.Deserialize<Models.Response.Response>(data, options);
            var choice = response?.Choices.FirstOrDefault();
            var responseMessage = choice?.Delta?.Content ?? string.Empty;
            consumer(responseMessage);
            stringBuilder.Append(responseMessage);

            if (!string.IsNullOrEmpty(choice?.FinishReason) && response?.Usage != null)
                usage = response.Usage;
        }

        var finalResponse = stringBuilder.ToString();

        if (!string.IsNullOrEmpty(finalResponse))
        {
            SaveMessage(message, Roles.User);
            SaveMessage(stringBuilder.ToString(), Roles.Assistant);
        }

        if (usage != null)
            currenWindowSize = usage.TotalTokens;
    }

    private Task<HttpResponseMessage> GetHttpResponseMessageAsync(string message, bool stream = false)
    {
        var request = new Body()
        {
            Model = model,
            Messages =
            [
                ..messages,
                new()
                {
                    Role = Roles.User,
                    Content = message,
                }
            ],
            Stream = stream
        };

        var json = JsonSerializer.Serialize(request, options);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        
        return client.PostAsync(CHAT_URL, content);
    }

    private void SaveMessage(string message, Roles role)
    {
        messages.Add(new()
        {
            Role = role,
            Content = message
        });
    }

    private async Task TryCompressContextAsync()
    {
        if (currenWindowSize < maxWindowSize * START_COMRESS_PERCENT)
            return;

        var response = await GetHttpResponseMessageAsync(compressPromt);

        response.EnsureSuccessStatusCode();

        var responseBody = await response.Content.ReadFromJsonAsync<Response>(options);
        var compressedHistory = responseBody?.Choices.FirstOrDefault()?.Message?.Content;

        if (string.IsNullOrEmpty(compressedHistory))
            return;

        messages.Clear();
        SaveMessage(compressedHistory, Roles.System);
    }
    
    public void Dispose()
    {
        client.Dispose();
    }
}