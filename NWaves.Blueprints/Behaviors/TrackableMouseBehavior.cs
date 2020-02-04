using System.Windows;
using System.Windows.Input;
using System.Windows.Interactivity;

namespace NWaves.Blueprints.Behaviors
{
    public class TrackableMouseBehavior : Behavior<FrameworkElement>
    {
        public static readonly DependencyProperty MouseYProperty = DependencyProperty.Register(
            "MouseY", typeof(double), typeof(TrackableMouseBehavior), new PropertyMetadata(default(double)));

        public double MouseY
        {
            get { return (double)GetValue(MouseYProperty); }
            set { SetValue(MouseYProperty, value); }
        }

        public static readonly DependencyProperty MouseXProperty = DependencyProperty.Register(
            "MouseX", typeof(double), typeof(TrackableMouseBehavior), new PropertyMetadata(default(double)));

        public double MouseX
        {
            get { return (double)GetValue(MouseXProperty); }
            set { SetValue(MouseXProperty, value); }
        }

        protected override void OnAttached()
        {
            AssociatedObject.MouseMove += AssociatedObjectOnMouseMove;
        }

        private void AssociatedObjectOnMouseMove(object sender, MouseEventArgs mouseEventArgs)
        {
            var pos = mouseEventArgs.GetPosition(AssociatedObject);
            MouseX = pos.X - 10;
            MouseY = pos.Y - 10;
        }

        protected override void OnDetaching()
        {
            AssociatedObject.MouseMove -= AssociatedObjectOnMouseMove;
        }
    }
}
