#nullable disable
using MelonLoader;

namespace CatchColdMod
{
    public class Main : MelonMod
    {
        private ColdManager _coldManager;

        public override void OnInitializeMelon()
        {
            _coldManager = new ColdManager();
            MelonLogger.Msg("[CatchColdMod] CatchColdMod initialized");
            DebugHelper.Init();
            CatchColdSettings.OnLoad();
        }

        public override void OnUpdate()
        {
            _coldManager?.Update();
        }
    }
}