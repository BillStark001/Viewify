using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Viewify.Core.Render;

public enum FiberTag : int
{
    Idle,
    Create,
    Update,
    Remove,
    Insert,
}

public enum FiberNextType: int
{
    Child,
    Sibling,
    Parent,
}

public class Fiber<N>(N content, object? key = default)
{



    // tree structure

    public N Content { get; } = content;
    public object? Key { get; set; } = key;
    public static N operator ~(Fiber<N> f) => f.Content;


    public Fiber<N>? Sibling { get; set; }
    public Fiber<N>? Child { get; set; }
    public Fiber<N>? Parent { get; set; }

    public Fiber<N>? Alternate { get; set; }
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
    public void Detach()
    {
        Alternate = null;
        Tag = null;
        OperativeFibers = null;
    }

    public Fiber<N>? Next(bool lastIsParent, out FiberNextType type)
    {
        if (!lastIsParent && Child != null)
        {
            type = FiberNextType.Child;
            return Child;
        }
        if (Sibling != null)
        {
            type = FiberNextType.Sibling;
            return Sibling;
        }
        type = FiberNextType.Parent;
        return Parent;
    }
}

