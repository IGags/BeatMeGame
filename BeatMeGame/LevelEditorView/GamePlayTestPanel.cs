using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using BeatMeGame.GameView;
using BeatMeGameModel.EitorModels;
using BeatMeGameModel.GameModels;

namespace BeatMeGame.MenuView
{
    public class GamePlayTestPanel : Panel
    {
        public Dictionary<string, Bitmap> Bitmaps { get; set; }
        public Bitmap PlayerBitmap { get; set; }

        private IEnumerable<GameObject> objects;
        private IPlayer player;
        public GamePlayTestPanel()
        {
            DoubleBuffered = true;
        }

        public void Redraw(IEnumerable<GameObject> objects, IPlayer player)
        {
            this.player = player;
            this.objects = objects;
            Invalidate();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            e.Graphics.InterpolationMode = InterpolationMode.NearestNeighbor;
            e.Graphics.CompositingQuality = CompositingQuality.HighSpeed;
            e.Graphics.SmoothingMode = SmoothingMode.None;
            if(Bitmaps != null) GameStateRenderer.VisualizeGameState(objects, player, e.Graphics, Bitmaps, PlayerBitmap);
            else GameStateRenderer.RenderEditorGrid(e.Graphics);
            base.OnPaint(e);
        }
    }

}
