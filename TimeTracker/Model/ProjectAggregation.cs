namespace TimeTracker.Model
{
    public class ProjectAggregation
    {       
        public int Duration { get; set; }
        public int Entries { get; set; }

        public void AddLogEntry(LogEntry entry)
        {            
            if(null != entry.Duration)
            {
                Duration += entry.Duration.Value;
            }

            Entries++;
        }

        public void RemoveLogEntry(LogEntry entry)
        {
            if (null != entry.Duration)
            {
                Duration -= entry.Duration.Value;
            }

            Entries--;
        }

        public bool IsEmpty()
        {
            return (Duration == 0) && (Entries == 0);
        }
    }
}
