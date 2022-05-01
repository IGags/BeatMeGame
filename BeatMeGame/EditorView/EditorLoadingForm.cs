using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using BeatMeGameModel;
using BeatMeGameModel.IOWorkers;
namespace BeatMeGame.EditorView
{
    public class EditorLoadingForm : Form
    {
        private int pointCount;
        private Timer timer;
        private LevelSave saveData;
        private Form parent;

        public EditorLoadingForm(Form parent, string levelName)
        {
            Initialize(parent);
            timer.Enabled = true;
            ParseLevelSave(levelName);
        }

        private void Initialize(Form parent)
        {
            DoubleBuffered = true;
            BackColor = Color.Black;
            this.parent = MdiParent = parent;
            FormBorderStyle = FormBorderStyle.None;

            var loadingLabel = new Label()
            {
                Text = "Редактор Загружается",
                TextAlign = ContentAlignment.MiddleCenter,
                Font = new Font(FontFamily.GenericSansSerif, 22),
                ForeColor = Color.AliceBlue
            };

            timer = new Timer()
            {
                Interval = 200
            };

            timer.Tick += (sender, args) =>
            {
                ++pointCount;
                if (pointCount > 3) pointCount = 0;
                var dotString = new string('.', pointCount);
                loadingLabel.Text = "Редактор Загружается" + dotString;
            };

            Load += (sender, args) =>
            {
                Location = Point.Empty;
                Size = new Size(MdiParent.ClientSize.Width - 4, MdiParent.ClientSize.Height - 4);
                loadingLabel.Location = new Point(0, ClientSize.Height / 2);
                loadingLabel.Size = new Size(ClientSize.Width, ClientSize.Height / 16);
            };
            Controls.Add(loadingLabel);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            var graphics = e.Graphics;
            var random = new Random();
            for (int i = 0; i < 500; i++)
            {
                var brush = new SolidBrush(Color.FromArgb(random.Next(255), 255, 255, 255));
                graphics.FillRectangle(brush, random.Next(ClientSize.Width), random.Next(ClientSize.Height), 3, 3);
            }
        }

        private void ParseLevelSave(string levelName)
        {
            var worker = new BackgroundWorker();
            worker.DoWork += (sender, args) => args.Result = LevelSaveParsers.TryParseLevelSave(levelName, LaunchType.Redactor);
            worker.RunWorkerCompleted += (sender, args) =>
            {
                saveData = (LevelSave)args.Result;
                OpenEditorForm(saveData);
            };
            worker.RunWorkerAsync();
        }

        private void OpenEditorForm(LevelSave saveData)
        {
            var creator = (IMainWindow)MdiParent;
            Close();
            creator.RunEditor(saveData);
            Dispose();
        }
    }
}
