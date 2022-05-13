
using System.Drawing;
using System.Windows.Forms;

namespace BeatMeGame.EditorView
{
    public class EditorExitDialogForm : EditorDialogForm
    {
        public EditorExitDialogForm(Form parent) : base(parent)
        {
            var buttonSize = new Size(3* ClientSize.Width / 4, ClientSize.Height/ 5 );
            var formSize = ClientSize;
            var buttonConstant = ClientSize.Height / 10;

            var saveAndExitButton = new Button()
            {
                Size = buttonSize,
                Location = new Point(formSize.Width / 2 - buttonSize.Width / 2, buttonConstant),
                Text = "Сохранить и выйти",
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.DarkGray
            };

            var exitButton = new Button()
            {
                Size = buttonSize,
                Location = new Point(saveAndExitButton.Left, saveAndExitButton.Bottom + buttonConstant),
                Text = "Выйти без сохранения",
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.DarkGray
            };

            var cancelButton = new Button()
            {
                Size = buttonSize,
                Location = new Point(exitButton.Left, exitButton.Bottom + buttonConstant),
                Text = "Отмена",
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.DarkGray
            };

            saveAndExitButton.Click += (sender, args) =>
            {
                DialogResult = DialogResult.OK;
                Close();
            };

            exitButton.Click += (sender, args) =>
            {
                DialogResult = DialogResult.Yes;
                Close();
            };

            cancelButton.Click += (sender, args) =>
            {
                DialogResult = DialogResult.Cancel;
                Close();
            };

            Controls.Add(saveAndExitButton);
            Controls.Add(exitButton);
            Controls.Add(cancelButton);
        }
    }
}
