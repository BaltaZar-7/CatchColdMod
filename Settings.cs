#nullable disable
using MelonLoader;
using ModSettings;

namespace CatchColdMod
{
    internal class CatchColdSettings : JsonModSettings
    {
        public static CatchColdSettings Instance { get; private set; }

        [Section("Cold Risk")]

        [Name("Cold Risk Timer")]
        [Description("Number of hours when the cold risk timer rolls its dice for the affliction. Default: 6 hours")]
        [Slider(2, 24)]
        public float RiskThresholdHours = 6;

        [Name("Cold Risk Roll Chance")]
        [Description("Percentage value indicating the likelihood of catching a cold when the roll comes. Default: 80%. -Modified further by: Vitamin C level, Cold Resistance buff")]
        [Slider(50, 100)]
        public float BaseRiskToColdChance = 80;

        [Section("Cold Affliction")]

        [Name("Cold Affliction base duration")]
        [Description("The base duration of the Cold affliction in days. It can be shortened with teas and sleep. Default: 7 days")]
        [Slider(3, 14)]
        public float ColdDurationDays = 7;

        [Section("Pneumonia")]

        [Name("Pneumonia Affliction duration")]
        [Description("The duration of the Pneumonia affliction. It is not advisable to set this a high number, as this affliction comes with constant condition damage. Default: 7 days; -The number of doses is cannot be modified (6 doses)")]
        [Slider(6, 10)]
        public float PneumoniaDurationDays = 7;

        [Name("Reishi and burdock tea as remedy for Pneumonia")]
        [Description("Should these teas be used to treat Pneumonia? Default: No")]
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
