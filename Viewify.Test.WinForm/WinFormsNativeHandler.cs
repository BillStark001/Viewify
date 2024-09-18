using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Viewify.Base;
using Viewify.Base.Native;

namespace Viewify.Test.WinForm;

public class WinFormsNativeHandler(Control baseControl) : INativeHandler
{

    public Control BaseControl { get; init; } = baseControl;
    public Control CurrentControl { get; private set; } = baseControl;
    public int CurrentIndex { get; private set; } = 0;

    public void AdvanceCursor()
    {
        ++CurrentIndex;
    }

    public void AscendCursor()
    {
        if (CurrentControl == BaseControl)
        {
            CurrentIndex = 0;
            return;
        }
        CurrentControl = CurrentControl.Parent;
        CurrentIndex = CurrentControl.Controls.GetChildIndex(CurrentControl);
    }

    public void DescendCursor()
    {
        var current = GetPointed() as Control;
        if (current != null)
        {
            CurrentControl = current;
            CurrentIndex = 0;
        }
    }
    public void ResetCursor(NativeView? v)
    {
        CurrentControl = v?.NativeObject as Control ?? BaseControl;
        CurrentIndex = 0;
    }

    public object? GetPointed()
    {
        return CurrentControl.Controls.Count > CurrentIndex
            ? CurrentControl.Controls[CurrentIndex]
            : null;
    }

    public void BindReference(NativeView v)
    {
        throw new NotImplementedException();
    }

    public void Mount(NativeView v) {
        if (v is Text t)
        {
            var tb = new TextBox();
            tb.Text = t.Value;
            t.NativeObject = tb;
            CurrentControl.Controls.Add(tb);
        }
    }

    public void Update(NativeView v)
    {
        if (v is Text t)
        {
            var tb = t.NativeObject as TextBox;
            tb!.Text = t.Value;
        }

    }

    public void Unmount(NativeView v) {
        if (v is Text t)
        {
            var tb = t.NativeObject as TextBox;
            CurrentControl.Controls.Remove(tb);
        }
    }

    public void Move(NativeView v) {
        if (v is Text t)
        {
            var tb = t.NativeObject as TextBox;
            CurrentControl.Controls.SetChildIndex(tb, CurrentIndex);
        }
    }

}
