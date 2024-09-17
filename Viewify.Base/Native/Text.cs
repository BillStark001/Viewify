using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Viewify.Base.Native;

public class Text(string value = "") : NativeView
{
    [Prop] public string Value { get; private set; } = value;

}
