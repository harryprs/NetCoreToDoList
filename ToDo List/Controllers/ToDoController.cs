using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using NuGet.Protocol.Plugins;
using ToDo_List.Data;
using ToDo_List.Helpers;
using ToDo_List.Models;
using ToDo_List.Services;

namespace ToDo_List.Controllers
{
    [Authorize(Roles = "User")]
    public class ToDoController : Controller
    {
        private readonly ToDoDbContext _context;
        private ToDoRepository toDoRepo;

        public ToDoController(ToDoDbContext context)
        {
            _context = context;
            toDoRepo = new ToDoRepository(context);
        }

        // GET: ToDo
        [HttpGet]
        public async Task<ActionResult> Index()
        {
            // Jwt Auth stuff
            // Our jwt and refresh tokens are being stored in context.request.Headers, for example:
            // jwtToken = this.context.request.Headers["jwt"]
            // refreshToken = this.context.request.Headers["refreshToken"]
            // We need to pull that token, read it and grab the claims. This will happen before routing - is it a setting in Program.cs?
            var userId = GetUserIdFromClaims();
            if (userId == 0)
            {
                return NotFound();
            }
            var toDoLists = await toDoRepo.GetAllToDoListsByUserId(userId);

            ViewBag.ProgressSl = GetProgressSelectList();
            return View(toDoLists);
        }

        // GET: ToDo/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.ToDoList == null)
            {
                return NotFound();
            }

            var toDoList = await _context.ToDoList
                .FirstOrDefaultAsync(m => m.Id == id);
            if (toDoList == null)
            {
                return NotFound();
            }

            return View(toDoList);
        }

        // GET: ToDo/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: ToDo/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Title")] ToDoList toDoList)
        {
            var userId = GetUserIdFromClaims();
            if(userId == 0)
            {
                return NotFound();
            }
            toDoList.UserId = userId;
            toDoList.IsFinished = false;
            if (ModelState.IsValid)
            {
                await toDoRepo.CreateList(toDoList);
                return RedirectToAction(nameof(Index));
            }
            return View(toDoList);
        }

        // GET: ToDo/CreateListItem
        public async Task<IActionResult> CreateListItem(string id)
        {
            if(!await ValidateUserOwnsToDoList(Int32.Parse(id)))
            {
                return NotFound();
            }
            ViewBag.ToDoListId = id;
            ViewBag.PrioritySl = GetPrioritySelectList();
            return View();
        }

        // POST: ToDo/CreateListItem
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateListItem([Bind("ItemName,ItemDescription,EstimatedTime,Priority,ToDoListId")] ListItemCreate listItemCreate)
        {
            if (!await ValidateUserOwnsToDoList(listItemCreate.ToDoListId))
            {
                return NotFound();
            }
            
            ListItem listItem = new ListItem()
            {
                Progress = (ProgressEnum)1,
                ProgressString = "Not Started",
                ItemName = listItemCreate.ItemName,
                ItemDescription = listItemCreate.ItemDescription,
                EstimatedTime = listItemCreate.EstimatedTime,
                TimeSpent = 0,
                Priority = listItemCreate.Priority,
                ToDoListId = listItemCreate.ToDoListId
            };

            if (ModelState.IsValid)
            {
                await toDoRepo.CreateListItem(listItem);
                return RedirectToAction(nameof(Index));
            }
            return View(listItem);
        }

        // GET: ToDo/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            if (id == 0 || _context.ToDoList == null)
            {
                return NotFound();
            }

            var toDoList = await toDoRepo.GetToDoListByToDoListId(id);
            int userId = GetUserIdFromClaims();
            if (userId == 0 || toDoList.UserId != userId)
            {
                return NotFound();
            }

