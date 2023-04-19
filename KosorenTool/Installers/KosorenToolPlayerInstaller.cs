using KosorenTool.Models;
using Zenject;

namespace KosorenTool.Installers
{
    public class KosorenToolPlayerInstaller : MonoInstaller
    {
        public override void InstallBindings()
        {
            this.Container.BindInterfacesAndSelfTo<KosorenToolPlayerController>().AsCached().NonLazy();
        }
    }
}
