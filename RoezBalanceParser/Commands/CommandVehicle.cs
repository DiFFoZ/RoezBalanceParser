using SDG.Unturned;
using Steamworks;
using UnityEngine;

namespace RoezBalanceParser.Commands;
internal class CommandVehicle : Command
{
    public CommandVehicle()
    {
        _command = "v";
    }

    protected override void execute(CSteamID executorID, string parameter)
    {
        var steamPlayer = Player.player.channel.owner;
        var componentsFromSerial = Parser.getComponentsFromSerial(parameter, ' ');
        if (componentsFromSerial.Length == 0)
        {
            ChatManager.receiveChatMessage(executorID, string.Empty, EChatMode.SAY, Color.white, false, "Invalid args");
            return;
        }

        var text = componentsFromSerial[0];
        if (Guid.TryParse(text, out var guid))
        {
            var asset = Assets.find(guid);
            if (asset is VehicleAsset)
            {
                VehicleTool.giveVehicle(steamPlayer.player, asset.id);
                return;
            }
        }
        else
        {
            if (!ushort.TryParse(text, out var vehicleId))
            {
                ChatManager.receiveChatMessage(CSteamID.Nil, string.Empty, EChatMode.SAY, Color.white, false, "No vehicle with that ID");
                return;
            }
            VehicleTool.giveVehicle(steamPlayer.player, vehicleId);
        }
    }
}
