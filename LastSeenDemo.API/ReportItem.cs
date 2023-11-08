// <copyright file="ReportItem.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace LastSeenDemo.API
{
    public class ReportItem
    {
        public Guid UserId { get; set; }

        public double Total { get; set; }

        public double DailyAverage { get; set; }

        public double WeeklyAverage { get; set; }

        public double Min { get; set; }

        public double Max { get; set; }
    }
}
