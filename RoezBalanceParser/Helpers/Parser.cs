using RoezBalanceParser.Models.Guns;
using SDG.Unturned;
using System.Globalization;
using System.Text;

namespace RoezBalanceParser.Helpers;
internal static class Parser
{
    public static List<(string tag, string value, bool isFound)> s_LineParsers = new();

    private static void AddTag(string tag, string value)
    {
        s_LineParsers.Add((tag, value, false));
    }

    public static void Parse(IGun gun, string gunPath, string? magazinePath, string range, string playerBaseMultiplier, string playerSkullMultiplier,
        string zombieBaseMultiplier, string zombieSkullMultiplier, string animalBaseMultiplier, string animalSkullMultiplier, out bool hasErrors)
    {
        s_LineParsers.Clear();
        hasErrors = false;

        var gunRarity = EItemRarity.EPIC;
        try
        {
            gunRarity = gun.Tier.Trim().ToLower() switch
            {
                "a" or "а" => EItemRarity.MYTHICAL,
                "b" or "в" => EItemRarity.LEGENDARY,
                "c" or "с" => EItemRarity.EPIC,
                _ => EItemRarity.EPIC
            };

            CommandWindow.Log("Parsed tier as " + gunRarity);
        }
        catch (Exception ex)
        {
            hasErrors = true;
            CommandWindow.LogError(ex);
        }

        var ammo = gun.Ammo.ToString(CultureInfo.InvariantCulture);
        AddTagsForGun(gun, ammo, range, playerBaseMultiplier, playerSkullMultiplier, zombieBaseMultiplier, zombieSkullMultiplier, animalBaseMultiplier,
            animalSkullMultiplier, gunRarity);

        using var gunStream = new FileStream(gunPath, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite);
        using var newStream = new MemoryStream((int)(gunStream.Length + 80));
        using var writer = new StreamWriter(newStream, Encoding.UTF8, 1024, true);
        {
            using var reader = new StreamReader(gunStream, Encoding.UTF8, true, 1024, true);
            {
                ParseLines(reader, writer, s_LineParsers);
            }

            writer.Flush();
            gunStream.Flush();

            // remove last new line \r\n
            newStream.SetLength(newStream.Length - 2);

            newStream.Position = 0;
            gunStream.Position = 0;
            newStream.CopyTo(gunStream);

            gunStream.Flush();
        }

        foreach (var tag in s_LineParsers)
        {
            if (!tag.isFound)
            {
                CommandWindow.LogWarning("Tag " + tag.tag + " doesn't found in .dat file. FIX ME!");
                hasErrors = true;
            }
        }

        if (magazinePath is null)
        {
            return;
        }

        OverrideMagazine(magazinePath, ammo);
    }

