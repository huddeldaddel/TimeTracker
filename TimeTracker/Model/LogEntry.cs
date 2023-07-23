using Newtonsoft.Json;

namespace TimeTracker.Model
{
    public class LogEntry
    {
        [JsonProperty(PropertyName = "id")]
        public string? Id { get; set; }
        [JsonProperty(PropertyName = "partitionKey")]
        public string? PartitionKey { get; set; }
        public string? Date { get; set; }
        public int? Year { get; set; }
        public int? Month { get; set; }
        public int? Week { get; set; }
        public string? Start { get; set; }
        public string? End { get; set; }
        public int? Duration { get; set; }
        public string? Project { get; set; }
        public string? Description { get; set; }

        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }
    }
}
