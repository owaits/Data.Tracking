using System;
using System.Collections.Generic;
using System.Text;

namespace Data.Tracking
{
    public interface ITrackableObject
    {
        Guid Id { get; set; }
    }
}
