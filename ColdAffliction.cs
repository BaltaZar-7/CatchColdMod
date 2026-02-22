#nullable disable
using AfflictionComponent.Components;
using AfflictionComponent.Enums;
using AfflictionComponent.Interfaces;
using Il2Cpp;
using MelonLoader;
using System;
using System.Collections;
using UnityEngine;

namespace CatchColdMod.Afflictions
{
	public class ColdAffliction : CustomAffliction, IDuration, IRemedies, IInstance
	{
        public InstanceType Type { get; set; } = InstanceType.Single;

		public float Duration { get; set; } = CatchColdSettings.Instance.ColdDurationDays * 24f;
        public float EndTime { get; set; }
        public bool InstantHeal { get; set; } = false;
        public static bool IsColdActive { get; private set; }
        public bool IsEscalatingCold { get; set; }


        public Tuple<string, int, int>[] RemedyItems { get; set; } =
		{
			Tuple.Create("GEAR_BirchbarkTea", 1, 99),
			Tuple.Create("GEAR_ReishiTea", 1, 99),
		};

        public Tuple<string, int, int>[] AltRemedyItems { get; set; } =
        {
			Tuple.Create("GEAR_GreenTeaCup", 1, 99),
			Tuple.Create("GEAR_RoseHipTea", 1, 99),
		};

		public ColdAffliction(): base("Cold", "Too much freezing, less warming up.",
            "You caught a cold. You recover if the affliction's timer runs out. You can shorten the duration by drinking teas, and sleep. Max condition is reduced, fatigue and hypothermia rate is increased.",
            null, "CatchColdMod.Resources.cold.png", AfflictionBodyArea.Chest, true)
		{
			float now = GameManager.GetTimeOfDayComponent().GetHoursPlayedNotPaused();
			EndTime = now + Duration;
		}

        public override void OnUpdate()
        {
            if (IsEscalatingCold)
            {
                Cure(false);
                return;
            }

            IsColdActive = true;
        }

        public void OnFoundExistingInstance(CustomAffliction existing)
        {
            ColdAffliction cold = existing as ColdAffliction;
            if (cold == null)
                return;

            AfflictionManager manager = AfflictionManager.GetAfflictionManagerInstance();
            if (manager == null)
                return;

            if (ColdUtils.IsEscalatingToPneumonia)
            {
                return;
            }

            if (IsEscalatingCold)
            {
                cold.IsEscalatingCold = true;
            }

            DebugHelper.Log("[CatchColdMod] Reinfected. Phase=" + SaveDataManager.ColdPhase);

            TimeOfDay tod = GameManager.GetTimeOfDayComponent();
            if (tod != null)
            {
                float now = tod.GetHoursPlayedNotPaused();
                cold.EndTime = now + cold.Duration;
            }

            cold.ResetAffliction(false);
        }
        public void CureSymptoms()
		{
			// none
		}

		public void OnCure()
		{
			IsColdActive = false;
            SaveDataManager.ColdRiskMinutes = 0f;
            SaveDataManager.ColdPhase = 0;
            DebugHelper.Log("[CatchColdMod] Cold cured");
        }
        public void MarkRemedyTaken()
        {
        }
    }
}