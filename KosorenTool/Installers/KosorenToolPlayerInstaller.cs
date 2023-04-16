using KosorenTool.Configuration;
using KosorenTool.Models;
using Zenject;

namespace KosorenTool.Installers
{
    public class KosorenToolPlayerInstaller : MonoInstaller
    {
        public override void InstallBindings()
        {
            if (PluginConfig.Instance.DisableSubmission)
                this.Container.BindInterfacesAndSelfTo<KosorenToolController>().FromNewComponentOnNewGameObject().AsCached().NonLazy();
        }
    }
}