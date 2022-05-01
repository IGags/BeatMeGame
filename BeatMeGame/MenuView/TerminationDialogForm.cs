using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace BeatMeGame
{
    class TerminationDialogForm : Form
    {
        public TerminationDialogForm(Form sender)
        {
            Size = new Size(500, 200);
            FormBorderStyle = FormBorderStyle.None;
            BackColor = Color.DarkGray;

            var gameClosingLabel = new Label
            {
                Font = new Font(FontFamily.GenericMonospace, 25f),
                Text = "Выйти из игры?",
                TextAlign = ContentAlignment.TopCenter
            };

            var confirmButton = new Button
            {
                Text = "Да",
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.DarkGray
            };

            confirmButton.Click += (send, args) =>
            {
                DialogResult = DialogResult.Yes;
                Close();
            };

            var denyButton = new Button
            {
                Text = "Нет",
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.DarkGray
            };

            denyButton.Click += (send, args) =>
            {
                DialogResult = DialogResult.No;
                Close();
            };

            Load += (send, args) =>
            {
                var buttonSize = new Size(ClientSize.Width / 4, ClientSize.Height / 5);
                Location = new Point(sender.Location.X + sender.Size.Width / 2 - Size.Width / 2, sender.Location.Y + sender.Size.Height / 3);
                gameClosingLabel.Size = new Size(ClientSize.Width, ClientSize.Height / 3);
                gameClosingLabel.Location = new Point(0, ClientSize.Height / 4);
                confirmButton.Location = new Point(ClientSize.Width / 6, 3 * ClientSize.Height / 4);
                confirmButton.Size = buttonSize;
                denyButton.Location = new Point(ClientSize.Width / 6 + confirmButton.Size.Width + ClientSize.Width / 6, 3 * ClientSize.Height / 4);
                denyButton.Size = buttonSize;
            };
            Controls.Add(gameClosingLabel);
            Controls.Add(confirmButton);
            Controls.Add(denyButton);
        }
    }
}
