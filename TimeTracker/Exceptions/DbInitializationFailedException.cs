namespace TimeTracker.Exceptions
{
    public class DbInitializationFailedException : IOException
    {
        public DbInitializationFailedException() : base("Failed to initialize DB connection")
        {

        }
    }
}
