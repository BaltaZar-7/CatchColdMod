#nullable disable
using AfflictionComponent.Components;
using CatchColdMod.Afflictions;
using Il2Cpp;
using MelonLoader;

namespace CatchColdMod
{
    internal static class ColdUtils
    {
        public static bool IsEscalatingToPneumonia => _isEscalatingToPneumonia;
        private static bool _isEscalatingToPneumonia = false;
        public static float NextRiskAllowedHour = 0f;


        public static bool HasActiveCold()
        {
            AfflictionManager manager = AfflictionManager.GetAfflictionManagerInstance();
            if (manager == null)
                return false;

            foreach (CustomAffliction aff in manager.m_Afflictions)
                if (aff is ColdAffliction)
                    return true;

            return false;
        }
       /* public static void ApplyCold()
        {
            if (_isEscalatingToPneumonia)
                return;

            AfflictionManager manager = AfflictionManager.GetAfflictionManagerInstance();
            if (manager == null)
                return;

            SaveDataManager.FailedColdRolls = 0;
            RemoveColdResistance();


            SaveDataManager.ColdRiskMinutes = 0f;

            SaveDataManager.ColdPhase++;

            if (SaveDataManager.ColdPhase == 2)
            {
                HUDMessage.AddMessage("You have caught a cold again without healing, next time it can get serious!");
                MelonLogger.Msg("[CatchColdMod] You have caught a cold again without healing, next time it can get serious!");
            }

            DebugHelper.Log("[Cold] Infection. Phase=" + SaveDataManager.ColdPhase);

            ColdAffliction cold = new ColdAffliction();
            cold.Start();

            TimeOfDay tod = GameManager.GetTimeOfDayComponent();
            if (tod != null)
            {
                ColdUtils.NextRiskAllowedHour = tod.GetHoursPlayedNotPaused() + 0.05f;
            }
            // ===== PNEUMONIA ESCALATION =====
            if (SaveDataManager.ColdPhase >= 3)
            {
                _isEscalatingToPneumonia = true;
                DebugHelper.Log("[Cold] Escalating to Pneumonia");

                SaveDataManager.ColdPhase = 0;

                MelonCoroutines.Start(EscalateToPneumoniaNextFrame());

                return;
            }
        }*/
        public static void ApplyCold()
        {
            if (ColdUtils.IsEscalatingToPneumonia)
                return;

            AfflictionManager manager = AfflictionManager.GetAfflictionManagerInstance();
            if (manager == null)
                return;

            SaveDataManager.FailedColdRolls = 0;
            RemoveColdResistance();
            SaveDataManager.ColdRiskMinutes = 0f;

            SaveDataManager.ColdPhase++;

            if (SaveDataManager.ColdPhase == 2)
            {
                HUDMessage.AddMessage("You have caught a cold again without healing, next time it can get serious!");
                MelonLogger.Msg("[CatchColdMod] You have caught a cold again without healing, next time it can get serious!");
            }

            DebugHelper.Log("[Cold] Infection. Phase=" + SaveDataManager.ColdPhase);

            // ===== PNEUMONIA ESCALATION FIRST =====
            if (SaveDataManager.ColdPhase >= 3)
            {
                _isEscalatingToPneumonia = true;
                DebugHelper.Log("[Cold] Escalating to Pneumonia");

                // 🔥 EZ HIÁNYZOTT AZ ÚJ VERZIÓBÓL
                /*System.Collections.Generic.List<CustomAffliction> afflictionsCopy =
                    new System.Collections.Generic.List<CustomAffliction>(manager.m_Afflictions);

                foreach (CustomAffliction affliction in afflictionsCopy)
                {
                    if (affliction is ColdRiskAffliction)
                    {
                        affliction.Cure(false);
                    }
                }*/

                SaveDataManager.ColdPhase = 0;

                MelonCoroutines.Start(EscalateToPneumoniaSafe());
                return;
            }

            // CSAK HA NINCS ESCALATION
            ColdAffliction cold = new ColdAffliction();
            cold.Start();

            // Autosave csak sikeres infection után
            MelonCoroutines.Start(DelayedSurvivalSave());

            TimeOfDay tod = GameManager.GetTimeOfDayComponent();
            if (tod != null)
            {
                ColdUtils.NextRiskAllowedHour = tod.GetHoursPlayedNotPaused() + 0.05f;
            }
        }
        /*private static System.Collections.IEnumerator EscalateToPneumoniaNextFrame()
        {
            yield return null; // 1 frame várakozás

            AfflictionManager manager = AfflictionManager.GetAfflictionManagerInstance();
            if (manager == null)
            {
                _isEscalatingToPneumonia = false;
                yield break;
            }


            // Cold törlése biztonságosan
            System.Collections.Generic.List<CustomAffliction> copy =
                new System.Collections.Generic.List<CustomAffliction>(manager.m_Afflictions);

            foreach (CustomAffliction aff in copy)
            {
                if (aff is ColdAffliction)
                {
                    aff.Cure(false);
                }
            }
            ApplyPneumonia();
            _isEscalatingToPneumonia = false;
        }*/
        private static System.Collections.IEnumerator EscalateToPneumoniaSafe()
        {
            // Hagyd teljesen kifutni az AC update ciklust
            yield return null;
            yield return null;
            yield return null;
            yield return null;

            AfflictionManager manager = AfflictionManager.GetAfflictionManagerInstance();
            if (manager == null)
            {
                _isEscalatingToPneumonia = false;
                yield break;
            }

            // 🔥 VISSZAFELÉ ITERÁLÁS
            for (int i = manager.m_Afflictions.Count - 1; i >= 0; i--)
            {
                CustomAffliction aff = manager.m_Afflictions[i];

                if (aff is ColdAffliction)
                {
                    aff.Cure(false);
                }
            }

            // adjunk még 1 frame-et a törlés után
            yield return null;

            ApplyPneumonia();

            _isEscalatingToPneumonia = false;
        }

        public static void ApplyPneumonia()
        {
            AfflictionManager manager = AfflictionManager.GetAfflictionManagerInstance();
            if (manager == null)
                return;

            if (manager.HasAfflictionOfType(typeof(PneumoniaAffliction)))
                return;

            PneumoniaAffliction pneumonia = new PneumoniaAffliction();
            pneumonia.Start();

            MelonCoroutines.Start(DelayedSurvivalSave());
        }
        public static void ActivateColdResistance()
        {
            AfflictionManager manager = AfflictionManager.GetAfflictionManagerInstance();
            if (manager == null)
                return;

            if (manager.HasAfflictionOfType(typeof(ColdResistance)))
                return;

            ColdResistance buff = new ColdResistance();
            buff.Start();

            DebugHelper.Log("[ColdResistance] Activated");
        }
        private static void RemoveColdResistance()
        {
            AfflictionManager manager = AfflictionManager.GetAfflictionManagerInstance();
            if (manager == null)
                return;

            foreach (CustomAffliction affliction in manager.m_Afflictions)
            {
                ColdResistance buff = affliction as ColdResistance;
                if (buff != null)
                {
                    buff.Cure(false);
                    break;
                }
            }
        }
        public static System.Collections.IEnumerator DelayedSurvivalSave()
        {
            yield return null;
            yield return null;

            GameManager.TriggerSurvivalSaveAndDisplayHUDMessage();
        }
    }
}