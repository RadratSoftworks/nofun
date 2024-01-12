using System;
using System.Collections.Generic;
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

        private Dictionary<int, NofunMessageBoxController> blockedPopUps = new();
        private int popupCounter = 0;

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

        public int OpenBlocked(Severity severity, string title, string details)
        {
            orderStackCount++;

            var instantiatedDialog = objectResolver.Instantiate(dialogPrefab, transform);
            int popupId = ++popupCounter;

            blockedPopUps.Add(popupId, instantiatedDialog.GetComponent<NofunMessageBoxController>());

            NofunMessageBoxController.Show(instantiatedDialog, severity, ButtonType.None, title, details, (button) =>
            {
                orderStackCount--;
                blockedPopUps.Remove(popupId);

                Destroy(instantiatedDialog);
            }, BaseSortOrder + orderStackCount);

            return popupId;
        }

        public void CloseBlocked(int popupId)
        {
            if (blockedPopUps.TryGetValue(popupId, out var popup))
            {
                popup.ForceClose();
            }
        }
    }
}
