using KosorenTool.Models;
using Zenject;

namespace KosorenTool.Installers
{
    public class KosorenToolStandardPlayerInstaller : MonoInstaller
    {
        public override void InstallBindings()
        {
            this.Container.BindInterfacesAndSelfTo<KosorenToolStandardPlayerController>().AsCached().NonLazy();
        }
    }
}