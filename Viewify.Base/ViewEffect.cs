using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Viewify.Base;

public enum ViewEffect
{
    None = 0,
    Performed = 1 << 0,

    Create = 1 << 1,
    Update = 1 << 2,
    Delete = 1 << 3,
}
