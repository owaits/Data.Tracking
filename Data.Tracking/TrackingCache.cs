using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Oarw.Data.Tracking
{
    internal class TrackingCache
    {
        public Dictionary<Type, PropertyInfo[]> Properties { get; set; } = new Dictionary<Type, PropertyInfo[]>();

        public Dictionary<PropertyInfo, bool> AllowedProperties { get; set; } = new Dictionary<PropertyInfo, bool>();
    }
}
