using System.Text.Json;

namespace LastSeenDemo
{
    public class ReportManagement
    {
        private List<Report> reports;
        private string reportsFilePath;

        public List<Report> Reports
        {
            get { return reports; }
            set { reports = value; }
        }

        public ReportManagement()
        {
            reports = new List<Report>();
            reportsFilePath = "reports.json";
            LoadReports();
        }

        public void AddReport(Report report)
        {
            reports.Add(report);
        }

        private void LoadReports()
        {
            if (File.Exists(this.reportsFilePath))
            {
                string json = File.ReadAllText(this.reportsFilePath);
                this.reports = JsonSerializer.Deserialize<List<Report>>(json);
            }
        }
    }
}
