using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using ToDo_List.Data;
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
            foreach (var item in listItems)
            {
                totalTimeSpent += item.TimeSpent;
                if (totalEstimatedTime != null)
                {
                    totalEstimatedTime += item.EstimatedTime;
                }
            }

            var dailyCompletedCount = new Dictionary<double, double>();
            foreach(var item in toDoLists)
            {
                if (item.IsFinished)
                {
                    // Convert DateTime to Javascript timestamp, ignore time
                    var date = item.DateTimeListFinished.Value.Date.ToUniversalTime().Subtract(
                        new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalMilliseconds;
                    if (!dailyCompletedCount.ContainsKey(date))
                    {
                        dailyCompletedCount[date] = 1;
                    }
                    else
                    {
                        dailyCompletedCount[date] += 1;
                    }
                }
            }

            List<DataPoint> dataPoints = new List<DataPoint>();
            foreach(var dictItem in dailyCompletedCount)
            {
                dataPoints.Add(new DataPoint(dictItem.Key, dictItem.Value));
            }

            ViewBag.DataPoints = JsonConvert.SerializeObject(dataPoints);
            ViewBag.TotalListsCompleted = totalListsCompleted;
            ViewBag.TotalTimeSpent = totalTimeSpent;
            ViewBag.TotalEstimatedTime = totalEstimatedTime;

            return View();
        }

        public int GetUserIdFromClaims()
        {
            return Int32.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
        }
    }
}
