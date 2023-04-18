using KosorenTool.Models;
using Zenject;

namespace KosorenTool.Installers
{
    public class KosorenToolAppInstaller : Installer
    {
        public override void InstallBindings()
        {
            this.Container.BindInterfacesAndSelfTo<KosorenToolController>().AsSingle().NonLazy();
            this.Container.BindInterfacesAndSelfTo<KosorenToolPlayData>().AsSingle().NonLazy();
        }
    }
}