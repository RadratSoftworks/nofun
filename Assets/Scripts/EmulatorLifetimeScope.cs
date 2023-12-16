using Nofun.DynamicIcons;
using Nofun.Services;
using Nofun.Services.Unity;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Nofun
{
    [DefaultExecutionOrder(-1)]
    public class EmulatorLifetimeScope : LifetimeScope
    {
        private static EmulatorLifetimeScope Instance { get; set; }
        public static IObjectResolver ContainerInstance => Instance.Container;

        protected override void Configure(IContainerBuilder builder)
        {
            Instance = this;

            builder.RegisterComponentInHierarchy<ScreenManager>();
            builder.RegisterComponentInHierarchy<LayoutService>().AsImplementedInterfaces();
            builder.RegisterComponentInHierarchy<DialogService>().AsImplementedInterfaces();
            builder.Register<ITranslationService, TranslationService>(Lifetime.Scoped);
        }
    }
}
