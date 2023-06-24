using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Viewify.Base.Data;

namespace Viewify.Base;

public class StateObject : INotifyPropertyChanged
{

    class State<T> : IState<T>
    {
        private T _value;
        private string? _name;
        private StateObject _obj;

        public State(StateObject obj, string? name, T value)
        {
            _obj = obj;
            _name = name;
            _value = value;
        }

        public T Get()
        {
            return _value;
        }

        public void Set(T value)
        {
            _value = value;
            _obj?.PropertyChanged?.Invoke(this, new(_name ?? ""));
        }
    }

    public IState<T?> UseState<T>() => UseState(default(T));
    public IState<T> UseState<T>(T initValue, string? name = null)
    {
        return new State<T>(this, name, initValue);
    }


    public event PropertyChangedEventHandler? PropertyChanged;

    
}
