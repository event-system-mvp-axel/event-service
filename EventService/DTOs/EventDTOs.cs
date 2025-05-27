using System;
using System.ComponentModel.DataAnnotations;

namespace EventService.DTOs
{
    public class EventDto
    {
        public Guid Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string Location { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Category { get; set; }
        public int MaxTickets { get; set; }
        public decimal Price { get; set; }
        public string ImageUrl { get; set; }
    }

    public class CreateEventDto
    {
        [Required]
        public string Title { get; set; }

        [Required]
        public string Description { get; set; }

        [Required]
        public string Location { get; set; }

        [Required]
        public DateTime StartDate { get; set; }

        [Required]
        public DateTime EndDate { get; set; }

        [Required]
        public string Category { get; set; }

        [Required]
        [Range(1, int.MaxValue)]
        public int MaxTickets { get; set; }

        [Required]
        [Range(0, double.MaxValue)]
        public decimal Price { get; set; }

        public string ImageUrl { get; set; }
    }

    public class UpdateEventDto
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public string Location { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string Category { get; set; }
        public int? MaxTickets { get; set; }
        public decimal? Price { get; set; }
        public string ImageUrl { get; set; }
    }
}