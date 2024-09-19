using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Viewify.Base;
using Viewify.Base.Native;
using View = Viewify.Base.View;

namespace Viewify.Test.WinForm;





internal class TestSubView : View
{
    [Context] public TestContext Context { get; set; } = new();
    public override View? Render() => 
        new Text(Context.TestField.ToString());

}

internal class TestContext
{
    public int TestField { get; init; } = 42;
}
internal class TestFactory : IDefaultValueFactory
{
    public object? Create() => 114514 | 1919810;
}

internal class TestView(int testValue) : View
{
    [Prop] public int TestValue = testValue;
    [State(factory: typeof(TestFactory))] public IState<int> IntState = null!;
    [State(1234)] IState<int> WeirdState = null!;
    public override View? Render() => 
        new ContextProvider(new TestContext() { TestField = ~WeirdState })
        .SetChildren(
            new TestSubView(),
            new TestSubView()
            );
    void IncrementState()
    {
        IntState %= ~IntState + 1;
    }

    [Effect(nameof(IntState))]
    void ChangeState()
    {
        WeirdState %= ~IntState;
    }
}