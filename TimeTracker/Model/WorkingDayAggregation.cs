namespace TimeTracker.Model
{
    public class WorkingDayAggregation
    {
        public int Duration { get; set; }        

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
            return Duration == 0;
        }
    }
}
