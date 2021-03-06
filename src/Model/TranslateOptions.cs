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
    }
}
