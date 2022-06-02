using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using BeatMeGameModel.IOWorkers;

namespace BeatMeGame.EditorView
{
    public class EditorSettingsForm : Form
    {
        public EditorSettingsForm(EditorSettings settings)
        {
            FormBorderStyle = FormBorderStyle.None;
            BackColor = Color.Gray;

            var textSizeLabel = new Label()
            {
                Text = "Размер шрифта в редакторе кода",
                Font = new Font(FontFamily.GenericMonospace, 14),
                TextAlign = ContentAlignment.MiddleCenter
            };

            var textSizeTextBox = new TextBox()
            {
                Text = settings.TextSize.ToString(),
                BackColor = Color.DarkSeaGreen
            };

            var exitButton = new Button()
            {
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.DarkGray,
                Text = "Применить"
            };

            textSizeTextBox.TextChanged += (sender, args) =>
            {
                if (int.TryParse(textSizeTextBox.Text, out var result) && result > 0)
                {
                    settings.TextSize = result;
                    textSizeTextBox.BackColor = Color.DarkSeaGreen;
                }
                else
                {
                    textSizeTextBox.BackColor = Color.IndianRed;
                }
            };

            exitButton.Click += (sender, args) =>
            {
                DialogResult = DialogResult.OK;
                Close();
            };

            Load += (sender, args) =>
            {
                Size = new Size(350, 450);
                Location = new Point(Screen.PrimaryScreen.Bounds.Size.Width / 3,
                    Screen.PrimaryScreen.Bounds.Size.Height / 3);
                var margin = Size.Height / 15;
                textSizeLabel.Size = new Size(ClientSize.Width / 2, ClientSize.Height / 8);
                textSizeLabel.Location = new Point(0, margin);
                textSizeTextBox.Size = textSizeLabel.Size;
                textSizeTextBox.Location = new Point(textSizeLabel.Right, textSizeLabel.Top);
                exitButton.Size = new Size(ClientSize.Width / 3, ClientSize.Height / 8);
                exitButton.Location = new Point(ClientSize.Width / 3, ClientSize.Height - exitButton.Height - margin);
            };

            Controls.Add(textSizeLabel);
            Controls.Add(textSizeTextBox);
            Controls.Add(exitButton);
        }
    }
}
