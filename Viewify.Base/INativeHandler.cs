using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Viewify.Base.Native;

namespace Viewify.Base;

public interface INativeHandler
{

    public void ResetCursor(NativeView v);

    public void AscendCursor();

    public void DescendCursor();

    public object? GetPointed();


    public void BindReference(NativeView v);


    public void Mount(NativeView v) { }
    public void Update(NativeView v) { }
    public void Unmount(NativeView v) { }
    public void Move(NativeView v) { }


}
