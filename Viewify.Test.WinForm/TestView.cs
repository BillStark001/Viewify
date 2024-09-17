using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Viewify.Base;
using Viewify.Base.Native;
using View = Viewify.Base.View;

namespace Viewify.Test.WinForm;



internal class TestFactory : IDefaultValueFactory
{
    public object? Create() => 114514 | 1919810;
}

internal class TestSubView : View
{

    [Context] public TestContext Context { get; set; } = new();

    public override View? Render()
    {
        return new Text(Context.TestField.ToString());
    }
}

internal class TestContext
{
    public int TestField { get; init; } = 0;
}

internal class TestView(int testValue) : View
{


    [Prop] public int TestValue { get; set; } = testValue;

    [DefaultStateFactory(typeof(TestFactory))] IState<int> IntState = null!;

    [DefaultState(1234)] IState<int> WeirdState = null!;

    void IncrementState()
    {
        IntState %= ~IntState + 1;
    }

    [Effect(nameof(IntState))]
    void ChangeState()
    {
        WeirdState %= ~IntState;
    }

    public override View? Render()
    {
        var intValue = ~WeirdState;
        return new ContextProvider(new TestContext() { TestField = intValue }).SetChildren(
                new TestSubView(),
                new TestSubView()
                );

    }
}