using NAudio.Wave;
using NWaves.Blueprints.Models;
using System;
using System.Collections.Generic;

namespace NWaves.Blueprints.Interfaces
{
    public interface IAudioService : ISampleProvider, IDisposable
    {
        void Play();
        void Pause();
        void Stop();

        void Load(string filename);
        void Update(IEnumerable<FilterNode> filters);
    }
}
