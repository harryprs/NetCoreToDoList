using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using System.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.AspNetCore.Mvc.Authorization;
using ToDo_List.Helpers;

namespace ToDo_List
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            builder.Services.AddControllersWithViews();
            // Configurations are stored as a Json in appsettings.json
            var configuration = builder.Configuration;

            // Start of Authentication
            // Don't seem to use any of this
            /*var identityUrl = configuration.GetValue<string>("IdentityUrl");
            var callBackUrl = configuration.GetValue<string>("CallBackUrl");
            var sessionCookieLifetime = configuration.GetValue("SessionCookieLifetimeMinutes", 60);*/

            //DB context - can create a AppDbContext class and call instead
            var cs = builder.Environment.IsDevelopment() ? configuration.GetConnectionString("DevSqlConnection") : configuration.GetConnectionString("defaultConnection");
            builder.Services.AddDbContext<ToDoDbContext>(options =>
            {
                options.UseSqlServer(cs);
            });

            // START OF ROLE BASED AUTH
            builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                            .AddCookie(options =>
                            {
                                options.LoginPath = "/Auth/Login";
                                options.ExpireTimeSpan = TimeSpan.FromMinutes(5);
                            });
            builder.Services.AddAuthorization(options =>
            {
                options.AddPolicy("Admin", build =>
                {
                    build.RequireClaim("role", "Admin");
                });
                options.AddPolicy("User", build =>
                {
                    build.RequireClaim("role", "User");
                });
            });
            //END OF ROLE BASED AUTH

            // Add Auth Services
            // START OF JWT AUTHENTICATION - WIP
            // https://www.youtube.com/watch?v=YmxtJ5euiSo&ab_channel=CsharpSpace
            // Set up authentication options
            /*services.AddAuthentication(options => {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme= JwtBearerDefaults.AuthenticationScheme;
            })*/
            /*
            builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        //ValidIssuer = "ToDoHprs",
                        ValidIssuer = builder.Configuration["JwtOption:Issuer"],
                        ValidateAudience = true,
                        //ValidAudience = "https://localhost:44363/",
                        ValidAudience = builder.Configuration["JwtOption:Audience"],

                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8
                            .GetBytes(builder.Configuration.GetSection("JwtOption:Key").Value)),
                    };
                })
                .AddCookie();
            builder.Services.AddAuthorization();
            // END OF JWT AUTHENTICATION

            // BEGINNING OF USING JWT AUTHENTICATION IN COOKIE TEST
            /*var jwt_key = configuration.GetSection("JwtOption:IssuerSigningKey").Value;
            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidIssuer = "ToDoHprs",

                ValidateAudience = true,
                ValidAudience = "https://localhost:44363/",

                ValidateIssuerSigningKey = true,
                //IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8
                //            .GetBytes(builder.Configuration.GetSection("Tokens:Token").Value)),
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8
                            .GetBytes(builder.Configuration.GetSection("JwtOption:IssuerSigningKey").Value)),
            };
            services.AddSingleton(tokenValidationParameters);
            services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme).AddCookie(options =>
            {
                int minute = 60;
                int hour = minute * 60;
                int day = hour * 24;
                int week = day * 7;
                int year = 365 * day;

                options.LoginPath = "/auth/login";
                options.AccessDeniedPath = "/auth/accessdenied";
                options.Cookie.IsEssential = true;
                options.SlidingExpiration = true;
                options.ExpireTimeSpan = TimeSpan.FromSeconds(day / 2);

                options.Cookie.Name = "jwt";
                options.TicketDataFormat = new CustomJwtDataFormat(
                    SecurityAlgorithms.HmacSha256,
                    tokenValidationParameters);
            });*/
            // END OF USING JWT AUTHENTICATION IN COOKIE TEST
            //services.AddIdentity<IdentityUser, IdentityRole>();

            // END OF JWT AUTHENTICATION

            // START OF EXTERNAL AUTHENTICATION SERVICES
            /*services.AddAuthentication(options =>
            {
                options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = GoogleDefaults.AuthenticationScheme;
            })
            .AddCookie()
            .AddGoogle(GoogleDefaults.AuthenticationScheme, options =>
            {
                options.ClientId = configuration["Authentication:Google:ClientId"];
                options.ClientSecret = configuration["Authentication:Google:ClientSecret"];
                options.ClaimActions.MapJsonKey("urn:google:picture", "picture", "url");
                //options.Scope.Add("https://www.googleapis.com/auth/user.birthday.read");
                options.SaveTokens = true;
                options.Events.OnCreatingTicket = ctx =>
                {
                    List<AuthenticationToken> tokens = ctx.Properties.GetTokens().ToList();
                    tokens.Add(new AuthenticationToken()
                    {
                        Name = "TicketCreated",
                        Value = DateTime.UtcNow.ToString()
                    });
                    ctx.Properties.StoreTokens(tokens);
                    return Task.CompletedTask;
                };
            })
            .AddMicrosoftAccount(microsoftOptions =>
            {
                microsoftOptions.ClientId = builder.Configuration["Authentication:Microsoft:ClientId"];
                microsoftOptions.ClientSecret = builder.Configuration["Authentication:Microsoft:ClientId"];
            });*/
            // END OF EXTERNAL AUTHENTICATION SERVICES
            //builder.Services.AddAuthorization();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");

            app.Run();
        }
    }
}