using Newtonsoft.Json;

namespace ModernMT.Model
{
    public class GlossaryTerm : Model
    {
        [JsonProperty("term")]
        public string Term { get; }
        
        [JsonProperty("language")]
        public string Language { get; }
        
        public GlossaryTerm(string term, string language)
        {
            Term = term;
            Language = language;
        }
    }
}