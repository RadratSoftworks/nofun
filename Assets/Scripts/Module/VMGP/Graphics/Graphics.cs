using Nofun.Driver.Graphics;
using System;

namespace Nofun.Module.VMGP
{
    [Module]
    public partial class VMGP
    {
        private SColor foregroundColor;
        private SColor backgroundColor;

        [ModuleCall]
        private void vClearScreen(Int32 color)
        {
            system.GraphicDriver.ClearScreen(GetColor(color));
        }

        [ModuleCall]
        private void vSetBackColor(Int32 color)
        {
            backgroundColor = GetColor(color);
        }

        [ModuleCall]
        private void vSetForeColor(Int32 color)
        {
            foregroundColor = GetColor(color);
        }


        [ModuleCall]
        private void vFlipScreen(Int32 block)
        {
            // This should always block
            system.GraphicDriver.FlipScreen();
        }

        [ModuleCall]
        private void vSetClipWindow(ushort x0, ushort y0, ushort x1, ushort y1)
        {
            system.GraphicDriver.SetClipRect(x0, y0, x1, y1);
        }
    }
}