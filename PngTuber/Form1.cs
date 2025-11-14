using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Speech.Recognition;
using System.Windows.Forms;

namespace PngTuber
{
    public partial class Form1 : Form
    {
        SpeechRecognitionEngine recognizer;
        JsonDataSet<Config> json;
        Config config;
        //PictureBox body, eyes, mouth;
        PictureBox[] frames;
        //Image idle, talking;
        Image[] mouthStates;
        Size oldScreenSize, newScreenSize;
        bool isSpeaking = false;
        int audioLevel = 8;
        public Form1()
        {
            InitializeComponent();
            this.Size = new Size(800, 800);
        }

        void Start()
        {
            recognizer = new SpeechRecognitionEngine();
            recognizer.AudioLevelUpdated += Recognizer_AudioLevelUpdated;
            DictationGrammar grammars = new DictationGrammar();
            recognizer.LoadGrammar(grammars);
            recognizer.SetInputToDefaultAudioDevice();
            recognizer.RecognizeAsync(RecognizeMode.Multiple);
        }

        private void Recognizer_AudioLevelUpdated(object sender, AudioLevelUpdatedEventArgs e)
        {
            if (e.AudioLevel <= audioLevel)
            {
                if (isSpeaking)
                {
                    isSpeaking = false;
                    frames[config.MouthLevel].Image = mouthStates[0];
                }
            }
            else
            if(e.AudioLevel > audioLevel)
            {
                if (!isSpeaking)
                {
                    isSpeaking = true;
                    frames[config.MouthLevel].Image = mouthStates[1];
                }
            }
        }

        PictureBox CreateFrame(string name, string src)
        {
            PictureBox pb = new PictureBox();
            pb.Name = name;
            pb.Tag = src;
            pb.BackColor = Color.Transparent;
            pb.SizeMode = PictureBoxSizeMode.StretchImage;
            pb.Dock = DockStyle.Fill;
            pb.Image = Image.FromFile(src);
            return pb;
        }

        void SetWindowless()
        {
            this.FormBorderStyle = this.FormBorderStyle == FormBorderStyle.Sizable ? FormBorderStyle.None : FormBorderStyle.Sizable;
            this.TransparencyKey = this.TransparencyKey == SystemColors.Control ? Color.Red : SystemColors.Control;
        }

        void StackFrames(PictureBox a, PictureBox b)
        {
            a.Controls.Add(b);
            b.Location = new Point(0, 0);
        }

        void ReadConfig()
        {
            this.Size = config.MainWindow.WindowSize;
            this.Location = config.MainWindow.WindowPosition;
            this.FormBorderStyle = config.MainWindow.BorderStyle;
            this.TransparencyKey = config.MainWindow.TransparentKey;
            this.TopMost = config.MainWindow.IsTopMost;
            this.audioLevel = config.AudioLevel;
        }

        void ChangeMouthState(string Path, int index)
        {
            mouthStates[index] = Image.FromFile(Path);
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            mouthStates = new Image[2];
            json = new JsonDataSet<Config>("config.txt");
            config = json.GetAll()[0];

            ChangeMouthState(config.MouthState.Idle, 0);
            ChangeMouthState(config.MouthState.Talking, 1);

            frames = new PictureBox[config.Frames.Count];

            foreach (Frame frame in config.Frames)
            {
                PictureBox pb = CreateFrame(frame.Name, frame.ImagePath);
                frames[frame.Level] = pb;
            }

            StackFrames(screen, frames[0]);
            for (int i = 1; i < frames.Length; i++)
            {
                StackFrames(frames[i - 1], frames[i]);
            }

            ReadConfig();
            Start();
            frames[config.MouthLevel].Image = mouthStates[1];
        }

        private void Form1_ResizeBegin(object sender, EventArgs e)
        {
            oldScreenSize = this.Size;
        }

        private void Form1_ResizeEnd(object sender, EventArgs e)
        {
            newScreenSize = this.Size;
            if (newScreenSize.Width != oldScreenSize.Width)
            {
                this.Height = newScreenSize.Width;
            }
            else if (newScreenSize.Height != oldScreenSize.Height)
            {
                this.Width = newScreenSize.Height;
            }
        }

        private void Form1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                Application.Exit();
            }
            else if (e.KeyCode == Keys.F2)
            {
                MainWindow mainWindow = new MainWindow(this.Size, this.Location, this.FormBorderStyle, this.TransparencyKey, this.TopMost);
                config.MainWindow = mainWindow;
                json.Add(config);
            }
            else if (e.KeyCode == Keys.F3)
            {

            }
            else if (e.KeyCode == Keys.F4)
            {
                SetWindowless();
            }
            else if (e.KeyCode == Keys.F5)
            {
                this.TopMost = !this.TopMost;
            }
        }
    }
}
