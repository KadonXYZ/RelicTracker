using HarmonyLib;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.Runs;

[HarmonyPatch(typeof(CombatRoom), "EnterInternal", new Type[] { typeof(IRunState), typeof(bool) })]
public static class CombatStartPatch
{
    static void Postfix()
    {
        CombatStartManager.NotifyCombatStarted();
    }
}

public static class CombatStartManager
{
    public static int _currentCombatId;

    public static void NotifyCombatStarted()
    {
        _currentCombatId++;
    }

    public static bool IsNewCombat(ref int lastSeenCombatId)
    {
        if (lastSeenCombatId == _currentCombatId)
        {
            return false;
        }

        lastSeenCombatId = _currentCombatId;
        return true;
    }
}
