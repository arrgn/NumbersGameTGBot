using Microsoft.Extensions.Configuration;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

var builder = new ConfigurationBuilder().AddJsonFile("appsettings.json", true, true);
var config = builder.Build();

if (config["TOKEN"] is null)
    throw new Exception("TOKEN wasn't found!");

var botClient = new TelegramBotClient(config["TOKEN"]);

using CancellationTokenSource cts = new();

ReceiverOptions receiverOptions = new()
{
    AllowedUpdates = Array.Empty<UpdateType>()
};

TimePeriod.MinTime = DateTime.Parse(config["MinTime"] ?? "00:00");
TimePeriod.MaxTime = DateTime.Parse(config["MaxTime"] ?? "23:59:59");

UserController userController = new();

botClient.StartReceiving(
    updateHandler: HandleUpdateAsync,
    pollingErrorHandler: HandlePollingErrorAsync,
    receiverOptions: receiverOptions,
    cancellationToken: cts.Token
);

var me = await botClient.GetMeAsync();

Console.WriteLine($"Start listening for @{me.Username}");
Console.ReadLine();

// Send cancellation request to stop bot
cts.Cancel();

async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
{
    // Only process Message updates: https://core.telegram.org/bots/api#message
    if (update.Message is not { } message)
        return;
    // Only process text messages
    if (message.Text is not { } messageText)
        return;

    if (message.From?.Id is null)
        return;

    var userId = message.From?.Id ?? default;
    var date = message.Date;
    var msgText = message.Text;

    if (Int32.TryParse(msgText, out int streak))
    {
        var res = userController.UpdateUserStreak(userId, streak, message.Date);

        if (res)
        {
            Message sentMessage = await botClient.SendTextMessageAsync(
                chatId: message.Chat.Id,
                text: $"You have reached the streak at {streak} day{(streak == 1 ? "" : "s")}",
                replyToMessageId: message.MessageId,
                cancellationToken: cancellationToken
            );
        }
    }
    else
    {
        Console.WriteLine($"Parse error: message:\n{msgText}\nis not a valid command!");
    }
}

Task HandlePollingErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
{
    var ErrorMessage = exception switch
    {
        ApiRequestException apiRequestException
            => $"Telegram API Error:\n[{apiRequestException.ErrorCode}]\n{apiRequestException.Message}",
        _ => exception.ToString()
    };

    Console.WriteLine(ErrorMessage);
    return Task.CompletedTask;
}
