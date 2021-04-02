﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Oarw.Data.Tracking.Blazor
{
    public interface ITrackedPrintProfile
    {
        string Id { get; set; }

        string Name { get; set; }

        string Description { get; set; }
    }
}
