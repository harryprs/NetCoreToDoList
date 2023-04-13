using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using NuGet.Common;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using ToDo_List.Data;
using ToDo_List.Helpers;

namespace ToDo_List.Controllers
{
    public class AuthController : Controller
    {
        private readonly ToDoDbContext _context;
        private readonly IConfiguration _configuration;

        public AuthController(ToDoDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        // GET: Auth/Register
        public IActionResult Register()
        {
            return View();
        }

        // POST: Auth/Register
        [HttpPost]
        public async Task<ActionResult<UserLogin>> Register(UserDto request)
        {
            var existingUser = await _context.User.Where(x => x.Username == request.Username).FirstOrDefaultAsync();
            // User with that username already exists
            if(existingUser != null)
            {
                return View(request);
            }

            // passwordSalt - a random piece of data added to the password before running it through the password hashing algorithm, make it more secure
            // passwordHash - an algorithm which morphs the password into a new string
            CreatePasswordHash(request.Password, out byte[] passwordHash, out byte[] passwordSalt);
            UserLogin user = new UserLogin();
            user.Username = request.Username;
            user.PasswordHash = passwordHash;
            user.PasswordSalt = passwordSalt;

            // save to db
            if (ModelState.IsValid)
            {
                _context.Add(user);
                await _context.SaveChangesAsync();
                return RedirectToAction("Index", "Home");
            }

            return Ok(user);
        }

        // GET: Auth/Login
        public IActionResult Login()
        {
            //Check if user is already logged in
            ClaimsPrincipal claimUser = HttpContext.User;
            if (claimUser.Identity.IsAuthenticated)
                return RedirectToAction("Index", "ToDo");

            return View();
        }

        // POST: Auth/Login
        [HttpPost]
        public async Task<ActionResult<string>> Login(UserDto request)
        {
            // provide with dummy data to test
            /*request.Username = "harryprs";
            request.Password = "admin123";*/
            /*
            var dbUser = await _context.User.Where(u => u.Username == request.Username).FirstOrDefaultAsync();
            if (dbUser.Username != request.Username)
            {
                return BadRequest("User not found.");
            }

            // verify if the salted hashed password given matches our stored salted hashed password
            if (!VerifyPasswordHash(request.Password, dbUser.PasswordHash, dbUser.PasswordSalt))
            {
                return BadRequest("Wrong password.");
            }
            
            // create a jwtToken to pass back to our now successfully logged in user so they can happily have access to our site
            string jwtToken = CreateToken(dbUser); // pass in our dbUser we've now verified
            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                //expires = jwtToken.expires, // can't check this for jwt
            };
            // Store jwt in our cookie
            Response.Cookies.Append("jwt", jwtToken, cookieOptions);
            // Store in Request header
            // Stores it in header but only in this context for the next routing.
            //HttpContext.Request.Headers.Add("Authorization", $"Bearer {jwtToken}");

            // generate refresh token and store it on our User
            var refreshToken = GenerateRefreshToken();
            var userToUpdate = SetRefreshToken(refreshToken, dbUser);

            HttpContext.Request.Headers.Add("Authorization", jwtToken);

            // save refresh token to db
            if (ModelState.IsValid)
            {
                _context.Update(userToUpdate);
                await _context.SaveChangesAsync();
                return RedirectToAction("Index", "ToDo");
            }
            */
            var dbUser = await _context.User.Where(u => u.Username == request.Username).FirstOrDefaultAsync();
            if (dbUser == null || dbUser.Username != request.Username)
            {
                ModelState.AddModelError(nameof(request.Username), "Username or Password is wrong");
                return View();
            }

            if (!VerifyPasswordHash(request.Password, dbUser.PasswordHash, dbUser.PasswordSalt))
            {
                ModelState.AddModelError(nameof(request.Username), "Username or Password is wrong");
                return View();
            }

            List<Claim> claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, dbUser.Id.ToString()),
                new Claim(ClaimTypes.Name, dbUser.Username),
                new Claim(ClaimTypes.Role, "User")// Is this claim stored in our db?
            };
            ClaimsIdentity claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            AuthenticationProperties properties = new AuthenticationProperties()
            {
                AllowRefresh = true,
                IsPersistent = true,//KeepLoggedIn
            };
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, 
                new ClaimsPrincipal(claimsIdentity), properties);
            //await HttpContext.SignInAsync(new ClaimsPrincipal(new ClaimsIdentity(claims, "Cookies", "user", "role")));

            return RedirectToAction("Index", "ToDo");
        }

        public async Task<IActionResult> LogOut()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Index", "Home");
        }

        // Refresh tokens are used to acquire new access tokens from the authentication component - refresh tokens have a much longer lifetime
        /* Functions used for JWT Auth
        HttpPost("refresh-token")]
        public async Task<ActionResult<string>> RefreshToken(UserLogin user)
        {
            var refreshToken = Request.Cookies["refreshToken"];
            // How should we handle this user context
            if (!user.RefreshToken.Equals(refreshToken))
            {
                return Unauthorized("Invalid Refresh Token.");
            }
            else if (user.TokenExpires < DateTime.Now)
            {
                return Unauthorized("Token expired.");
            }

            string token = CreateToken(user);
            var newRefreshToken = GenerateRefreshToken();
            var userToSave = SetRefreshToken(newRefreshToken, user);
            // save refresh token to db
            if (ModelState.IsValid)
            {
                _context.Add(userToSave);
                await _context.SaveChangesAsync();
            }

            return Ok(token);
        }
        

        private RefreshToken GenerateRefreshToken()
        {
            var refreshToken = new RefreshToken
            {
                Token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64)),
                Expires = DateTime.Now.AddDays(7),
                Created = DateTime.Now
            };

            return refreshToken;
        }

        private UserLogin SetRefreshToken(RefreshToken newRefreshToken, UserLogin user)
        {
            // set refresh token to our user
            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Expires = newRefreshToken.Expires
            };
            // Store jwt in our cookie?
            Response.Cookies.Append("refreshToken", newRefreshToken.Token, cookieOptions);
            
            // How should we handle this user context
            // Do we need to set this token? How exactly does refresh token work? Do we compare the refresh token we have in our cookies to the refresh token in our db?
            user.RefreshToken = newRefreshToken.Token;
            user.TokenCreated = newRefreshToken.Created;
            user.TokenExpires = newRefreshToken.Expires;

            return user;
        }

        private string CreateToken(UserLogin user)
        {
            // Don't need to be an admin! Fix our claims here
            List<Claim> claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Role, "Admin")
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(
                _configuration["JwtOption:Key"]));

            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);

            var token = new JwtSecurityToken(
                issuer: _configuration["JwtOption:Issuer"],
                audience: _configuration["JwtOption:Audience"],
                claims: claims,
                expires: DateTime.Now.AddDays(1),
                signingCredentials: creds);

            var jwt = new JwtSecurityTokenHandler().WriteToken(token);

            return jwt;
        }
    */
        private void CreatePasswordHash(string password, out byte[] passwordHash, out byte[] passwordSalt)
        {
            using (var hmac = new HMACSHA512())
            {
                passwordSalt = hmac.Key;
                passwordHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
            }
        }

        private bool VerifyPasswordHash(string password, byte[] passwordHash, byte[] passwordSalt)
        {
            // checks the passwordSalt, then tri
            using (var hmac = new HMACSHA512(passwordSalt))
            {
                var computedHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
                return computedHash.SequenceEqual(passwordHash);
            }
        }
    }
}