using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Oarw.Data.Tracking.Helpers
{
    public class GuidMap
    {
        private Dictionary<Guid, Guid> map = new Dictionary<Guid, Guid>();

        public Guid MapId(Guid source)
        {
            if (map.TryGetValue(source, out Guid destination))
                return destination;

            Guid newId = Guid.NewGuid();
            map.Add(source, newId);
            return newId;
        }
    }
}
