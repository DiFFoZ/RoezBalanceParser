using SDG.Unturned;
using Steamworks;
using UnityEngine;
using Parser = SDG.Unturned.Parser;

namespace RoezBalanceParser.Commands;
internal class CommandItem : Command
{
    public CommandItem()
    {
        _command = "i";
    }

    protected override void execute(CSteamID executorID, string parameter)
    {
        var steamPlayer = Player.player.channel.owner;
        var componentsFromSerial = Parser.getComponentsFromSerial(parameter, ' ');
        if (componentsFromSerial.Length is < 1 or > 2)
        {
            ChatManager.receiveChatMessage(executorID, string.Empty, EChatMode.SAY, Color.white, false, "Invalid args");
            return;
        }

        byte amount = 1;
        if (componentsFromSerial.Length == 2 && !byte.TryParse(componentsFromSerial[1], out amount))
        {
            ChatManager.receiveChatMessage(executorID, string.Empty, EChatMode.SAY, Color.white, false, "Invalid args");
            return;
        }

        var text = componentsFromSerial[0];
        if (Guid.TryParse(text, out var guid))
        {
            var asset = Assets.find(guid);
            if (asset is ItemAsset)
            {
                giveItem(steamPlayer, asset.id, amount);
                return;
            }

            if (asset is ItemCurrencyAsset itemCurrencyAsset)
            {
                itemCurrencyAsset.grantValue(steamPlayer.player, amount);
                return;
            }
        }
        else
        {
            if (!ushort.TryParse(text, out var itemID))
            {
                ChatManager.receiveChatMessage(CSteamID.Nil, string.Empty, EChatMode.SAY, Color.white, false, "No item with that ID");
                return;
            }
            giveItem(steamPlayer, itemID, amount);
        }
    }

    private void giveItem(SteamPlayer player, ushort itemID, byte amount)
    {
        if (!ItemTool.tryForceGiveItem(player.player, itemID, amount))
        {
            ChatManager.receiveChatMessage(CSteamID.Nil, string.Empty, EChatMode.SAY, Color.white, false, "No item with that ID");
        }
    }
}
