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

    public abstract void WillMount();

    public abstract void DidMount();

    public abstract void WillUpdate();

    public abstract void DidUpdate();

    public abstract void WillUnmount();

    public abstract void DidUnmount();
}
