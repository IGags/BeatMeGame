using System;
using System.Drawing;
using System.Windows.Forms;
using BeatMeGame.Interfaces;
using BeatMeGame.MenuView;

namespace BeatMeGame
{
    class SettingsPanel : Panel
    {
        private Settings settings;
        public SettingsPanel(Form parent)
        {
            Initialize(parent);
        }

        private void Initialize(Form parent)
        {
            BackColor = Color.DarkGray;
            settings = ((IMainWindow)parent.MdiParent).GetSettings();
            var font = new Font(FontFamily.GenericMonospace, 12);

            var headerLabel = new Label
            {
                Text = "Настройки",
                Font = new Font(FontFamily.GenericMonospace, 22),
                TextAlign = ContentAlignment.TopCenter
            };

            var backButton = new RedirectionButton()
            {
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.DarkGray,
                Text = "Назад"
            };

            backButton.Click += (sender, args) =>
            {
                ((IStateEditor)parent).Machine.ChangeState((RedirectionButton)backButton);
                var setter = (IMainWindow)(parent.MdiParent);
                setter.SetSettings(settings);
                Dispose();
            };

            var musicLabel = new Label
            {
                Text = "Громкость музыки",
                Font = font
            };

            var musicVolume = new TrackBar
            {
                Maximum = 100,
                Value = settings.MusicVolume,
                TickStyle = TickStyle.None
            };

            musicVolume.Scroll += (sender, args) =>
            {
                settings.MusicVolume = musicVolume.Value;
            };

            var sfxLabel = new Label
            {
                Text = "Громкость звуков",
                Font = font
            };

            var sfxVolume = new TrackBar
            {
                Maximum = 100,
                Value = settings.SfxVolume,
                TickStyle = TickStyle.None
            };

            sfxVolume.Scroll += (sender, args) =>
            {
                settings.SfxVolume = sfxVolume.Value;
            };

            var resolutionLabel = new Label
            {
                Text = "Разрешение экрана",
                Font = font
            };

            var resolutionList = new ComboBox
            {
                DropDownStyle = ComboBoxStyle.DropDownList,
            };

            foreach (var size in SizeExtensions.DefaultSizes)
            {
                resolutionList.Items.Add(size);
            }

            resolutionList.SelectedItem = resolutionList.Items[resolutionList.Items.IndexOf(settings.FormSize)];

            resolutionList.DropDownClosed += (sender, args) =>
            {
                if (resolutionList.SelectedItem == null) return;
                settings.FormSize = (Size)resolutionList.SelectedItem;
            };

            var windowStyleLabel = new Label
            {
                Text = "Полный экран",
                Font = font
            };

            var windowStyleCheckBox = new CheckBox
            {
                Checked = settings.ScreenState == ScreenState.FullScreen
            };

            windowStyleCheckBox.CheckStateChanged += (sender, args) =>
            {
                settings.ScreenState = windowStyleCheckBox.Checked ? ScreenState.FullScreen : ScreenState.None;
            };

            parent.SizeChanged += (sender, args) =>
            {
                OnSizeChanged(args);
            };

            SizeChanged += (sender, args) =>
            {
                Location = new Point(parent.ClientSize.Width / 2 - ClientSize.Width / 2, parent.ClientSize.Height / 4);
                Size = new Size(parent.ClientSize.Width / 4, parent.ClientSize.Height / 2);
                musicVolume.Location = new Point(5 * ClientSize.Width / 12, ClientSize.Height / 6);
                musicVolume.Size = new Size(ClientSize.Width / 2, musicVolume.Size.Height);
                sfxVolume.Location = new Point(5 * ClientSize.Width / 12, musicVolume.Location.Y + musicVolume.Size.Height);
                sfxVolume.Size = new Size(ClientSize.Width / 2, musicVolume.Size.Height);
                backButton.Size = new Size(ClientSize.Width / 4, ClientSize.Height / 10);
                backButton.Location = new Point(ClientSize.Width / 2 - backButton.Size.Width / 2, 4 * ClientSize.Height / 5);
                resolutionList.Size = new Size(ClientSize.Width / 2, musicVolume.Size.Height);
                resolutionList.Location = new Point(sfxVolume.Location.X + ClientSize.Width / 125, sfxVolume.Location.Y + sfxVolume.Size.Height);
                windowStyleCheckBox.Location =
                    new Point(sfxVolume.Location.X, resolutionList.Location.Y + resolutionList.Height + ClientSize.Height / 20);
                musicLabel.Location = new Point(ClientSize.Width / 20, musicVolume.Location.Y);
                musicLabel.Size = new Size(ClientSize.Width / 2, musicLabel.Height);
                sfxLabel.Location = new Point(musicLabel.Location.X, sfxVolume.Location.Y);
                sfxLabel.Size = musicLabel.Size;
                resolutionLabel.Location = new Point(musicLabel.Location.X, resolutionList.Location.Y);
                resolutionLabel.Size = musicLabel.Size;
                windowStyleLabel.Location = new Point(musicLabel.Location.X, windowStyleCheckBox.Location.Y);
                windowStyleLabel.Size = musicLabel.Size;
                headerLabel.Size = new Size(ClientSize.Width, ClientSize.Height / 7);
                headerLabel.Location = new Point(0, ClientSize.Height / 20);
            };

            Controls.Add(backButton);
            Controls.Add(musicVolume);
            Controls.Add(sfxVolume);
            Controls.Add(resolutionList);
            Controls.Add(windowStyleCheckBox);
            Controls.Add(musicLabel);
            Controls.Add(sfxLabel);
            Controls.Add(resolutionLabel);
            Controls.Add(windowStyleLabel);
            Controls.Add(headerLabel);
            OnSizeChanged(EventArgs.Empty);
        }
    }
}

