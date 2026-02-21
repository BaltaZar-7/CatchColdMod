#nullable disable
using AfflictionComponent.Components;
using CatchColdMod.Afflictions;
using Il2Cpp;
using MelonLoader;
using UnityEngine;

namespace CatchColdMod
{
    // Checks Temperature, starts Cold Risk if conditions are met
    public class ColdManager
    {
        private const float UPDATE_INTERVAL_MINUTES = 1f;
        private const float BUILDUP_THRESHOLD = 50f;
        private const float WARM_RECOVERY_MULTIPLIER = 1f;
        private static float _loadBlockUntilHour = -1f;

        private static ColdManager m_Instance;
        public static ColdManager Instance => m_Instance;

        public ColdManager()
        {
            m_Instance = this;
        }
        public void Update()
        {
            TimeOfDay tod = GameManager.GetTimeOfDayComponent();
            if (tod == null)
                return;

            float now = tod.GetHoursPlayedNotPaused();
            if (now < _loadBlockUntilHour)
                return;

            if (!IsValidGameState())
                return;

            Freezing freezing = GameManager.GetFreezingComponent();
            if (freezing == null)
                return;

            AfflictionManager manager = AfflictionManager.GetAfflictionManagerInstance();
            if (manager == null)
                return;

            if (ColdUtils.IsEscalatingToPneumonia)
                return;

            if (manager.HasAfflictionOfType(typeof(PneumoniaAffliction)))
                return;

            if (now < ColdUtils.NextRiskAllowedHour)
                return;

            if (manager.HasAfflictionOfType(typeof(ColdRiskAffliction)))
                return;

            bool isColdOrWorse = freezing.m_CurrentFreezing >= freezing.m_ColdThreshold;

            if (isColdOrWorse)
                {
                    ColdRiskAffliction risk = new ColdRiskAffliction();
                    risk.Start();
                    DebugHelper.Log("[CatchColdMod] Cold risk started");
                }
        }
        private bool IsValidGameState()
        {
            if (string.IsNullOrEmpty(GameManager.m_ActiveScene))
                return false;

            if (GameManager.m_ActiveScene.Contains("MainMenu"))
                return false;

            return true;
        }


        internal static float GetCurrentIngameMinutesStatic()
        {
            TimeOfDay tod = GameManager.GetTimeOfDayComponent();
            return tod != null ? tod.GetHoursPlayedNotPaused() * 60f : 0f;
        }
        public static void BlockRiskForSeconds(float seconds)
        {
            TimeOfDay tod = GameManager.GetTimeOfDayComponent();
            if (tod == null)
                return;

            float now = tod.GetHoursPlayedNotPaused();
            _loadBlockUntilHour = now + (seconds / 3600f);
        }

        private float GetCurrentIngameMinutes() =>
            GetCurrentIngameMinutesStatic();
    }
}