using UnityEngine;
using VContainer;
using ScreenOrientation = Nofun.Settings.ScreenOrientation;

namespace Nofun.Services.Unity
{
    public class LayoutService : MonoBehaviour, ILayoutService
    {
        [SerializeField] private Canvas canvasPotrait;
        [SerializeField] private Canvas canvasLandscape;

        private ScreenManager screenManager;

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
                currentCanvasVisibile = canvasLandscape.gameObject.activeSelf;
                canvasPotrait.gameObject.SetActive(currentCanvasVisibile);
                canvasLandscape.gameObject.SetActive(false);
            }
            else
            {
                currentCanvasVisibile = canvasPotrait.gameObject.activeSelf;
                canvasPotrait.gameObject.SetActive(false);
                canvasLandscape.gameObject.SetActive(currentCanvasVisibile);
            }
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
            Canvas.gameObject.SetActive(isVisible);
        }
    }
}
