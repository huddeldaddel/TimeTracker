namespace TimeTracker.Model
{
    public class WorkingDayAggregation
    {
        public int Duration { get; set; }
        public VacationType Vacation { get; set; }
        public bool SickLeave { get; set; }
        public bool HomeOffice { get; set; }

        public void AddLogEntry(LogEntry entry)
        {
            if (null != entry.Duration)
            {
                Duration += entry.Duration.Value;
            }         
        }

        public void RemoveLogEntry(LogEntry entry)
        {
            if (null != entry.Duration)
            {
                Duration -= entry.Duration.Value;
            }
        }
        public bool IsEmpty()
        {
            return (Duration == 0) && Vacation == VacationType.None && !SickLeave && !HomeOffice;
        }
    }
}
