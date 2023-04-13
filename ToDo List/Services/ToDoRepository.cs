using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ToDo_List.Helpers;
using ToDo_List.Models;

namespace ToDo_List.Services
{
    public class ToDoRepository
    {
        private ToDoDbContext _context;

        public ToDoRepository(ToDoDbContext context)
        {
            _context = context;
        }

        public async Task<List<ToDoList>> GetAllToDoListsByUserId(int? userId)
        {
            return await _context.ToDoList
                                          .Where(i => i.UserId == userId)
                                          .Include(i => i.ListItems)
                                          .ToListAsync();
        }

        public Task<IEnumerable<ListItem>> GetAllToDoListItemsByToDoListId(int toDoListId)
        {
            throw new NotImplementedException();
        }

        public async Task<ToDoList> GetToDoListByToDoListId(int? toDoListId)
        {
            return await _context.ToDoList.FindAsync(toDoListId);
        }

        public async Task<ListItem> GetListItemByListItemId(int? listItemId)
        {
            return await _context.ListItem.FindAsync(listItemId);
        }

        public async Task CreateList(ToDoList toDoList)
        {
            _context.ToDoList.Add(toDoList);
            await _context.SaveChangesAsync();
        }

        public async Task CreateListItem(ListItem listItem)
        {
            _context.ListItem.Add(listItem);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateList(ToDoList toDoList)
        {
            _context.ToDoList.Update(toDoList);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateListItem(ListItem listItem)
        {
            _context.ListItem.Update(listItem);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteList(ToDoList toDoList)
        {
            _context.ToDoList.Remove(toDoList);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteListItem(ListItem listItem)
        {
            _context.ListItem.Remove(listItem);
            await _context.SaveChangesAsync();
        }
    }
}
