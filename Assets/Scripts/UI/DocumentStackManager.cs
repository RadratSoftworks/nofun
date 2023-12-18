using System;
using System.Collections.Generic;
using DG.Tweening;
using Nofun.Services;
using Nofun.Util.Unity;
using UnityEngine;
using UnityEngine.UIElements;
using VContainer;

namespace Nofun.UI
{
    public class DocumentStackManager : MonoBehaviour
    {
        [SerializeField] private float transitionDuration = 0.3f;
        [Inject] protected ILayoutService layoutService;

        private Stack<FlexibleUIDocumentController> documentStack = new();

        public FlexibleUIDocumentController Active => documentStack.Peek();

        public void Push(FlexibleUIDocumentController uiDocument, Action onVisible = null)
        {
            documentStack.Push(uiDocument);
            layoutService.BlockInterfaceInteraction();

            uiDocument.Document.sortingOrder = documentStack.Count;

            VisualElement rootVisualElement = uiDocument.Document.rootVisualElement;
            rootVisualElement.style.display = DisplayStyle.Flex;
            rootVisualElement.DOFade(0.0f, 1.0f, transitionDuration)
                .OnComplete(() =>
                {
                    layoutService.UnblockInterfaceInteraction();
                    onVisible?.Invoke();
                });
        }

        public void Pop(FlexibleUIDocumentController requestedPopDocument, Action onInvisible = null)
        {
            if (documentStack.Count == 0)
            {
                Debug.LogError("No document to pop. Exiting app");
                Application.Quit();

                return;
            }

            if (requestedPopDocument != Active)
            {
                Debug.LogError("Requested pop document is not the active document");
                return;
            }

            documentStack.Pop();
            layoutService.BlockInterfaceInteraction();

            requestedPopDocument.Document.rootVisualElement.DOFade(1.0f, 0.0f, transitionDuration)
                .OnComplete(() =>
                {
                    requestedPopDocument.Document.rootVisualElement.style.display = DisplayStyle.None;
                    onInvisible?.Invoke();
                    layoutService.UnblockInterfaceInteraction();
                });
        }
    }
}
