using HarmonyLib;
using SDG.Unturned;

namespace RoezBalanceParser.Patches;
[HarmonyPatch(typeof(Zombie))]
internal static class Patch_Zombie
{
    [HarmonyPatch("reset")]
    [HarmonyPostfix]
    public static void reset(Zombie __instance, ref ushort ___health, ref ushort ___maxHealth)
    {
        var tableHealth = LevelZombies.tables[__instance.type].health;

        ___health = tableHealth;
        ___maxHealth = tableHealth;
    }
}
