using Newtonsoft.Json;

namespace TimeTracker.Model
{
    public class Absence
    {
        [JsonProperty(PropertyName = "id")]
        public string? Id { get; set; }
        [JsonProperty(PropertyName = "partitionKey")]
        public string? PartitionKey { get; set; }
        public string? Date { get; set; }
        public int? Year { get; set; }
        public int? Month { get; set; }        
        public bool HomeOffice { get; set; }
        public bool PublicHoliday { get; set; }
        public bool SickLeave { get; set; }
        public VacationType Vacation { get; set; }

        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }
    }
}
