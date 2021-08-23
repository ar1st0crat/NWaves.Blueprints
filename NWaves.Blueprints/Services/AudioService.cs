using System.Collections.Generic;
using NAudio.Wave;
using NWaves.Blueprints.Interfaces;
using NWaves.Blueprints.Models;
using NWaves.Filters.Base;

namespace NWaves.Blueprints.Services
{
    class AudioService : IAudioService
    {
        private readonly IAudioGraphBuilderService _audioGraphBuilder;
        private IOnlineFilter _filter;

        private AudioFileReader _reader;
        private WaveOutEvent _player;

        public WaveFormat WaveFormat => _reader?.WaveFormat;

        public AudioService(IAudioGraphBuilderService audioGraphBuilder)
        {
            _audioGraphBuilder = audioGraphBuilder;
        }

        public void Load(string filename)
        {
            _player?.Stop();
            _player?.Dispose();
            _reader?.Dispose();

            _reader = new AudioFileReader(filename);
            _player = new WaveOutEvent();
            _player.Init(this);
        }

        public void Update(IEnumerable<FilterNode> filters)
        {
            if (_reader is null)
            {
                return;
            }

            if (_reader.WaveFormat.Channels == 1)
            {
                _filter = _audioGraphBuilder.Build(filters);
            }
            else
            {
                _filter = new StereoFilter(_audioGraphBuilder.Build(filters), _audioGraphBuilder.Build(filters));
            }
        }

        public int Read(float[] buffer, int offset, int count)
        {
            var samplesRead = _reader.Read(buffer, offset, count);

            var channelCount = _reader.WaveFormat.Channels;

            for (var n = 0; n < samplesRead; )
            {
                for (var i = 0; i < channelCount; i++, n++)
                {
                    buffer[offset + n] = _filter.Process(buffer[offset + n]);
                }
            }

            return samplesRead;
        }

        public void Play()
        {
            _player?.Play();
        }

        public void Pause()
        {
            _player.Pause();
        }

        public void Stop()
        {
            _player?.Stop();
        }

        public void Dispose()
        {
            _player?.Dispose();
            _reader?.Dispose();
        }
    }
}
