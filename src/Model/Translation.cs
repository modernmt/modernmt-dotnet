using Newtonsoft.Json;

namespace ModernMT.Model
{
    public class Translation : Model
    {
        [JsonProperty("translation")]
        public string TranslationText { get; set; }
        
        [JsonProperty("contextVector")]
        public string ContextVector { get; set; }
        
        [JsonProperty("characters")]
        public int Characters { get; set; }
        
        [JsonProperty("billedCharacters")]
        public int BilledCharacters { get; set; }
        
        [JsonProperty("detectedLanguage")]
        public string DetectedLanguage { get; set; }
        
        [JsonProperty("altTranslations")]
        public string[] AltTranslations { get; set; }
    }
}
