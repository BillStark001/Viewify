using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Viewify.Base;

public abstract class HookHandler
{
    public abstract object MakeHook(ViewNode node, int index);

    public abstract bool Verify(ViewNode node, object mightBeHook, int index);

    public abstract object? UseHook(ViewNode node, object hook, Action markUpdate);
}

public abstract class HookHandler<H, R> : HookHandler
    where H : class
{
    public override object MakeHook(ViewNode node, int index) => Make(node, index);
    public override object? UseHook(ViewNode node, object hook, Action markUpdate) => Use(node, (H)hook, markUpdate);
    public override bool Verify(ViewNode node, object mightBeHook, int index)
    {
        return mightBeHook is H;
    }

    public abstract H Make(ViewNode node, int index);


    public abstract R Use(ViewNode node, H hook, Action markUpdate);

}
