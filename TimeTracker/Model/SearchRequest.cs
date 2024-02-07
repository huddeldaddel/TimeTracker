namespace TimeTracker.Model
{
    public partial class SearchRequest
    {
        public int Year { get; set; }        
        public string? Project { get; set; } 
        public string? Query { get; set; }

        public bool Validate()
        {
            return Year > 1000 && Year < 3000;
        }        
    }
}
