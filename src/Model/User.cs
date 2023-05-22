using Newtonsoft.Json;

namespace ModernMT.Model
{
    public class User : Model
    {
        [JsonProperty("id")]
        public long Id { get; set; }
        
        [JsonProperty("name")]
        public string Name { get; set; }
        
        [JsonProperty("email")]
        public string Email { get; set; }
        
        [JsonProperty("registrationDate")]
        public string RegistrationDate { get; set; }
        
        [JsonProperty("country")]
        public string Country { get; set; }
        
        [JsonProperty("isBusiness")]
        public int IsBusiness { get; set; }
        
        [JsonProperty("status")]
        public string Status { get; set; }
        
        [JsonProperty("billingPeriod")]
        public BillingPeriod BillingPeriod { get; set; }
    }
}