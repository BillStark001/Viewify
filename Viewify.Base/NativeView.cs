using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Viewify.Base;

public class NativeView : View
{
    public sealed override View? Render()
    {
        throw new InvalidOperationException();
    }

    public void Mount(INativeHandler h)
    {
        h.Mount(this);
    }

    public void Update(INativeHandler h)
    {
        h.Update(this);
    }

    public void Unmount(INativeHandler h)
    {
        h.Unmount(this);
    }

    public void Move(INativeHandler h)
    {
        h.Move(this);
    }
}
