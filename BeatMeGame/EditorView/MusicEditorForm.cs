using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using BeatMeGameModel;
using BeatMeGameModel.BeatVertexes;
using SoundEngineLibrary;
using BeatMeGameModel.EditorModels;
using BeatMeGameModel.IOWorkers;


namespace BeatMeGame.EditorView
{
    public class MusicEditorForm : Form
    {
        private readonly MusicEditorModel model;
        private List<BeatButton> beatButtons;
        private Panel beatStatusPanel;
        private Panel FFTCoefficientPanel;
        private FFTVertex flexibleVertex;
        public MusicEditorForm(Form parent, LevelSave save)
        {
            MdiParent = parent;
            var provider = (ISoundProvider)parent;
            var soundTestEngine = provider.GetMusicEngine();
            var treadName = soundTestEngine.CreateTread(ThreadOptions.StaticThread,
                "Levels" + "\\" + save.LevelName + "\\" + save.Manifest.SongName, FFTExistance.Exist);
            var workTread = soundTestEngine.GetTread(treadName);
            workTread.ChangePlaybackState();
            model = new MusicEditorModel(workTread, save);
            Initialize();
        }

        private void Initialize()
        {
            FormBorderStyle = FormBorderStyle.None;
            BackColor = Color.DarkGray;
            DoubleBuffered = true;

            var trackPositionTrackBar = new TrackBar()
            {
                TickStyle = TickStyle.None,
                Maximum = (int)model.WorkTread.MaxSongDuration.TotalSeconds
            };

            var trackPositionLabel = new Label()
            {
                TextAlign = ContentAlignment.MiddleCenter,
                Font = new Font(FontFamily.GenericSansSerif, 12),
                Text = "00:00"
            };

            var playTestButton = new Button()
            {
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.DarkGray,
                Text = "Пуск"
            };

            var startSettingButton = new Button()
            {
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.DarkGray,
                Text = "Поставить время старта"
            };

            var saveButton = new Button()
            {
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.DarkGray,
                Text = "Сохраить"
            };

            var analyzeTypeButton = new Button()
            {
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.DarkGray,
                Text = "Тип Анализа: FFT"
            };

            var spectrogramPanel = new Panel()
            {
                BackColor = Color.Black
            };

            beatStatusPanel = new Panel()
            {
                BackColor = Color.Gray
            };

            var decreaseBeatSecondButton = new Button()
            {
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.DarkGray,
                Text = "Назад"
            };

            var increaseBeatSecondButton = new Button()
            {
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.DarkGray,
                Text = "Вперёд"
            };

            FFTCoefficientPanel = new Panel()
            {
                BackColor = Color.Gray,
            };

            increaseBeatSecondButton.Click += (sender, args) =>
            {
                model.GetNextSecond();
                VisualizeModel();
                trackPositionTrackBar.Value = model.CurrentSecond;
            };

            decreaseBeatSecondButton.Click += (sender, args) =>
            {
                model.GetPreviousSecond();
                VisualizeModel();
                trackPositionTrackBar.Value = model.CurrentSecond;
            };

            trackPositionTrackBar.ValueChanged += (sender, args) =>
            {
                trackPositionLabel.Text = (new TimeSpan(0, 0, model.CurrentSecond)).ToString();
            };

            Load += (sender, args) =>
            {
                var marginRange = ClientSize.Width / 50; 
                Size = new Size(MdiParent.ClientSize.Width - 4, MdiParent.ClientSize.Height - 4);
                Location = Parent.Location;
                trackPositionTrackBar.Size = new Size(3 * ClientSize.Width / 8, trackPositionTrackBar.Height);
                trackPositionTrackBar.Location = new Point(ClientSize.Width / 16, 5 * ClientSize.Height / 6);
                trackPositionLabel.Size = new Size(ClientSize.Width / 20, ClientSize.Height / 40);
                trackPositionLabel.Location = new Point(trackPositionTrackBar.Right, trackPositionTrackBar.Location.Y);
                playTestButton.Size = new Size(ClientSize.Width / 18, ClientSize.Height / 30);
                playTestButton.Location = new Point(trackPositionLabel.Right + marginRange, trackPositionLabel.Location.Y);
                startSettingButton.Size = playTestButton.Size;
                startSettingButton.Location = new Point(playTestButton.Right + marginRange,
                    trackPositionLabel.Location.Y);
                saveButton.Size = playTestButton.Size;
                saveButton.Location = new Point(startSettingButton.Right + marginRange, trackPositionLabel.Location.Y);
                analyzeTypeButton.Size = playTestButton.Size;
                analyzeTypeButton.Location = new Point(saveButton.Right + marginRange, trackPositionLabel.Location.Y);
                spectrogramPanel.Size = new Size(3 * ClientSize.Width / 4, ClientSize.Height / 2);
                spectrogramPanel.Location = new Point(ClientSize.Width / 12, ClientSize.Height / 12);
                beatStatusPanel.Size = new Size(spectrogramPanel.Width, ClientSize.Height / 18);
                beatStatusPanel.Location =
                    new Point(spectrogramPanel.Left, spectrogramPanel.Bottom + ClientSize.Height / 12);
                decreaseBeatSecondButton.Size = analyzeTypeButton.Size;
                decreaseBeatSecondButton.Location = new Point(beatStatusPanel.Left - decreaseBeatSecondButton.Width - marginRange,
                    beatStatusPanel.Top);
                increaseBeatSecondButton.Size = decreaseBeatSecondButton.Size;
                increaseBeatSecondButton.Location =
                    new Point(beatStatusPanel.Right + marginRange,
                        decreaseBeatSecondButton.Location.Y);
                FFTCoefficientPanel.Size = new Size(ClientSize.Width / 6, spectrogramPanel.Height);
                FFTCoefficientPanel.Location = new Point(spectrogramPanel.Right + marginRange, spectrogramPanel.Top);
            };

            FFTCoefficientPanel.Resize += (sender, args) =>
            {
                InitFFTPanel();
            };

            beatStatusPanel.Resize += (sender, args) =>
            {
                InitializeBeatPanel();
            };

            Controls.Add(trackPositionTrackBar);
            Controls.Add(trackPositionLabel);
            Controls.Add(playTestButton);
            Controls.Add(startSettingButton);
            Controls.Add(saveButton);
            Controls.Add(analyzeTypeButton);
            Controls.Add(spectrogramPanel);
            Controls.Add(beatStatusPanel);
            Controls.Add(decreaseBeatSecondButton);
            Controls.Add(increaseBeatSecondButton);
            Controls.Add(FFTCoefficientPanel);
        }

