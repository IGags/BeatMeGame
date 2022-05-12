using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace BeatMeGame.EditorView
{
    public class SpectrumCanvas : Panel
    {
        private readonly SpectrogramWrapper wrapper = new SpectrogramWrapper();
        private List<List<double>> spectrogramData;
        protected override bool DoubleBuffered { get; set; } = true;

        public void VisualizeSpectrogram(List<List<double>> spectrogramData)
        {
            this.spectrogramData = spectrogramData;
            Invalidate();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            var graphics = e.Graphics;
            wrapper.DrawSpectrogram(graphics, spectrogramData);
        }

}
}
