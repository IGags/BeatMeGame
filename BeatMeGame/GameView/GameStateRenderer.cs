using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BeatMeGameModel.EitorModels;
using BeatMeGameModel.GameModels;

namespace BeatMeGame.GameView
{
    public static class GameStateRenderer
    {
        private const int GameUnitsResolution = 1000;
        public static void VisualizeGameState(IEnumerable<GameObject> gameObjects, IPlayer player, Graphics g, Dictionary<string, Bitmap> bitmaps, Bitmap playerBitmap)
        {
            RenderEditorGrid(g);
            foreach (var obj in gameObjects?.Where(obj => obj.Tag == Tag.Ship))
            {
                RenderObject(g, obj, bitmaps);
            }
            VisualizePlayer(g, player, playerBitmap);
            foreach (var obj in gameObjects?.Where(obj => obj.Tag == Tag.Bullet))
            {
                RenderObject(g, obj, bitmaps);
            }
            foreach (var obj in gameObjects?.Where(obj => obj.Tag == Tag.Laser))
            {
                RenderObject(g, obj, bitmaps);
            }
        }

        private static void RenderObject(Graphics g, GameObject obj, Dictionary<string, Bitmap> bitmaps)
        {
            var pixelsPerWidthUnit = GetPixelPerUnit(g.VisibleClipBounds.Width);
            var pixelsPerHeightUnit = GetPixelPerUnit(g.VisibleClipBounds.Height);
            var coordinates = ToGraphicsCoordinates((obj.XPosition, obj.YPosition));
            g.DrawImage(bitmaps[obj.Name], (float)(coordinates.Item1 * pixelsPerWidthUnit),
                (float)(coordinates.Item2 * pixelsPerHeightUnit), (float)(obj.XSize * pixelsPerWidthUnit),
                (float)(obj.YSize * pixelsPerHeightUnit));
        }

        private static void VisualizePlayer(Graphics g, IPlayer player, Bitmap playerBitmap)
        {
            var pixelsPerWidthUnit = GetPixelPerUnit(g.VisibleClipBounds.Width);
            var pixelsPerHeightUnit = GetPixelPerUnit(g.VisibleClipBounds.Height);
            var coordinates = ToGraphicsCoordinates((player.X, player.Y));
            g.DrawImage(playerBitmap, (float)(coordinates.Item1 - Game.PlayerSize / 2) * pixelsPerWidthUnit,
                (float)(coordinates.Item2 - Game.PlayerSize / 2) * pixelsPerHeightUnit,
                (float)Game.PlayerSize * pixelsPerWidthUnit,
                (float)Game.PlayerSize * pixelsPerHeightUnit);
        }

        private static (double, double) ToGraphicsCoordinates((double, double) cartesian)
        {
            return (cartesian.Item1, 1000 - cartesian.Item2);
        }

        private static float GetPixelPerUnit(float dimensionSize) => dimensionSize / GameUnitsResolution;

        public static void RenderEditorGrid(Graphics g)
        {
            var pixelsPerWidthUnit = GetPixelPerUnit(g.VisibleClipBounds.Width) * 25;
            var pixelsPerHeightUnit = GetPixelPerUnit(g.VisibleClipBounds.Height) * 25;
            var pen = new Pen(Color.Black, 0.1f);
            for (int i = 1; i < 40; i++)
            {
                g.DrawLine(pen, i * pixelsPerWidthUnit, 0, i * pixelsPerWidthUnit, pixelsPerHeightUnit * 40);
                g.DrawLine(pen, 0, pixelsPerHeightUnit * i , pixelsPerWidthUnit * 40, i * pixelsPerHeightUnit);
            }
        }
    }
}
