using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Viewify.Base;



public interface IView
{

}

public abstract class View : IView
{



    public abstract IEnumerable<ViewRecord> Render(IEnumerable<ViewRecord> children);

    // mounting

    /// <summary>
    /// onmounted
    /// </summary>
    public virtual void AfterMount() { }


    // updating

    public virtual void PropsChanged(object prevObj) { }

    /// <summary>
    /// return false to skip render
    /// </summary>
    public virtual bool ShouldUpdate() => true;



    public virtual void HandlePrevState(object prevObj) { }

    /// <summary>
    /// onupdated
    /// </summary>
    public virtual void AfterUpdate() { }

    // unmounting

    /// <summary>
    /// onunmount
    /// </summary>
    public virtual void BeforeUnmount() { }


    // utils

}
