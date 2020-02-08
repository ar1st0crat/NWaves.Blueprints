using System.Windows;
using System.Windows.Input;
using NetworkModel;
using NetworkUI;
using NWaves.Blueprints.ViewModels;

namespace NWaves.Blueprints.Views
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainView : Window
    {
        public MainView()
        {
            InitializeComponent();
        }

        /*
            Unfortunately, this MVVM-friendly stuff didn't work with NetworkView:  ((
    
            cal:Message.Attach="[Event ConnectionDragStarted] = [Action ConnectionDragStarted($eventArgs)];
                                [Event ConnectionDragging] = [Action ConnectionDragging($eventArgs)];
                                [Event ConnectionDragCompleted] = [Action ConnectionDragCompleted($eventArgs)]"
        */

        private void NetworkControl_ConnectionDragStarted(object sender, ConnectionDragStartedEventArgs e)
        {
            var draggedOutConnector = (ConnectorViewModel)e.ConnectorDraggedOut;
            var dragPoint = Mouse.GetPosition(networkControl);

            var context = DataContext as MainViewModel;
            e.Connection = context.ConnectionDragStarted(draggedOutConnector, dragPoint);
        }

        private void NetworkControl_ConnectionDragging(object sender, ConnectionDraggingEventArgs e)
        {
            var dragPoint = Mouse.GetPosition(networkControl);
            var connection = (ConnectionViewModel)e.Connection;

            var context = DataContext as MainViewModel;
            context.ConnectionDragging(connection, dragPoint);
        }

        private void NetworkControl_ConnectionDragCompleted(object sender, ConnectionDragCompletedEventArgs e)
        {
            var connectorDraggedOver = (ConnectorViewModel)e.ConnectorDraggedOver;
            var newConnection = (ConnectionViewModel)e.Connection;

            var context = DataContext as MainViewModel;
            context.ConnectionDragCompleted(newConnection, connectorDraggedOver);
        }
    }
}
