using System.Collections.Generic;
using Nofun.Data.Model;
using Nofun.Services;
using UnityEngine;

namespace Nofun.DynamicIcons
{
    /// <summary>
    /// This class is responsible for managing the rendering of dynamic icons.
    /// 
    /// It will be responsible for:
    /// 1. Spawning the renderer GameObject when an icon is requested.
    /// 2. Cache the renderer when it's not used anymore.
    /// </summary>
    public class DynamicIconsProvider
    {
        private Dictionary<string, GameObject> renderers = new();
        private Transform rendererParent;
        private static readonly Vector3 rendererOrigin = new Vector3(0, 10, 0);
        private static readonly Vector3 rendererDistance = new Vector3(5, 0, 0);

        private Vector3 currentRendererOrigin;

        public DynamicIconsProvider(Transform rendererParent)
        {
            this.rendererParent = rendererParent;
            this.currentRendererOrigin = rendererOrigin;
        }

        public void Cleanup()
        {
            foreach (var renderer in renderers.Values)
            {
                GameObject.Destroy(renderer);
            }

            renderers.Clear();
        }

        public RenderTexture GetIcon(DynamicGameIcon dynamicGameIcon)
        {
            if (renderers.TryGetValue(dynamicGameIcon.GameName, out var renderer))
            {
                renderer.SetActive(true);
                return dynamicGameIcon.Icon;
            }

            renderer = GameObject.Instantiate(dynamicGameIcon.Renderer, currentRendererOrigin, Quaternion.identity, rendererParent);
            renderers.Add(dynamicGameIcon.GameName, renderer);

            currentRendererOrigin += rendererDistance;

            return dynamicGameIcon.Icon;
        }

        public void ReturnIcon(DynamicGameIcon dynamicGameIcon)
        {
            if (renderers.TryGetValue(dynamicGameIcon.GameName, out var renderer))
            {
                renderer.SetActive(false);
            }
        }
    }
}