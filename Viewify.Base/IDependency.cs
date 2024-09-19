using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Viewify.Base;

public interface IDependency : IStateful
{
    public void Derive() { }
}
