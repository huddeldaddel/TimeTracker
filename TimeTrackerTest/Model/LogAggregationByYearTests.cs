using TimeTracker.Model;

namespace TimeTrackerTest.Model
{
    public class LogAggregationByYearTests
    {
        [Test]
        public void AddLogEntryToEmptyStructure()
        {
            var entry = new UpsertLogEntryRequest
            {
                Date = "2023-07-17",
                Start = "09:00",
                End = "11:45",
                Project = "Learning",
                Description = "Learned Azure Functions"
            }.ToLogEntry();

            Assert.That(entry, Is.Not.Null);

            var aggregation = new LogAggregationByYear();
            aggregation.AddLogEntry(entry!);
            
            Assert.Multiple(() =>
            {
                Assert.That(aggregation.Year.Entries, Is.EqualTo(1));
                Assert.That(aggregation.Year.Duration, Is.EqualTo(165));
                Assert.That(aggregation.Year.Projects["Learning"].Entries, Is.EqualTo(1));
                Assert.That(aggregation.Year.Projects["Learning"].Duration, Is.EqualTo(165));

                Assert.That(aggregation.Months, Has.Count.EqualTo(1));
                Assert.That(aggregation.Months["7"].Entries, Is.EqualTo(1));
                Assert.That(aggregation.Months["7"].Duration, Is.EqualTo(165));
                Assert.That(aggregation.Months["7"].Projects["Learning"].Entries, Is.EqualTo(1));
                Assert.That(aggregation.Months["7"].Projects["Learning"].Duration, Is.EqualTo(165));

                Assert.That(aggregation.Weeks, Has.Count.EqualTo(1));
                Assert.That(aggregation.Weeks["29"].Entries, Is.EqualTo(1));
                Assert.That(aggregation.Weeks["29"].Duration, Is.EqualTo(165));
                Assert.That(aggregation.Weeks["29"].Projects["Learning"].Entries, Is.EqualTo(1));
                Assert.That(aggregation.Weeks["29"].Projects["Learning"].Duration, Is.EqualTo(165));
            });            
        }

        [Test]
        public void RemoveLastLogEntry()
        {
            var entry = new UpsertLogEntryRequest
            {
                Date = "2023-07-17",
                Start = "09:00",
                End = "11:45",
                Project = "Learning",
                Description = "Learned Azure Functions"
            }.ToLogEntry();
            Assert.That(entry, Is.Not.Null);

            var aggregation = new LogAggregationByYear();
            aggregation.AddLogEntry(entry!);
            aggregation.RemoveLogEntry(entry!);

            Assert.Multiple(() =>
            {
                Assert.That(aggregation.Year.Entries, Is.EqualTo(0));
                Assert.That(aggregation.Year.Duration, Is.EqualTo(0));
                Assert.That(aggregation.Year.Projects.Count, Is.EqualTo(0));
                Assert.That(aggregation.Months.Count, Is.EqualTo(0));
                Assert.That(aggregation.Weeks.Count, Is.EqualTo(0));
            });
        }
    }
}
