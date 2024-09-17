using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Viewify.Base.Native;

public class Input(string value = "", EventHandler? onChange = null) : NativeView
{
    [Prop] public string Value { get; private set; } = value;
    [Prop] public EventHandler? OnChange { get; private set; } = onChange;


}
