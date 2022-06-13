using System;
using System.Collections.Generic;
using System.Linq;

namespace BeatMeGameModel.IOWorkers
{
    public class SpectrumBarsModel
    {
        private const int barDestructionSize = 20;
        private List<double> spectrumState = new List<double>();

        public SpectrumBarsModel() { }

        public List<double> Update(List<double> newSpectrum)
        {
            if (spectrumState.Count == 0) return spectrumState = newSpectrum;
            for (var i = 0; i < spectrumState.Count; i++)
            {
                if (spectrumState[i] - newSpectrum[i] < newSpectrum[i] / 3 && spectrumState[i] - newSpectrum[i] > 0) spectrumState[i] = newSpectrum[i];
                else if (newSpectrum[i] - spectrumState[i] < spectrumState[i] / 3 && spectrumState[i] - newSpectrum[i] < 0) spectrumState[i] = newSpectrum[i];
                else if (newSpectrum[i] - barDestructionSize < 0) spectrumState[i] = 0;
                else if (spectrumState[i] < newSpectrum[i]) spectrumState[i] += newSpectrum[i] / 3;
                else spectrumState[i] -= spectrumState[i] / 3;
            }

            return spectrumState;
        }

        private List<double> SmoothSpectrum(List<double> initialSpectrum)
        {
            var smoothedSpectrum = new double[initialSpectrum.Count];
            for (int i = 0; i < initialSpectrum.Count; i++)
            {
                for (int j = -3; j < 4; j++)
                {
                    if(i + j < 0 || i + j >= initialSpectrum.Count) continue;
                    smoothedSpectrum[i + j] = Math.Max(smoothedSpectrum[i + j], Math.Max(initialSpectrum[i + j], 0.25 * (4 - Math.Abs(j)) * initialSpectrum[i]));
                }
            }

            return smoothedSpectrum.ToList();
        }
    }
}
