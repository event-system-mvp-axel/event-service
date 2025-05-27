using Microsoft.EntityFrameworkCore;
using EventService.Models;

namespace EventService.Data
{
    public class EventContext : DbContext
    {
        public EventContext(DbContextOptions<EventContext> options)
            : base(options)
        {
        }

        public DbSet<Event> Events { get; set; }
    }
}