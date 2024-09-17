using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Viewify.Core.Fiber;

public enum FiberTag : int
{
    Idle,
    Create,
    Update,
    Remove,
    Insert,
}

public class Fiber<N>(N content, object? key = default)
{



    // tree structure

    public N Content { get; } = content;

    public Fiber<N>? Sibling { get; set; }
    public Fiber<N>? Child { get; set; }
    public Fiber<N>? Parent { get; set; }

    public Fiber<N>? Alternate { get; set; }


    public object? Key { get; set; } = key;

    public static N operator ~(Fiber<N> f) => f.Content;

    public FiberTag? Tag { get; set; }
    public List<Fiber<N>>? OperativeFibers { get; set; }

    public void AddOperativeFiber(Fiber<N> fiber)
    {
        if (OperativeFibers == null)
        {
            OperativeFibers = [];
        }
        OperativeFibers.Add(fiber);
    }

    /// <summary>
    /// Called after visited. 
    /// Detach any connections to old fibers to avoid memory leak.
    /// </summary>
    public void VisitComplete()
    {
        Alternate = null;
        Tag = null;
        OperativeFibers = null;
    }

    /// <summary>
    /// child -> sibling -> parent
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
            nextFiber = nextFiber.Parent;
        }
        return null;
    }
}

