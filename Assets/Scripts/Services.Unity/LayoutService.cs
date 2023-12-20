using UnityEngine;
using UnityEngine.UIElements;
using VContainer;
using ScreenOrientation = Nofun.Settings.ScreenOrientation;

namespace Nofun.Services.Unity
{
    public class LayoutService : MonoBehaviour, ILayoutService
    {
        [SerializeField] private Canvas canvasPotrait;
        [SerializeField] private Canvas canvasLandscape;
        [SerializeField] private UIDocument blockingInteractionDoc;

        private ScreenManager screenManager;
        private bool anyCanvasActivated = false;
        private bool? pendingRequestVisible = null;

        public Canvas Canvas => (screenManager.ScreenOrientation == Settings.ScreenOrientation.Potrait) ? canvasPotrait : canvasLandscape;

        [Inject]
        public void Construct(ScreenManager injectScreenManager)
        {
            screenManager = injectScreenManager;
        }

        private void UpdateControlLayout(ScreenOrientation screenOrientation)
        {
            bool currentCanvasVisibile = false;

            if (screenOrientation == Settings.ScreenOrientation.Potrait)
            {
                currentCanvasVisibile = pendingRequestVisible ?? (anyCanvasActivated ? canvasLandscape.gameObject.activeSelf : true);
                canvasPotrait.gameObject.SetActive(currentCanvasVisibile);
                canvasLandscape.gameObject.SetActive(false);
            }
            else
            {
                currentCanvasVisibile = pendingRequestVisible ?? (anyCanvasActivated ? canvasPotrait.gameObject.activeSelf : true);
                canvasPotrait.gameObject.SetActive(false);
                canvasLandscape.gameObject.SetActive(currentCanvasVisibile);
            }

            anyCanvasActivated = true;
            pendingRequestVisible = null;
        }

        private void OnEnable()
        {
            screenManager.ScreenOrientationChanged += UpdateControlLayout;
        }

        private void OnDisable()
        {
            screenManager.ScreenOrientationChanged -= UpdateControlLayout;
        }

        public void SetVisibility(bool isVisible)
        {
            if (!anyCanvasActivated)
            {
                pendingRequestVisible = isVisible;
            }

            if (Canvas != null)
            {
                Canvas.gameObject.SetActive(isVisible);
            }
        }

        public void BlockInterfaceInteraction()
        {
            blockingInteractionDoc.enabled = true;
        }

        public void UnblockInterfaceInteraction()
        {
            blockingInteractionDoc.enabled = false;
        }
    }
}
