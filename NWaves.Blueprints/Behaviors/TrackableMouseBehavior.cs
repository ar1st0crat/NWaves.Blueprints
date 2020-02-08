using System.Windows;
using System.Windows.Input;
using System.Windows.Interactivity;

namespace NWaves.Blueprints.Behaviors
{
    public class TrackableMouseBehavior : Behavior<FrameworkElement>
    {
        public static readonly DependencyProperty MousePosProperty = DependencyProperty.Register(
            "MousePos", typeof(Point), typeof(TrackableMouseBehavior), new PropertyMetadata(null));

        public Point MousePos
        {
            get => (Point)GetValue(MousePosProperty);
            set => SetValue(MousePosProperty, value);
        }

        protected override void OnAttached()
        {
            AssociatedObject.MouseMove += AssociatedObjectOnMouseMove;
        }

        private void AssociatedObjectOnMouseMove(object sender, MouseEventArgs mouseEventArgs)
        {
            MousePos = mouseEventArgs.GetPosition(AssociatedObject);
        }

        protected override void OnDetaching()
        {
            AssociatedObject.MouseMove -= AssociatedObjectOnMouseMove;
        }
    }
}
