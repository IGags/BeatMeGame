using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using BeatMeGame.EditorView;
using BeatMeGame.Interfaces;
using BeatMeGame.MenuView;
using BeatMeGameModel.IOWorkers;

namespace BeatMeGame
{
    public class MenuRedactorPanel : Panel
    {
        private List<string> levels;
        private Panel levelSelectionPanel;
        private Panel levelInformationPanel;
        private Button selectedButton;
        private Button backButton;
        private Label levelNameLabel;
        private Form parent;

        public MenuRedactorPanel(Form mdiParent)
        {
            Initialize(mdiParent);
        }

        private void Initialize(Form mdiParent)
        {
            parent = mdiParent;
            BackColor = Color.DarkGray;
            levels = LevelFolderWorker.FindLevels().ToList();

            backButton = new RedirectionButton()
            {
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.DarkGray,
                Text = "Назад"
            };

            levelSelectionPanel = new Panel()
            {
                BackColor = Color.Gray,
                AutoScroll = true
            };

            var levelCreationButton = new Button()
            {
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.Gray,
                Text = "Создать Уровень",
                TextAlign = ContentAlignment.MiddleCenter
            };

            levelInformationPanel = new Panel()
            {
                BackColor = Color.Gray,
            };

            levelCreationButton.Click += (sender, args) =>
            {
                ReconfigueLevel("Придумайте имя уровню", 
                    tuple => LevelFolderWorker.TryCreateLevelFolder(tuple.Item2));
            };

            backButton.Click += (sender, args) =>
            {
                ((IStateEditor)parent).StateMachine.ChangeState((RedirectionButton)backButton);
                Dispose();
            };

            parent.SizeChanged += (sender, args) =>
            {
                OnSizeChanged(args);
                VisualizeLevelList();
            };

            SizeChanged += (sender, args) =>
            {
                Size = new Size(7 * parent.ClientSize.Width / 8, 7 * parent.ClientSize.Height / 8);
                Location = new Point(parent.ClientSize.Width / 16, parent.ClientSize.Height / 16);
                backButton.Location = new Point(13 * ClientSize.Width / 16, 13 * ClientSize.Height / 15);
                backButton.Size = new Size(ClientSize.Width / 13, ClientSize.Height / 17);
                levelSelectionPanel.Size = new Size(ClientSize.Width / 4, 9 * ClientSize.Height / 10);
                levelCreationButton.Size = new Size(levelSelectionPanel.Width, ClientSize.Height / 10);
                levelCreationButton.Location = new Point(0, 9 * ClientSize.Height / 10);
                levelInformationPanel.Size = new Size(2 * ClientSize.Width / 3, 3 * ClientSize.Height / 4);
                levelInformationPanel.Location = new Point(levelSelectionPanel.Right + ClientSize.Width / 40,
                    ClientSize.Height / 50);
                InitializeLevelInfoPanel();
                VisualizeLevelList();
            };

            Controls.Add(backButton);
            Controls.Add(levelSelectionPanel);
            Controls.Add(levelCreationButton);
            Controls.Add(levelInformationPanel);
            OnSizeChanged(EventArgs.Empty);
        }

        private void VisualizeLevelList()
        {
            levelSelectionPanel.Controls.Clear();
            levels = LevelFolderWorker.FindLevels().ToList();
            selectedButton = null;
            for (var i = 0; i < levels.Count; i++)
            {
                var button = new Button()
                {
                    Text = levels[i],
                    Name = levels[i],
                    Size = levels.Count > 9 ? new Size(61 * ClientSize.Width / 256, levelSelectionPanel.Size.Height / 9)
                        : new Size(levelSelectionPanel.Width, levelSelectionPanel.Size.Height / 9),
                    Location = new Point(levelSelectionPanel.Location.X, i * levelSelectionPanel.ClientSize.Height / 9)
                    
                };
                
                button.Click += (sender, args) =>
                {
                    foreach (var obj in levelSelectionPanel.Controls)
                    {
                        var levelButton = (Button)obj;
                        levelButton.BackColor = Color.Gray;
                    }
                    selectedButton = button;
                    UpdateInformationPanel();
                    button.BackColor = Color.DarkGray;
                };

                levelSelectionPanel.Controls.Add(button);
            }
        }

