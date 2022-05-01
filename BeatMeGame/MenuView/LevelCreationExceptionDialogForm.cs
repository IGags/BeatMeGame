using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace BeatMeGame
{
    public class LevelCreationExceptionDialogForm : Form
    {
        public LevelCreationExceptionDialogForm(Form invokerForm)
        {
            Size = new Size(300, 200);
            BackColor = Color.DarkGray;
            FormBorderStyle = FormBorderStyle.None;
            Location = new Point(invokerForm.Location.X + invokerForm.Width + Width / 2, 
                invokerForm.Location.Y + invokerForm.Height + Height / 2);

            var messageLabel = new Label()
            {
                Text = "Уровень с таким именем уже существует",
                Font = new Font(FontFamily.GenericSansSerif, 16),
                TextAlign = ContentAlignment.TopCenter
            };

            var okButton = new Button()
            {
                FlatStyle = FlatStyle.Flat,
                Size = new Size(70, 30),
                Location = new Point(ClientSize.Width / 2, 3 * ClientSize.Height / 4),
                Text = "ОК"
            };

            okButton.Click += (sender, args) => { Close(); };

            Load += (sender, args) =>
            {
                messageLabel.Size = ClientSize;
                messageLabel.Location = new Point(0, ClientSize.Height / 5);
            };
            
            Controls.Add(okButton);
            Controls.Add(messageLabel);
        }
    }
}
