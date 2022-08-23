using Newtonsoft.Json;

namespace ModernMT.Model
{
    public class DetectedLanguage : Model
    {
        [JsonProperty("billedCharacters")]
        public int BilledCharacters { get; set; }
        
        [JsonProperty("detectedLanguage")]
        public string Language { get; set; }
    }
}
