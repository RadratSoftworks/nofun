using System;
using Nofun.Driver.UI;
using Nofun.UI;
using UnityEngine;

namespace Nofun.Services.Unity
{
    public class DialogService : MonoBehaviour, IDialogService
    {
        [SerializeField] private GameObject dialogPrefab;

        private const float BaseSortOrder = 8000.0f;
        private float orderStackCount = 0.0f;

        public void Show(Severity severity, ButtonType buttonType, string title, string details, Action<int> onButtonSubmit)
        {
            orderStackCount++;

            NofunMessageBoxController.Show(dialogPrefab, severity, buttonType, title, details, (button) =>
            {
                orderStackCount--;
                onButtonSubmit?.Invoke(button);
            }, BaseSortOrder + orderStackCount);
        }
    }
}
