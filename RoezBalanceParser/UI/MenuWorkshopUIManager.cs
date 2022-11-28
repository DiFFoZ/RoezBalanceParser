using CsvHelper;
using HarmonyLib;
using RoezBalanceParser.Models.Guns;
using SDG.Unturned;
using System.Diagnostics;
using System.Globalization;
using System.Reflection;
using UnityEngine;
using Unturned.SystemEx;
using Unturned.UnityEx;
using Parser = RoezBalanceParser.Helpers.Parser;

namespace RoezBalanceParser.UI;
public class MenuWorkshopUIManager
{
    private const string c_SaveKey = "rbp_path";
    private static readonly FieldInfo s_IconToolsContainerField = AccessTools.Field(typeof(MenuWorkshopUI), "iconToolsContainer");

    private static readonly Dictionary<ushort, decimal> s_ReplacedSells = new()
    {
        { 19359, 50m },
        { 19152, 200 },
        { 19350, 750 }
    };

    private static readonly Dictionary<GunType,
        (float playerSkull, float playerBase, float zombieSkull, float zombieBase, float animalSkull, float animalBase)> s_Multipliers = new()
        {
            { GunType.Raid, (1f, 0.65f, 1f, 0.5f, 1f, 0.65f) },
            { GunType.Sniper, (1f, 0.7f, 1f, 0.5f, 1f, 0.7f) },
            { GunType.Rifle, (1f, 0.65f, 1f, 0.5f, 1f, 0.65f) },
            { GunType.Boom, (1f, 0.65f, 1f, 0.5f, 1f, 0.65f) },
            { GunType.Pistol, (1f, 0.6f, 1f, 0.5f, 1f, 0.6f) },
        };

    private ISleekFloat32Field m_PlayerBaseMultiplier = null!;
    private ISleekFloat32Field m_PlayerSkullMultiplier = null!;

    private ISleekFloat32Field m_ZombieBaseMultiplier = null!;
    private ISleekFloat32Field m_ZombieSkullMultiplier = null!;

    private ISleekFloat32Field m_AnimalBaseMultiplier = null!;
    private ISleekFloat32Field m_AnimalSkullMultiplier = null!;

    private ISleekField m_CsvFilePath = null!;
    private ISleekInt32Field m_Range = null!;
    private ISleekLabel m_ErrorLabel = null!;

    private ISleekToggle[] m_GunToggles = null!;

    public void Load()
    {
        Main.OnMenuUICreated += InitUI;
    }

