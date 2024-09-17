using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Viewify.Base.Native;

public class Switch(bool value = false, EventHandler? onChange = null) : NativeView
{
    [Prop] public bool Value { get; private set; } = value;
    [Prop] public EventHandler? OnChange { get; private set; } = onChange;


}
