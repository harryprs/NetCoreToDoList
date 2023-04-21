using System.ComponentModel.DataAnnotations;

namespace ToDo_List.Data
{
    public class UserDto
    {
        public int Id { get; set; }
        [Required(ErrorMessage = "Username field is required. Must be 4-16 characters long.")]
        [RegularExpression(@"^[a-zA-Z][a-zA-Z0-9]+$", ErrorMessage = "Username must begin with a letter(a-z).<br/>Can contain any letters(a-z) and numbers(0-9).")]
        [StringLength(16, MinimumLength = 4, ErrorMessage = "Must be 4-16 characters long.")]
        public string Username { get; set; } = string.Empty;
        [Required(ErrorMessage = "Password field is required. Must be 8-16 characters long.")]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)[a-zA-Z\d]+$", 
            ErrorMessage = "Password must contain at least 1 uppercase letter(A-Z), 1 lowercase letter(a-z) and 1 number(0-9). No special characters.")]
        [StringLength(16, MinimumLength = 8, ErrorMessage = "Must be 8-16 characters long.")]
        public string Password { get; set; } = string.Empty;
    }
}
