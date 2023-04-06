using System.Security.Claims;
using ToDo_List.Helpers;
using ToDo_List.Models;

namespace ToDo_List.Services
{
    public class UserRepository
    {
        private ToDoDbContext _context;

        public UserRepository(ToDoDbContext context)
        {
            _context = context;
        }

        public User GetUser(int Id)
        {
            

            throw new NotImplementedException();
        }

        public string GetName()
        {

            var result = string.Empty;
            /*
            if (_httpContextAccessor.HttpContext != null)
            {
                result = _httpContextAccessor.HttpContext.User.FindFirstValue(ClaimTypes.Name);
            }
                */
            throw new NotImplementedException();
        }
    }
}
