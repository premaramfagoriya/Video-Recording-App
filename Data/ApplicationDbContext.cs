using Microsoft.EntityFrameworkCore;
using RecordingApp.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RecordingApp.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        public DbSet<UserRecording> UserRecordings { get; set; }
    }
}