using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;

namespace ExternalStartingItems
{
    [Serializable]
    public class Item
    {
        public string name;
        public int number;
        public bool isActive;
        public string mod;
        public Item(string name)
        {
            this.name = name;
            this.number = 1;
            this.isActive = false;
            this.mod = "ITEM";
        }

        public Item(string name, int number)
        {
            this.name = name;
            this.number = number;
            this.isActive = false;
            this.mod = "ITEM";
        }

        public Item(string name, int number, bool isActive)
        {
            this.name = name;
            this.number = number;
            this.isActive = isActive;
            this.mod = "ITEM";
        }

        public Item(string name, int number, bool isActive, string mod)
        {
            this.name = name;
            this.number = number;
            this.isActive = isActive;
            this.mod = mod;
        }

        public Item() { }

        public string toString()
        {
            return name + ";" + number.ToString() + ";" + isActive.ToString() + ";" + mod;
        }
    }

    [Serializable]
    public class ActiveProfile
    {
        public string name;
        public List<Item> items;
        public bool truerougelike;
        public bool spawnsEnabled;

        public int regularCredits;

        public int redCredits;
        public int yellowCredits;
        public int blueCredits;
        public int orangeCredits;
        public int purpleCredits;
        public int blackCredits;
        public int totalCreditsGained;
        public int totalItemsBought;
        public int totalStagesCompleted;
        public int creditsPerStage;
        public int creditIncreaseEveryNStages;
        public int bonusPerLoop;
        public int instantTeleportStartingOnStage;
        public bool voidFieldsCompleted;
        public int bonusCreditEveryNToms;
        public int totalCreditsSaved;
        public int totalSpecialCreditsGained;

        public ActiveProfile(string name)
        {
            this.name = name;
            this.items = new List<Item>();
            this.truerougelike = false;
            this.spawnsEnabled = true;

            this.regularCredits = 0;
            this.redCredits = 0;
            this.yellowCredits = 0;
            this.blueCredits = 0;
            this.orangeCredits = 0;
            this.purpleCredits = 0;
            this.blackCredits = 0;
            this.totalCreditsGained = 0;
            this.totalItemsBought = 0;
            this.totalStagesCompleted = 0;
            this.creditsPerStage = 5;
            this.creditIncreaseEveryNStages = 50;
            this.bonusPerLoop = 2;
            this.instantTeleportStartingOnStage = 50;
            this.bonusCreditEveryNToms = 75;
            this.totalCreditsSaved = 0;
            this.totalSpecialCreditsGained = 0;
            this.voidFieldsCompleted = false;
        }

        public ActiveProfile() { }
    }

    [Serializable]
    public class SaveFile
    {
        public ActiveProfile activeProfile;
        public List<ActiveProfile> ProfileList;
        public float version;
        public SaveFile() 
        {
            this.ProfileList = new List<ActiveProfile>();
            this.version = 2.1f;
        }

        public void addItem(Item item)
        {
            this.activeProfile.items.Add(item);
        }

        public bool hasBeads()
        {
            foreach (Item item in activeProfile.items)
            {
                if (item.name == "LunarTrinket" && item.number != 0)
                {
                    return true;
                }
            }
            return false;
        }

        public static int countCredits(int stages)
        {
            SaveFile save = SaveFile.readFile();
            ActiveProfile profile = save.activeProfile;
            int stageMoney;
            int credits = 0;

            if (profile.creditIncreaseEveryNStages != 0)
            {
                stageMoney = profile.creditsPerStage + (profile.totalStagesCompleted / profile.creditIncreaseEveryNStages) + SaveFile.extraForTomes();
            }
            else
            {
                stageMoney = profile.creditsPerStage;
            }

            while (stages > 0)
            {
                if (stages <= 5)
                {
                    credits += stageMoney * stages;
                    break;
                }
                else
                {
                    credits += stageMoney * 5;
                    stageMoney += profile.bonusPerLoop;
                    stages -= 5;
                }
            }

            return credits;
        }

