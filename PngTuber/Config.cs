using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PngTuber
{
    public class MainWindow
    {
        [JsonProperty("WindowSize")]
        public Size WindowSize { get; set; }
        [JsonProperty("WindowPosition")]
        public Point WindowPosition { get; set; }
        [JsonProperty("BorderStyle")]
        public FormBorderStyle BorderStyle { get; set; }
        [JsonProperty("TransparentKey")]
        public Color TransparentKey { get; set; }
        [JsonProperty("IsTopMost")]
        public bool IsTopMost { get; set; }

        public MainWindow(Size s, Point p, FormBorderStyle fbs, Color tk, bool topmost)
        {
            WindowSize = s;
            WindowPosition = p;
            BorderStyle = fbs;
            TransparentKey = tk;
            IsTopMost = topmost;
        }
    }
    public class MouthState
    {
        [JsonProperty("Idle")]
        public string Idle { get; set; }
        [JsonProperty("Talking")]
        public string[] Talking { get; set; }

        public MouthState(string idle, string[] talking)
        {
            Idle = idle;
            Talking = talking;
        }
    }
    public class Frame
    {
        [JsonProperty("Level")]
        public int Level { get; set; }
        [JsonProperty("Name")]
        public string Name { get; set; }
        [JsonProperty("ImagePath")]
        public string ImagePath { get; set; }
    }

    internal class Config
    {
        [JsonProperty("MouthLevel")]
        public int MouthLevel { get; set; }
        [JsonProperty("AudioLevel")]
        public int AudioLevel { get; set; }
        [JsonProperty("MainWindow")]
        public MainWindow MainWindow { get; set; }
        [JsonProperty("MouthState")]
        public MouthState MouthState { get; set; }
        [JsonProperty("Frames")]
        public List<Frame> Frames { get; set; }
    }
}
