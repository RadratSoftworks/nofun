using System;

namespace Nofun.Services
{
    public enum ButtonType
    {
        OK,
        OKCancel,
        YesNo,
        YesNoCancel
    };

    public enum Severity
    {
        Error,
        Warning,
        Info,
        Question
    };

    public interface IDialogService
    {
        public void Show(Severity severity, ButtonType buttonType, string title, string details,
            Action<int> onButtonSubmit);
    }
}
