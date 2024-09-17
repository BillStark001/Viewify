using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Viewify.Base.Native;

public class Image(string source = "") : NativeView
{
    [Prop] public string Source { get; private set; } = source;


}
