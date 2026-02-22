#nullable disable
using AfflictionComponent.Components;
using CatchColdMod.Afflictions;
using HarmonyLib;
using Il2Cpp;
using Il2CppParadoxNotion;
using MelonLoader;
using System.Collections;
using UnityEngine;

namespace CatchColdMod

{
    // Cold - reduce duration Teas (firstaid types)
    [HarmonyPatch(typeof(PlayerManager), "FirstAidConsumed")]
    internal static class ColdTeaPatch
    {
        private static void Postfix(GearItem gi)
        {
            if (gi == null)
                return;

            if (!ColdAffliction.IsColdActive)
                return;

            string name = gi.name;

            if (!IsColdTea(name))
                return;

            AfflictionManager manager = AfflictionManager.GetAfflictionManagerInstance();
            if (manager == null)
                return;

            for (int i = 0; i < manager.m_Afflictions.Count; i++)
            {
                ColdAffliction cold = manager.m_Afflictions[i] as ColdAffliction;
                if (cold == null)
                    continue;

                float now = GameManager.GetTimeOfDayComponent().GetHoursPlayedNotPaused();

                cold.EndTime -= 2f;
                cold.MarkRemedyTaken();

                HUDMessage.AddMessage("Tea reduced Cold duration by 2h");
                MelonLogger.Msg("[CatchColdMod] Tea reduced Cold duration by 2h");

                break;
            }
        }

        private static bool IsColdTea(string name)
        {
            return name == "GEAR_ReishiTea"
                || name == "GEAR_BurdockTea"
                || name == "GEAR_RoseHipTea"
                || name == "GEAR_BirchbarkTea";
        }
    }
    // Cold - reduce duration Teas (food types)
    [HarmonyPatch(typeof(PlayerManager), nameof(PlayerManager.OnEatingComplete),
    new Type[] { typeof(bool), typeof(bool), typeof(float) })]
    internal static class ColdTeaPatchFood
    {
        private static void Postfix(bool success, bool playerCancel, float progress)
        {
            if (!success || playerCancel)
                return;

            PlayerManager player = GameManager.GetPlayerManagerComponent();
            if (player == null)
                return;

            GearItem eatenGear = player.m_FoodItemEaten ?? player.m_FoodItemOpened;
            if (eatenGear == null)
                return;

            string gearName = eatenGear.name;
            if (string.IsNullOrEmpty(gearName))
                return;

            if (gearName != "GEAR_GreenTeaCup" &&
                gearName != "GEAR_PineNeedleTea")
                return;

            if (!ColdAffliction.IsColdActive)
                return;

            AfflictionManager manager = AfflictionManager.GetAfflictionManagerInstance();
            if (manager == null)
                return;

            TimeOfDay tod = GameManager.GetTimeOfDayComponent();
            if (tod == null)
                return;

            float now = tod.GetHoursPlayedNotPaused();

            for (int i = 0; i < manager.m_Afflictions.Count; i++)
            {
                ColdAffliction cold = manager.m_Afflictions[i] as ColdAffliction;
                if (cold == null)
                    continue;

                cold.EndTime -= 2f;

                HUDMessage.AddMessage("Tea reduced Cold duration by 2 hours.");
                MelonLogger.Msg("[CatchColdMod] Tea reduced Cold duration by 2 hours.");
                break;
            }
        }
    }
    // Pneumonia remedies
    [HarmonyPatch(typeof(PlayerManager), "FirstAidConsumed")]
    internal static class PneumoniaAntibioticPatch
    {
        private static void Postfix(GearItem gi)
        {
            if (gi == null)
                return;

            string name = gi.name;

            bool isAntibiotic = name == "GEAR_BottleAntibiotics";

            bool isTea =
                name == "GEAR_ReishiTea" ||
                name == "GEAR_BurdockTea";

            if (!isAntibiotic)
            {
                if (!CatchColdSettings.Instance.TeasCountForPneumonia)
                    return;

                if (!isTea)
                    return;
            }

            AfflictionManager manager = AfflictionManager.GetAfflictionManagerInstance();
            if (manager == null)
                return;

            TimeOfDay tod = GameManager.GetTimeOfDayComponent();
            if (tod == null)
                return;

            float now = tod.GetHoursPlayedNotPaused();

            for (int i = 0; i < manager.m_Afflictions.Count; i++)
            {
                PneumoniaAffliction pneumonia =
                    manager.m_Afflictions[i] as PneumoniaAffliction;

                if (pneumonia == null)
                    continue;

                float hoursSinceLastDose = now - pneumonia.LastDoseTime;

                if (pneumonia.DosesTaken > 0 && hoursSinceLastDose < 24f)
                {
                    HUDMessage.AddMessage("Too early for next dose.");
                    return;
                }

                if (pneumonia.DosesTaken >= 6)
                {
                    HUDMessage.AddMessage("All doses taken. Wait for recovery.");
                    return;
                }

                pneumonia.DosesTaken++;
                pneumonia.LastDoseTime = now;
                pneumonia.MarkRemedyTaken();

                HUDMessage.AddMessage(
                    "Antibiotic dose taken (" + pneumonia.DosesTaken + "/6). Wait 24 hours for the next dose!"
                );

                break;
            }
        }
    }
    // Cold - Sleep counts double (duration reduction)
    [HarmonyPatch(typeof(Rest), nameof(Rest.EndSleeping))]
    internal static class ColdSleepPatch
    {
        private static void Prefix(Rest __instance)
        {
            AfflictionManager manager = AfflictionManager.GetAfflictionManagerInstance();
            if (manager == null ||
                !manager.HasAfflictionOfType(typeof(ColdAffliction)))
                return;

            float hours = __instance.m_SleepDurationHours;
            if (hours <= 0f)
                return;

            foreach (CustomAffliction aff in manager.m_Afflictions)
            {
                if (aff is ColdAffliction cold)
                {
                    float now = GameManager.GetTimeOfDayComponent().GetHoursPlayedNotPaused();
                    float bonusReduction = hours;
                    cold.EndTime -= bonusReduction;
                    break;
                }
            }
        }
    }
    // Cold - max hp cap
    [HarmonyPatch(typeof(Condition), nameof(Condition.GetAdjustedMaxHPModifier))]
    internal static class ColdMaxHPModifierPatch
    {
        private static void Postfix(ref float __result)
        {
            AfflictionManager manager = AfflictionManager.GetAfflictionManagerInstance();
            if (manager == null ||
                !manager.HasAfflictionOfType(typeof(ColdAffliction)))
                return;

            __result -= 10f; // -10 max HP
        }
    }
    // Pneumonia - max hp cap
    [HarmonyPatch(typeof(Condition), nameof(Condition.GetAdjustedMaxHPModifier))]
    internal static class PneumoniaMaxHPModifierPatch
    {
        private static void Postfix(ref float __result)
        {
            AfflictionManager manager = AfflictionManager.GetAfflictionManagerInstance();
            if (manager == null ||
                !manager.HasAfflictionOfType(typeof(PneumoniaAffliction)))
                return;

            __result -= 40f; // -40 max HP
        }
    }
    // Fatigue rate increase (for both)
    [HarmonyPatch(typeof(Fatigue), "CalculateFatigueIncrease")]
    internal static class ColdFatiguePneumoniaPatch
    {
        private static void Postfix(Fatigue __instance, float realtimeSeconds, ref float __result)
        {
            AfflictionManager manager = AfflictionManager.GetAfflictionManagerInstance();
            if (manager == null)
                return;

            if (manager.HasAfflictionOfType(typeof(PneumoniaAffliction)))
            {
                __result *= 5f;   // 500%
            }
            else if (manager.HasAfflictionOfType(typeof(ColdAffliction)))
            {
                __result *= 1.6f; // 160%
            }
        }
    }
    // Allow unlimited sleep for pneumonia (in order to prevent low fatigue rate settings to not allowed to sleep)
    [HarmonyPatch(typeof(Rest), nameof(Rest.AllowUnlimitedSleep))]
    internal static class ColdUnlimitedSleepPatch
    {
        private static void Postfix(ref bool __result)
        {
            AfflictionManager manager = AfflictionManager.GetAfflictionManagerInstance();
            if (manager == null)
                return;

            if (manager.HasAfflictionOfType(typeof(PneumoniaAffliction)))
            {
                __result = true;
            }
        }
    }
    // Hypothermia causing Cold too
    [HarmonyPatch(typeof(Hypothermia), "HypothermiaEnd")]
    internal static class HypothermiaEndPatch
    {
        private static void Postfix()
        {
            AfflictionManager manager = AfflictionManager.GetAfflictionManagerInstance();
            if (manager == null)
                return;

            if (manager.HasAfflictionOfType(typeof(PneumoniaAffliction)))
                return;

            if (manager.HasAfflictionOfType(typeof(ColdAffliction)))
                return;

            MelonCoroutines.Start(StartColdAfterDelay());
        }

