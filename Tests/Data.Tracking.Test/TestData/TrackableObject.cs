using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Oarw.Data.Tracking.TestData
{
    public class TrackableObject : ITrackableObject
    {
        public Guid Id { get; set; }

        public int Number { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }

        public int IntValue { get; set; }

        public string TextValue { get; set; }

        [NotMapped]
        public string NotMappedValue { get; set; }

        public TrackingState Tracking { get; set; }

        public List<TrackableSubObject> Children { get; set; } = new List<TrackableSubObject>();

        public TrackableSubObject Child { get; set; } = new TrackableSubObject();
    }
}
