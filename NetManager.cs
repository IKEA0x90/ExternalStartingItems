using BepInEx;
using BepInEx.Configuration;
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
    public class NetManager : INetMessage
    {
        private string items; 
        private NetworkInstanceId ID;

        public NetManager()
        {
        }

        public NetManager(string itemstring, NetworkInstanceId PID)
        {
            items = itemstring;
            ID = PID;
        }
        public void Serialize(NetworkWriter writer)
        {
            writer.Write(items);
            writer.Write(ID);
        }

        public void Deserialize(NetworkReader reader)
        {
            items = reader.ReadString();
            ID = reader.ReadNetworkId();
        }

        public void OnReceived()
        {
            if (NetworkServer.active)
            {
                bool activeReverser = false;
                string[] itemlist = items.Split(','); 
                foreach (PlayerCharacterMasterController controllermaster in PlayerCharacterMasterController.instances)
                {
                    if (ID == controllermaster.networkUserInstanceId)
                    {
                        foreach (string item in itemlist)
                        {
                            
                            string itemID = item.Split(';')[0];
                            int itemcount = int.Parse(item.Split(';')[1]);
                            bool itemactive = bool.Parse(item.Split(';')[2]);
                            string mod = item.Split(';')[3];

                            ModData moddata = ModData.readModData();

                            ItemData i = moddata.lookup(itemID, mod);
                            if (!itemactive)
                            {
                                controllermaster.master.inventory.GiveItem((ItemIndex)i.id, itemcount);
                            }
                            else
                            {
                                for (int j = 0; j < itemcount; j++)
                                {
                                    controllermaster.master.inventory.SetActiveEquipmentSlot(activeReverser ? (byte)0 : (byte)1);
                                    //controllermaster.master.inventory.GiveItem((ItemIndex)i.id, 1);
                                    controllermaster.master.inventory.SetEquipmentIndex((EquipmentIndex)i.id);
                                    activeReverser = !activeReverser;
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}
