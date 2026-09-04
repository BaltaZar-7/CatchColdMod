#nullable disable
using MelonLoader;
using ModSettings;

namespace CatchColdMod
{
    internal class CatchColdSettings : JsonModSettings
    {
        public static CatchColdSettings Instance { get; private set; }

        [Section("CCM.ColdRisk.Section", Localize = true)]

        [Name("CCM.RiskThresholdHours.Name", Localize = true)]
        [Description("CCM.RiskThresholdHours.Decription", Localize = true)]
        [Slider(2, 24)]
        public float RiskThresholdHours = 6;

        [Name("CCM.BaseRiskToColdChance.Name", Localize = true)]
        [Description("CCM.BaseRiskToColdChance.Description", Localize = true)]
        [Slider(50, 100)]
        public float BaseRiskToColdChance = 80;

        [Section("CCM.ColdAffliction.Section", Localize = true)]

        [Name("CCM.ColdDurationDays.Name", Localize = true)]
        [Description("CCM.ColdDurationDays.Description", Localize = true)]
        [Slider(3, 14)]
        public float ColdDurationDays = 7;

        [Section("CCM.Pneumonia.Section", Localize = true)]

        [Name("CCM.PneumoniaDurationDays.Name", Localize = true)]
        [Description("CCM.PneumoniaDurationDays.Description", Localize = true)]
        [Slider(6, 10)]
        public float PneumoniaDurationDays = 7;

        [Name("CCM.TeasCountForPneumonia.Name", Localize = true)]
        [Description("CCM.TeasCountForPneumonia.Description", Localize = true)]
        public bool TeasCountForPneumonia = false;

        protected override void OnConfirm()
        {
            base.OnConfirm();
            Save();
            MelonLogger.Msg("[Settings] Saved mod settings.");
        }

        public static void OnLoad()
        {
            Instance = new CatchColdSettings();
            Instance.AddToModSettings("Catch Cold Mod");
        }
    }
}
