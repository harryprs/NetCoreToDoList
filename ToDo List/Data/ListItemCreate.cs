using System.ComponentModel.DataAnnotations;

namespace ToDo_List.Data
{
    public class ListItemCreate
    {
        [Required]
        [StringLength(50)]
        [Display(Name = "Task")]
        public string ItemName { get; set; }
        [Display(Name = "Description")]
        public string ItemDescription { get; set; }
        [Display(Name = "Estimated Time(mins)")]
        public int? EstimatedTime { get; set; }
        [Required]
        [Range(1,3)]
        public PriorityEnum Priority { get; set; }
        public int ToDoListId { get; set; }
    }
}
