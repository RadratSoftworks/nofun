using System;
using Nofun.Driver.UI;

namespace Nofun.Services
{
    public interface IDialogService
    {
        public void Show(IUIDriver.Severity severity, IUIDriver.ButtonType buttonType, string title, string details,
            Action<int> onButtonSubmit);
    }
}
