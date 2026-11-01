namespace OpenAiApi.Models;

public class Settings
{
    public string BaseAddress { get; set; } = string.Empty;
    public string Token { get; set; } = string.Empty;
    public string Model { get; set; } = string.Empty;
    public int MaxWindowSize { get; set; } = 128_000;
    public string CompressPromtPath { get; set; } = string.Empty;
}