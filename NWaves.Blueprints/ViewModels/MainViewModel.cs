using Caliburn.Micro;
using Microsoft.Win32;
using NetworkModel;
using NWaves.Blueprints.Interfaces;
using NWaves.Blueprints.Models;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace NWaves.Blueprints.ViewModels
{
    public class MainViewModel : Conductor<object>
    {
        private readonly IWindowManager _windowManager;
        private readonly IReflectionService _reflectionService;
        private IAudioService _audioService;

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


        public MainViewModel(IWindowManager windowManager,
                             IReflectionService reflectionService,
                             IAudioService audioService)
        {
            _windowManager = windowManager;
            _reflectionService = reflectionService;
            _audioService = audioService;
        }


        #region node operations

        public NodeViewModel CreateNode()
        {
            var filtersViewModel = IoC.Get<FiltersViewModel>();

            var result = _windowManager.ShowDialog(filtersViewModel);

            if (result == false)
            {
                return null;
            }

            var type = filtersViewModel.SelectedFilter.FilterType;
            
            var filter = new FilterNodeViewModel
            {
                FilterType = type,
                Parameters = _reflectionService.FilterParameters(type)
                                               .Select(name => new ParameterViewModel { Name = name, Value = 0 })
                                               .ToList()
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


        #region playback

        public void Play()
        {
            var openFileDialog = new OpenFileDialog
            {
                Filter = "All Supported Files (*.wav;*.mp3)|*.wav;*.mp3|All Files (*.*)|*.*"
            };

            var result = openFileDialog.ShowDialog();

            if (result == true)
            {
                _audioService.Load(openFileDialog.FileName);

                UpdateAudioGraph();

                _audioService.Play();
            }
        }

        public void Pause() => _audioService.Pause();

        public void Stop() => _audioService.Stop();

        public void UpdateAudioGraph()
        {
            var filters = FilterNodes.Select(f => new FilterNode
            {
                FilterType = f.FilterType,
                Parameters = f.Parameters.Select(p => p.Value).ToArray()
            });

            _audioService?.Update(filters);
        }

        #endregion


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

            var node = FilterNodes.First(f => f.NetworkNode == connectorDraggedOver.ParentNode);
            //node.Connected[]
        }

        #endregion
    }
}
