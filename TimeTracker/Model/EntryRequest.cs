using System.Text.RegularExpressions;

namespace TimeTracker.Model
{
    internal partial class EntryRequest
    {
        private const string DATE_PATTERN = @"\d{4}-\d{1,2}\d{1,2}";
        private const string TIME_PATTERN = @"\d{1,2}:\d{0,2}";

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

        [GeneratedRegex("\\d{4}-\\d{1,2}\\d{1,2}")]
        private static partial Regex DateRegEx();

        [GeneratedRegex("\\d{1,2}:\\d{0,2}")]
        private static partial Regex TimeRegEx();
    }
}
