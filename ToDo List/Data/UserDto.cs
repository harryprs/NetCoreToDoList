using System.ComponentModel.DataAnnotations;

namespace ToDo_List.Data
{
    public class UserDto
    {
        public int Id { get; set; }
        [Required]
        [RegularExpression(@"^[a-z]+$", ErrorMessage = "Username must only contain lowercase characters.")]
        [StringLength(16, MinimumLength = 4)]
        public string Username { get; set; } = string.Empty;
        [Required]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)[a-zA-Z\d]{8,}$", 
            ErrorMessage = "Password must contain at least 1 uppercase character(A-Z), 1 lowercase character(a-z) and 1 number(0-9).")]
        [StringLength(16, MinimumLength = 8)]
        public string Password { get; set; } = string.Empty;
    }
}
