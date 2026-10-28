using System;
using System.Linq;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

var botClient = new TelegramBotClient(""); // токен тг сюда
char[] prepinanie = { '.', ',', ';', ':', ')', '!', '?' };

botClient.StartReceiving(
    updateHandler: OnUpdateReceived,
    errorHandler: (_, ex, _) => Task.Run(() => Console.WriteLine(ex.Message))
);

Console.WriteLine("Бот работает.");
Console.ReadLine();

async Task OnUpdateReceived(ITelegramBotClient botClient, Update update, CancellationToken ct)
{
    if (update.Message is { Text: not null } msg)
    {
        if (msg.Text.Trim() == "/start") // фильтр старта
        {
            await botClient.SendMessage(
                chatId: msg.Chat.Id,
                text: "Отправь мне текст, а я его постараюсь отформатировать.",
                cancellationToken: ct
            );
            return;
        }

        string resultText = FormatText(msg.Text);

        await botClient.SendMessage( 
            chatId: msg.Chat.Id,
            text: resultText,
            cancellationToken: ct
        );
    }
}

string FormatText(string inputText)
{
    bool abbreviat = false;
    string newText = "";
    inputText = inputText.Replace((char)160, ' ');
    while (inputText.Contains("  ")) 
    {
        inputText = inputText.Replace("  ", " ");
    }
    inputText = inputText.Trim();
    for (int i = 0; i < inputText.Length; i++)
    {
        char c = inputText[i];
        char prevC = i - 1 < 0 ? '\0' : inputText[i - 1];
        char prevC2 = i - 2 < 0 ? '\0' : inputText[i - 2];
        char nextC = i + 1 < inputText.Length ? inputText[i + 1] : '\0';
        char nextC2 = i + 2 < inputText.Length ? inputText[i + 2] : '\0';
        if (Char.IsUpper(inputText[i]))
        {
            bool isFullAbbreviation = true;

            int start = i;
            while (start > 0 && Char.IsLetter(inputText[start - 1]))
            {
                start--;
            }
            int end = i;

            while (end < inputText.Length - 1 && Char.IsLetter(inputText[end + 1]))
            {
                end++;
            }

            for (int k = start; k <= end; k++)
            {
                if (Char.IsLower(inputText[k]))
                {
                    isFullAbbreviation = false;
                    break;
                }
            }

            if (isFullAbbreviation && (end - start + 1) > 1)
            {
                c = inputText[i];
            }
            else
            {
                c = Char.ToLower(c);
            }
        }
        else
        {
            c = Char.ToLower(c);
        }
        // Console.WriteLine(c);
        if (i == 0)
            c = Char.ToUpper(c);

        if (c == ' ' && prevC == '\n')
            continue;

        if ((prevC2 == '\n' || prevC2 == '\r'))
            c = Char.ToUpper(c);

        if ((c == '.' && nextC2 == '.') || (prevC == '.' && nextC == '.'))
        {
            newText += c;
            abbreviat = true;
            continue;
        }

        if ((prevC2 == '.') && !abbreviat)
            c = Char.ToUpper(c);
        if (prepinanie.Contains(c) && nextC != ' ')
        {
            abbreviat = false;
            newText += c + " ";
            continue;
        }
        else if (c == ' ' &&
            (prepinanie.Contains(nextC) || nextC == ' '))
        {
            continue;
        }

        newText += c;
    }
    return newText;
}