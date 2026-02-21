#nullable disable
using AfflictionComponent.Components;
using AfflictionComponent.Enums;
using AfflictionComponent.Interfaces;
using Il2Cpp;
using MelonLoader;
using System;
using UnityEngine;

namespace CatchColdMod
{
    public class PneumoniaAffliction : CustomAffliction, IDuration, IRemedies, IInstance
    {
        public InstanceType Type { get; set; } = InstanceType.Single;
        public float Duration { get; set; }
        public float EndTime { get; set; }
        public bool InstantHeal { get; set; } = false;
        public static bool IsPneumoniaActive { get; private set; }

        private bool m_TimerLocked;
        public float LastDoseTime = -1f;
        public float FullDuration;
        private bool m_DoseReminderShown;
        public int DosesTaken
        {
            get { return SaveDataManager.PneumoniaDosesTaken; }
            set { SaveDataManager.PneumoniaDosesTaken = value; }
        }

        public Tuple<string, int, int>[] RemedyItems { get; set; } =
        {
            Tuple.Create("GEAR_BottleAntibiotics", 6, 6),
        };

        public Tuple<string, int, int>[] AltRemedyItems { get; set; }

        public PneumoniaAffliction()
            : base("Pneumonia",
                  "You caught Cold 3 times in a row.",
                  "Your cold got worse, your condition depletes constantly. This is a serious condition, you must sleep and take antibiotics in order to survive. You need to take your doses one a day, until the treatment is fully done, you won't recover.",
                  null,
                  "ico_injury_suffocation",
                  AfflictionBodyArea.Chest)
        {
            FullDuration = CatchColdSettings.Instance.PneumoniaDurationDays * 24f;
            Duration = FullDuration;

            float now = GameManager.GetTimeOfDayComponent().GetHoursPlayedNotPaused();
            EndTime = now + Duration;

            if (CatchColdSettings.Instance.TeasCountForPneumonia)
            {
                AltRemedyItems = new Tuple<string, int, int>[]
                {
                 Tuple.Create("GEAR_ReishiTea", 6, 6),
                 Tuple.Create("GEAR_BurdockTea", 6, 6),
                };
            }
            else
            {
                AltRemedyItems = new Tuple<string, int, int>[0];
            }
        }
        public override void OnUpdate()
        {
            IsPneumoniaActive = true;

            CheckDurationGate();
            ApplyHealthDrain();
            CheckDoseReminder();
        }
        private void CheckDoseReminder()
        {
            if (DosesTaken == 0)
                return;

            if (DosesTaken >= 6)
                return;

            float now = GameManager.GetTimeOfDayComponent().GetHoursPlayedNotPaused();
            float hoursSinceLastDose = now - LastDoseTime;

            if (!m_DoseReminderShown && hoursSinceLastDose >= 24f)
            {
                HUDMessage.AddMessage("Next antibiotic dose for pneumonia is timely.");
                MelonLogger.Msg("[CatchColdMod] Next antibiotic dose for pneumonia is timely!");
                m_DoseReminderShown = true;
            }

            if (hoursSinceLastDose < 24f)
            {
                m_DoseReminderShown = false;
            }
        }

        // ================= TIMER GATE =================
        // To prevent duration end without the full treatment
        private void CheckDurationGate()
        {
            TimeOfDay tod = GameManager.GetTimeOfDayComponent();
            if (tod == null)
                return;

            float now = tod.GetHoursPlayedNotPaused();
            float minutesRemaining = (EndTime - now) * 60f;

            if (minutesRemaining <= 1f)
            {
                // Lock if full treatment isnt done
                if (DosesTaken < 6)
                {
                    EndTime = now + (1f / 60f);
                    m_TimerLocked = true;
                }
                else if (m_TimerLocked)
                {
                    // Unlock
                    EndTime = now + (1f / 60f);
                    m_TimerLocked = false;
                }
            }
        }

        // ================= DAMAGE =================
        private float _lastWholeMinute = -1f;

        private float GetCurrentWholeMinute()
        {
            TimeOfDay tod = GameManager.GetTimeOfDayComponent();
            if (tod == null)
                return 0f;

            return Mathf.Floor(tod.GetHoursPlayedNotPaused() * 60f);
        }

        private void ApplyHealthDrain()
        {
            Condition cond = GameManager.GetConditionComponent();
            if (cond == null)
                return;

            float currentMinute = GetCurrentWholeMinute();

            if (_lastWholeMinute < 0f)
            {
                _lastWholeMinute = currentMinute;
                return;
            }

            float minuteDelta = currentMinute - _lastWholeMinute;

            if (minuteDelta <= 0f)
                return;

            _lastWholeMinute = currentMinute;

            PlayerManager playerManager = GameManager.GetPlayerManagerComponent();
            bool isSleeping = playerManager != null && playerManager.PlayerIsSleeping();

            float hpPerHourAwake = 18f;
            float hpPerHourSleeping = 2.1f;

            float hpPerMinute = (isSleeping ? hpPerHourSleeping : hpPerHourAwake) / 60f;

            // sleep safety - if you sleep you cant die from pneumonia damage
            if (isSleeping)
            {
                float normalized = cond.GetNormalizedCondition();

                if (normalized < 0.1f)
                {
                    DebugHelper.Log("[Pneumonia] Sleep drain prevented (normalized < 0.1)");
                    return;
                }
            }

            float hpLoss = hpPerMinute * minuteDelta;

            cond.AddHealth(-hpLoss, DamageSource.Unspecified);

            DebugHelper.Log($"[Pneumonia] Drain: {hpLoss} HP ({minuteDelta} min)");
        }
        // ================= REMEDY =================

        public void MarkRemedyTaken()
        {
            DebugHelper.Log("[Pneumonia] Dose taken: " + DosesTaken);
        }

        public bool IsDurationUp()
        {
            TimeOfDay tod = GameManager.GetTimeOfDayComponent();
            if (tod == null)
                return false;

            float now = tod.GetHoursPlayedNotPaused();
            return now >= EndTime;
        }

        public void CureSymptoms() { }

        public void OnCure()
        {
            SaveDataManager.ColdPhase = 0;
            SaveDataManager.ColdRiskMinutes = 0f;
            SaveDataManager.PneumoniaDosesTaken = 0;
            IsPneumoniaActive = false;
            DebugHelper.Log("[Pneumonia] Cured");
        }

        public void OnFoundExistingInstance(CustomAffliction existing)
        {
        }
    }
}