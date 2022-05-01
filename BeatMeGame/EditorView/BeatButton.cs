using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using BeatMeGameModel.BeatVertexes;

namespace BeatMeGame.EditorView
{
    public class BeatButton : Button
    {
        public BeatVertex Vertex { get; set; }
        public int Number { get; }

        public BeatButton(BeatVertex vertex, int number) : base()
        {
            Vertex = vertex;
            Number = number;
        }
    }
}