        private void InitializeBeatPanel()
        {
            var buttonsCount = model.FramesPerSecond;
            model.UnpackVertexes(model.CurrentSecond, model.FramesPerSecond, model.FFTUnpacker, PackingDirection.Forward);
            beatButtons = new List<BeatButton>();
            for (int i = 0; i < buttonsCount; i++)
            {
                var j = i;
                beatButtons.Add(new BeatButton(model.Vertices[i], j)
                {
                    FlatStyle = FlatStyle.Flat,
                    BackColor = Color.DarkGray,
                    Size = new Size(beatStatusPanel.ClientSize.Width / buttonsCount,
                        beatStatusPanel.ClientSize.Height),
                    Location =
                    new Point(beatStatusPanel.Location.X + i * beatStatusPanel.ClientSize.Width / buttonsCount, beatStatusPanel.Location.Y)
                });
                beatStatusPanel.Controls.Add(beatButtons[i]);

                beatButtons[j].Click += (sender, args) =>
                {
                    BindButtons(beatButtons[j]);
                };
            }
        }

        private void InitFFTPanel()
        {
            var sliderSize = new Size(FFTCoefficientPanel.ClientSize.Width / 10,
                3 * FFTCoefficientPanel.ClientSize.Height / 4);
            var margin = FFTCoefficientPanel.ClientSize.Width / 8;
            var highFrequencySlider = new TrackBar()
            {
                Orientation = Orientation.Vertical,
                TickStyle = TickStyle.None,
                Maximum = model.WorkTread.TrackFFT.samplingFrequency / 2 - 1,
                Minimum = 10,
                Value = 10,
                Size = sliderSize,
                Location = new Point(margin, FFTCoefficientPanel.ClientSize.Height / 8)
            };

            var lowFrequencySlider = new TrackBar()
            {
                Orientation = Orientation.Vertical,
                TickStyle = TickStyle.None,
                Maximum = highFrequencySlider.Minimum - 1,
                Minimum = 0,
                Value = 0,
                Size = sliderSize,
                Location = new Point(highFrequencySlider.Right + margin, FFTCoefficientPanel.ClientSize.Height / 8)
            };

            var thresholdValueSlider = new TrackBar()
            {
                Orientation = Orientation.Vertical,
                TickStyle = TickStyle.None,
                Maximum = 300,
                Minimum = -300,
                Value = 0,
                Size = sliderSize,
                Location = new Point(lowFrequencySlider.Right + margin, FFTCoefficientPanel.ClientSize.Height / 8)
            };
            var labelSize = new Size(thresholdValueSlider.Width, FFTCoefficientPanel.ClientSize.Height / 20);

            var minimumLabel = new Label()
            {
                TextAlign = ContentAlignment.MiddleCenter,
                Text = "0",
                Location = new Point(lowFrequencySlider.Left, lowFrequencySlider.Bottom),
                Size = labelSize
            };

            var bottomMaximumLabel = new Label()
            {
                TextAlign = ContentAlignment.MiddleCenter,
                Text = highFrequencySlider.Value.ToString(),
                Size = labelSize,
                Location = new Point(lowFrequencySlider.Left, lowFrequencySlider.Top - labelSize.Height)
            };

            var lowBorder = new Label()
            {
                TextAlign = ContentAlignment.MiddleCenter,
                Text = lowFrequencySlider.Value.ToString(),
                Location = new Point(minimumLabel.Left, minimumLabel.Bottom),
                Size = labelSize
            };

            var topMinimumLabel = new Label()
            {
                TextAlign = ContentAlignment.MiddleCenter,
                Text = lowFrequencySlider.Value.ToString(),
                Location = new Point(highFrequencySlider.Left, highFrequencySlider.Bottom),
                Size = labelSize
            };

            var maximumLabel = new Label()
            {
                TextAlign = ContentAlignment.MiddleCenter,
                Text = highFrequencySlider.Maximum.ToString(),
                Size = labelSize,
                Location = new Point(highFrequencySlider.Left, highFrequencySlider.Top - labelSize.Height)
            };

            var highBorder = new Label()
            {
                TextAlign = ContentAlignment.MiddleCenter,
                Text = highFrequencySlider.Value.ToString(),
                Location = new Point(highFrequencySlider.Left, topMinimumLabel.Bottom),
                Size = labelSize
            };

            var maxThreshold = new Label()
            {
                TextAlign = ContentAlignment.MiddleCenter,
                Text = "3.0",
                Location = new Point(thresholdValueSlider.Left, thresholdValueSlider.Top - labelSize.Height),
                Size = labelSize
            };

            var minThreshold = new Label()
            {
                TextAlign = ContentAlignment.MiddleCenter,
                Text = "-3.0",
                Location = new Point(thresholdValueSlider.Left, thresholdValueSlider.Bottom),
                Size = labelSize
            };

            var thresholdValue = new Label()
            {
                TextAlign = ContentAlignment.MiddleCenter,
                Text = "0.0",
                Location = new Point(thresholdValueSlider.Left, minThreshold.Bottom),
                Size = labelSize
            };

            flexibleVertex = new FFTVertex(TimeSpan.Zero, VertexType.FFT, highFrequencySlider.Value,
                lowFrequencySlider.Value, (double)thresholdValueSlider.Value / 100);

            highFrequencySlider.ValueChanged += (sender, args) =>
            {
                lowFrequencySlider.Maximum = highFrequencySlider.Value;
                flexibleVertex.TopFrequency = highFrequencySlider.Value;
                var stringValue = highFrequencySlider.Value.ToString();
                bottomMaximumLabel.Text = stringValue;
                highBorder.Text = stringValue;
            };

            lowFrequencySlider.ValueChanged += (sender, args) =>
            {
                highFrequencySlider.Minimum = lowFrequencySlider.Value;
                flexibleVertex.BotFrequency = lowFrequencySlider.Value;
                var stringValue = lowFrequencySlider.Value.ToString();
                lowBorder.Text = stringValue;
                topMinimumLabel.Text = stringValue;
            };

            thresholdValueSlider.ValueChanged += (sender, args) =>
            {
                flexibleVertex.ThresholdValue = (double)thresholdValueSlider.Value / 100;
                thresholdValue.Text = ((double)thresholdValueSlider.Value / 100).ToString();
            };

            FFTCoefficientPanel.Controls.Add(highFrequencySlider);
            FFTCoefficientPanel.Controls.Add(lowFrequencySlider);
            FFTCoefficientPanel.Controls.Add(thresholdValueSlider);
            FFTCoefficientPanel.Controls.Add(minimumLabel);
            FFTCoefficientPanel.Controls.Add(lowBorder);
            FFTCoefficientPanel.Controls.Add(topMinimumLabel);
            FFTCoefficientPanel.Controls.Add(maximumLabel);
            FFTCoefficientPanel.Controls.Add(highBorder);
            FFTCoefficientPanel.Controls.Add(bottomMaximumLabel);
            FFTCoefficientPanel.Controls.Add(maxThreshold);
            FFTCoefficientPanel.Controls.Add(minThreshold);
            FFTCoefficientPanel.Controls.Add(thresholdValue);
        }