        private static IEnumerator StartColdAfterDelay()
        {
            yield return new WaitForSeconds(1f);
            ColdUtils.ApplyCold();
        }
    }
    // Cold resistance buff get +1 bonus temp
    [HarmonyPatch(typeof(Freezing), "CalculateBodyTemperature")]
    internal static class ColdResistanceTempPatch
    {
        private static void Postfix(ref float __result)
        {
            AfflictionManager manager = AfflictionManager.GetAfflictionManagerInstance();
            if (manager == null)
                return;

            if (manager.HasAfflictionOfType(typeof(ColdResistance)))
            {
                __result += 1f;
            }
        }
    }
    // Hypothermia risk rate increased (both)
    [HarmonyPatch(typeof(Hypothermia), "Update")]
    internal static class HypothermiaThresholdModifierPatch
    {
        private static float _baseRequiredHours = -1f;

        private static void Prefix(Hypothermia __instance)
        {
            if (__instance == null)
                return;

            if (GameManager.IsMainMenuActive())
                return;

            if (_baseRequiredHours < 0f)
                _baseRequiredHours = __instance.m_HoursSpentFreezingRequired;

            AfflictionManager manager = AfflictionManager.GetAfflictionManagerInstance();
            if (manager == null)
                return;

            float modifiedValue = _baseRequiredHours;

            if (manager.HasAfflictionOfType(typeof(PneumoniaAffliction)))
            {
                modifiedValue *= 0.35f; //
            }
            else if (manager.HasAfflictionOfType(typeof(ColdAffliction)))
            {
                modifiedValue *= 0.55f; //
            }

            __instance.m_HoursSpentFreezingRequired = modifiedValue;
        }
    }
}