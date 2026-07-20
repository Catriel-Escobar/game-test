using System;
using System.Collections.Generic;

[Serializable]
public class LocalizationConfig
{
    public string defaultLanguage;
    public LocalizationEntry[] entries;

    public string CurrentLanguage { get; private set; }
    public event Action OnLanguageChanged;

    private Dictionary<string, Dictionary<string, string>> _lookup;

    public void BuildLookup()
    {
        _lookup = new Dictionary<string, Dictionary<string, string>>();
        if (entries == null) return;

        for (int i = 0; i < entries.Length; i++)
        {
            var entry = entries[i];
            var langs = new Dictionary<string, string>();

            if (entry.en != null) langs["en"] = entry.en;
            if (entry.es != null) langs["es"] = entry.es;

            _lookup[entry.key] = langs;
        }

        CurrentLanguage = defaultLanguage;
    }

    public void SetLanguage(string language)
    {
        if (CurrentLanguage == language) return;
        CurrentLanguage = language;
        OnLanguageChanged?.Invoke();
    }

    public string Get(string key, string language = null)
    {
        language = language ?? CurrentLanguage ?? defaultLanguage;

        if (_lookup != null && _lookup.TryGetValue(key, out var langs))
        {
            if (langs.TryGetValue(language, out var text))
                return text;

            if (langs.TryGetValue(defaultLanguage, out var fallback))
                return fallback;
        }

        return key;
    }
}

[Serializable]
public class LocalizationEntry
{
    public string key;
    public string en;
    public string es;
}
