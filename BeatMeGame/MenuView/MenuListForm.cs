using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace BeatMeGame
{
    public class MenuListForm : Form
    {
        public MenuListForm(Form parent)
        {
            SetStyle(ControlStyles.SupportsTransparentBackColor, true);
            Initialize(parent);
        }

        private void Initialize(Form parent)
        {
            FormBorderStyle = FormBorderStyle.None;
            MdiParent = parent;

            var startGameButton = new Button
            {
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.DarkGray,
                Text = "Играть"
            };

            var redactorButton = new Button
            {
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.DarkGray,
                Text = "Редактор"
            };

            var settingsButton = new Button
            {
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.DarkGray,
                Text = "Настройки"
            };

            var exitButton = new Button
            {
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.DarkGray,
                Text = "Выход"
            };

            exitButton.Click += (sender, args) =>
            {
                var terminatable = (ITerminatable)parent;
                terminatable.TerminateForm();
            };

            settingsButton.Click += (sender, args) =>
            {
                var creator = (IFormCreator)parent;
                creator.CreateChildForm(new SettingsForm(parent));
                Close();
            };

            redactorButton.Click += (sender, args) =>
            {
                var creator = (IFormCreator)parent;
                creator.CreateChildForm(new MenuRedactorForm(parent));
                Close();
            };

            Load += (sender, args) =>
            {
                OnSizeChanged(args);
            };

            MdiParent.SizeChanged += (sender, args) =>
            {
                OnSizeChanged(args);
            };

            SizeChanged += (sender, args) =>
            {
                if (MdiParent == null) return;
                Location = new Point(MdiParent.ClientSize.Width / 2 - ClientSize.Width / 2, MdiParent.ClientSize.Height / 4);
                Size = new Size(MdiParent.ClientSize.Width / 4, MdiParent.ClientSize.Height / 2);
                startGameButton.Size = new Size(Size.Width, Size.Height / 4);
                startGameButton.Location = new Point(0, 0);
                redactorButton.Size = new Size(Size.Width, Size.Height / 4);
                redactorButton.Location = new Point(0, Size.Height / 4);
                settingsButton.Size = new Size(Size.Width, Size.Height / 4);
                settingsButton.Location = new Point(0, Size.Height / 2);
                exitButton.Size = new Size(Size.Width, Size.Height / 4);
                exitButton.Location = new Point(0, 3 * Size.Height / 4);
            };

            Controls.Add(startGameButton);
            Controls.Add(redactorButton);
            Controls.Add(settingsButton);
            Controls.Add(exitButton);
        }
    }
}
