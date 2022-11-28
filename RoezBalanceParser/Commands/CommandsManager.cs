using HarmonyLib;
using SDG.Unturned;
using System.Reflection;

namespace RoezBalanceParser.Commands;
internal class CommandsManager
{
    internal void Load()
    {
        Main.OnCommandsCreated += Main_OnCommandsCreated;
    }

    private void Main_OnCommandsCreated()
    {
        var types = new List<Type>();
        try
        {
            types = typeof(CommandsManager).Assembly.GetTypes().ToList();
        }
        catch (ReflectionTypeLoadException rtle)
        {
            types = rtle.Types.ToList();
        }

        var commands = types.Where(x => x.BaseType == (typeof(Command))).ToList();
        foreach (var type in commands)
        {
            if (Activator.CreateInstance(type) is not Command command)
                continue;

            Commander.register(command);
        }

        CommandWindow.Log("registered " + commands.Count);
    }
}
