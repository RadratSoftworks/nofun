using System;
using Nofun.Driver.UI;
using Nofun.UI;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Nofun.Services.Unity
{
    public class DialogService : MonoBehaviour, IDialogService
    {
        [SerializeField] private GameObject dialogPrefab;
        [Inject] private IObjectResolver objectResolver;

        private const float BaseSortOrder = 8000.0f;
        private float orderStackCount = 0.0f;

        public void Show(Severity severity, ButtonType buttonType, string title, string details, Action<int> onButtonSubmit)
        {
            orderStackCount++;

            var instantiatedDialog = objectResolver.Instantiate(dialogPrefab, transform);

            NofunMessageBoxController.Show(instantiatedDialog, severity, buttonType, title, details, (button) =>
            {
                orderStackCount--;
                onButtonSubmit?.Invoke(button);
            }, BaseSortOrder + orderStackCount);
        }
    }
}
