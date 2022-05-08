using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using BeatMeGame.Interfaces;
using BeatMeGame.MenuView;

namespace BeatMeGame
{
    public class MenuListPanel : Panel
    {
        public MenuListPanel(Form parent)
        {
            SetStyle(ControlStyles.SupportsTransparentBackColor, true);
            Initialize(parent);
        }

        private void Initialize(Form parent)
        {
            BorderStyle = BorderStyle.None;

            var startGameButton = new RedirectionButton(State.Play)
            {
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.DarkGray,
                Text = "Играть"
            };

            var redactorButton = new RedirectionButton(State.Editor)
            {
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.DarkGray,
                Text = "Редактор"
            };

            var settingsButton = new RedirectionButton(State.Settings)
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
                var terminatable = (ITerminatable)parent.MdiParent;
                terminatable.TerminateForm();
            };

            settingsButton.Click += (sender, args) =>
            {
                ((IStateEditor)parent).Machine.ChangeState(settingsButton);
                Dispose();
            };

            redactorButton.Click += (sender, args) =>
            {
                ((IStateEditor)parent).Machine.ChangeState(redactorButton);
                Dispose();
            };

            parent.SizeChanged += (sender, args) =>
            {
                OnSizeChanged(args);
            };

            SizeChanged += (sender, args) =>
            {
                Location = new Point(parent.ClientSize.Width / 2 - ClientSize.Width / 2, parent.ClientSize.Height / 4);
                Size = new Size(parent.ClientSize.Width / 4, parent.ClientSize.Height / 2);
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
            OnSizeChanged(EventArgs.Empty);
        }
    }
}
