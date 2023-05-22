using Newtonsoft.Json;

namespace ModernMT.Model
{
    public class BillingPeriod : Model
    {
        [JsonProperty("begin")]
        public string Begin { get; set; }
        
        [JsonProperty("end")]
        public string End { get; set; }
        
        [JsonProperty("chars")]
        public long Chars { get; set; }
        
        [JsonProperty("plan")]
        public string Plan { get; set; }
        
        [JsonProperty("planDescription")]
        public string PlanDescription { get; set; }
        
        [JsonProperty("planForCatTool")]
        public bool PlanForCatTool { get; set; }
        
        [JsonProperty("amount")]
        public float Amount { get; set; }
        
        [JsonProperty("currency")]
        public string Currency { get; set; }
        
        [JsonProperty("currencySymbol")]
        public string CurrencySymbol { get; set; }
    }
}