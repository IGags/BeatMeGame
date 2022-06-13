using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace BeatMeGame.LevelEditorView
{
    public class ScriptCreationDialogForm : Form
    {
        public string ScriptName;
        private bool isValidName;
        private readonly string[] scripts;
        public ScriptCreationDialogForm(string[] scripts)
        {
            this.scripts = scripts;
            BackColor = Color.Gray;
            Size = new Size(300, 200);
            FormBorderStyle = FormBorderStyle.FixedDialog;

            var scriptNameTextBox = new TextBox()
            {
                Location = new Point(0, 70),
                Size = new Size(300, 50),
                BackColor = Color.IndianRed
            };

            var confirmButton = new Button()
            {
                Text = "OK",
                Size = new Size(70, 50),
                Location = new Point(115, scriptNameTextBox.Bottom + 10),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.DarkGray
            };

            scriptNameTextBox.TextChanged += (sender, args) =>
            {
                isValidName = CheckTextValid(scriptNameTextBox.Text);
                scriptNameTextBox.BackColor = isValidName ? Color.DarkSeaGreen : Color.IndianRed;
                ScriptName = scriptNameTextBox.Text;
            };

            confirmButton.Click += (sender, args) =>
            {
                if(!isValidName) return;
                DialogResult = DialogResult.OK;
                ScriptName = scriptNameTextBox.Text;
            };

            Controls.Add(scriptNameTextBox);
            Controls.Add(confirmButton);
        }

        public bool CheckTextValid(string text)
        {
            if (!text.Any()) return false;
            if (scripts.Contains(text)) return false;

            foreach (var symbol in text)
            {
                if (symbol < 48 || (symbol > 57 && 65 > symbol) || (symbol > 90 && symbol < 97) || symbol > 122) return false;
            }

            return true;
        }
    }
}
