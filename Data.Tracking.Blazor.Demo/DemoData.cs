using Oarw.Data.Tracking;
using System;

namespace Data.Tracking.Blazor.Demo
{
    public class DemoData:ITrackableObject
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public string Name { get; set; }

        public DateTime StartDate { get; set; } = DateTime.UtcNow;        
    }
}
