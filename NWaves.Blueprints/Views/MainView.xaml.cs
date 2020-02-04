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

        private void NetworkControl_ConnectionDragStarted(object sender, ConnectionDragStartedEventArgs e)
        {
            var draggedOutConnector = (ConnectorViewModel)e.ConnectorDraggedOut;
            var curDragPoint = Mouse.GetPosition(networkControl);

            e.Connection = (DataContext as MainViewModel).ConnectionDragStarted(draggedOutConnector, curDragPoint);
        }

        private void NetworkControl_ConnectionDragging(object sender, ConnectionDraggingEventArgs e)
        {
            var curDragPoint = Mouse.GetPosition(networkControl);
            var connection = (ConnectionViewModel)e.Connection;

            (DataContext as MainViewModel).ConnectionDragging(connection, curDragPoint);
        }

        private void NetworkControl_ConnectionDragCompleted(object sender, ConnectionDragCompletedEventArgs e)
        {
            var connectorDraggedOver = (ConnectorViewModel)e.ConnectorDraggedOver;
            var newConnection = (ConnectionViewModel)e.Connection;
            
            (DataContext as MainViewModel).ConnectionDragCompleted(newConnection, connectorDraggedOver);
        }
    }
}
