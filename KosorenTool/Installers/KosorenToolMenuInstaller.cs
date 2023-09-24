using KosorenTool.Views;
using KosorenTool.Models;
using Zenject;

namespace KosorenTool.Installers
{
    public class KosorenToolMenuInstaller : MonoInstaller
    {
        public override void InstallBindings()
        {
            this.Container.BindInterfacesAndSelfTo<KosorenToolUIManager>().AsSingle().NonLazy();
            this.Container.BindInterfacesAndSelfTo<SettingTabViewController>().AsSingle().NonLazy();
        }
    }
}
