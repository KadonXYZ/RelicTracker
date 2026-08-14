using HarmonyLib;
using MegaCrit.Sts2.Core.Models.Relics;
using MegaCrit.Sts2.Core.Rooms;

[HarmonyPatch(typeof(DataDisk), nameof(DataDisk.AfterRoomEntered))]
public static class DataDiskPatch
{
    static void Postfix(DataDisk __instance, AbstractRoom room)
    {
        if (room is not CombatRoom)
        {
            return;
        }

        int focusGained = 1;
        if (__instance.DynamicVars.ContainsKey("Focus"))
        {
            focusGained = __instance.DynamicVars["Focus"].IntValue;
        }

        RelicStatCache.RecordCustomStat(__instance.Id.Entry, new List<int> { focusGained });
    }
}
