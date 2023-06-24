using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Viewify.Base;

public abstract class HookHandler<H, R>
{
    public abstract H Make();

    public virtual bool Verify(object mightBeHook, int index)
    {
        return mightBeHook is H;
    }

    public abstract R Use(Action markUpdate);
}
