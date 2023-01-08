using System.Collections.Concurrent;

namespace ExpiredReminderBot.Extensions
{
    static class DictionaryExtension
    {
        // Either Add or overwrite
        public static V AddOrUpdate<K, V>(this ConcurrentDictionary<K, V> dictionary, K key, V value)
        {
            return dictionary.AddOrUpdate(key, value, (oldkey, oldvalue) => value);
        }
    }
}
