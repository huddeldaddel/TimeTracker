using Newtonsoft.Json;
using System.Globalization;
using System.Text.RegularExpressions;

namespace TimeTracker.Model
{
    public partial class UpsertEntryRequest
    {
        private const string DATE_PATTERN = @"\d{4}-\d{1,2}\d{1,2}";
        private const string TIME_PATTERN = @"(\d{1,2}):(\d{0,2})";

        [JsonProperty(PropertyName = "id")]
        public string? Id { get; set; }        
        public string? Date { get; set; }
        public string? Start { get; set; }
        public string? End { get; set; }
        public string? Project { get; set; } 
        public string? Description { get; set; }

        public bool Validate()
        {
            var result = ValidateDate(Date);
            result &= ValidateTime(Start);
            result &= ValidateTime(End);
            return result;
        }

        /// <summary>
        /// Converts this instance to an Entry.
        /// </summary>
        /// <returns>An Entry or null if this instance did not validate successfully</returns>
        public Entry? ToEntry()
        {
            if(!Validate())
            {
                return null;
            }

            DateTime date = DateTime.ParseExact(Date!, "yyyy-MM-dd", CultureInfo.InvariantCulture);                        
            DateTime startDateTime = DateTime.ParseExact($"{Date} {ReformatTime(Start)}", "yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture);
            DateTime endDateTime = DateTime.ParseExact($"{Date} {ReformatTime(End)}", "yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture);
            if (endDateTime < startDateTime)
            {
                endDateTime = endDateTime.AddDays(1);
            }

            var duration = endDateTime - startDateTime;
            return new Entry
            {
                Id = Id,
                PartitionKey = Id,
                Date = Date,
                Year = date.Year,
                Month = date.Month,
                Week = ISOWeek.GetWeekOfYear(date),
                Start = ReformatTime(Start),
                End = ReformatTime(End),
                Duration = duration.Minutes + 60 * duration.Hours,
                Project = Project,
                Description = Description
            };
        }

        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }

        private static bool ValidateDate(string? date)
        {
            if(date == null) 
            {
                return false; 
            }
                        
            return DateRegEx().Match(date).Success;
        }

        private static bool ValidateTime(string? time)
        {
            if (time == null)
            {
                return false;
            }

            return TimeRegEx().Match(time).Success;            
        }

        private static string? ReformatTime(string? time)
        {
            if(time == null)
            {
                return time;
            }
            
            var match = TimeRegEx().Match(time);
            var hours = match.Groups[1].Value;
            if(hours.Length == 1)
                hours = "0" + hours;
            var minutes = match.Groups[2].Value;
            if (minutes.Length == 0)
                minutes = "0";
            if (minutes.Length == 1)
                minutes = "0" + minutes;
            return $"{hours}:{minutes}";
            
        }

        [GeneratedRegex("\\d{4}-\\d{1,2}\\d{1,2}")]
        private static partial Regex DateRegEx();

        [GeneratedRegex("(\\d{1,2}):(\\d{0,2})")]
        private static partial Regex TimeRegEx();
    }
}
