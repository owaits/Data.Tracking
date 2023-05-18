using Oarw.Data.Tracking;
using System;
using System.ComponentModel.DataAnnotations;

namespace Data.Tracking.Blazor.Demo
{
    public class DemoData:ITrackableObject
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public string? Name { get; set; }

        public DateTime StartDate { get; set; } = DateTime.UtcNow;        
    }
}
