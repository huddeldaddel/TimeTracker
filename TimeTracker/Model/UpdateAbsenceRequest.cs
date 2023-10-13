using Newtonsoft.Json;
using System.Globalization;
using System.Text.RegularExpressions;

namespace TimeTracker.Model
{
    public partial class UpdateAbsenceRequest
    {
        public string? Date { get; set; }
        public bool HomeOffice { get; set; }
        public bool PublicHoliday { get; set; }
        public bool SickLeave { get; set; }
        public VacationType Vacation { get; set; }

        public bool Validate()
        {
            return ValidateDate(Date);                        
        }

        public Absence? ToAbsence()
        {
            if (!Validate())
            {
                return null;
            }

            var dateTime = DateTime.ParseExact(Date!, "yyyy-MM-dd", CultureInfo.InvariantCulture);
            var key = $"00000000-0000-0000-0000-0000{Date!.Replace("-", "")}";
            return new Absence()
            {
                Id = key,
                PartitionKey = key,
                Date = Date,
                HomeOffice = HomeOffice,
                PublicHoliday = PublicHoliday,
                SickLeave = SickLeave,
                Vacation = Vacation,
                Month = dateTime.Month,
                Year = dateTime.Year                
            };
        }

        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }

        internal static bool ValidateDate(string? date)
        {
            if (date == null)
            {
                return false;
            }

            return DateRegEx().Match(date).Success;
        }

        [GeneratedRegex("\\d{4}-\\d{2}-\\d{2}")]
        private static partial Regex DateRegEx();
    }
}
