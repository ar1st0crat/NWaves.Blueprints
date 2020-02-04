using System;
using System.Linq;
using NAudio.Wave;
using NWaves.Filters.Base;

namespace NWaves.Blueprints.Services
{
    /// <summary>
    /// https://github.com/naudio/NAudio/blob/master/NAudioWpfDemo/EqualizationDemo/Equalizer.cs
    /// </summary>
    class AudioProcessor : ISampleProvider
    {
        private readonly ISampleProvider _sourceProvider;

        private readonly IOnlineFilter _filter;

        public WaveFormat WaveFormat => _sourceProvider.WaveFormat;

        public AudioProcessor(ISampleProvider sourceProvider, Type type)
        {
            _sourceProvider = sourceProvider;

            //var type = filter.FilterType;
            var info = type.GetConstructors()[0];
            var pinfos = info.GetParameters();
            var ctor = type.GetConstructor(pinfos.Select(p => p.ParameterType).ToArray());

            //var parameters = filter.Params.Select(p => p.Item2).ToArray();

            _filter = (IOnlineFilter)ctor.Invoke(new object[] { 44100, 0.8, 2048, 120 });// parameters);
        }

        public void Update()
        {
        }

        public int Read(float[] buffer, int offset, int count)
        {
            var samplesRead = _sourceProvider.Read(buffer, offset, count);

            for (var n = 0; n < samplesRead; n++)
            {
                buffer[offset + n] = _filter.Process(buffer[offset + n]);
            }

            return samplesRead;
        }
    }
}
