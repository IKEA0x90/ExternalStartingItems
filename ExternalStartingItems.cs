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
        public const string PluginVersion = "1.0.0";

        public void Start()
        {
            RoR2.Run.onRunStartGlobal += (run) =>
            {
                NetworkingAPI.RegisterMessageType<NetManager>();
                SaveFile save = SaveFile.readFile();
                List<Item> items = save.activeProfile.items;
                string itemstring = "";
                foreach (Item item in items)
                {
                    itemstring += item.toString();
                    itemstring += ",";
                }
                new NetManager(itemstring, NetworkUser.readOnlyLocalPlayersList[0].netId).Send(NetworkDestination.Server);
                //}
            };
        }
    }
}
