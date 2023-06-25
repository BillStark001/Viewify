using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Viewify.Base;

public interface IDispatcher
{
    public void Dispatch(
        Fiber<ViewNode> target,
        ViewEffect effect,
        Action<ViewNode>? work
    );
}

public static class Dispatcher
{
    public delegate void DispatcherDelegate(
        Fiber<ViewNode> target,
        ViewEffect effect,
        Action<ViewNode>? work
    );
}
