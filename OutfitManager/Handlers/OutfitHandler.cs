using Dalamud.Interface.Windowing;
using Dalamud.Logging;
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
using XivCommon;
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

        public bool AutoGlamourerUsed { get; set; }

        public OutfitHandler(Plugin plugin)
        {
            this._plugin = plugin;

            MigrateOutfits();

            // Load outfits from the JSON file
            this.Outfits = LoadOutfits();
            this.Snapshot = new OmgOutfit();

            try
            {
                foreach (var fit in this.Outfits)
                {
                    if (!string.IsNullOrEmpty(fit.Value.GlamourerData))
                    {
                        this.AutoGlamourerUsed = true;
                    }
                

                }
            }
            catch(Exception ex)
            {

            }
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
           // DalamudService.Chat.Print("Incompatability with new glamourer, please await update of outfitmanager.");
            try
            {
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
                        if (this._plugin.Configuration.PenumbraCollectionType != "Your Character")
                        {
                            SetCollectionForObject.Subscriber(DalamudService.PluginInterface).Invoke(0, outfit.CollectionName, true, false);
                        }
                        else
                        {
                            SetCollectionForType.Subscriber(DalamudService.PluginInterface).Invoke(ApiCollectionType.Yourself, outfit.CollectionName, true, false);
                        }
                    }

                    if (!string.IsNullOrEmpty(outfit.DesignPath.Trim()))
                    {
                        commands.Add(new RecievedCommand { CommandType = "plugin", Command = $"/glamour apply {outfit.DesignPath}|<me>" });
                    }
                    //else if (outfit.GlamourerData != null && !string.IsNullOrEmpty(outfit.GlamourerData.Trim()))
                    //{
                    //    GlamourerIpc.Instance?.ApplyOnlyEquipmentToCharacterIpc(outfit.GlamourerData, DalamudService.ClientState.LocalPlayer);
                    //}

                    if (this._plugin.Configuration.EnableCustomizeSupport)
                    {
                        if (outfit.CustomizeScaleName != null && outfit.CustomizeScaleName.Trim() != "")
                        {

                            this._plugin.Configuration.LastAppliedScale = outfit.CustomizeScaleName;
                            commands.Add(new RecievedCommand { CommandType = "plugin", Command = $"/capply {DalamudService.ClientState.LocalPlayer.Name.TextValue},{outfit.CustomizeScaleName}" });

                        }
                        else if (this._plugin.Configuration.ResetScalesToDefault)
                        {
                            this._plugin.Configuration.LastAppliedScale = "default-omg-scale";
                            commands.Add(new RecievedCommand { CommandType = "plugin", Command = $"/capply {DalamudService.ClientState.LocalPlayer.Name.TextValue},default-omg-scale" });

                        }
                    }

                    int delay = 0;
                    if (!string.IsNullOrEmpty(outfit.GearSet) && gearset)
                    {
                        this.IgnoreGsEquip = true;
                        this._plugin.Common.Functions.Chat.SendMessage("/gearset change " + outfit.GearSet.Trim());
                        delay = 300;
                    }

                    ExecuteCommands(commands, outfitName);

                }
            }
            catch (NullReferenceException ex)
            {
                // Log the error
                PluginLog.Error(ex, "Error: NullReferenceException caught.");
            }
            catch (KeyNotFoundException ex)
            {
                // Log the error
                PluginLog.Error(ex, "Error: KeyNotFoundException caught.");
            }
            catch (Exception ex)
            {
                // Log the error
                PluginLog.Error(ex, "Error: An unexpected error occurred.");
            }
        }

        private void ExecuteCommands(List<RecievedCommand> commands, string outfitname)
        {
            int delay = 100;
            int commandCount = commands.Count;

            for (int i = 0; i < commandCount; i++)
            {
                var recievedCommand = commands[i];
                var isLastCommand = (i == commandCount - 1);

                var timer = new System.Timers.Timer(delay);
                timer.Elapsed += (sender, e) =>
                {
                    timer.Stop();
                    _plugin.RelayCommand(recievedCommand.Command);

                    // If it's the last command, execute the subsequent logic
                    if (isLastCommand)
                    {
                        if (this._plugin.Configuration.LastOutfits.ContainsKey(DalamudService.ClientState.LocalPlayer.Name.TextValue))
                        {
                            this._plugin.Configuration.LastOutfits[DalamudService.ClientState.LocalPlayer.Name.TextValue] = outfitname;
                        }
                        else
                        {
                            this._plugin.Configuration.LastOutfits.Add(DalamudService.ClientState.LocalPlayer.Name.TextValue, outfitname);
                        }
                        this._plugin.Configuration.Save();
                    }
                };
                timer.Start();

                delay += 100;
            }
        }



        public string GetOutfitJsonFilePath()
        {
            string configDirectory = DalamudService.PluginInterface.GetPluginConfigDirectory();
            return Path.Combine(configDirectory, "Outfits.json");
        }

        public string GetGlamourerDesignFilePath()
        {
            string configDirectory = DalamudService.PluginInterface.GetPluginConfigDirectory().Replace("OutfitManager","Glamourer");
            if(Directory.Exists(configDirectory))
            {
                return Path.Combine(configDirectory, "Designs.json");
            }
            else
            {
                return "";
            }
        }

        public void MigrateToGlamourer()
        {
            // Fetch paths using your existing methods
            string designsPath = GetGlamourerDesignFilePath();
            string outfitsPath = GetOutfitJsonFilePath();

            if (!string.IsNullOrEmpty(designsPath))
            {
                // Read Designs.json
                var designs = JsonConvert.DeserializeObject<Dictionary<string, string>>(File.ReadAllText(designsPath));

                // Read Outfits.json
                var outfits = this.Outfits;

                try
                {
                    foreach (var outfitEntry in this.Outfits)
                    {
                        var outfit = outfitEntry.Value;
                        if (string.IsNullOrEmpty(outfit.DesignPath) && !string.IsNullOrEmpty(outfit.GlamourerData))
                        {
                            var baseKey = $"omgoutfits/{outfit.Name.ToLower()}";
                            var newDesignKey = baseKey;
                            int counter = 1;

                            while (designs.ContainsKey(newDesignKey))
                            {
                                newDesignKey = baseKey + counter;
                                counter++;
                            }

                            designs[newDesignKey] = outfit.GlamourerData;
                            outfit.DesignPath = $"/{newDesignKey}"; // Removing 'Collections/' prefix
                        }

                        outfitEntry.Value.GlamourerData = "";
                    }

               
                    // Save back to the files
                    File.WriteAllText(designsPath, JsonConvert.SerializeObject(designs, Formatting.Indented));
                    File.WriteAllText(outfitsPath, JsonConvert.SerializeObject(outfits, Formatting.Indented));
             
                    this.LoadOutfits();
                }
                catch(Exception ex) { 
                }
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
