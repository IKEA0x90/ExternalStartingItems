using BepInEx;
using BepInEx.Configuration;
using R2API.Networking;
using R2API.Networking.Interfaces;
using R2API;
using R2API.Utils;
using RoR2;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using System.IO;

namespace ExternalStartingItems
{
    [BepInDependency(R2API.R2API.PluginGUID)]
    [R2API.Utils.R2APISubmoduleDependency("NetworkingAPI")]
    [BepInPlugin(PluginGUID, PluginName, PluginVersion)]

    public class ExternalStartingItems : BaseUnityPlugin
    {
        public const string PluginGUID = PluginAuthor + "." + PluginName;
        public const string PluginAuthor = "IKEA";
        public const string PluginName = "ExternalStartingItems";
        public const string PluginVersion = "2.0.0";

        static public List<ItemData> getModdedItems()
        {
            List<ItemData> itemList = new List<ItemData>();
            foreach (EquipmentIndex ei in RoR2.EquipmentCatalog.allEquipment)
            {
                EquipmentDef eq = RoR2.EquipmentCatalog.GetEquipmentDef(ei);
                //if (eq.nameToken.Split('_')[0] != "EQUIPMENT")
                //{
                    itemList.Add(new ItemData(eq.name, eq.nameToken.Split('_')[0], (int)eq.equipmentIndex, true));
                //}
            }
            foreach (ItemDef itemdef in RoR2.ItemCatalog.allItemDefs)
            {
                //if (itemdef.nameToken.Split('_')[0] != "ITEM")
                //{
                    itemList.Add(new ItemData(itemdef.name, itemdef.nameToken.Split('_')[0], (int)itemdef.itemIndex, false));
                //}
            }
            return itemList;
        }

