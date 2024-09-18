using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Viewify.Base;



public abstract class View
{
    // needed to be overloaded
    public abstract View? Render();


    // properties

    [Prop] public object? Key { get; protected set; }
    public object? Reference { get; protected set; }

    [Prop] public IList<View> Children { get; protected set; } = new List<View>().AsReadOnly();


    public View SetKey(object key)
    {
        Key = key;
        return this;
    }

    public View SetReference(object reference)
    {
        Reference = reference;
        return this;
    }

    public View SetChildren(params View[] children)
    {
        var lst = children.ToList();
        Children = lst.AsReadOnly();
        return this;
    }

    public static View Case(bool cond, View? v1, View? v2)
    {
        return new ConditionView(cond, v1, v2);
    }

    public static View Loop(params View[] children)
    {
        return new Fragment(true).SetChildren(children);
    }

    
}
