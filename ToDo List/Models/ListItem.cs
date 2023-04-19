using System.ComponentModel.DataAnnotations;
using ToDo_List.Data;

namespace ToDo_List.Models
{
    public class ListItem
    {
        public int Id { get; set; }
        [Required]
        [StringLength(50)]
        [Display(Name = "Task")]
        public string ItemName { get; set; }
        [Display(Name = "Description")]
        public string ItemDescription { get; set; }
        [Required]
        [Range(1, 3)]
        public PriorityEnum Priority { get; set; }
        [Required]
        [Range(1, 3)]
        public ProgressEnum Progress { get; set; }
        [Display(Name = "Estimated Time(mins)")]
        public int? EstimatedTime { get; set; }
        [Display(Name = "Time Spent(mins)")]
        public int TimeSpent { get; set; }
        public DateTime? DateTimeItemFinished { get; set; }
        //Navigation Properties
        public int ToDoListId { get; set; }
        public ToDoList ToDoList { get; set; }
    }
}
