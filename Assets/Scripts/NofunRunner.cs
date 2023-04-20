using Nofun.Driver.Unity.Audio;
using Nofun.Driver.Unity.Graphics;
using Nofun.Driver.Unity.Input;
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

        private VMGPExecutable executable;
        private VMSystem system;

        private const string testFile = "E:\\Jeff.mpn";

        private void SetupLogger()
        {
            Util.Logging.Logger.AddTarget(new UnityLogTarget());
        }

        private void Start()
        {
            SetupLogger();

            executable = new VMGPExecutable(File.OpenRead(testFile));
            system = new VMSystem(executable, graphicDriver, inputDriver, audioDriver);

            // Setup graphics driver
            graphicDriver.StopProcessorAction = () => system.Processor.Stop();
        }

        private void Update()
        {
            system.Run();
        }
    }
}