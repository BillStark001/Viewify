using System.ComponentModel;

namespace Viewify.Base.Render;

public sealed class Viewer : IDisposable
{

    public enum RenderStage
    {
        None,
        View,
        Node,
        Difference,
        Native
    }

    #region general

    private readonly Thread _thread;

    private Viewer()
    {

        _thread = Thread.CurrentThread;
        _exited = false;
        _stage = RenderStage.None;
    }

    public void Dispose()
    {
        _exited = true;
    }

    private void CheckThread()
    {
        if (Thread.CurrentThread != _thread)
            throw new Exception($"Illegal thread access from thread #{Environment.CurrentManagedThreadId}");
    }

    #endregion

    #region public

    private bool _exited;
    public bool Exited
    {
        get
        {
            return _exited && _thread.IsAlive;
        }
        private set
        {
            CheckThread();
            _exited = value;
        }
    }

    public void Exit()
    {
        Exited = true;
    }

    #endregion

    #region hooks


    public IState<T?> UseState<T>() => UseState(default(T));
    public IState<T> UseState<T>(T initValue)
    {
        CheckThread();
        CheckStage(RenderStage.Node);
        throw new NotImplementedException();
    }


    public void UseEffect(Action callback, params object[] watchVars)
    {
        CheckThread();
        CheckStage(RenderStage.Node);
    }

    #endregion


    #region render

    private RenderStage _stage;

    private void CheckStage(RenderStage stage = RenderStage.None)
    {
        if (stage != _stage)
            throw new Exception($"Illegal render stage: {stage}");
    }

    public void Update(in View rootView)
    {
        // dfs current node to check if it needs rerender, add needed nodes to a queue
        // rerender all needed nodes per queue
        // update native views

    }

    public static void StartRender(in View rootView)
    {

        var thread = new Thread(() =>
        {
            var viewer = new Viewer();
            while (!viewer.Exited)
            {

            }
        });
        thread.Start();
    }


    #endregion
}


// functional view related


internal class FunctionalView : View
{
    public delegate View Renderer();
    private Renderer _renderer;

    public FunctionalView(Renderer func)
    {
        _renderer = func;
    }

    public View Render() => _renderer();
}


public static class ViewerExtensions
{
    // func view
    public static View F(
        this Viewer viewer,
        Func<Viewer, View[], View> f,
        params View[] children)
    {
        return new FunctionalView(() => f(
            viewer,
            children));
    }

    public static View F<T>(
        this Viewer viewer,
        Func<Viewer, T, View[], View> f,
        T t,
        params View[] children)
    {
        return new FunctionalView(() => f(
            viewer,
            t,
            children));
    }

    public static View F<T1, T2>(
        this Viewer viewer,
        Func<Viewer, T1, T2, View[], View> f,
        T1 t1, T2 t2,
        params View[] children)
    {
        return new FunctionalView(() => f(
            viewer,
            t1, t2,
            children));
    }

    public static View F<T1, T2, T3>(
        this Viewer viewer,
        Func<Viewer, T1, T2, T3, View[], View> f,
        T1 t1, T2 t2, T3 t3,
        params View[] children)
    {
        return new FunctionalView(() => f(
            viewer,
            t1, t2, t3,
            children));
    }

    public static View F<T1, T2, T3, T4>(
    this Viewer viewer,
    Func<Viewer, T1, T2, T3, T4, View[], View> f,
    T1 t1, T2 t2, T3 t3, T4 t4,
    params View[] children)
    {
        return new FunctionalView(() => f(
            viewer,
            t1, t2, t3, t4,
            children));
    }


    // children


}