        private void InitializeLevelInfoPanel()
        {
            levelInformationPanel.Controls.Clear();
            var deletionButton = new Button()
            {
                Text = "Удалить",
                BackColor = Color.IndianRed,
                Size = backButton.Size,
                FlatStyle = FlatStyle.Flat,
                Location = new Point(levelInformationPanel.Location.X + 7 * levelInformationPanel.Size.Width / 16, 
                    levelInformationPanel.Location.Y + 8 * levelInformationPanel.Size.Height / 10)
            };

            var editorModeButton = new Button()
            {
                Size = backButton.Size,
                FlatStyle = FlatStyle.Flat,
                Text = "Редактировать",
                Location = new Point(3 * levelInformationPanel.Size.Width / 16,
                    levelInformationPanel.Location.Y + 8 * levelInformationPanel.Size.Height / 10),
                BackColor = Color.DarkKhaki
            };

            var normalModeButton = new Button()
            {
                Size = backButton.Size,
                FlatStyle = FlatStyle.Flat,
                Text = "Играть",
                Location = new Point(1 * levelInformationPanel.Size.Width / 16,
                    levelInformationPanel.Location.Y + 8 * levelInformationPanel.Size.Height / 10),
                BackColor = Color.DarkSeaGreen
            };

            var renameButton = new Button()
            {
                Size = backButton.Size,
                FlatStyle = FlatStyle.Flat,
                Text = "Переименовать",
                Location = new Point(5 * levelInformationPanel.Size.Width / 16,
                    levelInformationPanel.Location.Y + 8 * levelInformationPanel.Size.Height / 10),
                BackColor = Color.CornflowerBlue
            };

            var levelNameLabel = new Label()
            {
                Size = new Size(levelInformationPanel.Size.Width / 2, 50),
                Font = new Font(FontFamily.GenericSansSerif, 22),
                TextAlign = ContentAlignment.MiddleCenter,
                Location = new Point(5 * levelInformationPanel.Location.X / 8, ClientSize.Height / 16)
            };

            deletionButton.Click += (sender, args) =>
            {
                if(selectedButton == null) return;
                LevelFolderWorker.DeleteLevel(selectedButton.Name);
                selectedButton = null;
                VisualizeLevelList();
                UpdateInformationPanel();
            };

            renameButton.Click += (sender, args) =>
            {
                if(selectedButton == null) return;
                ReconfigueLevel("Придумайте новое имя",
                    tuple => LevelFolderWorker.TryRenameLevel(tuple.Item1, tuple.Item2));
            };

            editorModeButton.Click += (sender, args) =>
            {
                if(selectedButton == null) return;
                ((ISoundPlayer)parent).MusicEngine.PauseTread();
                var creator = (IFormCreator)(parent.MdiParent);
                creator.ChangeScene(((IStateEditor)parent).StateMachine, new EditorLoadingForm(parent, selectedButton.Name));
            };

            this.levelNameLabel = levelNameLabel;
            levelInformationPanel.Controls.Add(deletionButton);
            levelInformationPanel.Controls.Add(levelNameLabel);
            levelInformationPanel.Controls.Add(editorModeButton);
            levelInformationPanel.Controls.Add(normalModeButton);
            levelInformationPanel.Controls.Add(renameButton);
        }

        private void UpdateInformationPanel()
        {
            levelNameLabel.Text = selectedButton == null ? "N/A" : selectedButton.Name ?? "N/A";
        }

        private void ReconfigueLevel(string text, Func<(string, string), bool> configurationFunc)
        {
            var dialogForm = new LevelCreationDialogForm(parent, text);
            var result = dialogForm.ShowDialog();
            var isCreated = true;
            var oldName = "";
            if (selectedButton != null) oldName = selectedButton.Name;
            if (result == DialogResult.Yes) isCreated = configurationFunc((oldName ?? "", dialogForm.LevelName));
            dialogForm.Close();
            if (!isCreated)
            {
                var dialog = new LevelCreationExceptionDialogForm(parent);
                dialog.ShowDialog();
            }
            VisualizeLevelList();
            UpdateInformationPanel();
        }
    }
}
