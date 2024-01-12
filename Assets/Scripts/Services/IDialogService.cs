using System;

namespace Nofun.Services
{
    public enum ButtonType
    {
        OK,
        OKCancel,
        YesNo,
        YesNoCancel,
        None
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

        public int OpenBlocked(Severity severity, string title, string details);

        public void CloseBlocked(int popupId);
    }
}
