#nullable disable
using HarmonyLib;
using Il2Cpp;
using MelonLoader;
using ModData;
using Newtonsoft.Json;
using System;

namespace CatchColdMod
{
    internal static class SaveDataManager
    {
        private static readonly ModDataManager ModData =
            new ModDataManager(nameof(CatchColdMod));

        private const string SUFFIX = "colddata";

        internal static int ColdPhase;
        public static float ColdRiskMinutes;
        internal static int FailedColdRolls;
        internal static int PneumoniaDosesTaken;


        // ================= SAVE =================

        internal static void OnSaveGame()
        {
            ModSaveData data = new ModSaveData
            {
                ColdPhase = ColdPhase,
                ColdRiskMinutes = ColdRiskMinutes,
                FailedColdRolls = FailedColdRolls,
                PneumoniaDosesTaken = PneumoniaDosesTaken
            };


            string json = JsonConvert.SerializeObject(data);
            ModData.Save(json, SUFFIX);

            DebugHelper.Log("[Cold] Saved");
        }

        // ================= LOAD =================

        internal static void OnLoadGame()
        {
            string json = ModData.Load(SUFFIX);

            if (string.IsNullOrEmpty(json))
            {
                OnNewgame();
                return;
            }

            ModSaveData data = JsonConvert.DeserializeObject<ModSaveData>(json);

            ColdPhase = data?.ColdPhase ?? 0;
            FailedColdRolls = data != null ? data.FailedColdRolls : 0;
            ColdRiskMinutes = data?.ColdRiskMinutes ?? 0f;
            PneumoniaDosesTaken = data?.PneumoniaDosesTaken ?? 0;

            DebugHelper.Log("[Cold] Loaded");
        }

        // ================= NEW / RESET =================

        internal static void OnNewgame()
        {
            ColdPhase = 0;
            ColdRiskMinutes = 0f;
            FailedColdRolls = 0;

            PneumoniaDosesTaken = 0;

            DebugHelper.Log("[Cold] Clearing data for new game");
        }
    }

    // =====================================================
    // PATCHES – EXACT STOLENMEAT PATTERN
    // =====================================================

    // SAVE
    [HarmonyPatch(typeof(SaveGameSlots), nameof(SaveGameSlots.WriteSlotToDisk), new Type[] { typeof(SlotData), typeof(SaveGameSlots.Timestamp) })]
    internal class SavePatch
    {
        private static void Prefix()
        {
            SaveDataManager.OnSaveGame();
        }
    }

    [HarmonyPatch(typeof(GameManager), nameof(GameManager.LoadSaveGameSlot), new Type[] { typeof(string), typeof(int) })]
    internal class LoadPatch
    {
        private static void Postfix()
        {
            SaveDataManager.OnLoadGame();
            TimeOfDay tod = GameManager.GetTimeOfDayComponent();
            if (tod != null)
            {
                ColdManager.BlockRiskForSeconds(1f);
            }
        }
    }

    [HarmonyPatch(typeof(SaveGameSlots), nameof(SaveGameSlots.CreateSlot), new Type[] { typeof(string), typeof(SaveSlotType), typeof(uint), typeof(Episode) })]
    internal class NewGamePatch
    {
        private static void Postfix()
        {
            SaveDataManager.OnNewgame();
        }
    }

    [HarmonyPatch(typeof(GameManager), nameof(GameManager.DoExitToMainMenu))]
    [HarmonyPatch(typeof(GameManager), nameof(GameManager.LoadMainMenu))]
    internal class MainMenuPatch
    {
        private static void Postfix()
        {
            SaveDataManager.OnNewgame();
        }
    }
}