using Newtonsoft.Json;

namespace ModernMT.Model
{
    public class TranslateOptions : Model
    {
        [JsonProperty("priority")]
        public string Priority { get; set; }
        
        [JsonProperty("project_id")]
        public string ProjectId { get; set; }
        
        [JsonProperty("multiline")]
        public bool Multiline { get; set; }
        
        [JsonProperty("timeout")]
        public int Timeout { get; set; }
        
        [JsonProperty("format")]
        public string Format { get; set; }
        
        [JsonProperty("alt_translations")]
        public int AltTranslations { get; set; }

        [JsonProperty("metadata")]
        public dynamic Metadata { get; set; }
        
        [JsonProperty("session")]
        public string Session { get; set; }
        
        // not in json body but in request headers
        public string IdempotencyKey { get; set; }
    }
}
