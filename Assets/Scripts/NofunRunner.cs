using Nofun.Driver.Unity.Audio;
using Nofun.Driver.Unity.Graphics;
using Nofun.Driver.Unity.Input;
using Nofun.Driver.Unity.Time;
using Nofun.Parser;
using Nofun.Util.Unity;
using Nofun.VM;
using System.IO;
using UnityEngine;

namespace Nofun
{
    public class NofunRunner: MonoBehaviour
    {
        [SerializeField]
        private InputDriver inputDriver;

        [SerializeField]
        private AudioDriver audioDriver;

        [SerializeField]
        private GraphicDriver graphicDriver;

        private TimeDriver timeDriver;

        [Range(1, 60)]
        [SerializeField]
        private int fpsLimit = 30;

        private VMGPExecutable executable;
        private VMSystem system;

        [SerializeField]
        private const string executableFilePath = "E:\\spacebox.mpn";

        private void SetupLogger()
        {
            Util.Logging.Logger.AddTarget(new UnityLogTarget());
        }

        private void Start()
        {
            SetupLogger();

            timeDriver = new TimeDriver();

            executable = new VMGPExecutable(new FileStream(executableFilePath, FileMode.Open, FileAccess.ReadWrite,
                FileShare.Read));

            system = new VMSystem(executable, new VMSystemCreateParameters(graphicDriver, inputDriver, audioDriver, timeDriver,
                Application.persistentDataPath, executableFilePath));

            // Setup graphics driver
            graphicDriver.StopProcessorAction = () => system.Processor.Stop();
        }

        private void Update()
        {
            if (!graphicDriver.FrameFlipFinishedEmulating())
            {
                return;
            }

            // Only set FPS when it's not emulating frame flip
            graphicDriver.FpsLimit = fpsLimit;

            system.Run();
            timeDriver.Update();
        }
    }
}