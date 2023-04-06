using ToDo_List.Models;

namespace ToDo_List.Models.ViewModels
{
    public class ToDoListDetails
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public int Progress { get; set; }
        public string ItemComment { get; set; }
        public List<ListItem> ListItems { get; set; } = new List<ListItem>();
        public int UserId { get; set; }
    }
}
