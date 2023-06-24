using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Viewify.Base.Views;

public class NativeView : View
{
    public virtual void OnMounted() { }

    public virtual void OnUpdate() { }

    public virtual void OnUpdated() { }

    public virtual void OnUnmount() { }


    public View Render()
    {
        return this;
    }

}