        public void Awake()
        {
            On.RoR2.Run.Start += (orig, self) =>
            {
                orig(self);

                ModData modData = new ModData();
                List<ItemData> itemList = getModdedItems();
                foreach (ItemData i in itemList)
                {
                    modData.addItem(i);
                }
                modData.writeModData();

                if (self.spawnWithPod)
                {
                    NetworkingAPI.RegisterMessageType<NetManager>();
                    SaveFile save = SaveFile.readFile();

                    if (save.version < 2) //UPDATE ON EACH REVISION
                    {
                        R2API.Utils.ChatMessage.Send("ExternalStartingItems: You are using an older version of the GUI application. Consider updating it for new functionality.\nThis message is only sent once.");
                        save.version = 2;
                        save.writeFile();
                    }

                    List<Item> items = save.activeProfile.items;
                    string itemstring = "";
                    foreach (Item item in items)
                    {
                        itemstring += item.toString();
                        itemstring += ",";
                    }
                    new NetManager(itemstring, NetworkUser.readOnlyLocalPlayersList[0].netId).Send(NetworkDestination.Server);
                }
            };

            /*
            SaveFile save = SaveFile.readFile();
            ActiveProfile profile = save.activeProfile;
            if (profile.truerougelike)
            {
                RoR2.Run.onClientGameOverGlobal += (run, runReport) =>
                {
                    if (runReport.gameEnding.gameEndingIndex == RoR2Content.GameEndings.MainEnding.gameEndingIndex)
                    {
                        profile.redCredits += 1;
                        profile.totalSpecialCreditsGained += 1;
                        //Chat.AddMessage("Gained 1 Red Credit");
                    }
                    else if (runReport.gameEnding.gameEndingIndex == RoR2Content.GameEndings.StandardLoss.gameEndingIndex)
                    {
                        ;
                    }
                    else if (runReport.gameEnding.gameEndingIndex == RoR2Content.GameEndings.ObliterationEnding.gameEndingIndex)
                    {
                        profile.blueCredits += 1;
                        profile.totalSpecialCreditsGained += 1;
                        //Chat.AddMessage("Gained 1 Blue Credit");
                    }
                    else if (runReport.gameEnding.gameEndingIndex == RoR2Content.GameEndings.LimboEnding.gameEndingIndex)
                    {
                        if (save.hasBeads())
                        {
                            profile.blueCredits += 3;
                            profile.totalSpecialCreditsGained += 3;
                            //Chat.AddMessage("Gained 3 Blue Credits");
                        }
                        else
                        {
                            profile.blueCredits += 1;
                            profile.totalSpecialCreditsGained += 1;
                        }
                    }
                    else if (runReport.gameEnding.gameEndingIndex == DLC1Content.GameEndings.VoidEnding.gameEndingIndex)
                    {
                        profile.blackCredits += 1;
                        profile.totalSpecialCreditsGained += 1;
                        //Chat.AddMessage("Gained 1 Black Credit");
                    }

                    int credits;
                    if (save.activeProfile.voidFieldsCompleted)
                    {
                        credits = SaveFile.countCredits(run.stageClearCount - 1);
                    }
                    else
                    {
                        credits = SaveFile.countCredits(run.stageClearCount);
                    }
                    Chat.AddMessage("Gained " + credits + " Regular Credits");
                    profile.regularCredits += credits;
                    profile.totalStagesCompleted += run.stageClearCount;
                    profile.totalCreditsGained += credits;

                    profile.voidFieldsCompleted = false;

                    save.writeFile();
                };

                RoR2.Stage.onServerStageComplete += (stage) =>
                {
                    SaveFile save = SaveFile.readFile();
                    ActiveProfile profile = save.activeProfile;
                    //NetworkingAPI.RegisterMessageType<PurpleCreditManager>();

                    //Chat.AddMessage(stage.sceneDef.ToString());
                    if (stage.sceneDef == SceneCatalog.FindSceneDef("artifactworld"))
                    {
                        //new PurpleCreditManager("orange").Send(NetworkDestination.Clients);
                        profile.orangeCredits += 1;
                        profile.totalSpecialCreditsGained += 1;
                        //Chat.AddMessage("Gained 1 Orange Credit");
                    }
                    else if (stage.sceneDef == SceneCatalog.FindSceneDef("goldshores"))
                    {
                        //new PurpleCreditManager("yellow").Send(NetworkDestination.Clients);
                        profile.yellowCredits += 1;
                        profile.totalSpecialCreditsGained += 1;
                        //Chat.AddMessage("Gained 1 Yellow Credit");
                    }
                    else if (stage.sceneDef == SceneCatalog.FindSceneDef("arena"))
                    {
                        //new PurpleCreditManager("voidset").Send(NetworkDestination.Clients);
                        profile.voidFieldsCompleted = true;
                    }

                    save.writeFile();
                };

                RoR2.ArenaMissionController.onBeatArena += () =>
                {
                    //NetworkingAPI.RegisterMessageType<PurpleCreditManager>();
                    //new PurpleCreditManager("purple").Send(NetworkDestination.Clients);
                    SaveFile save = SaveFile.readFile();
                    ActiveProfile profile = save.activeProfile;

                    profile.purpleCredits += 1;
                    profile.totalSpecialCreditsGained += 1;
                    profile.voidFieldsCompleted = true;
                    //Chat.AddMessage("Gained 1 Purple Credit");

                    save.writeFile();
                };

                On.RoR2.TeleporterInteraction.UpdateMonstersClear += (orig, self) =>
                {
                    orig(self);
                    if (self.monstersCleared && self.holdoutZoneController && self.activationState == TeleporterInteraction.ActivationState.Charging && self.chargeFraction > 0.02f && SaveFile.isTeleportInstant())
                    {
                        int displayChargePercent = TeleporterInteraction.instance.holdoutZoneController.displayChargePercent;
                        float runStopwatch = Run.instance.GetRunStopwatch();
                        int num = Math.Min(Util.GetItemCountForTeam(self.holdoutZoneController.chargingTeam, RoR2Content.Items.FocusConvergence.itemIndex, true, true), 3);
                        float num2 = (100f - (float)displayChargePercent) / 100f * (TeleporterInteraction.instance.holdoutZoneController.baseChargeDuration / (1f + 0.3f * (float)num));
                        num2 = (float)Math.Round((double)num2, 2);
                        float runStopwatch2 = runStopwatch + (float)Math.Round((double)num2, 2);
                        Run.instance.SetRunStopwatch(runStopwatch2);
                        TeleporterInteraction.instance.holdoutZoneController.FullyChargeHoldoutZone();
                        //Chat.AddMessage("Added " + num2.ToString() + " seconds to the game timer.");
                    }
                };

                if (!profile.spawnsEnabled)
                {
                    On.RoR2.BossGroup.DropRewards += (orig, self) =>
                    {
                        ;
                    };

                    On.RoR2.SceneDirector.GenerateInteractableCardSelection += (orig, self) =>
                    {
                        //SaveFile save = SaveFile.readFile();
                        //ActiveProfile profile = save.activeProfile;

                        var weightedSelection = orig(self);

                        for (var i = 0; i < weightedSelection.Count; i++)
                        {
                            var choiceInfo = weightedSelection.GetChoice(i);
                            SpawnCard spawnCard = choiceInfo.value.spawnCard;

                            if (!isInteractibleAllowed(spawnCard))
                            {
                                weightedSelection.ModifyChoiceWeight(i, 0);
                            }
                            //else
                            //{
                            //profile.debug.Add(spawnCard.name.ToLower());
                            //}
                        }

                        //save.writeFile();
                        return weightedSelection;
                    };
                }
            }
        }
        private static bool isInteractibleAllowed(SpawnCard spawnCard)
        {
            List<string> dissalowed = new List<string>();

            dissalowed.Add(DirectorAPI.Helpers.InteractableNames.AdaptiveChest.ToLower());
            dissalowed.Add(DirectorAPI.Helpers.InteractableNames.BasicChest.ToLower());
            dissalowed.Add(DirectorAPI.Helpers.InteractableNames.ChanceShrine.ToLower());
            dissalowed.Add(DirectorAPI.Helpers.InteractableNames.CloakedChest.ToLower());
            dissalowed.Add(DirectorAPI.Helpers.InteractableNames.DamageChest.ToLower());
            dissalowed.Add(DirectorAPI.Helpers.InteractableNames.EquipmentBarrel.ToLower());
            dissalowed.Add(DirectorAPI.Helpers.InteractableNames.HealingChest.ToLower());
            dissalowed.Add(DirectorAPI.Helpers.InteractableNames.LargeChest.ToLower());
            dissalowed.Add(DirectorAPI.Helpers.InteractableNames.LegendaryChest.ToLower());
            dissalowed.Add(DirectorAPI.Helpers.InteractableNames.Lockbox.ToLower());
            dissalowed.Add(DirectorAPI.Helpers.InteractableNames.LunarBud.ToLower());
            dissalowed.Add(DirectorAPI.Helpers.InteractableNames.MultiShopCommon.ToLower());
            dissalowed.Add(DirectorAPI.Helpers.InteractableNames.MultiShopUncommon.ToLower());
            dissalowed.Add(DirectorAPI.Helpers.InteractableNames.ScavengerBackpack.ToLower());
            dissalowed.Add(DirectorAPI.Helpers.InteractableNames.UtilityChest.ToLower());
            dissalowed.Add(DirectorAPI.Helpers.InteractableNames.CleansingPool.ToLower());
            dissalowed.Add("iscScrapper".ToLower());
            dissalowed.Add("iscVoidChest".ToLower());
            dissalowed.Add("iscvoidcamp".ToLower());
            dissalowed.Add("isccategorychest2healing".ToLower());
            dissalowed.Add("isccategorychest2damage".ToLower());
            dissalowed.Add("iscshrinechancesandy".ToLower());
            dissalowed.Add("isccategorychest2utility".ToLower());
            dissalowed.Add("iscshrinechancesnowy".ToLower());
            dissalowed.Add("isctripleshopequipment".ToLower());

            return !dissalowed.Contains(spawnCard.name.ToLower());
        }
            **/
        }
    }
}