        private void BindButtons(BeatButton button)
        {
            if (button.Vertex.Type == VertexType.None)
            {
                var dialog = new VertexTypeDialogForm(model.Save.Manifest.DetectionType, this);
                if (dialog.ShowDialog() != DialogResult.OK) return;
                switch (dialog.OutType)
                {
                    case VertexType.FFT:
                    {
                        var updatedVertex = new FFTVertex(
                            new TimeSpan(0, 0, 0, model.CurrentSecond, model.Position2Millisecond(button.Number)),
                            VertexType.FFT, flexibleVertex.TopFrequency, flexibleVertex.BotFrequency,
                            flexibleVertex.ThresholdValue);
                        button.Vertex = updatedVertex;
                        model.AddFFTVertex(button.Number, updatedVertex);
                        VisualizeModel();
                        break;
                    }
                    case VertexType.Artificial:
                    {
                        var additionalVertex = new BeatVertex(
                            new TimeSpan(0, 0, 0, model.CurrentSecond, model.Position2Millisecond(button.Number)),
                            VertexType.Artificial);
                        model.AddMonoVertex(button.Number, additionalVertex);
                        VisualizeButton(button.Number);
                        break;
                    }
                    case VertexType.BPM:
                        break;
                }
                return;
            }

            if (button.Vertex.Type == VertexType.FFT)
            {
                model.DeleteFFTVertex(button.Number);
                VisualizeModel();
                return;
            }

            if (button.Vertex.Type == VertexType.AdditionalFFT)
            {
                model.AddMonoVertex(button.Number,
                    new BeatVertex(
                        new TimeSpan(0, 0, 0, model.CurrentSecond, model.Position2Millisecond(button.Number)),
                        VertexType.Deletion));
                VisualizeButton(button.Number);
                return;
            }

            if (button.Vertex.Type == VertexType.Deletion)
            {
                var vertexType = model.Save.Manifest.DetectionType == BeatDetectionType.FFT
                    ? VertexType.AdditionalFFT
                    : VertexType.AdditionalBPM;
                model.AddMonoVertex(button.Number,
                    new BeatVertex(
                        new TimeSpan(0, 0, 0, model.CurrentSecond, model.Position2Millisecond(button.Number)),
                        vertexType));
                VisualizeButton(button.Number);
                return;
            }

            if (button.Vertex.Type == VertexType.Artificial)
            {
                model.AddMonoVertex(button.Number, new BeatVertex(
                    new TimeSpan(0, 0, 0, model.CurrentSecond, model.Position2Millisecond(button.Number)),
                    VertexType.None));
                VisualizeButton(button.Number);
                return;
            }

        }

        private void VisualizeModel()
        {
            for (int i = 0; i < model.Vertices.Length; i++)
            {
                VisualizeButton(i);
            }
        }

        private void VisualizeButton(int index)
        {
            beatButtons[index].Vertex = model.Vertices[index];
            switch (beatButtons[index].Vertex.Type)
            {
                case VertexType.FFT:
                {
                    beatButtons[index].BackColor = Color.Green;
                    beatButtons[index].Text = "FFT";
                    break;
                }
                case VertexType.AdditionalFFT:
                {
                    beatButtons[index].BackColor = Color.DarkOliveGreen;
                    beatButtons[index].Text = "Цепной FFT";
                    break;
                }
                case VertexType.Artificial:
                {
                    beatButtons[index].BackColor = Color.BlueViolet;
                    beatButtons[index].Text = "Ручная";
                    break;
                }
                case VertexType.Deletion:
                {
                    beatButtons[index].BackColor = Color.DarkRed;
                    beatButtons[index].Text = "Скрытая";
                    break;
                }
                default:
                {
                    beatButtons[index].BackColor = Color.DarkGray;
                    beatButtons[index].Text = "";
                    break;
                }
            }
        }
    }
}
