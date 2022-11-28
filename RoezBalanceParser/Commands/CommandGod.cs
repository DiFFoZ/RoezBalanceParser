using SDG.Unturned;
using Steamworks;
using UnityEngine;

namespace RoezBalanceParser.Commands;
internal class CommandGod : Command
{
    private bool m_IsGod;

    public CommandGod()
    {
        _command = "god";
    }

    protected override void execute(CSteamID executorID, string parameter)
    {
        if (m_IsGod)
        {
            m_IsGod = false;

            Player.player.life.onHurt -= Life_onHurt;
        }
        else
        {
            m_IsGod = true;

            Player.player.life.onHurt += Life_onHurt;
        }

        ChatManager.receiveChatMessage(executorID, string.Empty, EChatMode.SAY, Color.white, false, "Am I God? " + m_IsGod);
    }

    private void Life_onHurt(Player player, byte damage, UnityEngine.Vector3 force, EDeathCause cause, ELimb limb, CSteamID killer)
    {
        player.life.askHeal(100, true, true);
    }
}
