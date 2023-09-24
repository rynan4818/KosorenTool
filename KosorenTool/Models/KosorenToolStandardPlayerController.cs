using Zenject;

namespace KosorenTool.Models
{
    public class KosorenToolStandardPlayerController : IInitializable
    {
        private KosorenToolController _kosorenToolController;
        public KosorenToolStandardPlayerController(KosorenToolController kosorenToolController)
        {
            this._kosorenToolController = kosorenToolController;
        }
        public void Initialize()
        {
            this._kosorenToolController._standardPlayerActive = true;
        }
    }
}