            return View(toDoList);
        }

        // POST: ToDo/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Title")] ToDoList toDoList)
        {
            if (!await ValidateUserOwnsToDoList(id))
            {
                return NotFound();
            }

            if (id != toDoList.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    await toDoRepo.UpdateList(toDoList);
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ToDoListExists(toDoList.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(toDoList);
        }

        // GET: ToDo/EditListItem/5
        public async Task<IActionResult> EditListItem(int? id)
        {
            if (id == null || _context.ListItem == null)
            {
                return NotFound();
            }

            var listItem = await toDoRepo.GetListItemByListItemId(id);
            if (listItem == null)
            {
                return NotFound();
            }

            if (!await ValidateUserOwnsToDoList(listItem.ToDoListId))
            {
                return NotFound();
            }

            ViewBag.ProgressSl = GetProgressSelectList();
            ViewBag.PrioritySl = GetPrioritySelectList();

            return View(listItem);
        }

        // POST: ToDo/EditListItem/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditListItem(int id, [Bind("Id,ItemName,ItemDescription,Progress,EstimatedTime,TimeSpent,Priority,ToDoListId")] ListItem listItem)
        {
            listItem.ProgressString = GetProgressString((int)listItem.Progress);

            if (!await ValidateUserOwnsToDoList(listItem.ToDoListId))
            {
                return NotFound();
            }

            if (id != listItem.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    await toDoRepo.UpdateListItem(listItem);
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ToDoListExists(listItem.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(listItem);
        }

         // GET: ToDo/Delete/5
        public async Task<IActionResult> Delete(int id)
        {
            if (id == 0 || _context.ToDoList == null)
            {
                return NotFound();
            }

            var toDoList = await toDoRepo.GetToDoListByToDoListId(id);
            int userId = GetUserIdFromClaims();
            if (userId == 0 || toDoList.UserId != userId)
            {
                return NotFound();
            }

            if (toDoList == null)
            {
                return NotFound();
            }

            return View(toDoList);
        }

        // POST: ToDo/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.ToDoList == null)
            {
                return Problem("Entity set 'ToDoDbContext.ToDoList'  is null.");
            }
            var toDoList = await toDoRepo.GetToDoListByToDoListId(id);
            int userId = GetUserIdFromClaims();
            if (userId == 0 || toDoList.UserId != userId)
            {
                return NotFound();
            }

            if (toDoList != null)
            {
                await toDoRepo.DeleteList(toDoList);
            }
            
            return RedirectToAction(nameof(Index));
        }

        // GET: ToDo/DeleteListItem/5
        public async Task<IActionResult> DeleteListItem(int id)
        {
            if (id == 0 || _context.ListItem == null)
            {
                return NotFound();
            }
            var listItem = await toDoRepo.GetListItemByListItemId(id);

            if (!await ValidateUserOwnsToDoList(listItem.ToDoListId))
            {
                return NotFound();
            }

            if (listItem == null)
            {
                return NotFound();
            }

            return View(listItem);
        }

        // POST: ToDo/DeleteListItem/5
        [HttpPost, ActionName("DeleteListItem")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteListItemConfirmed(int id)
        {
            if (_context.ListItem == null)
            {
                return Problem("Entity set 'ToDoDbContext.ListItem'  is null.");
            }
            var listItem = await toDoRepo.GetListItemByListItemId(id);

            if (!await ValidateUserOwnsToDoList(listItem.ToDoListId))
            {
                return NotFound();
            }

            if (listItem != null)
            {
                await toDoRepo.DeleteListItem(listItem);
            }

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> ListToggleIsFinished(int id)
        {
            if (id == 0 || _context.ListItem == null)
            {
                return NotFound();
            }

            int userId = GetUserIdFromClaims();
            var toDoList = await toDoRepo.GetToDoListByToDoListId(id);
            if(toDoList != null && userId == toDoList.UserId)
            {
                toDoList.IsFinished = !toDoList.IsFinished;
                await toDoRepo.UpdateList(toDoList);
            }

            return RedirectToAction(nameof(Index));
        }

        private bool ToDoListExists(int id)
        {
            return (_context.ToDoList?.Any(e => e.Id == id)).GetValueOrDefault();
        }

        public async Task UpdateProgress(int id, int progress)
        {
            ListItem itemToUpdate = await toDoRepo.GetListItemByListItemId(id);
            if (!await ValidateUserOwnsToDoList(itemToUpdate.ToDoListId))
            {
                return;
            }
            itemToUpdate.Progress = (ProgressEnum)progress;
            itemToUpdate.ProgressString = GetProgressString(progress);
            await toDoRepo.UpdateListItem(itemToUpdate);
        }

        public ActionResult ShowFinishedStatus(bool showFinished)
        {
            ViewBag.showFinished = !showFinished;
            return new EmptyResult();
        }

        public string GetProgressString(int progress)
        {
            switch (progress)
            {
                case 1:
                    return "Not Started";
                case 2:
                    return "In Progress";
                case 3:
                    return "Completed";
                default:
                    return "";
            }
        }

        // Create a Progress SelectList from ProgressEnum
        public List<SelectListItem> GetProgressSelectList()
        {
            List<SelectListItem> progressSl = new List<SelectListItem>();
            var enumValues = Enum.GetValues(typeof(ProgressEnum));

            var enumType = typeof(ProgressEnum);
            foreach (var enumValue in enumValues)
            {
                // Get Display Name attribute for each enumValue
                var enumInfo = enumType.GetMember(enumValue.ToString());
                var enumValueMemberInfo = enumInfo.FirstOrDefault(m => m.DeclaringType == enumType);
                var valueAttributes = enumValueMemberInfo.GetCustomAttributes(typeof(DisplayAttribute), false);
                var description = ((DisplayAttribute)valueAttributes[0]).Name;
                progressSl.Add(new SelectListItem { Text = description, Value = ((int)enumValue).ToString() });
            }

            return progressSl;
        }

        // Create a Priority SelectList from PriorityEnum
        public List<SelectListItem> GetPrioritySelectList()
        {
            List<SelectListItem> prioritySl = new List<SelectListItem>();
            var enumValues = Enum.GetValues(typeof(PriorityEnum));

            foreach (var enumValue in enumValues)
            {
                prioritySl.Add(new SelectListItem { Text = enumValue.ToString(), Value = ((int)enumValue).ToString() });
            }
            return prioritySl;
        }

        public int GetUserIdFromClaims()
        {
            return Int32.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
        }

        public async Task<bool> ValidateUserOwnsToDoList(int toDoListId)
        {
            int userId = GetUserIdFromClaims();
            var toDoList = await toDoRepo.GetToDoListByToDoListId(toDoListId);
            if (toDoList == null || userId == 0 || toDoList.UserId != userId)
            {
                return false;
            }
            return true;
        }
    }
}
