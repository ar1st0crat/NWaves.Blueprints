using Caliburn.Micro;
using Microsoft.Win32;
using NetworkModel;
using NWaves.Blueprints.Interfaces;
using NWaves.Blueprints.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace NWaves.Blueprints.ViewModels
{
    public class MainViewModel : Conductor<object>
    {
        private readonly IWindowManager _windowManager;
        private readonly IReflectionService _reflectionService;
        private readonly ISerializationService _serializationService;
        private readonly IAudioService _audioService;

        private List<FilterNode> _filterNodes = new List<FilterNode>();

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

        public List<FilterNodeViewModel> FilterNodeViews { get; set; } = new List<FilterNodeViewModel>();

        public Point MousePosition { get; set; }

        private bool _isPaused;


        public MainViewModel(IWindowManager windowManager,
                             IReflectionService reflectionService,
                             ISerializationService serializationService,
                             IAudioService audioService)
        {
            _windowManager = windowManager;
            _reflectionService = reflectionService;
            _serializationService = serializationService;
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

            var node = new NodeViewModel(type.Name)
            {
                X = MousePosition.X,
                Y = MousePosition.Y
            };
            node.Connectors.Add(new ConnectorViewModel());
            node.Connectors.Add(new ConnectorViewModel());

            var filterViewModel = new FilterNodeViewModel
            {
                NetworkNode = node,
                Parameters = new BindableCollection<ParameterViewModel>(
                    _reflectionService.GetFilterParameters(type)
                                      .Select(p => new ParameterViewModel
                                      {
                                          Name = p.Name,
                                          Value = p.Value == DBNull.Value ? 0 : p.Value,
                                          Type = p.Type
                                      }))
            };

            var filter = new FilterNode
            {
                FilterType = type,
                Parameters = _reflectionService.GetFilterParameters(type)
            };

            FilterNodeViews.Add(filterViewModel);
            _filterNodes.Add(filter);
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

            for (var i = 0; i < _filterNodes.Count; i++)
            {
                if (FilterNodeViews[i].NetworkNode == node)
                {
                    RemoveConnections(_filterNodes[i]);

                    _filterNodes.RemoveAt(i);
                    FilterNodeViews.RemoveAt(i);
                    break;
                }
            }
        }

        private void RemoveConnections(FilterNode filter)
        {
            foreach (var filterNode in _filterNodes)
            {
                if (filterNode.Nodes != null && filterNode.Nodes.Any(f => f == filter))
                {
                    filterNode.Nodes = null;
                }
            }
        }

        #endregion


        #region playback and audio graph

        public void Play()
        {
            if (_isPaused)
            {
                _audioService.Play();
                _isPaused = false;
                return;
            }

            var openFileDialog = new OpenFileDialog
            {
                Filter = "All Supported Files (*.wav;*.mp3)|*.wav;*.mp3|All Files (*.*)|*.*"
            };

            if (openFileDialog.ShowDialog() != true)
            {
                return;
            }

            _audioService.Load(openFileDialog.FileName);

            UpdateAudioGraph();

            _audioService.Play();
        }

        public void Pause()
        {
            _audioService.Pause();
            _isPaused = true;
        }

        public void Stop()
        {
            _audioService.Stop();
        }

        public void UpdateAudioGraph()
        {
            if (_audioService.WaveFormat != null)
            {
                foreach (var node in FilterNodeViews)
                {
                    var sampleRateParameter = node.Parameters.FirstOrDefault(p => p.Name == "samplingRate");

                    if (sampleRateParameter != null)
                    {
                        sampleRateParameter.Value = _audioService.WaveFormat.SampleRate;
                    }
                }
            }

            UpdateFiltersParameters();

            _audioService.Update(_filterNodes);
        }

        private void UpdateFiltersParameters()
        {
            for (var i = 0; i < _filterNodes.Count; i++)
            {
                for (var j = 0; j < _filterNodes[i].Parameters.Length; j++)
                {
                    _filterNodes[i].Parameters[j].Value = FilterNodeViews[i].Parameters[j].Value;
                }
            }
        }

        #endregion


        #region serialization

        public void Load()
        {
            var openFileDialog = new OpenFileDialog
            {
                Filter = "Json Files (*.json)|*.json|All Files (*.*)|*.*"
            };

            if (openFileDialog.ShowDialog() != true)
            {
                return;
            }

            _filterNodes = _serializationService.Deserialize(openFileDialog.FileName);


            // update view models and network view:

            Network.Nodes.Clear();
            Network.Connections.Clear();
            FilterNodeViews.Clear();

            var PosX = 50;
            var PosY = 50;

            const int StepX = 180;

            foreach (var filter in _filterNodes)
            {
                var type = filter.FilterType;

                var node = new NodeViewModel(type.Name)
                {
                    X = PosX,
                    Y = PosY
                };
                
                PosX += StepX;

                node.Connectors.Add(new ConnectorViewModel());
                node.Connectors.Add(new ConnectorViewModel());

                var filterViewModel = new FilterNodeViewModel
                {
                    NetworkNode = node,
                    Parameters = new BindableCollection<ParameterViewModel>(
                                    filter.Parameters
                                          .Select(p => new ParameterViewModel
                                          {
                                              Name = p.Name,
                                              Value = p.Value ?? 0,
                                              Type = p.Type
                                          }))
                };

                FilterNodeViews.Add(filterViewModel);
                Network.Nodes.Add(node);
            }

            // update connections in network view:

            for (var srcIndex = 0; srcIndex < _filterNodes.Count; srcIndex++)
            {
                var node = _filterNodes[srcIndex];

                if (node.Nodes == null) continue;

                var destNode = node.Nodes[0];
                var destIndex = -1;

                while (_filterNodes[++destIndex] != destNode) ;

                Network.Connections.Add(
                        new ConnectionViewModel
                        {
                            SourceConnector = Network.Nodes[srcIndex].Connectors[1],
                            DestConnector = Network.Nodes[destIndex].Connectors[0]
                        });
            }
        }

        public void Save()
        {
            var saveFileDialog = new SaveFileDialog
            {
                Filter = "Json Files (*.json)|*.json|All Files (*.*)|*.*"
            };

            if (saveFileDialog.ShowDialog() != true)
            {
                return;
            }

            UpdateFiltersParameters();

            _serializationService.Serialize(saveFileDialog.FileName, _filterNodes);
        }

        #endregion


        #region dragging connections in NetworkView

        public ConnectionViewModel ConnectionDragStarted(ConnectorViewModel draggedOutConnector, Point dragPoint)
        {
            if (draggedOutConnector.AttachedConnection != null)
            {
                var destNode = draggedOutConnector.AttachedConnection.DestConnector.ParentNode;

                for (var i = 0; i < FilterNodeViews.Count; i++)
                {
                    if (FilterNodeViews[i].NetworkNode == destNode)
                    {
                        RemoveConnections(_filterNodes[i]);
                        break;
                    }
                }

                Network.Connections.Remove(draggedOutConnector.AttachedConnection);
            }

            var connection = new ConnectionViewModel
            {
                SourceConnector = draggedOutConnector,
                DestConnectorHotspot = dragPoint
            };

            Network.Connections.Add(connection);

            return connection;
        }

        public void ConnectionDragging(ConnectionViewModel connection, Point dragPoint)
        {
            connection.DestConnectorHotspot = dragPoint;
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
                var destNode = existingConnection.DestConnector.ParentNode;

                for (var i = 0; i < FilterNodeViews.Count; i++)
                {
                    if (FilterNodeViews[i].NetworkNode == destNode)
                    {
                        RemoveConnections(_filterNodes[i]);
                        break;
                    }
                }

                Network.Connections.Remove(existingConnection);
            }

            newConnection.DestConnector = connectorDraggedOver;


            // correctly connect filter nodes (underlying models)

            var srcIndex = 0;
            var destIndex = 0;

            for (var i = 0; i < FilterNodeViews.Count; i++)
            {
                if (FilterNodeViews[i].NetworkNode == newConnection.SourceConnector.ParentNode)
                {
                    srcIndex = i;
                    break;
                }
            }
            for (var i = 0; i < FilterNodeViews.Count; i++)
            {
                if (FilterNodeViews[i].NetworkNode == connectorDraggedOver.ParentNode)
                {
                    destIndex = i;
                    break;
                }
            }

            if (_filterNodes[srcIndex].Nodes == null)
            {
                _filterNodes[srcIndex].Nodes = new List<FilterNode>();
            }

            _filterNodes[srcIndex].Nodes.Add(_filterNodes[destIndex]);
        }

        #endregion
    }
}
