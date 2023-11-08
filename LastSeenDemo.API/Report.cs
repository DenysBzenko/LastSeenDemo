// <copyright file="Report.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace LastSeenDemo.API
{
    public class Report
    {
        public string Name { get; set; }

        public List<Guid>? Users { get; }

        public List<string>? Metrics { get; }

#pragma warning disable SA1201
        private readonly Worker worker;
#pragma warning restore SA1201
        private readonly OnlineDetector detector;
        private readonly UserMinMaxCalculator minMax;

        public Report(string reportName, List<Guid>? users, List<string>? metrics, Worker worker, OnlineDetector onlineDetector)
        {
            this.Name = reportName;
            this.Metrics = metrics;
            this.worker = worker;
            this.detector = onlineDetector;
            this.minMax = new UserMinMaxCalculator(this.detector);
            this.Users = users;
        }

        public List<LastSeenDemo.ReportItem> CreateReport(DateTimeOffset from, DateTimeOffset to)
        {
            var report = new List<LastSeenDemo.ReportItem>();

            foreach (var userId in this.Users!)
            {
                if (this.worker.Users.TryGetValue(userId, out var user))
                {
                    var userReport = new LastSeenDemo.ReportItem
                    {
                        UserId = userId,
                        Total = this.Metrics!.Contains("total") ? this.detector.CalculateTotalTimeForUser(user) : 0,
                        DailyAverage = this.Metrics.Contains("dailyAverage") ? this.detector.CalculateDailyAverageForUser(user) : 0,
                        WeeklyAverage = this.Metrics.Contains("weeklyAverage") ? this.detector.CalculateWeeklyAverageForUser(user) : 0,
                        Min = this.Metrics.Contains("min") ? this.minMax.CalculateMinMax(user, from, to).Item1 : 0,
                        Max = this.Metrics.Contains("max") ? this.minMax.CalculateMinMax(user, from, to).Item2 : 0,
                    };

                    report.Add(userReport);
                }
            }

            return report;
        }
    }
}
