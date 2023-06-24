using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Viewify.Base;

namespace Viewify.Core.Views;



internal class TestView : View
{


    public int TestValue { get; set; }

    public TestView(int testValue)
    {
        TestValue = testValue;
    }


    public override IEnumerable<ViewRecord> Render()
    {
        return new ViewBuilder()
            .V((v, c) => Enumerable.Empty<ViewRecord>())
            .V<TestView>(new(114514))
            .C(b => b
                .V((v, c) => Enumerable.Empty<ViewRecord>())
            )
            .Build();
    }
}