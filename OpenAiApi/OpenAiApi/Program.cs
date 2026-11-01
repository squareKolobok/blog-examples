using OpenAiApi;
using OpenAiApi.Models;

var settings = new Settings()
{
    BaseAddress = "",
    Token = "",
    Model = "",
    MaxWindowSize = 30_000,
    CompressPromtPath = "./Promts/comress.txt",
};

using var client = new OpenAiClient(settings);

Console.WriteLine("Приложение запущено. Начните беседу (для выхода напишите 'exit'):");

while (true)
{
    Console.WriteLine();
    Console.WriteLine("////////////////////////////////////");
    Console.WriteLine($"Текущий размер контекста: {client.CurrentContextSize}");
    Console.WriteLine("////////////////////////////////////");
    Console.WriteLine();

    Console.Write("> ");
    var input = Console.ReadLine();

    if (string.Equals(input?.Trim(), "exit", StringComparison.OrdinalIgnoreCase))
    {
        Console.WriteLine("Завершение работы программы.");
        break;
    }

    //Запрос без стриминга
    //Console.WriteLine(await client.GetAiResponseAsync(input));

    //Запрос со стримингом
    await client.SendAiStreamResponseAsync(input, Console.Write);

    Console.WriteLine();
}
