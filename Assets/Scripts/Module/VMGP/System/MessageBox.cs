/*
 * (C) 2023 Radrat Softworks
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *     http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

using Nofun.VM;
using Nofun.Driver.UI;
using Nofun.Util;
using Nofun.Util.Logging;
using Nofun.Services;

namespace Nofun.Module.VMGP
{
    [Module]
    public partial class VMGP
    {

        [ModuleCall]
        private int vMsgBox(uint flags, VMString message, VMString optionalTitle)
        {
            Severity boxSeverity;
            switch (true)
            {
                case true when BitUtil.FlagSet(flags, MessageBoxFlags.Error):
                    boxSeverity = Severity.Error;
                    break;

                case true when BitUtil.FlagSet(flags, MessageBoxFlags.Warning):
                    boxSeverity = Severity.Warning;
                    break;

                case true when BitUtil.FlagSet(flags, MessageBoxFlags.Info):
                    boxSeverity = Severity.Info;
                    break;

                case true when BitUtil.FlagSet(flags, MessageBoxFlags.Question):
                    boxSeverity = Severity.Question;
                    break;

                default:
                    Logger.Warning(LogClass.VMGPSystem, "Unknown message box severity, defaulting to info");
                    boxSeverity = Severity.Info;
                    break;
            }

            ButtonType buttonType;
            switch (true)
            {
                case true when BitUtil.FlagSet(flags, MessageBoxFlags.OKCancel):
                    buttonType = ButtonType.OKCancel;
                    break;

                case true when BitUtil.FlagSet(flags, MessageBoxFlags.YesNo):
                    buttonType = ButtonType.YesNo;
                    break;

                default:
                    Logger.Warning(LogClass.VMGPSystem, "Unknown message box button type, defaulting to OK");
                    buttonType = ButtonType.OK;
                    break;
            }

            string title = null;

            if (BitUtil.FlagSet(flags, MessageBoxFlags.Title))
            {
                title = optionalTitle.Get(system.Memory);
            }

            string content = message.Get(system.Memory);
            int buttonValue = 0;

            // 0 is already cancel
            system.UIDriver.Show(boxSeverity, title, content, buttonType, (int button) =>
            {
                buttonValue = button;
            });

            return buttonValue;
        }
    }
}