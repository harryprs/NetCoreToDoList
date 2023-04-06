using Google.Apis.PeopleService.v1.Data;
using Google.Apis.Util;
using System.ComponentModel.DataAnnotations;

namespace ToDo_List.Data
{
    public enum ProgressEnum
    {
        [Display(Name = "Not Started")]
        NotStarted = 1,
        [Display(Name = "In Progress")]
        InProgress = 2,
        [Display(Name = "Completed")]
        Completed = 3,
    }
}