using Caliburn.Micro;
using Microsoft.Win32;
using NAudio.Wave;
using NetworkModel;
using NWaves.Blueprints.Services;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;

namespace NWaves.Blueprints.ViewModels
{
    public class MainViewModel : Conductor<object>
    {
        private readonly IWindowManager _windowManager;

        private NetworkViewModel _network = new NetworkViewModel();
        public NetworkViewModel Network
        {
            get => _network;
            set
            {
                _network = value;
                NotifyOfPropertyChange(() => Network);
            }
        }

        public List<FilterNodeViewModel> FilterNodes { get; set; } = new List<FilterNodeViewModel>();

        public int MouseX { get; set; }
        public int MouseY { get; set; }


        public MainViewModel(IWindowManager windowManager)
        {
            _windowManager = windowManager;
        }


        #region node operations

        public NodeViewModel CreateNode()
        {
            var filtersViewModel = new FiltersViewModel();

            bool? result = _windowManager.ShowDialog(filtersViewModel);

            if (result == false)
            {
                return null;
            }

            var type = filtersViewModel.SelectedFilter.FilterType;
            var info = type.GetConstructors()[0];

            var filter = new FilterNodeViewModel
            {
                FilterType = type,
                Parameters = new ObservableCollection<ParameterViewModel>(
                    info.GetParameters().Select(p => new ParameterViewModel { Name = p.Name, Value = 0 }))
            };
            
            var node = new NodeViewModel(filter.FilterType.Name)
            {
                X = MouseX,
                Y = MouseY
            };
            node.Connectors.Add(new ConnectorViewModel());
            node.Connectors.Add(new ConnectorViewModel());
            node.Connectors.Add(new ConnectorViewModel());
            node.Connectors.Add(new ConnectorViewModel());

            filter.NetworkNode = node;

            FilterNodes.Add(filter);
            Network.Nodes.Add(node);

            return node;
        }

        public void DeleteSelectedNodes()
        {
            var nodesCopy = Network.Nodes.ToArray();

            foreach (var node in nodesCopy)
            {
                if (node.IsSelected)
                {
                    DeleteNode(node);
                }
            }
        }

        public void DeleteNode(NodeViewModel node)
        {
            Network.Connections.RemoveRange(node.AttachedConnections);
            Network.Nodes.Remove(node);

            var filterNode = FilterNodes.First(f => f.NetworkNode == node);
            FilterNodes.Remove(filterNode);
        }

        #endregion


        public void Play()
        {
            var openFileDialog = new OpenFileDialog
            {
                Filter = "All Supported Files (*.wav;*.mp3)|*.wav;*.mp3|All Files (*.*)|*.*"
            };

            bool? result = openFileDialog.ShowDialog();

            if (result.HasValue && result.Value)
            {
                var selectedFile = openFileDialog.FileName;
                var reader = new AudioFileReader(selectedFile);
                var equalizer = new AudioProcessor(reader, FilterNodes[0].FilterType);
                var player = new WaveOutEvent();
                player.Init(equalizer);
                player.Play();

                //player?.Dispose();
                //reader?.Dispose();
            }
        }

        #region dragging connections

        public ConnectionViewModel ConnectionDragStarted(ConnectorViewModel draggedOutConnector, Point curDragPoint)
        {
            if (draggedOutConnector.AttachedConnection != null)
            {
                Network.Connections.Remove(draggedOutConnector.AttachedConnection);
            }

            var connection = new ConnectionViewModel
            {
                SourceConnector = draggedOutConnector,
                DestConnectorHotspot = curDragPoint
            };

            Network.Connections.Add(connection);

            return connection;
        }

        public void ConnectionDragging(ConnectionViewModel connection, Point curDragPoint)
        {
            connection.DestConnectorHotspot = curDragPoint;
        }

        public void ConnectionDragCompleted(ConnectionViewModel newConnection, ConnectorViewModel connectorDraggedOver)
        {
            if (connectorDraggedOver == null)
            {
                Network.Connections.Remove(newConnection);
                return;
            }

            var existingConnection = connectorDraggedOver.AttachedConnection;
            if (existingConnection != null)
            {
                Network.Connections.Remove(existingConnection);
            }

            newConnection.DestConnector = connectorDraggedOver;
        }

        #endregion
    }
}
