using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Viewify.Base;

public sealed class Fragment : View
{
    [Prop]
    public bool UseKey { get; init; }

    public Fragment(bool useKey = false)
    {
        UseKey = useKey;
    }

    public override View? Render()
    {
        throw new InvalidOperationException();
    }
}