    private static void AddTagsForGun(IGun gun, string ammo, string range, string playerBaseMultiplier, string playerSkullMultiplier, string zombieBaseMultiplier,
        string zombieSkullMultiplier, string animalBaseMultiplier, string animalSkullMultiplier, EItemRarity gunRarity)
    {
        AddTag("Rarity", gunRarity.ToString());

        if (gun.Firerate.HasValue)
            AddTag("Firerate", gun.Firerate.Value.ToString(CultureInfo.InvariantCulture));

        AddTag("Ammo_Min", ammo);
        AddTag("Ammo_Max", ammo);

        if (gun.Range != null)
        {
            CommandWindow.Log("Gun override range from " + range + " to " + gun.Range);
            AddTag("Range", gun.Range.Value.ToString(CultureInfo.InvariantCulture));
        }
        else
        {
            AddTag("Range", range);
        }

        if (gun.PlayerDamage != null)
        {
            AddTag("Player_Damage", gun.PlayerDamage.Value.ToString(CultureInfo.InvariantCulture));

            if (gun is not Boom)
            {
                AddTag("Player_Leg_Multiplier", playerBaseMultiplier);
                AddTag("Player_Arm_Multiplier", playerBaseMultiplier);
                AddTag("Player_Spine_Multiplier", playerBaseMultiplier);
                AddTag("Player_Skull_Multiplier", playerSkullMultiplier);
            }
        }
        else
        {
            CommandWindow.Log("Gun doesn't have a player damage. So ignoring those tags:\nPlayer_Damage Player_Leg_Multiplier Player_Arm_Multiplier Player_Spine_Multiplier Player_Skull_Multiplier");
        }

        if (gun.ZombieDamage != null)
        {
            AddTag("Zombie_Damage", gun.ZombieDamage.Value.ToString(CultureInfo.InvariantCulture));

            if (gun is not Boom)
            {
                AddTag("Zombie_Leg_Multiplier", zombieBaseMultiplier);
                AddTag("Zombie_Arm_Multiplier", zombieBaseMultiplier);
                AddTag("Zombie_Spine_Multiplier", zombieBaseMultiplier);
                AddTag("Zombie_Skull_Multiplier", zombieSkullMultiplier);
            }
        }
        else
        {
            CommandWindow.Log("Gun doesn't have a zombie damage. So ignoring those tags:\nZombie_Damage Zombie_Leg_Multiplier Zombie_Arm_Multiplier Zombie_Spine_Multiplier Zombie_Skull_Multiplier");
        }

        if (gun.AnimalDamage != null)
        {
            AddTag("Animal_Damage", gun.AnimalDamage.Value.ToString(CultureInfo.InvariantCulture));

            if (gun is not Boom)
            {
                AddTag("Animal_Leg_Multiplier", animalBaseMultiplier);
                AddTag("Animal_Spine_Multiplier", animalBaseMultiplier);
                AddTag("Animal_Skull_Multiplier", animalSkullMultiplier);
            }
        }
        else
        {
            CommandWindow.Log("Gun doesn't have a animal damage. So ignoring those tags:\nAnimal_Damage Animal_Leg_Multiplier Animal_Spine_Multiplier Animal_Skull_Multiplier");
        }

        if (gun.BarricadeDamage != null)
        {
            AddTag("Barricade_Damage", gun.BarricadeDamage.Value.ToString(CultureInfo.InvariantCulture));
        }
        else
        {
            CommandWindow.Log("Gun doesn't have a barricade damage. So ignoring those tags:\nBarricade_Damage");
        }

        if (gun.StructureDamage != null)
        {
            AddTag("Structure_Damage", gun.StructureDamage.Value.ToString(CultureInfo.InvariantCulture));
        }
        else
        {
            CommandWindow.Log("Gun doesn't have a structure damage. So ignoring those tags:\nStructure_Damage");
        }

        if (gun.VehicleDamage != null)
        {
            AddTag("Vehicle_Damage", gun.VehicleDamage.Value.ToString(CultureInfo.InvariantCulture));
        }
        else
        {
            CommandWindow.Log("Gun doesn't have a vehicle damage. So ignoring those tags:\nVehicle_Damage");
        }

        if (gun.ResourceDamage != null)
        {
            AddTag("Resource_Damage", gun.ResourceDamage.Value.ToString(CultureInfo.InvariantCulture));
        }
        else
        {
            CommandWindow.Log("Gun doesn't have a resource damage. So ignoring those tags:\nResource_Damage");
        }

        if (gun.ObjectDamage != null)
        {
            AddTag("Object_Damage", gun.ObjectDamage.Value.ToString(CultureInfo.InvariantCulture));
        }
        else
        {
            CommandWindow.Log("Gun doesn't have a object damage. So ignoring those tags:\nObject_Damage");
        }
    }

    private static void ParseLines(StreamReader reader, StreamWriter writer, List<(string tag, string value, bool isFound)> tags)
    {
        string? line;
        bool isWrited;

        while ((line = reader.ReadLine()) != null)
        {
            isWrited = false;

            for (var i = 0; i < tags.Count; i++)
            {
                var tag = tags[i];
                if (tag.isFound)
                {
                    continue;
                }

                if (line.StartsWith(tag.tag, StringComparison.InvariantCultureIgnoreCase))
                {
                    writer.WriteLine(tag.tag + ' ' + tag.value);

                    tag.isFound = true;
                    tags[i] = tag;
                    isWrited = true;
                    break;
                }
            }

            if (!isWrited)
            {
                writer.WriteLine(line);
            }
        }
    }

    private static void OverrideMagazine(string path, string ammo)
    {
        const EItemRarity c_MagazineRariry = EItemRarity.COMMON;

        using var gunStream = new FileStream(path, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite);
        using var newStream = new MemoryStream((int)(gunStream.Length + 80));
        using var writer = new StreamWriter(newStream, Encoding.UTF8, 1024, true);
        {
            using var streamReader = new StreamReader(gunStream, Encoding.UTF8, true, 1024, true);
            {
                string? line;

                while ((line = streamReader.ReadLine()) != null)
                {
                    if (line.StartsWith("rarity", StringComparison.InvariantCultureIgnoreCase))
                    {
                        writer.WriteLine("Rarity " + c_MagazineRariry.ToString());

                        continue;
                    }

                    if (line.StartsWith("amount", StringComparison.InvariantCultureIgnoreCase))
                    {
                        writer.WriteLine("Amount " + ammo);

                        continue;
                    }

                    if (line.StartsWith("Count_Min", StringComparison.InvariantCultureIgnoreCase))
                    {
                        writer.WriteLine("Count_Min " + ammo);

                        continue;
                    }

                    if (line.StartsWith("Count_Max", StringComparison.InvariantCultureIgnoreCase))
                    {
                        writer.WriteLine("Count_Max " + ammo);

                        continue;
                    }

                    writer.WriteLine(line);
                }
            }

            writer.Flush();
            gunStream.Flush();

            // remove last new line \r\n
            newStream.SetLength(newStream.Length - 2);

            newStream.Position = 0;
            gunStream.Position = 0;
            newStream.CopyTo(gunStream);

            gunStream.Flush();
        }
    }
}
