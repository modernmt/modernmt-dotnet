using Newtonsoft.Json;

namespace ModernMT.Model
{
    public class ImportJob : Model
    {
        [JsonProperty("id")]
        public string Id { get; set; }
        
        [JsonProperty("memory")]
        public long Memory { get; set; }
        
        [JsonProperty("size")]
        public int Size { get; set; }
        
        [JsonProperty("progress")]
        public float Progress { get; set; }
    }
}
