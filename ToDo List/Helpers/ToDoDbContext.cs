using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using ToDo_List.Data;
using ToDo_List.Models;

namespace ToDo_List.Helpers
{
    public class ToDoDbContext : DbContext
    {
        public DbSet<ToDoList> ToDoList { get; set; }
        public DbSet<ListItem> ListItem { get; set; }
        public DbSet<UserLogin> User { get; set; }
        IWebHostEnvironment HostEnv { get; }

        public ToDoDbContext() { }

        public ToDoDbContext(DbContextOptions<ToDoDbContext> options,
                               IWebHostEnvironment env) : base(options)
        {
            HostEnv = env;
        }


        protected override void OnModelCreating(ModelBuilder builder)
        {
            // We can store variables as the int representation of an enum, and easily convert when reading and writing to our db
            builder.Entity<ListItem>()
                    .Property(p => p.Priority)
                    .HasConversion(new EnumToNumberConverter<PriorityEnum, int>());
            builder.Entity<ListItem>()
                    .Property(p => p.Progress)
                    .HasConversion(new EnumToNumberConverter<ProgressEnum, int>());
            base.OnModelCreating(builder);
        }


        //public DbSet<UserDto> UserDto { get; set; }
    }
}