    private void InitUI()
    {
        if (s_IconToolsContainerField.GetValue(null) is not ISleekElement container)
        {
            return;
        }

        container.isVisible = true;

        var positionOffsetX = 0;
        var positionOffsetY = 175;

        void AddElement<TElement>(Func<TElement> constructor, Action<TElement> modifiers)
                    where TElement : ISleekElement
        {
            var element = constructor();

            element.sizeOffset_X = typeof(TElement) == typeof(ISleekField) ? 500 : 200;
            element.sizeOffset_Y = 25;

            if (typeof(TElement) == typeof(ISleekToggle))
            {
                element.sizeOffset_X = 40;
                element.sizeOffset_Y = 40;
            }

            element.positionOffset_Y = positionOffsetY;
            element.positionOffset_X = positionOffsetX;
            positionOffsetY += 25;

            modifiers(element);

            container.AddChild(element);
        }

        void SkipSpace()
        {
            positionOffsetY += 25;
        }

        AddElement(Glazier.Get().CreateStringField, csvFile =>
        {
            csvFile.maxLength = -1;
            csvFile.addLabel("Path to CSV file", ESleekSide.RIGHT);

            csvFile.text = PlayerPrefs.GetString(c_SaveKey, "");

            m_CsvFilePath = csvFile;
        });

        var array = Enum.GetValues(typeof(GunType));

        m_GunToggles = new ISleekToggle[array.Length];
        for (var i = 0; i < array.Length; i++)
        {
            var type = array.GetValue(i);
            var enumType = (GunType)type;

            AddElement(Glazier.Get().CreateToggle, toggle =>
            {
                m_GunToggles[i] = toggle;

                toggle.addLabel(enumType.ToString(), ESleekSide.RIGHT);
                toggle.onToggled += (_, state) => OnToggled(enumType, state);
            });

            positionOffsetY -= 25;
            positionOffsetX += 100;
        }
        positionOffsetX = 0;

        SkipSpace();
        SkipSpace();

        AddElement(Glazier.Get().CreateInt32Field, range =>
        {
            range.addLabel("Range", ESleekSide.RIGHT);
            range.state = 390;

            m_Range = range;
        });

        SkipSpace();

        AddElement(Glazier.Get().CreateFloat32Field, skullMultiplier =>
        {
            skullMultiplier.addLabel("Player Skull Multiplier", ESleekSide.RIGHT);
            skullMultiplier.state = 1f;

            m_PlayerSkullMultiplier = skullMultiplier;
        });

        AddElement(Glazier.Get().CreateFloat32Field, multiplier =>
        {
            multiplier.addLabel("Player Base Multiplier", ESleekSide.RIGHT);
            multiplier.state = 0.65f;

            m_PlayerBaseMultiplier = multiplier;
        });

        SkipSpace();

        AddElement(Glazier.Get().CreateFloat32Field, skullMultiplier =>
        {
            skullMultiplier.addLabel("Zombie Skull Multiplier", ESleekSide.RIGHT);
            skullMultiplier.state = 1f;

            m_ZombieSkullMultiplier = skullMultiplier;
        });

        AddElement(Glazier.Get().CreateFloat32Field, multiplier =>
        {
            multiplier.addLabel("Zombie Base Multiplier", ESleekSide.RIGHT);
            multiplier.state = 0.5f;

            m_ZombieBaseMultiplier = multiplier;
        });

        SkipSpace();

        AddElement(Glazier.Get().CreateFloat32Field, skullMultiplier =>
        {
            skullMultiplier.addLabel("Animal Skull Multiplier", ESleekSide.RIGHT);
            skullMultiplier.state = 1f;

            m_AnimalSkullMultiplier = skullMultiplier;
        });

        AddElement(Glazier.Get().CreateFloat32Field, multiplier =>
        {
            multiplier.addLabel("Animal Base Multiplier", ESleekSide.RIGHT);
            multiplier.state = 0.5f;

            m_AnimalBaseMultiplier = multiplier;
        });

        SkipSpace();
        SkipSpace();

        AddElement(Glazier.Get().CreateButton, start =>
        {
            start.text = "Override assets";
            start.onClickedButton += OnClickedOverride;
        });

        AddElement(Glazier.Get().CreateButton, logs =>
        {
            logs.text = "Open logs folder";
            logs.onClickedButton += Logs_onClickedButton;
        });

        AddElement(Glazier.Get().CreateButton, chars =>
        {
            chars.text = "Find invalid char";
            chars.onClickedButton += Chars_onClickedButton;
        });

        SkipSpace();

        AddElement(Glazier.Get().CreateLabel, error =>
        {
            m_ErrorLabel = error;
        });
    }

    private void Chars_onClickedButton(ISleekElement button)
    {
        foreach (var item in Assets.find(EAssetType.ITEM).Cast<ItemAsset>())
        {
            using var reader = new StreamReader(item.absoluteOriginFilePath);
            var text = reader.ReadToEnd();

            if (text.Contains("\uFEFF"))
            {
                UnturnedLog.info(item.absoluteOriginFilePath);
            }
        }
    }

    private void OnToggled(GunType enumType, bool state)
    {
        for (var i = 0; i < m_GunToggles.Length; i++)
        {
            if (i == (int)enumType)
            {
                continue;
            }

            m_GunToggles[i].state = false;
        }

        if (state)
        {
            var (playerSkull, playerBase, zombieSkull, zombieBase, animalSkull, animalBase) = s_Multipliers[enumType];

            m_PlayerBaseMultiplier!.state = playerBase;
            m_PlayerSkullMultiplier!.state = playerSkull;

            m_ZombieBaseMultiplier!.state = zombieBase;
            m_ZombieSkullMultiplier!.state = zombieSkull;

            m_AnimalBaseMultiplier!.state = animalBase;
            m_AnimalSkullMultiplier!.state = animalSkull;
        }
    }

    private void Logs_onClickedButton(ISleekElement button)
    {
        var logPath = PathEx.Join(UnityPaths.GameDirectory, "Logs");

        Process.Start("explorer.exe", logPath);
    }

    private void OnClickedOverride(ISleekElement button)
    {
        var path = m_CsvFilePath?.text;
        if (path == null || !path.EndsWith(".csv") || !File.Exists(path))
        {
            UnturnedLog.warn("Unable to find .csv file");
            m_ErrorLabel.text = "Unable to find .csv file";
            return;
        }

        Type? gunType = null;
        for (var i = 0; i < m_GunToggles.Length; i++)
        {
            var toggle = m_GunToggles[i];
            if (toggle.state)
            {
                gunType = i switch
                {
                    0 => typeof(Raid),
                    1 => typeof(Sniper),
                    2 => typeof(Rifle),
                    3 => typeof(Boom),
                    4 => typeof(Pistol),
                    _ => throw new Exception("INVALID INDEX")
                };
                break;
            }
        }

        if (gunType is null)
        {
            UnturnedLog.warn("Hmm type is null somehow");
            m_ErrorLabel.text = "Gun type is not selected";
            return;
        }

        ParseWeapon(gunType, path);
    }

