using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace BeatMeGame.EditorView
{
    public class EditorElementsPanel : Panel
    {
        public EditorElementsPanel()
        {
            BackColor = Color.Gray;

            var createNewElementButton = new Button()
            {
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.Gray,
                Text = "Создать объект"
            };

            SizeChanged += (sender, args) =>
            {
                createNewElementButton.Size = new Size(Width, Height / 10);
                createNewElementButton.Location = new Point(0, 9 * ClientSize.Height / 10);
            };

            Controls.Add(createNewElementButton);
        }
    }
}
