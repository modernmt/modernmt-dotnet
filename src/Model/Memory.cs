using Newtonsoft.Json;

namespace ModernMT.Model
{
    public class Memory : Model
    {
        [JsonProperty("id")]
        public long Id { get; set; }
        
        [JsonProperty("name")]
        public string Name { get; set; }
        
        [JsonProperty("description")]
        public string Description { get; set; }
        
        [JsonProperty("creationDate")]
        public string CreationDate { get; set; }
    }
}