    private void ParseWeapon(Type type, string path)
    {
        var hasAnyError = false;
        UnturnedLog.warn("Started parsing type " + type.Name);

        UnturnedLog.warn("Saving path to PlayerPrefs");
        PlayerPrefs.SetString(c_SaveKey, path);
        UnturnedLog.warn("Saved path to PlayerPrefs");

        try
        {
            ParseMultipliers(out var range, out var playerBaseMultiplier, out var playerSkullMultiplier,
    out var zombieBaseMultiplier, out var zombieSkullMultiplier, out var animalBaseMultiplier, out var animalSkullMultiplier);

            using var reader = new StreamReader(path);
            using var csv = new CsvReader(reader, new(CultureInfo.InvariantCulture)
            { HeaderValidated = null, MissingFieldFound = null });
            {
                foreach (var weapon in csv.GetRecords(type).Cast<IGun>().Where(x => x.Name is not null).ToList())
                {
                    var ids = weapon.Id.Split('/');
                    if (ids.Length != 2)
                    {
                        hasAnyError = true;
                        UnturnedLog.warn("Cannot parse ids for weapon " + weapon.Name);
                        continue;
                    }

                    if (!ushort.TryParse(ids[0], out var weaponId) || !ushort.TryParse(ids[1], out var magazineId))
                    {
                        hasAnyError = true;
                        UnturnedLog.warn("Cannot parse ushort ids for weapon " + weapon.Name);
                        continue;
                    }

                    if (Assets.find(EAssetType.ITEM, weaponId) is not ItemGunAsset gunAsset
                        || Assets.find(EAssetType.ITEM, magazineId) is not ItemMagazineAsset magazineAsset)
                    {
                        hasAnyError = true;
                        UnturnedLog.warn("Cannot parse assets ids for weapon " + weapon.Name);
                        continue;
                    }

                    var magazinePath = magazineAsset.originMasterBundle.assetBundleName == "core.masterbundle" ? null : magazineAsset.absoluteOriginFilePath;
                    UnturnedLog.info("Magazine parsed as: " + (magazinePath ?? "NOT USED MAGAZINE (could be because it's from core masterbundle)"));

                    var ammo = weapon.Ammo.ToString(CultureInfo.InvariantCulture);
                    UnturnedLog.info("Amount for magazine has been parsed as: " + ammo);

                    Parser.Parse(weapon, gunAsset.absoluteOriginFilePath, magazinePath, range, playerBaseMultiplier, playerSkullMultiplier,
                        zombieBaseMultiplier, zombieSkullMultiplier, animalBaseMultiplier, animalSkullMultiplier, out var hasErrors);

                    hasAnyError |= hasErrors;

                    UnturnedLog.info(gunAsset.absoluteOriginFilePath);
                }
            }
        }
        catch (Exception ex)
        {
            m_ErrorLabel.text = "Error occured! Check logs";
            UnturnedLog.exception(ex);
            return;
        }

        m_ErrorLabel.text = !hasAnyError ? "Parsed OK!" : "Parsed OK! Some errors occured check logs!";
    }

    private void ParseMultipliers(out string range, out string playerBaseMultiplier, out string playerSkullMultiplier,
        out string zombieBaseMultiplier, out string zombieSkullMultiplier, out string animalBaseMultiplier, out string animalSkullMultiplier)
    {
        range = (m_Range?.state ?? 1).ToString(CultureInfo.InvariantCulture);

        playerBaseMultiplier = (m_PlayerBaseMultiplier?.state ?? 1f).ToString(CultureInfo.InvariantCulture);
        playerSkullMultiplier = (m_PlayerSkullMultiplier?.state ?? 1f).ToString(CultureInfo.InvariantCulture);

        zombieBaseMultiplier = (m_ZombieBaseMultiplier?.state ?? 1f).ToString(CultureInfo.InvariantCulture);
        zombieSkullMultiplier = (m_ZombieSkullMultiplier?.state ?? 1f).ToString(CultureInfo.InvariantCulture);

        animalBaseMultiplier = (m_AnimalBaseMultiplier?.state ?? 1f).ToString(CultureInfo.InvariantCulture);
        animalSkullMultiplier = (m_AnimalSkullMultiplier?.state ?? 1f).ToString(CultureInfo.InvariantCulture);
    }
}
