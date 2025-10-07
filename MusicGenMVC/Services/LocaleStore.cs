using System.Text.Json;

namespace MusicGenMVC.Services
{
    // Loads region-specific words from JSON without hardcoding in code.
    public sealed class LocaleStore
    {
        private readonly Dictionary<string, LocaleData> _cache = new(StringComparer.OrdinalIgnoreCase);
        private readonly IWebHostEnvironment _env;
        public LocaleStore(IWebHostEnvironment env) { _env = env; }


        public IReadOnlyList<string> Supported => new[] { "en-US", "de-DE", "uk-UA"  };


        public LocaleData Get(string lang)
        {
            lang = Supported.Contains(lang) ? lang : "en-US";
            if (_cache.TryGetValue(lang, out var data)) return data;
            var path = Path.Combine(_env.ContentRootPath, "App_Data", "locales", lang + ".json");
            var json = File.ReadAllText(path);
            data = JsonSerializer.Deserialize<LocaleData>(json) ?? new LocaleData();
            _cache[lang] = data;
            return data;
        }
    }


    public sealed class LocaleData
    {
        public List<string> FirstNames { get; set; } = new();
        public List<string> LastNames { get; set; } = new();
        public List<string> BandWords { get; set; } = new();
        public List<string> Nouns { get; set; } = new();
        public List<string> Adjectives { get; set; } = new();
        public List<string> Genres { get; set; } = new();
        public List<string> ReviewPhrases { get; set; } = new();
    }
}
