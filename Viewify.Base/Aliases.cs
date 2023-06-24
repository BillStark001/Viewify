using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Viewify.Base;

using static View;
using static Viewify.Base.ViewRecord;

public static class Aliases
{

    public static ViewRecord R(FuncView view, params ViewRecord[] children) 
        => new FuncViewRecord(view, children.Length == 0 ? null : children);


    public static ViewRecord R<V>(V view, params ViewRecord[] children)
        where V : View
    {
        return new ClassViewRecord(view, children.Length == 0 ? null : children);
    }


}