using Dalamud.Interface.Windowing;
using Dalamud.Plugin;
using Newtonsoft.Json;
using OutfitManager.Ipc;
using OutfitManager.Models;
using OutfitManager.Services;
using Penumbra.Api.Enums;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Penumbra.Api.Ipc;

namespace OutfitManager.Handlers
{
    public class OutfitHandler
    {
        private readonly Plugin _plugin;

        public OmgOutfit Snapshot { get; set; }
        public bool IgnoreGsEquip { get; set; }  
        public bool OutfitLock { get; set; }

        public string SnapshotCollection { get; set; }
        public string SnapshotGlamourer { get; set; }
        public Dictionary<string,OmgOutfit> Outfits { get; set; }

   

        public OutfitHandler(Plugin plugin)
        {
            this._plugin = plugin;

            MigrateOutfits();

            // Load outfits from the JSON file
            this.Outfits = LoadOutfits();
            this.Snapshot = new OmgOutfit();
        }

        public async Task UnlockOutfit(int delay)
        {
            Task task = Task.Delay(delay * 1000);
            await task;

            this.ToggleLock();
        }
        public void ToggleLock()
        {
            OutfitLock = !OutfitLock;

            if (OutfitLock)
            {
                this._plugin.HideAllWindows();
            }
            DalamudService.Chat.Print($"Your outfitmanager lock status is: {OutfitLock}");
        }

        public void EquipOutfit(string outfitName = "", string tag = "", bool gearset = true, bool ignoreCollection = false)
        {
            OmgOutfit outfit = null;
            this.Snapshot = new OmgOutfit();
            if (!string.IsNullOrEmpty(tag))
            {
                List<OmgOutfit> outfits = this._plugin.OutfitHandler.Outfits.Values.Where(x => x.Tags.Contains(tag)).ToList();

                if (outfits.Count > 0)
                {
                    Random random = new Random();
                    int index = random.Next(outfits.Count);
                    outfit = outfits[index];
                }
            }
            else
            {
                outfit = this._plugin.OutfitHandler.Outfits[outfitName];
            }

            if (outfit != null)
            {

                List<RecievedCommand> commands = new List<RecievedCommand>();

                if (!string.IsNullOrEmpty(outfit.CollectionName.Trim()) && !ignoreCollection)
                {
                    if (this._plugin.Configuration.PenumbraCollectionType != "Your Character")
                    {
                        SetCollectionForObject.Subscriber(DalamudService.PluginInterface).Invoke(0, outfit.CollectionName, true, false);
                    }
                    else
                    {
                        SetCollectionForType.Subscriber(DalamudService.PluginInterface).Invoke(ApiCollectionType.Yourself, outfit.CollectionName, true, false);
                    }

                    //  commands.Add(new RecievedCommand { CommandType = "plugin", Command = $"/penumbra collection {this.Configuration.PenumbraCollectionType} | {outfit.CollectionName} | <me>" });
                }
                if (!string.IsNullOrEmpty(outfit.DesignPath.Trim()))
                {
                    commands.Add(new RecievedCommand { CommandType = "plugin", Command = $"/glamour apply,<me>,{outfit.DesignPath}" });
                }
                else if (!string.IsNullOrEmpty(outfit.GlamourerData.Trim()))
                {
                    GlamourerIpc.Instance.ApplyOnlyEquipmentToCharacterIpc(outfit.GlamourerData, DalamudService.ClientState.LocalPlayer);
                }
                int delay = 0;
                if (!string.IsNullOrEmpty(outfit.GearSet) && gearset)
                {
                    this.IgnoreGsEquip = true;
                    this._plugin.Common.Functions.Chat.SendMessage("/gearset change " + outfit.GearSet.Trim());

                    delay = 300;
                }

                foreach (RecievedCommand recievedCommand in commands)
                {
                    _plugin.RelayCommand(recievedCommand.Command, delay += 100);
                }


                this._plugin.Configuration.OutfitName = outfitName;
                this._plugin.Configuration.Save();

            }
        }

        public string GetOutfitJsonFilePath()
        {
            string configDirectory = DalamudService.PluginInterface.GetPluginConfigDirectory();
            return Path.Combine(configDirectory, "Outfits.json");
        }

        public Dictionary<string, OmgOutfit> LoadOutfits()
        {
            string filePath = GetOutfitJsonFilePath();

            if (!File.Exists(filePath))
            {
                return new Dictionary<string, OmgOutfit>();
            }

            string json = File.ReadAllText(filePath);
            return JsonConvert.DeserializeObject<Dictionary<string, OmgOutfit>>(json);
        }

        public void SaveOutfits(Dictionary<string, OmgOutfit> outfits)
        {
            string filePath = GetOutfitJsonFilePath();

            string json = JsonConvert.SerializeObject(outfits, Formatting.Indented);
            File.WriteAllText(filePath, json);
        }

        public void MigrateOutfits()
        {
            // Check if the Outfits.json file exists, if it does, the migration is not needed
            if (File.Exists(GetOutfitJsonFilePath()))
            {
                return;
            }

            // Get the outfits from the old configuration
            Dictionary<string, OmgOutfit> oldOutfits = this._plugin.Configuration.MyCharacter.Outfits;

            // Save the outfits to the new JSON file
            SaveOutfits(oldOutfits);

            // Clear the old outfits from the configuration and save the configuration
        //    this._plugin.Configuration.MyCharacter.Outfits.Clear();
            this._plugin.Configuration.Save();
        }

        public void CreateSnapshot()
        {
            try
            {

                string collectionName = "";
                if (this._plugin.Configuration.PenumbraCollectionType != "Your Character")
                {
                    collectionName = GetCollectionForType.Subscriber(DalamudService.PluginInterface).Invoke(ApiCollectionType.Current);
                }
                else
                {
                    collectionName = GetCollectionForType.Subscriber(DalamudService.PluginInterface).Invoke(ApiCollectionType.Yourself);
                }

                this.Snapshot = new OmgOutfit
                {
                    IsSnapshot = true,
                    GlamourerData = GlamourerIpc.Instance.GetAllCustomizationFromCharacterIpc(DalamudService.ClientState.LocalPlayer),
                    CollectionName = collectionName
                };
            }
            catch
            {
                this.Snapshot = new OmgOutfit();
            }
        }

        public void ClearSnapshot(string collection, string glamourerBase64)
        {
            this.Snapshot = new OmgOutfit();
        }

        public void ApplySnapshot()
        {
            string collectionName = "";

            if (this._plugin.Configuration.PenumbraCollectionType != "Your Character")
            {
                collectionName = GetCollectionForType.Subscriber(DalamudService.PluginInterface).Invoke(ApiCollectionType.Current);
            }
            else
            {
                collectionName = GetCollectionForType.Subscriber(DalamudService.PluginInterface).Invoke(ApiCollectionType.Yourself);
            }

            GlamourerIpc.Instance.ApplyAllToCharacterIpc(this.Snapshot.GlamourerData, DalamudService.ClientState.LocalPlayer);
        }
    }
}
