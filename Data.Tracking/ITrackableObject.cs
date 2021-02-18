using System;
using System.Collections.Generic;
using System.Text;

namespace Oarw.Data.Tracking
{
    public interface ITrackableObject
    {
        Guid Id { get; set; }
    }
}
