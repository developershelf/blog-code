using Microsoft.EntityFrameworkCore;
using MyApp.Models;

namespace MyApp.Infrastructure
{
    public class MyAppDbContext : DbContext
    {
        public MyModel MyModel { get; set; }
    }
}
