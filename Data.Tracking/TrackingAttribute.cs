using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Oarw.Data.Tracking
{
    public class TrackingAttribute: Attribute
    {
        public bool Ignore { get; set; } = false;
    }
}
