using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Viewify.Base;

public abstract class NativeView : View
{
    public sealed override View? Render()
    {
        throw new InvalidOperationException();
    }

    public abstract void Mount();

    public abstract void Update();

    public abstract void Unmount();

    public abstract void Move();
}
