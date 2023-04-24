using Nofun.Driver.Audio;
using Nofun.Driver.Graphics;
using Nofun.Driver.Input;
using Nofun.Driver.Time;

namespace Nofun.VM
{
    public class VMSystemCreateParameters
    {
        public IGraphicDriver graphicDriver;
        public IInputDriver inputDriver;
        public IAudioDriver audioDriver;
        public ITimeDriver timeDriver;
        public string persistentDataPath;
        public string inputFileName;

        public VMSystemCreateParameters(IGraphicDriver graphicDriver, IInputDriver inputDriver, IAudioDriver audioDriver, ITimeDriver timeDriver,
            string persistentDataPath, string inputFileName = "")
        {
            this.graphicDriver = graphicDriver;
            this.inputDriver = inputDriver;
            this.audioDriver = audioDriver;
            this.timeDriver = timeDriver;
            this.persistentDataPath = persistentDataPath;
            this.inputFileName = inputFileName;
        }
    }
}