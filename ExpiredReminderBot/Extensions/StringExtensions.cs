namespace ExpiredReminderBot.Extensions;

public static class StringExtensions
{
    public static KeyValuePair<string, string> ToKeyValuePairs(this string text)
    {
        if (text == null)
            return new KeyValuePair<string, string>(string.Empty, string.Empty);

        string[] keyValuePairs = text.Split(':');

        if(keyValuePairs.Length == 2) 
            return new KeyValuePair<string, string>(keyValuePairs[0], keyValuePairs[1]);
        else
            return new KeyValuePair<string, string>(keyValuePairs[0], string.Empty);
    }
}