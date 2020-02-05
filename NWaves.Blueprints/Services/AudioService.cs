using System;
using System.Collections.Generic;
using System.Linq;
using NAudio.Wave;
using NWaves.Blueprints.Interfaces;
using NWaves.Blueprints.Models;

namespace NWaves.Blueprints.Services
{
    class AudioService : IAudioService
    {
        private readonly IAudioGraphBuilderService _audioGraphBuilder;

        private AudioFileReader _reader;
        private WaveOutEvent _player;

        /// <summary>
        /// There may be several channels
        /// </summary>
        private Func<float, float>[] _processFuncs;

        public WaveFormat WaveFormat => _reader.WaveFormat;

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
            _processFuncs = Enumerable
                .Range(0, _reader.WaveFormat.Channels)
                .Select(_ => _audioGraphBuilder.Build(filters))
                .ToArray();
        }

        public int Read(float[] buffer, int offset, int count)
        {
            var samplesRead = _reader.Read(buffer, offset, count);

            var channelCount = _reader.WaveFormat.Channels;

            for (var n = 0; n < samplesRead; )
            {
                for (var i = 0; i < channelCount; i++, n++)
                {
                    buffer[offset + n] = _processFuncs[i](buffer[offset + n]);
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
