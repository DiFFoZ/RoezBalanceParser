using SDG.Unturned;
using Steamworks;

namespace RoezBalanceParser.Commands;
internal class CommandHeal : Command
{
    public CommandHeal()
    {
        _command = "heal";
    }

    protected override void execute(CSteamID executorID, string parameter)
    {
        if (Player.player)
        {
            Player.player.life.askHeal(100, true, true);
        }
    }
}
