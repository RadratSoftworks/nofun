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

        private bool updateRan = false;
        private const string testFile = "E:\\Fonts.mpn";

        private void Start()
        {
            graphicDriver = new GraphicDriver(new Vector2(176, 208));

            using (FileStream file = File.OpenRead(testFile))
            {
                executable = new VMGPExecutable(file);
                system = new VMSystem(executable, graphicDriver);

                // Setup graphics driver
                graphicDriver.StopProcessorAction = () => system.Processor.Stop();
            }
        }

        private void Update()
        {
            if (updateRan)
            {
                return;
            }

            updateRan = true;
            system.Run();

            displayImage.texture = graphicDriver.DisplayResult;
        }
    }
}