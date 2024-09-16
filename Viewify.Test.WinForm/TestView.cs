using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Viewify.Base;
using View = Viewify.Base.View;

namespace Viewify.Core.Views;



internal class TestFactory : IDefaultValueFactory
{
    public object? Create() => 114514 | 1919810;
}

internal class TestSubView : View
{
    public override View? Render()
    {
        return null;
    }
}

internal class TestView : View
{


    [Prop] public int TestValue { get; set; }

    [DefaultStateFactory(typeof(TestFactory))] IState<int> IntState = null!;

    [DefaultState(1234)] IState<int> WeirdState = null!;

    public TestView(int testValue)
    {
        TestValue = testValue;

    }

    void IncrementState()
    {
        IntState %= (~IntState) + 1;
    }

    [Effect(nameof(IntState))]
    void ChangeState()
    {
        WeirdState %= (~IntState);
    }

    public override View? Render()
    {
        var intValue = ~IntState;
        return new Fragment().SetChildren(
                new TestSubView(),
                new TestSubView()
                );

    }
}