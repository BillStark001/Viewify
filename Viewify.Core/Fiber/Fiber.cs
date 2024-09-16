using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Viewify.Base;

public enum FiberTag : int
{
    Create,
    Update,
    Remove,
    Insert,
}

public class Fiber<N>
{



    // tree structure

    public N Content { get; }

    public Fiber<N>? Sibling { get; set; }
    public Fiber<N>? Child { get; set; }
    public Fiber<N>? Return { get; set; }

    public Fiber<N>? Alternate { get; set; }

    public FiberTag? Tag { get; set; } 

    public object? Key { get; set; }

    public static N operator ~(Fiber<N> f) => f.Content;


    public Fiber(N content, object? key = default)
    {
        Content = content;
        Key = key;
    }

    /// <summary>
    /// child -> sibling -> return
    /// </summary>
    /// <returns></returns>
    public Fiber<N>? Next()
    {
        if (Child != null)
        {
            return Child;
        }
        var nextFiber = this;
        while (nextFiber != null)
        {
            if (nextFiber.Sibling != null)
            {
                return nextFiber.Sibling;
            }
            nextFiber = nextFiber.Return;
        }
        return null;
    }
}