        public static int extraForTomes()
        {
            SaveFile file = SaveFile.readFile();
            foreach (Item item in file.activeProfile.items)
            {
                if (item.name == "BonusGoldPackOnKill" && item.number != 0)
                {
                    return item.number / file.activeProfile.bonusCreditEveryNToms;
                }
            }
            return 0;
        }

        public static bool isTeleportInstant()
        {
            SaveFile file = SaveFile.readFile();
            if ((file.activeProfile.totalStagesCompleted >= file.activeProfile.instantTeleportStartingOnStage) && (file.activeProfile.instantTeleportStartingOnStage >= 0))
            {
                return true;
            }
            return false;
        }

        public static SaveFile readFile()
        {
            try
            {
                XmlSerializer xml = new XmlSerializer(typeof(SaveFile));

                string specificFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "externalitems");
                string filename = Path.Combine(specificFolder, "profiles.xml");

                Directory.CreateDirectory(specificFolder);

                using (FileStream fs = new FileStream(filename, FileMode.OpenOrCreate))
                {
                    return (SaveFile)xml.Deserialize(fs);
                }
            }
            catch (InvalidOperationException)
            {
                SaveFile saveFile = new SaveFile();
                saveFile.activeProfile = new ActiveProfile("Default");
                saveFile.writeFile();
                return saveFile;
            }
        }

        public void writeFile()
        {
            XmlSerializer xml = new XmlSerializer(typeof(SaveFile));

            string specificFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "externalitems");
            string filename = Path.Combine(specificFolder, "profiles.xml");

            Directory.CreateDirectory(specificFolder);

            using (FileStream fs = new FileStream(filename, FileMode.OpenOrCreate))
            {
                fs.SetLength(0);
                xml.Serialize(fs, this);
            }

        }

        public void addProfile(string name)
        {
            this.ProfileList.Add(new ActiveProfile(name));
        }
        public bool setActive(string name)
        {
            foreach (ActiveProfile profile in this.ProfileList)
            {
                if (profile.name == name)
                {
                    this.ProfileList.Add(this.activeProfile);
                    this.activeProfile = profile;
                    this.ProfileList.Remove(profile);
                    return true;
                }
            }
            return false;
        }
    }
    [Serializable]
    public class ItemData
    {
        public string name;
        public string mod;
        public int id;
        public bool isActive;

        public ItemData(string name, string mod, int id, bool isActive)
        {
            this.name = name;
            this.mod = mod;
            this.id = id;
            this.isActive = isActive;
        }

        public ItemData() { }

    }

    [Serializable]
    public class ModData
    {
        public List<ItemData> itemList = new List<ItemData>();

        public ModData() { }

        public void addItem(ItemData item)
        {
            itemList.Add(item);
        }

        public ItemData lookup(String name, String mod)
        {
            foreach (ItemData item in itemList)
            {
                if (item.name == name && item.mod == mod)
                {
                    return item;
                }
            }
            return new ItemData("Incorrect", "Incorrect", -1, false);
        }

        public void writeModData()
        {
            XmlSerializer xml = new XmlSerializer(typeof(ModData));

            string specificFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "externalitems");
            string filename = Path.Combine(specificFolder, "moddata.xml");

            Directory.CreateDirectory(specificFolder);

            using (FileStream fs = new FileStream(filename, FileMode.OpenOrCreate))
            {
                fs.SetLength(0);
                xml.Serialize(fs, this);
            }
        }

        public static ModData readModData()
        {
            try
            {
                XmlSerializer xml = new XmlSerializer(typeof(ModData));

                string specificFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "externalitems");
                string filename = Path.Combine(specificFolder, "moddata.xml");

                Directory.CreateDirectory(specificFolder);

                using (FileStream fs = new FileStream(filename, FileMode.OpenOrCreate))
                {
                    return (ModData)xml.Deserialize(fs);
                }
            }
            catch (InvalidOperationException)
            {
                ModData modData = new ModData();
                modData.writeModData();
                return modData;
            }
        }
    }
}
