using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace BeatMeGame
{
    public class LevelCreationDialogForm : Form
    {
        public string LevelName { get; private set; }
        private bool isValidName = false;

        public LevelCreationDialogForm(Form sender, string text)
        {
            Initialize(sender, text);
        }

        private void Initialize(Form sender, string title)
        {
            BackColor = Color.DarkGray;
            Size = new Size(500, 200);
            FormBorderStyle = FormBorderStyle.None;

            var formNameLabel = new Label()
            {
                Text = title,
                Font = new Font(FontFamily.GenericSansSerif, 16),
                TextAlign = ContentAlignment.TopCenter
            };

            var levelNameTextBox = new TextBox()
            { };

            var okButton = new Button()
            {
                Text = "ОК"
            };

            var cancelButton = new Button()
            {
                Text = "Отмеа"
            };

            okButton.Click += (send, args) =>
            {
                DialogResult = DialogResult.Yes;
                Close();
            };

            cancelButton.Click += (send, args) =>
            {
                Close();
            };

            levelNameTextBox.TextChanged += (send, args) =>
            {
                isValidName = CheckTextValid(levelNameTextBox.Text);
                levelNameTextBox.BackColor = isValidName ? Color.DarkSeaGreen : Color.IndianRed;
                LevelName = levelNameTextBox.Text;
            };

            Load += (send, args) => { OnSizeChanged(args); };

            SizeChanged += (send, args) =>
            {
                var buttonSize = new Size(ClientSize.Width / 4, ClientSize.Height / 5);
                Location = new Point(sender.Location.X + sender.Size.Width / 2 - Size.Width / 2, 
                    sender.Location.Y + sender.Size.Height / 2 - Size.Height / 2);
                formNameLabel.Size = new Size(3 * ClientSize.Width / 4, ClientSize.Height / 5);
                formNameLabel.Location = new Point(ClientSize.Width / 2 - formNameLabel.ClientSize.Width / 2,
                    ClientSize.Height / 5);
                levelNameTextBox.Location = new Point(formNameLabel.Left, formNameLabel.Bottom);
                levelNameTextBox.Size = new Size(formNameLabel.Width, levelNameTextBox.Height);
                okButton.Location = new Point(ClientSize.Width / 6, 3 * ClientSize.Height / 4);
                cancelButton.Location = new Point(ClientSize.Width / 6 + cancelButton.Size.Width + ClientSize.Width / 6, 
                    3 * ClientSize.Height / 4);
                okButton.Size = cancelButton.Size = buttonSize;
            };
            
            Controls.Add(levelNameTextBox);
            Controls.Add(formNameLabel);
            Controls.Add(okButton);
            Controls.Add(cancelButton);
        }

        public static bool CheckTextValid(string text)
        {
            if (!text.Any()) return false;

            foreach (var symbol in text)
            {
                if (symbol < 48 || (symbol > 57 && 65 > symbol) || (symbol > 90 && symbol < 97) || symbol > 122) return false;
            }

            return true;
        }
    }
}
