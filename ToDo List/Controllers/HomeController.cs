using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using ToDo_List.Models;
using Microsoft.CodeAnalysis;
using ToDo_List.Helpers;

namespace ToDo_List.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ToDoDbContext _context;

        public HomeController(ILogger<HomeController> logger, ToDoDbContext toDoDbContext)
        {
            _logger = logger;
            _context = toDoDbContext;
        }

        public IActionResult Index()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        // App specific processing
        public IActionResult SignInSuccess()
        {
            return RedirectToAction("Index");
        }

        // Oauth2 Microsoft Login
        public IActionResult LoginWithMicrosoft()
        {
            var props = new AuthenticationProperties();
            props.RedirectUri = "/Home/SignInSuccess";

            return Challenge(props);
        }

        // Redirection to Google login page
        public async Task LoginWithGoogle()
        {
            await HttpContext.ChallengeAsync(GoogleDefaults.AuthenticationScheme, new AuthenticationProperties()
            {
                RedirectUri = Url.Action("GoogleResponse")
            });
        }

        // Google login response after login operations from google page
        // NameIdentifier is persistant, but maybe not unique
        // We can get a unique identifier through OpenIDConnect.. but can't find the correct docs
        public async Task<IActionResult> GoogleResponse()
        {
            var result = await HttpContext.AuthenticateAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            // Query result, and re-package it
            var claims = result.Principal.Identities
                .FirstOrDefault().Claims.Select(claim => new
                {
                    claim.Issuer,
                    claim.OriginalIssuer,
                    claim.Type,
                    claim.Value
                });
            //NameIdentifier is not necessarily a unique identifier - the unique identifier is stored in sub, which is accessed somehow through another google api using an access token

            return Json(claims);
        }

        // Run by Logout button
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync();
            return RedirectToAction("About");
        }
    }
}