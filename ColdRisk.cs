#nullable disable
using AfflictionComponent.Components;
using AfflictionComponent.Enums;
using AfflictionComponent.Interfaces;
using Il2Cpp;
using Il2CppTLD.Gameplay;
using MelonLoader;
using UnityEngine;
using Random = UnityEngine.Random;

namespace CatchColdMod.Afflictions
{
    public class ColdRiskAffliction : CustomAffliction, IRiskPercentage, IInstance
    {
        public InstanceType Type { get; set; } = InstanceType.Single;
        public bool Risk { get; set; } = true;

        private float m_RiskMinutes;
        private float m_LastUpdateHour;

        public ColdRiskAffliction(): base("Cold Risk","The cold","You are at risk of catching a cold. To decrease the risk, warm yourself up completely!",null, "CatchColdMod.Resources.snowflake.png", AfflictionBodyArea.Chest, true)
        {
            m_LastUpdateHour = GameManager.GetTimeOfDayComponent().GetHoursPlayedNotPaused();
            m_RiskMinutes = SaveDataManager.ColdRiskMinutes;
        }

        public float GetRiskValue()
        {
            float threshold = CatchColdSettings.Instance.RiskThresholdHours * 60f;
            return Mathf.Clamp01(m_RiskMinutes / threshold) * 100f;
        }

        public void UpdateRiskValue()
        {

            float now = GameManager.GetTimeOfDayComponent().GetHoursPlayedNotPaused();
            float deltaMinutes = Mathf.Clamp((now - m_LastUpdateHour) * 60f, 0f, 5f);
            m_LastUpdateHour = now;

            Freezing freezing = GameManager.GetFreezingComponent();
            if (freezing == null)
                return;

            bool isColdOrWorse =
                freezing.m_CurrentFreezing > freezing.m_ColdThreshold;

            bool isMaxWarm =
                freezing.m_CurrentFreezing <= 0f;

            if (isColdOrWorse)
            {
                m_RiskMinutes += deltaMinutes;
            }
            else if (isMaxWarm)
            {
                m_RiskMinutes -= deltaMinutes;
            }

            float threshold = CatchColdSettings.Instance.RiskThresholdHours * 60f;
            m_RiskMinutes = Mathf.Clamp(m_RiskMinutes, 0f, threshold);
            SaveDataManager.ColdRiskMinutes = m_RiskMinutes;

            if (GetRiskValue() >= 100f)
            {
                Risk = false;

                TryTriggerCold();

                SaveDataManager.ColdRiskMinutes = 0f;
                m_RiskMinutes = 0f;

                Cure(false);

                return;
            }
        }

        public override void OnUpdate()
        {
            if (!Risk)
                return;

            UpdateRiskValue();
        }

        private void TryTriggerCold()
        {
            float vitaminModifier = 0f;

            ScurvyManager scurvy = GameManager.GetScurvyComponent();
            if (scurvy != null)
            {
                float normalized = scurvy.GetVitaminCNormalized();
                DebugHelper.Log("[CatchColdMod] VitaminC normalized: " + normalized);

                if (normalized < 0.2f) //below 100 units of vitamin C 
                {
                    vitaminModifier = 20f; // +20%
                    DebugHelper.Log("[CatchColdMod] Very low Vitamin C (<20%) → +20% cold chance");
                }
                else if (normalized < 0.5f) //below half units (250) of vitamin C 
                {
                    vitaminModifier = 5f; // +5%
                    DebugHelper.Log("[CatchColdMod] Low Vitamin C (<50%) → +10% cold chance");
                }
                else if (normalized > 0.8f) //above 400 units of vitamin C
                {
                    vitaminModifier = -5f; // -5%
                    DebugHelper.Log("[CatchColdMod] High Vitamin C (>80%) → -10% cold chance");
                }
            }

            float roll = Random.Range(0f, 100f);

            AfflictionManager manager = AfflictionManager.GetAfflictionManagerInstance();
            float resistanceBonus = 0f;

            if (manager != null &&
                manager.HasAfflictionOfType(typeof(ColdResistance)))
            {
                resistanceBonus = 5f;
                DebugHelper.Log("[CatchColdMod] ColdResistance active → -5% cold chance");
            }

            float baseChance = CatchColdSettings.Instance.BaseRiskToColdChance;

            float finalChance = baseChance
                                + vitaminModifier
                                - resistanceBonus;

            finalChance = Mathf.Clamp(finalChance, 0f, 100f);

            MelonLogger.Msg("[CatchColdMod] Final cold chance: " + finalChance + "% | Roll: " + roll);

            if (roll < finalChance)
            {
                MelonLogger.Msg("[CatchColdMod] You caught a Cold!");
                ColdUtils.ApplyCold();
            }
            else
            {
                MelonLogger.Msg("[CatchColdMod] Lucky you! You avoided getting a Cold.");
                SaveDataManager.FailedColdRolls++;
                HUDMessage.AddMessage("You have avoided getting a Cold!");

                if (SaveDataManager.FailedColdRolls >= 2)
                {
                    ColdUtils.ActivateColdResistance();
                }
            }
        }
        public void OnFoundExistingInstance(CustomAffliction existing)
        {
            if (existing is ColdRiskAffliction risk)
            {
                m_RiskMinutes = Mathf.Max(m_RiskMinutes, risk.m_RiskMinutes);
            }
        }
        public void OnCure()
        {

        }
    }
}