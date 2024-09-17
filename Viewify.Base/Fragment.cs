using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Viewify.Base;

public class Fragment(bool useKey = false) : View
{
    [Prop] public bool UseKey { get; private set; } = useKey;

    public sealed override View? Render()
    {
        throw new InvalidOperationException();
    }
}
