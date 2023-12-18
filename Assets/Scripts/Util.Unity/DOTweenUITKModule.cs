using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using UnityEngine.UIElements;

namespace Nofun.Util.Unity
{
    public static class DOTweenUITKModule
    {
        public static TweenerCore<float, float, FloatOptions> DOFade(this VisualElement target, float startValue, float endValue, float duration)
        {
            target.style.opacity = startValue;
            target.style.display = DisplayStyle.Flex;

            TweenerCore<float, float, FloatOptions> t = DOTween.To(() => target.style.opacity.value, value => target.style.opacity = (float)value, endValue, duration);
            t.SetTarget(target);

            return t;
        }
    }
}
