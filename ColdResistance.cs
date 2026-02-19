#nullable disable
using AfflictionComponent.Components;
using AfflictionComponent.Enums;
using AfflictionComponent.Interfaces;
using Il2Cpp;
using UnityEngine;

namespace CatchColdMod
{
    public class ColdResistance : CustomAffliction, IInstance, IBuff
    {
        public InstanceType Type { get; set; } = InstanceType.Single;
        public bool Buff { get; set; } = true;
        public bool BuffCold { get; set; }
        public bool BuffFatigue { get; set; }
        public bool BuffHunger { get; set; }
        public bool BuffThirst { get; set; }
        public static bool IsActive { get; private set; }

        public ColdResistance()
            : base("Cold Resistance",
                  "You resisted catching a cold twice in a row",
                  "Your immune system is stronger. You are more likely to be resistant to catching a cold and you gained 1 °C bonus for temperature. You lose this bonus if you catch a cold.",
                  null,
                  "ico_injury_pain",
                  AfflictionBodyArea.Chest)
        {
        }

        public override void OnUpdate()
        {
            IsActive = true;
        }

        public void OnFoundExistingInstance(CustomAffliction existing)
        {
            // nincs reinfection logika
        }

        public void OnCure()
        {
            IsActive = false;
            DebugHelper.Log("[ColdResistance] Removed");
        }
    }
}
