using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Viewify.Base.Views;
using static Viewify.Base.ViewRecord;

namespace Viewify.Base;

public abstract class ViewRecord
{
    public delegate IEnumerable<ViewRecord> FuncView(
        IViewifyInstance v,
        IEnumerable<ViewRecord> children
    );

    public virtual Type ViewType => typeof(IView);
    public IEnumerable<ViewRecord>? Children { get; set; }

    public ViewRecord(IEnumerable<ViewRecord>? children, string? key)
    {
        Children = children;
        Key = key;
    }

    public virtual bool IsNative => true;

    public string? Key { get; set; }
}

public class EmptyViewRecord : ViewRecord
{
    public EmptyViewRecord(IEnumerable<ViewRecord>? children = null, string? key = null) : base(children, key)
    {
    }
}

public class ClassViewRecord : ViewRecord
{
    public Func<View> View { get; }


    public ClassViewRecord(Func<View> view, IEnumerable<ViewRecord>? children = null, string? key = null) : base(children, key)
    {
        View = view;
    }

    public override Type ViewType => View?.GetType() ?? typeof(IView);

    public override bool IsNative => View is NativeView;
}


public class FuncViewRecord : ViewRecord
{
    public FuncView View { get; }

    public FuncViewRecord(FuncView view, IEnumerable<ViewRecord>? children = null, string? key = null) : base(children, key)
    {
        View = view;
    }

    public override bool IsNative => false;
}


public class ViewBuilder
{

    public delegate IEnumerable<ViewRecord> UseBuilderFunction(ViewBuilder b);

    private readonly List<ViewRecord> _records;

    public ViewBuilder()
    {
        _records = new();
    }


    public ViewBuilder V(FuncView view, bool condition = true)
    {
        _records.Add(new FuncViewRecord(view));
        return this;
    }

    public ViewBuilder V(Func<View> view, bool condition = true)
    {
        _records.Add(new ClassViewRecord(view));
        return this;
    }

    public ViewBuilder V()
    {
        _records.Add(new EmptyViewRecord());
        return this;
    }

    public ViewBuilder C(params ViewRecord[] children)
    {
        if (_records.Count > 0)
            _records.Last().Children = children;
        return this;
    }

    public ViewBuilder C(IEnumerable<ViewRecord> children)
    {
        if (_records.Count > 0)
            _records.Last().Children = children;
        return this;
    }

    public ViewBuilder C<R>(Func<ViewBuilder, R> f)
    {
        if (_records.Count > 0)
        {
            ViewBuilder b = new();
            f(b);
            _records.Last().Children = b.Build();
        }
        return this;
    }

    public IEnumerable<ViewRecord> Build()
    {
        return _records.AsReadOnly();
    }
}