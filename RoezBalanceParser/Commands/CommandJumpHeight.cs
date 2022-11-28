using SDG.Unturned;
using Steamworks;
using UnityEngine;

namespace RoezBalanceParser.Commands;
internal class CommandJumpHeight : Command
{
    public CommandJumpHeight()
    {
        _command = "jh";
    }

    protected override void execute(CSteamID executorID, string parameter)
    {
        var player = Player.player;

        var componentsFromSerial = Parser.getComponentsFromSerial(parameter, ' ');
        if (componentsFromSerial.Length == 0)
        {
            ChatManager.receiveChatMessage(executorID, string.Empty, EChatMode.SAY, Color.white, false, "Invalid args");
            return;
        }

        if (!float.TryParse(componentsFromSerial[0], out var jump))
        {
            ChatManager.receiveChatMessage(executorID, string.Empty, EChatMode.SAY, Color.white, false, "Invalid args");
            return;
        }

        var position = player.transform.position;
        position.y += jump;

        player.transform.position = position;
    }
}
