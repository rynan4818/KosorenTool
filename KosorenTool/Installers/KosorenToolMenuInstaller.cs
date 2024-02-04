using KosorenTool.Views;
using KosorenTool.Models;
using Zenject;

namespace KosorenTool.Installers
{
    public class KosorenToolMenuInstaller : MonoInstaller
    {
        public override void InstallBindings()
        {
            this.Container.BindInterfacesAndSelfTo<KosorenToolUIManager>().AsSingle();
            this.Container.BindInterfacesAndSelfTo<KosorenInfoView>().FromNewComponentOnNewGameObject().AsSingle();
            this.Container.BindInterfacesAndSelfTo<SettingTabViewController>().AsSingle();
        }
    }
}
