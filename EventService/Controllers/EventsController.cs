using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EventService.Data;
using EventService.DTOs;
using EventService.Models;

namespace EventService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EventsController : ControllerBase
    {
        private readonly EventContext _context;

        public EventsController(EventContext context)
        {
            _context = context;
        }

        // GET: api/events/test
        [HttpGet("test")]
        public ActionResult<string> Test()
        {
            return Ok("Events API is working!");
        }

        // GET: api/events
        [HttpGet]
        public async Task<ActionResult<IEnumerable<EventDto>>> GetEvents()
        {
            try
            {
                // Testa först om tabellen finns
                var count = await _context.Events.CountAsync();

                var events = await _context.Events
                    .Select(e => new EventDto
                    {
                        Id = e.Id,
                        Title = e.Title,
                        Description = e.Description,
                        Location = e.Location,
                        StartDate = e.StartDate,
                        EndDate = e.EndDate,
                        Category = e.Category,
                        MaxTickets = e.MaxTickets,
                        Price = e.Price,
                        ImageUrl = e.ImageUrl
                    })
                    .ToListAsync();

                return Ok(events);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Database error: {ex.Message}");
            }
        }

        // GET: api/events/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<EventDto>> GetEvent(Guid id)
        {
            var eventItem = await _context.Events.FindAsync(id);

            if (eventItem == null)
            {
                return NotFound();
            }

            return Ok(new EventDto
            {
                Id = eventItem.Id,
                Title = eventItem.Title,
                Description = eventItem.Description,
                Location = eventItem.Location,
                StartDate = eventItem.StartDate,
                EndDate = eventItem.EndDate,
                Category = eventItem.Category,
                MaxTickets = eventItem.MaxTickets,
                Price = eventItem.Price,
                ImageUrl = eventItem.ImageUrl
            });
        }

        // GET: api/events/search?q=searchterm
        [HttpGet("search")]
        public async Task<ActionResult<IEnumerable<EventDto>>> SearchEvents([FromQuery] string q)
        {
            if (string.IsNullOrWhiteSpace(q))
            {
                return BadRequest("Search query is required");
            }

            var events = await _context.Events
                .Where(e => e.Title.Contains(q) ||
                           e.Description.Contains(q) ||
                           e.Location.Contains(q) ||
                           e.Category.Contains(q))
                .Select(e => new EventDto
                {
                    Id = e.Id,
                    Title = e.Title,
                    Description = e.Description,
                    Location = e.Location,
                    StartDate = e.StartDate,
                    EndDate = e.EndDate,
                    Category = e.Category,
                    MaxTickets = e.MaxTickets,
                    Price = e.Price,
                    ImageUrl = e.ImageUrl
                })
                .ToListAsync();

            return Ok(events);
        }

        // POST: api/events
        [HttpPost]
        [Authorize]
        public async Task<ActionResult<EventDto>> CreateEvent(CreateEventDto createDto)
        {
            var eventItem = new Event
            {
                Id = Guid.NewGuid(),
                Title = createDto.Title,
                Description = createDto.Description,
                Location = createDto.Location,
                StartDate = createDto.StartDate,
                EndDate = createDto.EndDate,
                Category = createDto.Category,
                MaxTickets = createDto.MaxTickets,
                Price = createDto.Price,
                ImageUrl = createDto.ImageUrl,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.Events.Add(eventItem);
            await _context.SaveChangesAsync();

            var eventDto = new EventDto
            {
                Id = eventItem.Id,
                Title = eventItem.Title,
                Description = eventItem.Description,
                Location = eventItem.Location,
                StartDate = eventItem.StartDate,
                EndDate = eventItem.EndDate,
                Category = eventItem.Category,
                MaxTickets = eventItem.MaxTickets,
                Price = eventItem.Price,
                ImageUrl = eventItem.ImageUrl
            };

            return CreatedAtAction(nameof(GetEvent), new { id = eventDto.Id }, eventDto);
        }

        // PUT: api/events/{id}
        [HttpPut("{id}")]
        [Authorize]
        public async Task<ActionResult> UpdateEvent(Guid id, UpdateEventDto updateDto)
        {
            var eventItem = await _context.Events.FindAsync(id);
            if (eventItem == null)
            {
                return NotFound();
            }

            eventItem.Title = updateDto.Title ?? eventItem.Title;
            eventItem.Description = updateDto.Description ?? eventItem.Description;
            eventItem.Location = updateDto.Location ?? eventItem.Location;
            eventItem.StartDate = updateDto.StartDate ?? eventItem.StartDate;
            eventItem.EndDate = updateDto.EndDate ?? eventItem.EndDate;
            eventItem.Category = updateDto.Category ?? eventItem.Category;
            eventItem.MaxTickets = updateDto.MaxTickets ?? eventItem.MaxTickets;
            eventItem.Price = updateDto.Price ?? eventItem.Price;
            eventItem.ImageUrl = updateDto.ImageUrl ?? eventItem.ImageUrl;
            eventItem.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return NoContent();
        }

        // DELETE: api/events/{id}
        [HttpDelete("{id}")]
        [Authorize]
        public async Task<ActionResult> DeleteEvent(Guid id)
        {
            var eventItem = await _context.Events.FindAsync(id);
            if (eventItem == null)
            {
                return NotFound();
            }

            _context.Events.Remove(eventItem);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}