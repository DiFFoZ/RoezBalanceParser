using SDG.Unturned;
using UnityEngine;

namespace RoezBalanceParser.UI;

internal class PlayerDashboardCraftingUIManager : MonoBehaviour
{
    private SleekFullscreenBox container;
    private ISleekBox box;
    private SleekList<Blueprint> blueprintsScrollBox;
    private readonly List<Blueprint> filteredBlueprints = new();
    private readonly List<Blueprint> allBlueprints = new();
    private readonly HashSet<Blueprint> IgnoredBlueprints = new();
    private int Index;

    internal void Load()
    {
        var comp = Main.Instance.gameObject.AddComponent<PlayerDashboardCraftingUIManager>();
        comp.Start();
    }

    private void Start()
    {
        Main.OnGameUICreated += InitUI;
    }

    private void InitUI()
    {
        container = new SleekFullscreenBox
        {
            positionScale_Y = 1f,
            /*positionOffset_X = 10,
            positionOffset_Y = 10,
            sizeOffset_X = -20,
            sizeOffset_Y = -20,*/
            sizeScale_X = 1f,
            sizeScale_Y = 1f,
        };

        PlayerUI.container.AddChild(container);

        box = Glazier.Get().CreateBox();
        /*        box.positionOffset_Y = 60;
                box.sizeOffset_Y = -60;*/
        box.sizeScale_X = 1f;
        box.sizeScale_Y = 1f;
        //box.backgroundColor = new Color32(255, 255, 255, 255);
        container.AddChild(box);

        blueprintsScrollBox = new SleekList<Blueprint>
        {
            //blueprintsScrollBox.scrollView.isVisible = true;
            positionOffset_X = 5,
            //blueprintsScrollBox.positionOffset_Y = 110;
            sizeOffset_X = 20,
            //blueprintsScrollBox.sizeOffset_Y = -120;
            sizeScale_X = 1f,
            sizeScale_Y = 1f,
            itemHeight = 216
        };
        //blueprintsScrollBox.itemPadding = 10;
        //blueprintsScrollBox.scrollView.backgroundColor = new Color32(255,255,255,255);
        blueprintsScrollBox.onCreateElement += BlueprintsScrollBox_onCreateElement;
        blueprintsScrollBox.SetData(filteredBlueprints);
        box.AddChild(blueprintsScrollBox);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.F7))
        {
            ToggleUI();
        }

        if (Input.GetKeyDown(KeyCode.F5))
        {
            filteredBlueprints.Clear();
            allBlueprints.Clear();

            foreach (var itemAsset in Assets.find(EAssetType.ITEM)
                .Cast<ItemAsset>()
                .Where(x => x.absoluteOriginFilePath.Contains("RoezBundle")))
            {
                if (itemAsset is null)
                    continue;

                allBlueprints.AddRange(itemAsset.blueprints.Where(x => x.type is not EBlueprintType.REPAIR));
            }

            filteredBlueprints.AddRange(allBlueprints.Skip(Index));
            blueprintsScrollBox.ForceRebuildElements();
        }

        if (Input.GetKeyDown(KeyCode.F8))
        {
            Index += 5;

            UpdateBox();
        }

        if (Input.GetKeyDown(KeyCode.F9))
        {
            Index++;
            UpdateBox();
        }

        if (Input.GetKeyDown(KeyCode.F10))
        {
            Index -= 5;

            UpdateBox();
        }

        if (Input.GetKeyDown(KeyCode.F11))
        {
            Index--;
            UpdateBox();
        }

        if (Input.GetKeyDown(KeyCode.F6))
        {
            IgnoredBlueprints.Clear();
            UpdateBox();
        }
    }

    private void UpdateBox()
    {
        filteredBlueprints.Clear();
        filteredBlueprints.AddRange(allBlueprints.Where(x => !IgnoredBlueprints.Contains(x)).Skip(Index));

        blueprintsScrollBox.ForceRebuildElements();
    }

    private void ToggleUI()
    {
        box.backgroundColor = new Color32(0, 0, 0, 0);

        if (!container.isVisible)
        {
            container.isVisible = true;
            container.AnimateIntoView();
            PlayerLifeUI.close();
        }
        else
        {
            container.isVisible = false;
            container.AnimateOutOfView(0, 1);
            PlayerLifeUI.open();
        }
    }

    private ISleekElement BlueprintsScrollBox_onCreateElement(Blueprint blueprint)
    {
        blueprint.hasItem = true;
        blueprint.hasSkills = true;
        blueprint.hasSupplies = true;
        blueprint.hasTool = true;
        blueprint.tools = 1;

        foreach (var supply in blueprint.supplies)
        {
            supply.hasAmount = supply.amount;
        }

        var ex = new SleekBlueprintEx(blueprint);
        ex.OnClickedRemove += Ex_OnClickedRemove;

        return ex;
    }

    private void Ex_OnClickedRemove(Blueprint blueprint)
    {
        IgnoredBlueprints.Add(blueprint);
        UpdateBox();
        blueprintsScrollBox.ForceRebuildElements();
    }

    private class SleekBlueprintEx : SleekWrapper
    {
        public Blueprint blueprint { get; }

        public SleekBlueprintEx(Blueprint newBlueprint)
        {
            blueprint = newBlueprint;

            if (Glazier.Get().SupportsDepth)
            {
                backgroundButton = Glazier.Get().CreateButton();
                backgroundButton.sizeScale_X = 1f;
                backgroundButton.sizeScale_Y = 1f;
                backgroundButton.onClickedButton += onClickedBackgroundButton;
                AddChild(backgroundButton);
            }

            var sleekLabel = Glazier.Get().CreateLabel();
            sleekLabel.positionOffset_X = 5;
            sleekLabel.positionOffset_Y = 5;
            sleekLabel.sizeOffset_X = -10;
            sleekLabel.sizeOffset_Y = 30;
            sleekLabel.sizeScale_X = 1f;
            sleekLabel.textColor = ESleekTint.FONT;
            sleekLabel.shadowStyle = ETextContrastContext.Default;
            sleekLabel.fontSize = ESleekFontSize.Medium;
            AddChild(sleekLabel);

            if (blueprint.skill != EBlueprintSkill.NONE)
            {
                var sleekLabel2 = Glazier.Get().CreateLabel();
                sleekLabel2.positionOffset_X = 5;
                sleekLabel2.positionOffset_Y = -35;
                sleekLabel2.positionScale_Y = 1f;
                sleekLabel2.sizeOffset_X = -10;
                sleekLabel2.sizeOffset_Y = 30;
                sleekLabel2.sizeScale_X = 1f;
                sleekLabel2.text = PlayerDashboardCraftingUI.localization.format("Skill_" + ((int)blueprint.skill).ToString(), new[]
                {
                    PlayerDashboardSkillsUI.localization.format("Level_" + blueprint.level.ToString())
                });

                sleekLabel2.textColor = ESleekTint.FONT;
                sleekLabel2.shadowStyle = ETextContrastContext.Default;
                sleekLabel2.fontSize = ESleekFontSize.Medium;
                AddChild(sleekLabel2);
            }

            container = Glazier.Get().CreateFrame();
            container.positionOffset_Y = 40;
            container.positionScale_X = 0.5f;
            container.sizeOffset_Y = -45;
            container.sizeScale_Y = 1f;
            AddChild(container);

            var num = 0;
            for (var i = 0; i < blueprint.supplies.Length; i++)
            {
                var blueprintSupply = blueprint.supplies[i];
                if (Assets.find(EAssetType.ITEM, blueprintSupply.id) is ItemAsset itemAsset)
                {
                    sleekLabel.text += itemAsset.itemName + $" [{itemAsset.id}]";
                    var sleekItemIcon = new SleekItemIcon
                    {
                        positionOffset_X = num,
                        positionOffset_Y = -itemAsset.size_y * 25,
                        positionScale_Y = 0.5f,
                        sizeOffset_X = itemAsset.size_x * 50,
                        sizeOffset_Y = itemAsset.size_y * 50
                    };
                    this.container.AddChild(sleekItemIcon);
                    sleekItemIcon.Refresh(blueprintSupply.id, 100, itemAsset.getState(false), itemAsset);
                    var sleekLabel4 = Glazier.Get().CreateLabel();
                    sleekLabel4.positionOffset_X = -100;
                    sleekLabel4.positionOffset_Y = -30;
                    sleekLabel4.positionScale_Y = 1f;
                    sleekLabel4.sizeOffset_X = 100;
                    sleekLabel4.sizeOffset_Y = 30;
                    sleekLabel4.sizeScale_X = 1f;
                    sleekLabel4.fontAlignment = TextAnchor.MiddleRight;
                    sleekLabel4.text = blueprintSupply.hasAmount.ToString() + "/" + blueprintSupply.amount.ToString();
                    sleekLabel4.shadowStyle = ETextContrastContext.InconspicuousBackdrop;
                    sleekItemIcon.AddChild(sleekLabel4);
                    var sleekLabel5 = sleekLabel;
                    sleekLabel5.text = string.Concat(new string[]
                    {
                        sleekLabel5.text,
                        " ",
                        blueprintSupply.hasAmount.ToString(),
                        "/",
                        blueprintSupply.amount.ToString()
                    });
                    if (this.blueprint.type == EBlueprintType.AMMO)
                    {
                        if (blueprintSupply.hasAmount == 0 || blueprintSupply.amount == 0)
                        {
                            sleekLabel4.textColor = ESleekTint.BAD;
                        }
                    }
                    else if (blueprintSupply.hasAmount < blueprintSupply.amount)
                    {
                        sleekLabel4.textColor = ESleekTint.BAD;
                    }
                    num += (itemAsset.size_x * 50) + 25;
                    if (i < this.blueprint.supplies.Length - 1 || this.blueprint.tool != 0 || this.blueprint.type == EBlueprintType.REPAIR || this.blueprint.type == EBlueprintType.AMMO)
                    {
                        var sleekLabel6 = sleekLabel;
                        sleekLabel6.text += " + ";
                        var sleekImage = Glazier.Get().CreateImage(PlayerDashboardCraftingUI.icons.load<Texture2D>("Plus"));
                        sleekImage.positionOffset_X = num;
                        sleekImage.positionOffset_Y = -20;
                        sleekImage.positionScale_Y = 0.5f;
                        sleekImage.sizeOffset_X = 40;
                        sleekImage.sizeOffset_Y = 40;
                        sleekImage.color = ESleekTint.FOREGROUND;
                        this.container.AddChild(sleekImage);
                        num += 65;
                    }
                }
            }
            if (this.blueprint.tool != 0)
            {
                if (Assets.find(EAssetType.ITEM, this.blueprint.tool) is ItemAsset itemAsset2)
                {
                    var sleekLabel7 = sleekLabel;
                    sleekLabel7.text += itemAsset2.itemName + $" [{itemAsset2.id}]";
                    var sleekItemIcon2 = new SleekItemIcon
                    {
                        positionOffset_X = num,
                        positionOffset_Y = -itemAsset2.size_y * 25,
                        positionScale_Y = 0.5f,
                        sizeOffset_X = itemAsset2.size_x * 50,
                        sizeOffset_Y = itemAsset2.size_y * 50
                    };
                    this.container.AddChild(sleekItemIcon2);
                    sleekItemIcon2.Refresh(this.blueprint.tool, 100, itemAsset2.getState(), itemAsset2);
                    var sleekLabel8 = Glazier.Get().CreateLabel();
                    sleekLabel8.positionOffset_X = -100;
                    sleekLabel8.positionOffset_Y = -30;
                    sleekLabel8.positionScale_Y = 1f;
                    sleekLabel8.sizeOffset_X = 100;
                    sleekLabel8.sizeOffset_Y = 30;
                    sleekLabel8.sizeScale_X = 1f;
                    sleekLabel8.fontAlignment = TextAnchor.MiddleRight;
                    sleekLabel8.text = this.blueprint.tools.ToString() + "/1";
                    sleekLabel8.shadowStyle = ETextContrastContext.InconspicuousBackdrop;
                    sleekItemIcon2.AddChild(sleekLabel8);
                    var sleekLabel9 = sleekLabel;
                    sleekLabel9.text = sleekLabel9.text + " " + this.blueprint.tools.ToString() + "/1";
                    if (!this.blueprint.hasTool)
                    {
                        sleekLabel8.textColor = ESleekTint.BAD;
                    }
                    num += (itemAsset2.size_x * 50) + 25;
                    if (this.blueprint.type is EBlueprintType.REPAIR or EBlueprintType.AMMO)
                    {
                        var sleekLabel10 = sleekLabel;
                        sleekLabel10.text += " + ";
                        var sleekImage2 = Glazier.Get().CreateImage(PlayerDashboardCraftingUI.icons.load<Texture2D>("Plus"));
                        sleekImage2.positionOffset_X = num;
                        sleekImage2.positionOffset_Y = -20;
                        sleekImage2.positionScale_Y = 0.5f;
                        sleekImage2.sizeOffset_X = 40;
                        sleekImage2.sizeOffset_Y = 40;
                        sleekImage2.color = ESleekTint.FOREGROUND;
                        this.container.AddChild(sleekImage2);
                        num += 65;
                    }
                }
            }
            if (this.blueprint.type is EBlueprintType.REPAIR or EBlueprintType.AMMO)
            {
                if (Assets.find(EAssetType.ITEM, this.blueprint.outputs[0].id) is ItemAsset itemAsset3)
                {
                    var sleekLabel11 = sleekLabel;
                    sleekLabel11.text += itemAsset3.itemName;
                    var sleekItemIcon3 = new SleekItemIcon
                    {
                        positionOffset_X = num,
                        positionOffset_Y = -itemAsset3.size_y * 25,
                        positionScale_Y = 0.5f,
                        sizeOffset_X = itemAsset3.size_x * 50,
                        sizeOffset_Y = itemAsset3.size_y * 50
                    };
                    this.container.AddChild(sleekItemIcon3);
                    sleekItemIcon3.Refresh(this.blueprint.outputs[0].id, 100, itemAsset3.getState(), itemAsset3);
                    var sleekLabel12 = Glazier.Get().CreateLabel();
                    sleekLabel12.positionOffset_X = -100;
                    sleekLabel12.positionOffset_Y = -30;
                    sleekLabel12.positionScale_Y = 1f;
                    sleekLabel12.sizeOffset_X = 100;
                    sleekLabel12.sizeOffset_Y = 30;
                    sleekLabel12.sizeScale_X = 1f;
                    sleekLabel12.fontAlignment = TextAnchor.MiddleRight;
                    if (this.blueprint.type == EBlueprintType.REPAIR)
                    {
                        var sleekLabel13 = sleekLabel;
                        sleekLabel13.text = sleekLabel13.text + " " + this.blueprint.items.ToString() + "%";
                        sleekLabel12.text = this.blueprint.items.ToString() + "%";
                        sleekLabel12.textColor = ItemTool.getQualityColor(blueprint.items / 100f);
                    }
                    else if (this.blueprint.type == EBlueprintType.AMMO)
                    {
                        var sleekLabel5 = sleekLabel;
                        sleekLabel5.text = string.Concat(new string[]
                        {
                            sleekLabel5.text,
                            " ",
                            this.blueprint.items.ToString(),
                            "/",
                            this.blueprint.products.ToString()
                        });
                        sleekLabel12.text = this.blueprint.items.ToString() + "/" + itemAsset3.amount.ToString();
                    }
                    if (!this.blueprint.hasItem)
                    {
                        sleekLabel12.textColor = ESleekTint.BAD;
                    }
                    sleekLabel12.shadowStyle = ETextContrastContext.InconspicuousBackdrop;
                    sleekItemIcon3.AddChild(sleekLabel12);
                    num += (itemAsset3.size_x * 50) + 25;
                }
            }
            var sleekLabel14 = sleekLabel;
            sleekLabel14.text += " = ";
            var sleekImage3 = Glazier.Get().CreateImage(PlayerDashboardCraftingUI.icons.load<Texture2D>("Equals"));
            sleekImage3.positionOffset_X = num;
            sleekImage3.positionOffset_Y = -20;
            sleekImage3.positionScale_Y = 0.5f;
            sleekImage3.sizeOffset_X = 40;
            sleekImage3.sizeOffset_Y = 40;
            sleekImage3.color = ESleekTint.FOREGROUND;
            this.container.AddChild(sleekImage3);
            num += 65;
            for (var j = 0; j < this.blueprint.outputs.Length; j++)
            {
                var blueprintOutput = this.blueprint.outputs[j];
                if (Assets.find(EAssetType.ITEM, blueprintOutput.id) is ItemAsset itemAsset4)
                {
                    var sleekLabel15 = sleekLabel;
                    sleekLabel15.text += itemAsset4.itemName + $" [{itemAsset4.id}]";
                    var sleekItemIcon4 = new SleekItemIcon
                    {
                        positionOffset_X = num,
                        positionOffset_Y = -itemAsset4.size_y * 25,
                        positionScale_Y = 0.5f,
                        sizeOffset_X = itemAsset4.size_x * 50,
                        sizeOffset_Y = itemAsset4.size_y * 50
                    };
                    this.container.AddChild(sleekItemIcon4);
                    sleekItemIcon4.Refresh(blueprintOutput.id, 100, itemAsset4.getState(), itemAsset4);
                    var sleekLabel16 = Glazier.Get().CreateLabel();
                    sleekLabel16.positionOffset_X = -100;
                    sleekLabel16.positionOffset_Y = -30;
                    sleekLabel16.positionScale_Y = 1f;
                    sleekLabel16.sizeOffset_X = 100;
                    sleekLabel16.sizeOffset_Y = 30;
                    sleekLabel16.sizeScale_X = 1f;
                    sleekLabel16.fontAlignment = TextAnchor.MiddleRight;
                    sleekLabel16.shadowStyle = ETextContrastContext.InconspicuousBackdrop;
                    if (this.blueprint.type == EBlueprintType.REPAIR)
                    {
                        var sleekLabel17 = sleekLabel;
                        sleekLabel17.text += " 100%";
                        sleekLabel16.text = "100%";
                        sleekLabel16.textColor = Palette.COLOR_G;
                    }
                    else if (this.blueprint.type == EBlueprintType.AMMO)
                    {
                        if (Assets.find(EAssetType.ITEM, blueprintOutput.id) is ItemAsset itemAsset5)
                        {
                            var sleekLabel5 = sleekLabel;
                            sleekLabel5.text = string.Concat(new string[]
                            {
                                sleekLabel5.text,
                                " ",
                                this.blueprint.products.ToString(),
                                "/",
                                itemAsset5.amount.ToString()
                            });
                            sleekLabel16.text = this.blueprint.products.ToString() + "/" + itemAsset5.amount.ToString();
                        }
                    }
                    else
                    {
                        var sleekLabel18 = sleekLabel;
                        sleekLabel18.text = sleekLabel18.text + " x" + blueprintOutput.amount.ToString();
                        sleekLabel16.text = "x" + blueprintOutput.amount.ToString();
                    }
                    sleekItemIcon4.AddChild(sleekLabel16);
                    num += itemAsset4.size_x * 50;
                    if (j < this.blueprint.outputs.Length - 1)
                    {
                        num += 25;
                        var sleekLabel19 = sleekLabel;
                        sleekLabel19.text += " + ";
                        var sleekImage4 = Glazier.Get().CreateImage(PlayerDashboardCraftingUI.icons.load<Texture2D>("Plus"));
                        sleekImage4.positionOffset_X = num;
                        sleekImage4.positionOffset_Y = -20;
                        sleekImage4.positionScale_Y = 0.5f;
                        sleekImage4.sizeOffset_X = 40;
                        sleekImage4.sizeOffset_Y = 40;
                        sleekImage4.color = ESleekTint.FOREGROUND;
                        this.container.AddChild(sleekImage4);
                        num += 65;
                    }
                }
            }
            this.container.positionOffset_X = -num / 2;
            this.container.sizeOffset_X = num;
        }

        private void onClickedBackgroundButton(ISleekElement button)
        {
            OnClickedRemove(blueprint);
        }

        public event SleekBlueprint.Clicked OnClickedRemove;

        private readonly ISleekElement container;
        private readonly ISleekButton backgroundButton;
    }
}
