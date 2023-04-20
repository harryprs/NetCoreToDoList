using System.Data;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using ToDo_List.Helpers;
using ToDo_List.Models;
using ToDo_List.Services;

namespace ToDo_List.Controllers
{
    [Authorize(Roles = "User")]
    public class ReportingController : Controller
    {
        private readonly ToDoDbContext _context;
        private ToDoRepository toDoRepo;

        public ReportingController(ToDoDbContext context)
        {
            _context = context;
            toDoRepo = new ToDoRepository(context);
        }

        // GET: Reporting/OverallReport
        public async Task<IActionResult> OverallReport()
        {
            var userId = GetUserIdFromClaims();
            var toDoLists = await toDoRepo.GetAllToDoListsByUserId(userId);

            var totalListsCompleted = toDoLists.Where(l => l.IsFinished).Count();

            var listItems = toDoLists.SelectMany(l => l.ListItems).ToList();
            int totalTimeSpent = 0;
            int? totalEstimatedTime = 0;
            int listItemsCompletedCount = 0;
            int listItemsInProgressCount = 0;
            int listItemsNotStartedCount = 0;
            foreach(var item in listItems)
            {
                totalTimeSpent += item.TimeSpent;
                if (totalEstimatedTime != null)
                {
                    totalEstimatedTime += item.EstimatedTime;
                }
                switch((int)item.Progress)
                {
                    case 1:
                        listItemsNotStartedCount++;
                        break;
                    case 2:
                        listItemsInProgressCount++;
                        break;
                    case 3:
                        listItemsCompletedCount++;
                        break;
                }
            }
            // Get the count of ToDoLists completed, indexed by date completed
            var dailyCompletedListCount = new Dictionary<double, double>();
            foreach(var item in toDoLists)
            {
                if(item.IsFinished)
                {
                    // Convert DateTime to Javascript timestamp, ignore time
                    var date = item.DateTimeListFinished.Value.Date.ToUniversalTime().Subtract(
                        new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalMilliseconds;
                    if(!dailyCompletedListCount.ContainsKey(date))
                    {
                        dailyCompletedListCount[date] = 1;
                    }
                    else
                    {
                        dailyCompletedListCount[date] += 1;
                    }
                }
            }
            
            List<DataPoint> listsDataPoints = new List<DataPoint>();
            foreach(var dictItem in dailyCompletedListCount)
            {
                listsDataPoints.Add(new DataPoint(dictItem.Key, dictItem.Value));
            }

            // Get the count of ListItems completed, indexed by daate completed
            var dailyCompletedItemCount = new Dictionary<double, double>();
            foreach (var item in listItems)
            {
                if ((int)item.Progress == 3)
                {
                    var date = item.DateTimeItemFinished.Value.Date.ToUniversalTime().Subtract(
                            new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalMilliseconds;
                    if (!dailyCompletedItemCount.ContainsKey(date))
                    {
                        dailyCompletedItemCount[date] = 1;
                    }
                    else
                    {
                        dailyCompletedItemCount[date] += 1;
                    }
                }
            }

            List<DataPoint> listItemsDataPoints = new List<DataPoint>();
            foreach(var dictItem in dailyCompletedItemCount)
            {
                listItemsDataPoints.Add(new DataPoint(dictItem.Key, dictItem.Value));
            }

            ReportingData reportingData = new ReportingData()
            {
                ListsCompletedDataPoint = JsonConvert.SerializeObject(listsDataPoints),
                ListItemsCompletedDataPoint = JsonConvert.SerializeObject(listItemsDataPoints),
                TotalListsCompleted = totalListsCompleted,
                TotalTimeSpent = totalTimeSpent,
                TotalEstimatedTime = totalEstimatedTime,
                ListItemsCompletedCount = listItemsCompletedCount,
                ListItemsInProgressCount = listItemsInProgressCount,
                ListItemsNotStartedCount = listItemsNotStartedCount
            };

            return View(reportingData);
        }

        public int GetUserIdFromClaims()
        {
            return Int32.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
        }
    }
}
