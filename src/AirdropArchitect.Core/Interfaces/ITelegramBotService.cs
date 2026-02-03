namespace AirdropArchitect.Core.Interfaces;

public interface ITelegramBotService
{
    Task HandleUpdateAsync(string updateJson, CancellationToken cancellationToken = default);
}
