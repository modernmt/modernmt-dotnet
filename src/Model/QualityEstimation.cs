using Newtonsoft.Json;

namespace ModernMT.Model
{
    public class QualityEstimation : Model
    {
        [JsonProperty("score")]
        public double Score { get; set; }
    }
}