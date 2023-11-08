// <copyright file="ReportManagement.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace LastSeenDemo.API
{
    using System.Text.Json;

    // Assuming Report is a class in your namespace
    public class ReportManagement
    {
        // Fields
        private readonly string reportsFilePath = "reports.json";

        // Constructors
        public ReportManagement()
        {
            this.Reports = new List<Report>();
            this.LoadReports();
        }

        // Properties
        public List<Report>? Reports { get; private set; }

        // Methods
        public void AddReport(Report report)
        {
            this.Reports?.Add(report);
        }

        private void LoadReports()
        {
            if (File.Exists(this.reportsFilePath))
            {
                string json = File.ReadAllText(this.reportsFilePath);
                this.Reports = JsonSerializer.Deserialize<List<Report>>(json);
            }
        }
    }
}
