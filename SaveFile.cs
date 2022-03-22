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

        public Item(string name)
        {
            this.name = name;
            this.number = 1;
            this.isActive = false;
        }

        public Item(string name, int number)
        {
            this.name = name;
            this.number = number;
            this.isActive = false;
        }

        public Item(string name, int number, bool isActive)
        {
            this.name = name;
            this.number = number;
            this.isActive = isActive;
        }

        public Item() { }

        public string toString()
        {
            return name + ";" + number.ToString() + ";" + isActive.ToString();
        }
    }

    [Serializable]
    public class ActiveProfile
    {
        public string name;
        public List<Item> items;

        public ActiveProfile(string name)
        {
            this.name = name;
            this.items = new List<Item>();
        }

        public ActiveProfile() { }
    }

    [Serializable]
    public class SaveFile
    {
        public ActiveProfile activeProfile;
        public List<ActiveProfile> ProfileList = new List<ActiveProfile>();

        public SaveFile() { }

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
    }
}
