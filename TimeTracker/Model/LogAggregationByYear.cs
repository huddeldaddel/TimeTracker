using Newtonsoft.Json;
using System.Globalization;

namespace TimeTracker.Model
{
    public class LogAggregationByYear
    {
        [JsonProperty(PropertyName = "id")]
        public string? Id { get; set; }

        [JsonProperty(PropertyName = "partitionKey")]
        public string? PartitionKey { get; set; }

        public LogAggregation Year { get; set; } = new LogAggregation();
        public Dictionary<string, LogAggregation> Months { get; set; } = new Dictionary<string, LogAggregation>();
        public Dictionary<string, LogAggregation> Weeks { get; set; } = new Dictionary<string, LogAggregation>();

        public void AddLogEntry(LogEntry entry)
        {
            Year.AddLogEntry(entry);

            if(null != entry.Month)
            {
                LogAggregation month;
                var key = entry.Month.Value.ToString(CultureInfo.InvariantCulture.NumberFormat);
                if(Months.ContainsKey(key))
                {
                    month = Months[key];
                }
                else
                {
                    month = new LogAggregation();
                    Months.Add(key, month);
                }
                month.AddLogEntry(entry);
            }

            if (null != entry.Week)
            {
                LogAggregation week;
                var key = entry.Week.Value.ToString(CultureInfo.InvariantCulture.NumberFormat);
                if(Weeks.ContainsKey(key))
                {
                    week = Weeks[key];
                } 
                else
                {
                    week = new LogAggregation();
                    Weeks.Add(key, week);
                }                
                week.AddLogEntry(entry);
            }
        }

        public void RemoveLogEntry(LogEntry entry)
        {
            Year.RemoveLogEntry(entry);

            if (null != entry.Month)
            {
                var key = entry.Month.Value.ToString(CultureInfo.InvariantCulture.NumberFormat);
                if(Months.TryGetValue(key, out LogAggregation? month))
                {
                    month.RemoveLogEntry(entry);
                    if (month.IsEmpty())
                    {
                        Months.Remove(key);
                    }
                }
            }

            if (null != entry.Week)
            {
                var key = entry.Week.Value.ToString(CultureInfo.InvariantCulture.NumberFormat);
                if(Weeks.TryGetValue(key, out LogAggregation? week))
                {
                    week.RemoveLogEntry(entry);
                    if (week.IsEmpty())
                    {
                        Weeks.Remove(key);
                    }
                }                
            }
        }
    }
}
