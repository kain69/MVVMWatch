using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Timers;

namespace MVVMWatch
{
    internal class Program
    {
        public static void Main(string[] args)
        {
            var view = new View(new ViewModel(new Model()));

            view.Show();
        }
    }

    internal class Model
    {
        public event Action<DateTime> TimeChanged;

        public Model()
        {
            var timer = new Timer(1000);
            timer.Elapsed += TimerOnElapsed;
            timer.Start();
        }

        private void TimerOnElapsed(object sender, ElapsedEventArgs e)
            => TimeChanged?.Invoke(e.SignalTime);
    }

    internal class ViewModel : INotifyPropertyChanged
    {
        private string _time;

        public string Time
        {
            get => _time;
            set
            {
                _time = value;
                OnPropertyChanged();
            } 
        }

        public ViewModel(Model model)
        {
            Time = "00:00:00";

            model.TimeChanged += ModelOnTimeChanged;
        }

        private void ModelOnTimeChanged(DateTime obj)
        {
            Time = obj.ToLongTimeString();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    internal class View
    {
        private object DataContext { get; }
        private string _text;

        public string Text
        {
            get => _text;
            set
            {
                _text = value;

                Update();
            }
        }

        public View(ViewModel dataContext)
        {
            DataContext = dataContext;
            var binding = new Binding("Time");
            SetBinding(nameof(Text), binding);
        }

        private void SetBinding(string dependencyPropertyName, Binding binding)
        {
            var sourceProperty = DataContext.GetType().GetProperty(binding.DataContextPropertyName);

            var targetProperty = this.GetType().GetProperty(dependencyPropertyName);

            targetProperty.SetValue(this, sourceProperty.GetValue(DataContext));

            if (DataContext is INotifyPropertyChanged notify)
            {
                notify.PropertyChanged += (s, e) =>
                {
                    if (e.PropertyName == binding.DataContextPropertyName)
                    {
                        var sourceProperty = DataContext.GetType().GetProperty(binding.DataContextPropertyName);

                        var targetProperty = this.GetType().GetProperty(dependencyPropertyName);

                        targetProperty.SetValue(this, sourceProperty.GetValue(DataContext));
                    }
                };
            }
        }

        public void Show()
        {
            Update();
            Console.ReadLine();
        }

        private void Update()
        {
            Console.Clear();
            foreach (var text in Text)
            {
                Console.ForegroundColor = text == ':'
                    ? ConsoleColor.DarkRed
                    : ConsoleColor.Cyan;
                
                Console.Write(text);
            }
        }
    }

    internal class Binding
    {
        public string DataContextPropertyName { get; }

        public Binding(string dataContextPropertyName)
        {
            DataContextPropertyName = dataContextPropertyName;
        }
    }
}
