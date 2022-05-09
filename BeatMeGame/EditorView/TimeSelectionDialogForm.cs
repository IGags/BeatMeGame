using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace BeatMeGame.EditorView
{
    public class TimeSelectionDialogForm : EditorDialogForm
    {
        public int StartSecond { get; private set; }

        public TimeSelectionDialogForm(Form parent, TimeSpan maxValue, int value) : base(parent)
        {
            var formClientSize = ClientSize;

            var formNameLabel = new Label()
            {
                Text = "Введите время старта",
                Size = new Size(formClientSize.Width, formClientSize.Height / 5),
                Location = new Point(0, formClientSize.Height / 4),
                TextAlign = ContentAlignment.MiddleCenter,
                Font = new Font(FontFamily.GenericSansSerif, 20)
            };

            var timeTextBox = new TextBox()
            {
                Size = new Size(formClientSize.Width / 4, formClientSize.Height / 5),
                Location = new Point(formClientSize.Width / 4 - 5, formNameLabel.Bottom + 5),
                Text = value.ToString(),
                BackColor = Color.DarkSeaGreen
            };

            var confirmButton = new Button()
            {
                Size = timeTextBox.Size,
                Text = "Подтвердить",
                Location = new Point(timeTextBox.Right + 10, timeTextBox.Top)
            };

            var warningLabel = new Label()
            {
                Text = "Внимание, изменение времени удалит все предыдущие вершины",
                TextAlign = ContentAlignment.MiddleCenter,
                Size = new Size(formClientSize.Width, formClientSize.Height / 5),
                Location = new Point(0, confirmButton.Bottom + 5),
                ForeColor = Color.Gray
            };

            timeTextBox.TextChanged += (sender, args) =>
            {
                if (!ValidateText(timeTextBox.Text, maxValue))
                {
                    confirmButton.Enabled = false;
                    timeTextBox.BackColor = Color.IndianRed;
                }
                else
                {
                    confirmButton.Enabled = true;
                    timeTextBox.BackColor = Color.DarkSeaGreen;
                }
            };

            confirmButton.Click += (sender, args) =>
            {
                DialogResult = DialogResult.OK;
                StartSecond = int.Parse(timeTextBox.Text);
                Close();
            };

            Controls.Add(formNameLabel);
            Controls.Add(timeTextBox);
            Controls.Add(confirmButton);
            Controls.Add(warningLabel);
        }

        public bool ValidateText(string text, TimeSpan totalTime)
        {
            if (!int.TryParse(text, out var time)) return false;
            return time >= 0 && !(time > totalTime.TotalSeconds);
        }
    }
}
