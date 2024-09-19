using Dalamud.Interface.Windowing;
using Dalamud.Logging;
using Dalamud.Plugin;
using Newtonsoft.Json;
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

        public bool AutoGlamourerUsed { get; set; }

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
            try
            {
                this._plugin.RelayCommand("/penumbra bulktag inherit clothing | b:-=CLOTHING=-");
                OmgOutfit outfit = null;
                this.Snapshot = new OmgOutfit();

                if (this._plugin == null)
                {
                    throw new NullReferenceException("Plugin is not initialized");
                }

                if (!string.IsNullOrEmpty(tag))
                {
                    List<OmgOutfit> outfits = this._plugin.OutfitHandler.Outfits.Values.Where(x => x.Tags.Contains(tag)).ToList();

                    if (outfits.Count > 0)
                    {
                        Random random = new Random();
                        int index = random.Next(outfits.Count);
                        outfit = outfits[index];
                    }
                    else
                    {
                        throw new Exception("No outfits found with the provided tag");
                    }
                }
                else
                {
                    if (!this._plugin.OutfitHandler.Outfits.ContainsKey(outfitName))
                    {
                        throw new KeyNotFoundException($"No outfit found with the name {outfitName}");
                    }
                    outfit = this._plugin.OutfitHandler.Outfits[outfitName];
                }

                if (outfit != null)
                {
                    List<RecievedCommand> commands = new List<RecievedCommand>();

                    if (!string.IsNullOrEmpty(outfit.CollectionName.Trim()) && !ignoreCollection)
                    {

                        commands.Add(new RecievedCommand { CommandType = "plugin", Command = $"/penumbra collection Individual | {outfit.CollectionName} | <me>" });

                    }

                    if (!string.IsNullOrEmpty(outfit.DesignPath.Trim()))
                    {
                        commands.Add(new RecievedCommand { CommandType = "plugin", Command = $"/glamour apply {outfit.DesignPath}|<me>;true" });
                    }

                    int delay = 0;

                    foreach (RecievedCommand recievedCommand in commands)
                    {
                        _plugin.RelayCommand(recievedCommand.Command, delay += 100);
                    }

                    if (this._plugin.Configuration.LastOutfits.ContainsKey(DalamudService.ClientState.LocalPlayer.Name.TextValue))
                    {
                        this._plugin.Configuration.LastOutfits[DalamudService.ClientState.LocalPlayer.Name.TextValue] = outfitName;
                    }
                    else
                    {
                        this._plugin.Configuration.LastOutfits.Add(DalamudService.ClientState.LocalPlayer.Name.TextValue, outfitName);
                    }

                    //   this._plugin.Configuration.OutfitName = outfitName;
                    this._plugin.Configuration.Save();
                }
            }
            catch (NullReferenceException ex)
            {
                // Log the error
                //PluginLog.Error(ex, "Error: NullReferenceException caught.");
            }
            catch (KeyNotFoundException ex)
            {
                // Log the error
          //      PluginLog.Error(ex, "Error: KeyNotFoundException caught.");
            }
            catch (Exception ex)
            {
                // Log the error
         //       PluginLog.Error(ex, "Error: An unexpected error occurred.");
            }
        }


        public string GetOutfitJsonFilePath()
        {
            string configDirectory = Plugin.PluginInterface.GetPluginConfigDirectory();
            return Path.Combine(configDirectory, "Outfits.json");
        }

        public string GetGlamourerDesignFilePath()
        {
           
            string configDirectory = Plugin.PluginInterface.GetPluginConfigDirectory().Replace("OutfitManager","Glamourer");
            if(Directory.Exists(configDirectory))
            {
                return Path.Combine(configDirectory, "Designs.json");
            }
            else
            {
                return "";
            }
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
            this._plugin.Configuration.Save();
        }

        public void ClearSnapshot(string collection, string glamourerBase64)
        {
            this.Snapshot = new OmgOutfit();
        }
    }
}
