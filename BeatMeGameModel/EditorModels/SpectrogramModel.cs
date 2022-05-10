using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BeatMeGameModel.EditorModels
{
    public class SpectrogramModel
    {
        public int LowFrequency { get; set; }
        public int HighFrequency { get; set; }
        private readonly int frequencyLimit;

        public SpectrogramModel(int lowFrequency, int highFrequency, int sampleRate)
        {
            LowFrequency = lowFrequency;
            HighFrequency = highFrequency;
            frequencyLimit = sampleRate / 2;
        }

        public List<List<double>> GetNormalizedSpectrogramMap(double[][] fftData)
        {
            var selectedFrequencies = new List<List<double>>();
            foreach (var fftFrame in fftData)
            {
                var count = fftFrame.Count();
                var frequenciesPerCell = frequencyLimit / count;
                var lowLimitCell = LowFrequency / frequenciesPerCell;
                var highLimitCell = HighFrequency / frequenciesPerCell;
                var cells = fftFrame.Where(value => value >= lowLimitCell && value <= highLimitCell).ToList();
                for (int i = 0; i < cells.Count; i++)
                {
                    if(selectedFrequencies.Count < i) selectedFrequencies.Add(new List<double>());
                    selectedFrequencies[i].Add(cells[i]);
                }
            }

            var normalizedFrequencies = selectedFrequencies.Select(NormalizeList).ToList();
            return normalizedFrequencies;
        }

        public List<double> NormalizeList(List<double> values)
        {
            var maximum = values.Max();
            values = values.Select(value => value / maximum).ToList();
            return values;
        }
    }
}
