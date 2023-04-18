using Nofun.Driver.Unity.Graphics;
using Nofun.Parser;
using Nofun.VM;
using System.IO;
using UnityEngine;

namespace Nofun
{
    public class NofunRunner: MonoBehaviour
    {
        [SerializeField]
        private UnityEngine.UI.RawImage displayImage;

        private VMGPExecutable executable;
        private VMSystem system;
        private GraphicDriver graphicDriver;

        private const string testFile = "E:\\Jeff.mpn";

        private void Start()
        {
            graphicDriver = new GraphicDriver(new Vector2(176, 208));

            executable = new VMGPExecutable(File.OpenRead(testFile));
            system = new VMSystem(executable, graphicDriver);

            // Setup graphics driver
            graphicDriver.StopProcessorAction = () => system.Processor.Stop();
            displayImage.texture = graphicDriver.DisplayResult;
        }

        private void Update()
        {
            system.Run();
        }
    }
}