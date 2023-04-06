using System.ComponentModel.DataAnnotations;

namespace ToDo_List.Data
{
    public class UserDto
    {
        public int Id { get; set; }
        [Required]
        [StringLength(16, MinimumLength = 4)]
        public string Username { get; set; } = string.Empty;
        [Required]
        [StringLength(16, MinimumLength = 8)]
        public string Password { get; set; } = string.Empty;
    }
}
