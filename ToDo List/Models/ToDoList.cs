using System.ComponentModel.DataAnnotations;
using ToDo_List.Data;

namespace ToDo_List.Models
{
    public class ToDoList
    {
        public int Id { get; set; }
        [Required]
        [StringLength(30)]
        public string Title { get; set; }
        public bool IsFinished { get; set; }
        public IList<ListItem> ListItems { get; set; }
        public int UserId { get; set; }
        public UserLogin User { get; set; }
    }
}
