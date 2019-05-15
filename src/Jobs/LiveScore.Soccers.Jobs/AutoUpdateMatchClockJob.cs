namespace LiveScore.Soccers.Jobs
{
    using System;
    using Hangfire;
    using Hangfire.Common;

    public class AutoUpdateMatchClockJob
    {
        public AutoUpdateMatchClockJob(IRecurringJobManager recurringJobManager)
        {
            recurringJobManager.AddOrUpdate(nameof(AutoUpdateMatchClockJob), Job.FromExpression(() => UpdateMatchClock()), Cron.Minutely());
        }

        private void UpdateMatchClock()
        {

        }
    }
}
