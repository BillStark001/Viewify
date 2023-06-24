using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Viewify.Base.Views;

public class NativeView : View
{
    public override IEnumerable<ViewRecord> Render(IEnumerable<ViewRecord> children)
    {
        throw new InvalidOperationException("The `Render()` function of a native view should never be called.");
    }
}
