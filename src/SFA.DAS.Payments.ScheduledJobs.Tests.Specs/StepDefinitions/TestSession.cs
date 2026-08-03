using NUnit.Framework;
using SFA.DAS.Payments.ScheduledJobs.Tests.Specs.Data;

namespace SFA.DAS.Payments.ScheduledJobs.Tests.Specs.StepDefinitions
{
    public class TestSession
    {
        public string SessionId { get; }
        private readonly Random random;
        public TestSessionDataContext DataContext { get; }
        public TimeSpan TimeToWait => TimeSpan.FromSeconds(30);
        public TimeSpan TimeToPause => TimeSpan.FromSeconds(2);
        public long JobId { get; set; }

        public TestSession()
        {
            SessionId = Guid.NewGuid().ToString();
            random = new Random(Guid.NewGuid().GetHashCode());
            
            var cnn = TestRunBindings.Config["ConnectionStrings:PaymentsConnectionString"];
            DataContext = new TestSessionDataContext(cnn);
            
            JobId = GenerateId();
        }

        public long GenerateId(int maxValue = 1000000)
        {
            var id = random.Next(maxValue);
            //TODO: make sure that the id isn't already in use.
            return id;
        }
        
        public async Task WaitForIt(Func<Task<bool>> lookForIt, string failText)
        {
            var endTime = DateTime.Now.Add(TimeToWait);
            var lastRun = false;

            while (DateTime.Now < endTime || lastRun)
            {
                if (await lookForIt())
                {
                    if (lastRun) return;
                    lastRun = true;
                }
                else
                {
                    if (lastRun) break;
                }

                await Task.Delay(TimeToPause);
            }
            Assert.Fail($"{failText}  Time: {DateTime.Now:G}. Job Id: {JobId}");
        }

        public async Task WaitForIt(Func<bool> lookForIt, string failText)
        {
            var endTime = DateTime.Now.Add(TimeToWait);
            var lastRun = false;

            while (DateTime.Now < endTime || lastRun)
            {
                if (lookForIt())
                {
                    if (lastRun) return;
                    lastRun = true;
                }
                else
                {
                    if (lastRun) break;
                }

                await Task.Delay(TimeToPause);
            }
            Assert.Fail($"{failText}  Time: {DateTime.Now:G}. Job Id: {JobId}");
        }

        public async Task WaitForItAndFail(Func<bool> lookForIt, string failText)
        {
            var endTime = DateTime.Now.Add(TimeToWait);
            var lastRun = false;

            while (DateTime.Now < endTime || lastRun)
            {
                if (lookForIt())
                {
                    if (lastRun) return;
                    lastRun = true;
                    Assert.Fail($"{failText}  Time: {DateTime.Now:G}. Job Id: {JobId}");
                }
                else
                {
                    if (lastRun) break;
                }

                await Task.Delay(TimeToPause);
            }
        }
    }
}

