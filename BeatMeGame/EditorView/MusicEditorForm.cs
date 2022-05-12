using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
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
        private readonly string treadName;
        private readonly SoundEngine engine;
        private List<BeatButton> beatButtons;
        private Panel beatStatusPanel;
        private Panel FFTCoefficientPanel;
        private Panel beatCoefficientPanel;
        private FFTVertex flexibleVertexFFT;
        private BPMVertex flexibleBPMVertex;
        private SpectrumCanvas spectrogramPanel;
        public MusicEditorForm(Form parent, LevelSave save)
        {
            MdiParent = parent;
            var provider = (ISoundProvider)parent;
            var soundTestEngine = provider.GetMusicEngine();
            var treadName = soundTestEngine.CreateTread(ThreadOptions.StaticThread,
                "Levels" + "\\" + save.LevelName + "\\" + save.Manifest.SongName, FFTExistance.Exist);
            var workTread = soundTestEngine.GetTread(treadName);
            this.treadName = treadName;
            engine = soundTestEngine;
            workTread.ChangePlaybackState();
            model = new MusicEditorModel(workTread, save);
            Initialize();
        }

        private void Initialize()
        {
            FormBorderStyle = FormBorderStyle.None;
            BackColor = Color.DarkGray;
            DoubleBuffered = true;
            var startTime = new TimeSpan(0, 0, model.Save.Manifest.StartSecond);

            var trackPositionTrackBar = new TrackBar()
            {
                Minimum = (int)startTime.TotalSeconds,
                TickStyle = TickStyle.None,
                Maximum = (int)model.WorkTread.MaxSongDuration.TotalSeconds
            };

            var trackPositionLabel = new Label()
            {
                TextAlign = ContentAlignment.MiddleCenter,
                Font = new Font(FontFamily.GenericSansSerif, 12),
                Text = startTime.ToString()
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
                Text = "Тип Анализа: " + (model.Save.Manifest.DetectionType == BeatDetectionType.FFT ? "FFT" : "BPM")
            };

            spectrogramPanel = new SpectrumCanvas()
            {
                BackColor = Color.Black
            };

            beatStatusPanel = new Panel()
            {
                BackColor = Color.Gray
            };

            beatCoefficientPanel = new Panel()
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

            var saveAndExitButton = new Button()
            {
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.DarkGray,
                Text = "Сохранить и выйти"
            };

            increaseBeatSecondButton.Click += (sender, args) =>
            {
                model.GetNextSecond();
                VisualizeModel();
                VisualizeSpectrogram();
                trackPositionTrackBar.Value = model.CurrentSecond;
            };

            decreaseBeatSecondButton.Click += (sender, args) =>
            {
                model.GetPreviousSecond();
                VisualizeModel();
                VisualizeSpectrogram();
                trackPositionTrackBar.Value = model.CurrentSecond;
            };

            trackPositionTrackBar.Scroll += (sender, args) =>
            {
                model.GetSecondByTime(trackPositionTrackBar.Value);
                VisualizeModel();
                VisualizeSpectrogram();
            };

            saveButton.Click += (sender, args) =>
            {
                model.SaveModel();
            };

            startSettingButton.Click += (sender, args) =>
            {
                var dialog = new TimeSelectionDialogForm(this, model.GetTimeLimit(), model.Save.Manifest.StartSecond);
                if (dialog.ShowDialog() != DialogResult.OK) return;
                trackPositionTrackBar.Minimum = dialog.StartSecond;
                trackPositionTrackBar.Value = dialog.StartSecond;
                trackPositionLabel.Text = new TimeSpan(0, 0, dialog.StartSecond).ToString();
                model.ChangeStartTime(dialog.StartSecond);
                VisualizeModel();
            };

            saveAndExitButton.Click += (sender, args) =>
            {
                var dialog = new EditorExitDialogForm(this);
                var result = dialog.ShowDialog();
                switch (result)
                {
                    case DialogResult.OK:
                    {
                        model.SaveModel();
                        engine.TerminateTread(treadName);
                        var creator = (IFormCreator)MdiParent;
                        creator.ReestablishScene();
                        Close();
                        break;
                    }
                    case DialogResult.Yes:
                    {
                        engine.TerminateTread(treadName);
                        var creator = (IFormCreator)MdiParent;
                        creator.ReestablishScene();
                        Close();
                        break;
                    }
                }
            };

            trackPositionTrackBar.ValueChanged += (sender, args) =>
            {
                trackPositionLabel.Text = (new TimeSpan(0, 0, model.CurrentSecond)).ToString();
            };

            analyzeTypeButton.Click += (sender, args) =>
            {
                model.ChangeAnalyzeType();
                InitializeAnalyzeState();
                trackPositionTrackBar.Value = model.Save.Manifest.StartSecond;
                analyzeTypeButton.Text = @"Тип анализа ";
                analyzeTypeButton.Text += model.Save.Manifest.DetectionType == BeatDetectionType.FFT ? "FFT" : "BPM";
            };

            Load += (sender, args) =>
            {
                var marginRange = ClientSize.Width / 50;
                Size = new Size(MdiParent.ClientSize.Width - 4, MdiParent.ClientSize.Height - 4);
                Location = Parent.Location;
                var buttonSize = new Size(ClientSize.Width / 18, ClientSize.Height / 30);
                trackPositionTrackBar.Size = new Size(3 * ClientSize.Width / 8, trackPositionTrackBar.Height);
                trackPositionTrackBar.Location = new Point(ClientSize.Width / 16, 5 * ClientSize.Height / 6);
                trackPositionLabel.Size = new Size(ClientSize.Width / 20, ClientSize.Height / 40);
                trackPositionLabel.Location = new Point(trackPositionTrackBar.Right, trackPositionTrackBar.Location.Y);
                playTestButton.Size = buttonSize;
                playTestButton.Location = new Point(trackPositionLabel.Right + marginRange, trackPositionLabel.Location.Y);
                startSettingButton.Size = buttonSize;
                startSettingButton.Location = new Point(playTestButton.Right + marginRange,
                    trackPositionLabel.Location.Y);
                saveButton.Size = buttonSize;
                saveButton.Location = new Point(startSettingButton.Right + marginRange, trackPositionLabel.Location.Y);
                analyzeTypeButton.Size = buttonSize;
                analyzeTypeButton.Location = new Point(saveButton.Right + marginRange, trackPositionLabel.Location.Y);
                spectrogramPanel.Size = new Size(3 * ClientSize.Width / 4, ClientSize.Height / 2);
                spectrogramPanel.Location = new Point(ClientSize.Width / 12, ClientSize.Height / 12);
                beatStatusPanel.Size = new Size(spectrogramPanel.Width, ClientSize.Height / 18);
                beatStatusPanel.Location =
                    new Point(spectrogramPanel.Left, spectrogramPanel.Bottom + ClientSize.Height / 12);
                decreaseBeatSecondButton.Size = buttonSize;
                decreaseBeatSecondButton.Location = new Point(beatStatusPanel.Left - decreaseBeatSecondButton.Width - marginRange,
                    beatStatusPanel.Top);
                increaseBeatSecondButton.Size = decreaseBeatSecondButton.Size;
                increaseBeatSecondButton.Location =
                    new Point(beatStatusPanel.Right + marginRange,
                        decreaseBeatSecondButton.Location.Y);
                FFTCoefficientPanel.Size = new Size(ClientSize.Width / 6, spectrogramPanel.Height);
                FFTCoefficientPanel.Location = new Point(spectrogramPanel.Right + marginRange, spectrogramPanel.Top);
                beatCoefficientPanel.Size = FFTCoefficientPanel.Size;
                beatCoefficientPanel.Location = FFTCoefficientPanel.Location;
                saveAndExitButton.Size = buttonSize;
                saveAndExitButton.Location = new Point(increaseBeatSecondButton.Location.X, 24 * ClientSize.Height / 25);
            };

            FFTCoefficientPanel.Resize += (sender, args) =>
            {
                InitFFTPanel();
            };

            beatCoefficientPanel.Resize += (sender, args) =>
            {
                InitializeBPMCoefficientPanel();
            };

            beatStatusPanel.Resize += (sender, args) =>
            {
                InitializeBeatPanel();
                InitializeAnalyzeState();
            };

            spectrogramPanel.Resize += (sender, args) =>
            {
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
            Controls.Add(beatCoefficientPanel);
            Controls.Add(saveAndExitButton);
        }

        private void VisualizeSpectrogram()
        {
            if(flexibleVertexFFT == null) return;
            var spectrogramData = model.GetSpectrogram(flexibleVertexFFT.BotFrequency, flexibleVertexFFT.TopFrequency);
            spectrogramPanel.VisualizeSpectrogram(spectrogramData);
        }

        private void InitializeAnalyzeState()
        {
            if (model.Save.Manifest.DetectionType == BeatDetectionType.FFT)
            {
                beatCoefficientPanel.Hide();
                FFTCoefficientPanel.Show();
            }
            else
            {
                FFTCoefficientPanel.Hide();
                beatCoefficientPanel.Show();
            }
            VisualizeModel();
            VisualizeSpectrogram();
        }

        private void InitializeBeatPanel()
        {
            var buttonsCount = model.FramesPerSecond;
            if (model.Save.Manifest.DetectionType == BeatDetectionType.FFT)
                model.UnpackVertices(model.CurrentSecond, PackingDirection.Forward);
            else
                model.UnpackVertices(model.CurrentSecond, PackingDirection.Forward);
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
                    new Point(beatStatusPanel.Location.X + i * beatStatusPanel.ClientSize.Width / buttonsCount, 
                        beatStatusPanel.Location.Y)
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
                Minimum = 100,
                Value = 300,
                Size = sliderSize,
                Location = new Point(margin, FFTCoefficientPanel.ClientSize.Height / 8)
            };

            var lowFrequencySlider = new TrackBar()
            {
                Orientation = Orientation.Vertical,
                TickStyle = TickStyle.None,
                Maximum = highFrequencySlider.Minimum - 1,
                Minimum = 0,
                Value = 90,
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
                Text = @"0",
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
                Text = @"3.0",
                Location = new Point(thresholdValueSlider.Left, thresholdValueSlider.Top - labelSize.Height),
                Size = labelSize
            };

            var minThreshold = new Label()
            {
                TextAlign = ContentAlignment.MiddleCenter,
                Text = @"-3.0",
                Location = new Point(thresholdValueSlider.Left, thresholdValueSlider.Bottom),
                Size = labelSize
            };

            var thresholdValue = new Label()
            {
                TextAlign = ContentAlignment.MiddleCenter,
                Text = @"0.0",
                Location = new Point(thresholdValueSlider.Left, minThreshold.Bottom),
                Size = labelSize
            };

            flexibleVertexFFT = new FFTVertex(TimeSpan.Zero, VertexType.FFT, highFrequencySlider.Value,
                lowFrequencySlider.Value, (double)thresholdValueSlider.Value / 100);

            highFrequencySlider.ValueChanged += (sender, args) =>
            {
                lowFrequencySlider.Maximum = highFrequencySlider.Value;
                flexibleVertexFFT.TopFrequency = highFrequencySlider.Value;
                var stringValue = highFrequencySlider.Value.ToString();
                bottomMaximumLabel.Text = stringValue;
                highBorder.Text = stringValue;
                VisualizeSpectrogram();
            };

            lowFrequencySlider.ValueChanged += (sender, args) =>
            {
                highFrequencySlider.Minimum = lowFrequencySlider.Value;
                flexibleVertexFFT.BotFrequency = lowFrequencySlider.Value;
                var stringValue = lowFrequencySlider.Value.ToString();
                lowBorder.Text = stringValue;
                topMinimumLabel.Text = stringValue;
                VisualizeSpectrogram();
            };

            thresholdValueSlider.ValueChanged += (sender, args) =>
            {
                flexibleVertexFFT.ThresholdValue = (double)thresholdValueSlider.Value / 100;
                thresholdValue.Text = ((double)thresholdValueSlider.Value / 100).ToString(CultureInfo.InvariantCulture);
            };

            VisualizeSpectrogram();
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

        private void InitializeBPMCoefficientPanel()
        {
            var bpmTrackBar = new TrackBar()
            {
                Maximum = 7000,
                Value = 0,
                Minimum = 0,
                Orientation = Orientation.Vertical,
                TickStyle = TickStyle.None,
                Location = new Point(beatCoefficientPanel.ClientSize.Width / 2, 
                    beatCoefficientPanel.ClientSize.Height / 8),
                Size = new Size(Size.Width, 3 * beatCoefficientPanel.ClientSize.Height / 4)
            };

            var labelSize = new Size(bpmTrackBar.Width, beatCoefficientPanel.ClientSize.Height / 20);

            var maxValueLabel = new Label()
            {
                Text = @"700",
                TextAlign = ContentAlignment.MiddleCenter,
                Size = labelSize,
                Location = new Point(bpmTrackBar.Left, bpmTrackBar.Top - Size.Height)
            };

            var minValueLabel = new Label()
            {
                Text = @"0",
                TextAlign = ContentAlignment.MiddleCenter,
                Size = labelSize,
                Location = new Point(bpmTrackBar.Left, bpmTrackBar.Bottom)
            };

            var currentValueLabel = new Label()
            {
                Text = @"0",
                TextAlign = ContentAlignment.MiddleCenter,
                Size = labelSize,
                Location = new Point(minValueLabel.Left, minValueLabel.Bottom)
            };

            flexibleBPMVertex = new BPMVertex(TimeSpan.MaxValue, VertexType.BPM, 0.0);
            flexibleVertexFFT = new FFTVertex(TimeSpan.Zero, VertexType.FFT, 300, 150, 0);

            VisualizeSpectrogram();
            bpmTrackBar.ValueChanged += (sender, args) =>
            {
                var newValue = (double)bpmTrackBar.Value / 10;
                flexibleBPMVertex.BPM = newValue;
                currentValueLabel.Text = newValue.ToString(CultureInfo.InvariantCulture);
            };

            beatCoefficientPanel.Controls.Add(maxValueLabel);
            beatCoefficientPanel.Controls.Add(minValueLabel);
            beatCoefficientPanel.Controls.Add(currentValueLabel);
            beatCoefficientPanel.Controls.Add(bpmTrackBar);
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
                        var updatedFFTVertex = new FFTVertex(
                            new TimeSpan(0, 0, 0, model.CurrentSecond, model.Position2Millisecond(button.Number)),
                            VertexType.FFT, flexibleVertexFFT.TopFrequency, flexibleVertexFFT.BotFrequency,
                            flexibleVertexFFT.ThresholdValue);
                        button.Vertex = updatedFFTVertex;
                        model.AddChainVertex(button.Number, updatedFFTVertex);
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
                    {
                        var updatedBPMVertex = new BPMVertex(
                            new TimeSpan(0, 0, 0, model.CurrentSecond, model.Position2Millisecond(button.Number)),
                            VertexType.BPM, flexibleBPMVertex.BPM);
                        button.Vertex = updatedBPMVertex;
                        model.AddChainVertex(button.Number, updatedBPMVertex);
                        VisualizeModel();
                        break;
                    }
                }
                return;
            }

            if (button.Vertex.Type == VertexType.FFT)
            {
                model.DeleteChainVertex(button.Number);
                VisualizeModel();
                return;
            }

            if (button.Vertex.Type == VertexType.AdditionalFFT || button.Vertex.Type == VertexType.AdditionalBPM)
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

            if (button.Vertex.Type == VertexType.BPM)
            {
                model.DeleteChainVertex(button.Number);
                VisualizeModel();
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
                case VertexType.BPM:
                {
                    beatButtons[index].BackColor = Color.Blue;
                    beatButtons[index].Text = "BPM";
                    break;
                }
                case VertexType.AdditionalBPM:
                {
                    beatButtons[index].BackColor = Color.DarkBlue;
                    beatButtons[index].Text = "Цепной BPM";
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
