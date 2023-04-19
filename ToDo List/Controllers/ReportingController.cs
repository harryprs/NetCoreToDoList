using System;
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

            var dailyCompletedCount = new Dictionary<double, double>();

            foreach(var item in toDoLists)
            {
                if (item.IsFinished)
                {
                    // Convert DateTime to Javascript timestamp, ignore time
                    var day = item.DateTimeListFinished.Value.Date.ToUniversalTime().Subtract(
                        new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalMilliseconds;

                    if (!dailyCompletedCount.ContainsKey(day))
                    {
                        dailyCompletedCount[day] = 1;
                    }
                    else
                    {
                        dailyCompletedCount[day] += 1;
                    }
                }
            }

            List<DataPoint> dataPoints = new List<DataPoint>();
            foreach(var dictItem in dailyCompletedCount)
            {
                dataPoints.Add(new DataPoint(dictItem.Key, dictItem.Value));
            }

            ViewBag.DataPoints = JsonConvert.SerializeObject(dataPoints);

            return View();
        }

        public int GetUserIdFromClaims()
        {
            return Int32.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
        }
    }
}
