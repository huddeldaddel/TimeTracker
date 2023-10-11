namespace TimeTracker.Model
{
    public class Absence
    {
        public bool HomeOffice { get; set; }
        public bool SickLeave { get; set; }
        public VacationType Vacation { get; set; }

        public Absence() { }

        public Absence(WorkingDayAggregation workingDayAggregation) { 
            this.HomeOffice = workingDayAggregation.HomeOffice;
            this.SickLeave = workingDayAggregation.SickLeave;
            this.Vacation = workingDayAggregation.Vacation;
        }
    }
}
