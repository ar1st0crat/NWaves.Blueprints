using Caliburn.Micro;
using System;

namespace NWaves.Blueprints.ViewModels
{
    public class ParameterViewModel : PropertyChangedBase
    {
        public string Name { get; set; }
        public Type Type { get; set; }

        private object _value;
        public object Value
        {
            get => _value;
            set
            {
                _value = value;
                NotifyOfPropertyChange(() => Value);
            }
        }
    }
}
