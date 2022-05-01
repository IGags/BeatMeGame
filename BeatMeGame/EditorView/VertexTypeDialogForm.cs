using System.Drawing;
using System.Windows.Forms;
using BeatMeGameModel.BeatVertexes;
using BeatMeGameModel.IOWorkers;

namespace BeatMeGame.EditorView
{
    public class VertexTypeDialogForm : Form
    {
        public VertexType OutType { get; private set; }

        public VertexTypeDialogForm(BeatDetectionType type, Form parent)
        {
            Size = new Size(parent.ClientSize.Width / 4, parent.ClientSize.Height / 4);
            Location = new Point(parent.ClientSize.Width / 2 - ClientSize.Width / 2,
                parent.ClientSize.Height / 2 - ClientSize.Height / 2);
            BackColor = Color.DarkGray;
            FormBorderStyle = FormBorderStyle.None;
            var buttonSize = new Size(3 * ClientSize.Width / 4, ClientSize.Height / 4);
            var buttonLocationConstant = new Point(Size.Width / 2 - buttonSize.Width / 2,
                ClientSize.Width / 8);

            var artificialButton = new Button()
            {
                Size = buttonSize,
                Text = "Добавочный бит",
                Location = buttonLocationConstant
            };

            var transitionButton = new Button()
            {
                Size = buttonSize,
                Text = type == BeatDetectionType.FFT? "Вершина FFT" : "Вершина BPM",
                Location = new Point(buttonLocationConstant.X, artificialButton.Bottom + ClientSize.Height / 16)
            };

            artificialButton.Click += (sender, args) =>
            {
                DialogResult = DialogResult.OK;
                OutType = VertexType.Artificial;
                Close();
            };

            transitionButton.Click += (sender, args) =>
            {
                DialogResult = DialogResult.OK;
                OutType = type == BeatDetectionType.FFT ? VertexType.FFT : VertexType.BPM;
                Close();
            };

            Controls.Add(artificialButton);
            Controls.Add(transitionButton);
        }
    }
}
