namespace TimeTracker.Model
{
    public class LogAggregation
    {
        public int Duration { get; set; }
        public int Entries { get; set; }
        public Dictionary<string, ProjectAggregation> Projects { get; set; } = new Dictionary<string, ProjectAggregation>();

        public void AddLogEntry(LogEntry entry)
        {
            if (null != entry.Duration)
            {
                Duration += entry.Duration.Value;
            }

            Entries++;

            if(null != entry.Project)
            {                
                var key = entry.Project.Trim();
                if (!Projects.TryGetValue(key, out ProjectAggregation? project))
                {
                    project = new ProjectAggregation();
                    Projects.Add(key, project);
                }
                project.AddLogEntry(entry);
            }
        }

        public void RemoveLogEntry(LogEntry entry)
        {
            if (null != entry.Duration)
            {
                Duration -= entry.Duration.Value;
            }

            Entries--;

            if (null != entry.Project)
            {
                var key = entry.Project.Trim();
                if(Projects.TryGetValue(key, out ProjectAggregation? project))
                {
                    project.RemoveLogEntry(entry);
                    if (project.IsEmpty())
                    {
                        Projects.Remove(key);
                    }
                }
            }
        }
        public bool IsEmpty()
        {
            return (Duration == 0) && (Entries == 0);
        }
    }
}
