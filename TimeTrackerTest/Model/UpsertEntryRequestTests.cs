using TimeTracker.Model;

namespace TimeTrackerTest.Model
{
    public class UpsertEntryRequestTests
    {        
        [Test]
        public void ValidateSuccess()
        {
            var request = new UpsertEntryRequest
            {
                Date = "2023-07-17",
                Start = "09:00",
                End = "11:45",
                Project = "Learning",
                Description = "Learned Azure Functions"
            };

            Assert.That(request.Validate(), Is.True);
        }

        [Test]
        public void ValidateDateFailure()
        {
            var request = new UpsertEntryRequest
            {
                Date = "20230717",
                Start = "09:00",
                End = "11:45",
                Project = "Learning",
                Description = "Learned Azure Functions"
            };

            Assert.That(request.Validate(), Is.False);
        }

        [Test]
        public void ValidateTimeFailure()
        {
            var request = new UpsertEntryRequest
            {
                Date = "2023-07-17",
                Start = "9",
                End = "11:45",
                Project = "Learning",
                Description = "Learned Azure Functions"
            };

            Assert.That(request.Validate(), Is.False);
        }

        [Test]
        public void ValidateShortTime()
        {
            var request = new UpsertEntryRequest
            {
                Date = "2023-07-17",
                Start = "9:",
                End = "11:45",
                Project = "Learning",
                Description = "Learned Azure Functions"
            };

            Assert.That(request.Validate(), Is.True);
        }

        [Test]
        public void ToEntryValid()
        {
            var entry = new UpsertEntryRequest
            {
                Date = "2023-07-17",
                Start = "9:",
                End = "11:45",
                Project = "Learning",
                Description = "Learned Azure Functions"
            }.ToEntry();

            Assert.That(entry, Is.Not.Null);
            Assert.Multiple(() =>
            {
                Assert.That(entry.Date, Is.EqualTo("2023-07-17"));
                Assert.That(entry.Start, Is.EqualTo("09:00"));
                Assert.That(entry.End, Is.EqualTo("11:45"));
                Assert.That(entry.Year, Is.EqualTo(2023));
                Assert.That(entry.Month, Is.EqualTo(7));
                Assert.That(entry.Week, Is.EqualTo(29));
                Assert.That(entry.Duration, Is.EqualTo(165));
                Assert.That(entry.Project, Is.EqualTo("Learning"));
                Assert.That(entry.Description, Is.EqualTo("Learned Azure Functions"));
            });
        }

        [Test]
        public void ToEntryNotValid()
        {
            var entry = new UpsertEntryRequest
            {
                Date = null,
                Start = "09:00",
                End = "11:45",
                Project = "Learning",
                Description = "Learned Azure Functions"
            }.ToEntry();

            Assert.That(entry, Is.Null);
        }
    }
}