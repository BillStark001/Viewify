using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Viewify.Base;
using Viewify.Core.Utils;

namespace Viewify.Core.Render;

public class ViewNode
{
    public View? View { get; private set; }
    public Scheduler Scheduler { get; init; }
    public ViewRecord Record { get; init; }

    public ImmutableTreeHashTable<object> Context { get; private set; } = _defaultContext;

    private static readonly ImmutableTreeHashTable<object> _defaultContext = new();

    // TODO context record
    // TODO effect deps record

    private object?[] _effectDeps;
    private bool[] _effectDepChanged;

    public ViewNode(View? view, Scheduler scheduler, ViewRecordCache cache)
    {
        View = view;
        Scheduler = scheduler;
        Record = cache.Get(view?.GetType() ?? typeof(View));
        _effectDeps = new object?[Record.EffectDepFields.Count + Record.EffectDepProperties.Count];
        _effectDepChanged = new bool[Record.EffectDepFields.Count + Record.EffectDepProperties.Count];
    }

    public void OnVisit(Fiber<ViewNode> newFiber, bool operative = false)
    {
        // assume this == newFiber.Content

        var oldFiber = newFiber.Alternate;

        if (newFiber.OperativeFibers != null)
        {
            foreach (var item in newFiber.OperativeFibers)
            {
                item.Content.OnVisit(item);
            }
        }

        switch (newFiber.Tag)
        {
            // TODO delay effects to execute after traverse
            case FiberTag.Create:
                OnMount(newFiber.Parent);
                break;
            case FiberTag.Update:
                OnUpdate(oldFiber!, newFiber);
                break;
            case FiberTag.Remove:
                OnUnmount(operative ? newFiber : oldFiber!);
                break;
            case FiberTag.Insert:
                OnMove(operative ? newFiber : oldFiber!);
                break;

            case FiberTag.Idle:
            default:
                break;
        }

        newFiber.VisitComplete();
    }

    public void UpdateContext(View? newView, Fiber<ViewNode>? parentFiber)
    {
        if (newView is ContextProvider cp)
        {
            var key = cp.Value?.GetType().GetUniqueName() ?? "";
            Context = new(parentFiber?.Content.Context, new Dictionary<string, object>
            {
                [key] = newView,
            });
        }
        else
        {
            Context = new(parentFiber?.Content.Context);
        }
    }

    public void OnMount(Fiber<ViewNode>? parentFiber)
    {
        // inject variables
        // state
        Record.InitializeState(View);
        // context
        UpdateContext(View, parentFiber);
        Record.InitializeContext(View, Context);

        if (View is NativeView nativeView)
        {
            nativeView.Mount();
        }

        // calc effect deps
        // execute effects for case 1 and case 2
        Record.CompareAndCalculateEffectDependencies(View, _effectDeps, _effectDepChanged);
        Record.ExecuteMountEffects(View);

    }

    public void OnUpdate(Fiber<ViewNode> oldFiber, Fiber<ViewNode> newFiber)
    {
        // assume newFiber.Content == this
        var oldView = oldFiber.Content.View;
        var newView = newFiber.Content.View;

        // compare props
        // if props are not the same:
        // migrate new props to old ones
        Record.CompareAndMigrateProps(newView, oldView);

        // reuse the old view in all cases
        // also migrate information
        newFiber.Content.View = oldView;
        newFiber.Content._effectDeps = oldFiber.Content._effectDeps;

        // migrate & init context
        UpdateContext(newView, newFiber.Parent);
        Record.InitializeContext(newView, Context);

        if (newFiber.Content.View is NativeView nativeView)
        {
            nativeView.Update();
        }

        // compare & calc effect deps
        var hasChange = Record.CompareAndCalculateEffectDependencies(oldView, _effectDeps, _effectDepChanged);
        if (hasChange)
        {
            // execute effects for case 2
            Record.ExecuteEffects(oldView, _effectDepChanged);
        }
    }

    public void OnUnmount(Fiber<ViewNode> oldFiber)
    {
        // execute effects for case 3
        Record.ExecuteUnmountEffects(oldFiber.Content.View);

        if (oldFiber.Content.View is NativeView nativeView)
        {
            nativeView.Unmount();
        }
    }

    public void OnMove(Fiber<ViewNode> oldFiber)
    {
        if (oldFiber.Content.View is NativeView nativeView)
        {
            nativeView.Move();
        }
    }
}
