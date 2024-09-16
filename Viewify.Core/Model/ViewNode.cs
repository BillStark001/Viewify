using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Viewify.Base;

namespace Viewify.Core.Model;

public class ViewNode
{
    public View? View { get; private set; }
    public Scheduler Scheduler { get; init; }
    public ViewRecord Record { get; init; }

    private Dictionary<string, object?> _context = [];

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

    public void OnVisit(Fiber<ViewNode> fiber)
    {
        
    }

    public void GetContextFrom(ViewNode another)
    {
        _context = new(another._context);
    }

    public void OnMount(Fiber<ViewNode> oldFiber, Fiber<ViewNode> newFiber)
    {
        var newView = newFiber.Content.View;
        // init state
        Record.InitializeState(newView);
        // migrate / init context
        // TODO

        // calc effect deps
        // execute effects for case 1 and case 2
        Record.CompareAndCalculateEffectDependencies(newView, _effectDeps, _effectDepChanged);
        Record.ExecuteMountEffects(newView);
    }

    public void OnUpdate(Fiber<ViewNode> oldFiber, Fiber<ViewNode> newFiber)
    {
        var oldView = oldFiber.Content.View;
        var newView = newFiber.Content.View;

        // compare props
        // if props are not the same:
        // migrate new props to old ones
        var hasChange = Record.CompareAndMigrateProps(newView, oldView);

        // reuse the old view in all cases
        // also migrate information
        newFiber.Content.View = oldView;
        newFiber.Content._effectDeps = oldFiber.Content._effectDeps;
        // TODO context

        // compare & calc effect deps
        hasChange = Record.CompareAndCalculateEffectDependencies(oldView, _effectDeps, _effectDepChanged);
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
    }
}
