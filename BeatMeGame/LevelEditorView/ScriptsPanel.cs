using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace BeatMeGame.EditorView
{
    public sealed class ScriptsPanel : Panel
    {
        public string SelectedButton;
        public string PreviousButton;
        public event Action ButtonChanged;
        public ScriptsPanel(string[] scripts)
        {
            VScroll = true;
            AutoScroll = true;
            Redraw(scripts);
        }

        public void Redraw(string[] scripts)
        {
            Controls.Clear();
            BackColor = Color.Gray;
            var size = ClientSize;
            var buttonSize = new Size(VerticalScroll.Visible ? 13 * size.Width / 14 : size.Width, size.Height / 20);
            for (int i = 0; i < scripts.Length; i++)
            {
                var button = new Button()
                {
                    Text = scripts[i],
                    FlatStyle = FlatStyle.Flat,
                    BackColor = Color.DarkGray,
                    Size = buttonSize,
                    Location = new Point(0, i * buttonSize.Height)
                };

                button.Click += (sender, args) =>
                {
                    PreviousButton = SelectedButton;
                    SelectedButton = button.Text;
                    ButtonChanged?.Invoke();

                };
                Controls.Add(button);
            }

            SizeChanged += (sender, args) =>
            {
                for (int i = 0; i < Controls.Count; i++)
                {
                    var button = (Button)Controls[i];
                    button.Size = new Size(ClientSize.Width, ClientSize.Height / 20);
                    button.Location = new Point(0, i * button.Size.Height);
                }
            };
        }
    }
}
