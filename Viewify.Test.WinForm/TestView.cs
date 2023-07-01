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

internal class TestView : View
{

    public static IEnumerable<ViewRecord> TestFuncView(IViewifyInstance v, string dispData)
    {
        yield break;
    }

    public int TestValue { get; set; }

    [DefaultValueFactory(typeof(TestFactory))]
    IState<int> IntState = null!;

    public TestView(int testValue)
    {
        TestValue = testValue;
        
    }

    void IncrementState()
    {
        IntState %= (~IntState) + 1;
    }

    public override IEnumerable<ViewRecord> Render(IEnumerable<ViewRecord> children)
    {
        var intValue = ~IntState;
        return new ViewBuilder()
            .V((v, c) => TestFuncView(v, ""))
            .V(() => new TestView(114514))
            .C(b => b
                .V((v, c) => Enumerable.Empty<ViewRecord>())
            )
            .Build();
    }
}