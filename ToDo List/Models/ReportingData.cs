namespace ToDo_List.Models
{
    public class ReportingData
    {
        public string ListsCompletedDataPoint { get; set; }
        public string ListItemsCompletedDataPoint { get; set; }
        public int TotalListsCompleted { get; set; }
        public int TotalTimeSpent { get; set; }
        public int? TotalEstimatedTime { get; set; }
        public int ListItemsNotStartedCount { get; set; }
        public int ListItemsInProgressCount { get; set; }
        public int ListItemsCompletedCount { get; set; }
    }
}
