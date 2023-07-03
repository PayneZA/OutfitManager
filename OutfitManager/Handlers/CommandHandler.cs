using Dalamud.Interface.Windowing;
using OutfitManager.Ipc;
using OutfitManager.Services;
using Penumbra.Api.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;
using static Penumbra.Api.Ipc;

namespace OutfitManager.Handlers
{
    public class CommandHandler
    {
        private readonly Plugin _plugin;
        public CommandHandler(Plugin plugin)
        {
            _plugin = plugin;
        }

        public void OnCommand(string command, string args)
        {
            string originalArgs = args;
            args = args.ToLower().Trim();

            if (this._plugin.OutfitHandler.OutfitLock)
            {
                DalamudService.Chat.Print("You have been locked out of omg by a friend.");
                return;
            }

            if (string.IsNullOrEmpty(args))
            {
                if (!string.IsNullOrEmpty(this._plugin.Configuration.MyCharacter.Name) && !string.IsNullOrEmpty(this._plugin.Configuration.MyCharacter.World))
                {
                    this._plugin.WindowSystem.GetWindow("OutfitManager Outfit List Window").IsOpen = true;
                }
                else
                {
                    this._plugin.WindowSystem.GetWindow("OutfitManager").IsOpen = true;
                }
                return;
            }

            string[] splitArgs = args.Split(' ', 2);
            string action = splitArgs[0];
            string actionArgs = splitArgs.Length > 1 ? splitArgs[1].Trim() : "";

            switch (action)
            {
                case "config":
                    this._plugin.WindowSystem.GetWindow("Outfit Manager Configuration Window").IsOpen = true;
                    break;
                case "menu":
                    this._plugin.WindowSystem.GetWindow("OutfitManager").IsOpen = true;
                    break;
                case "wear":
                    this._plugin.OutfitHandler.EquipOutfit(actionArgs);
                    break;
                case "random":
                    this._plugin.OutfitHandler.EquipOutfit("", actionArgs);
                    break;
                case "other":
                    this._plugin.WindowSystem.GetWindow("OutfitManager Other Character Window").IsOpen = true;
                    break;
                case "persist":
                    this._plugin.Configuration.Persist = actionArgs == "true" || actionArgs == "on";
                    this._plugin.Configuration.Save();
                    break;
                case "lockoutfit":
                    this._plugin.OutfitHandler.ToggleLock();
                    if (int.TryParse(actionArgs, out int locktime) && locktime > 0)
                    {
                        this._plugin.OutfitHandler.UnlockOutfit(locktime);
                    }
                    break;
                case "reset":

                    args = args.ToLower().Replace("reset", "").Trim();

                    this._plugin.OutfitName = "";
                    this._plugin.Configuration.OutfitName = "";
                    this._plugin.OutfitHandler.Snapshot = new Models.OmgOutfit();
                    if (!string.IsNullOrEmpty(this._plugin.Configuration.PrimaryCollection))
                    {
                        if (this._plugin.Configuration.PenumbraCollectionType != "Your Character")
                        {
                            SetCollectionForObject.Subscriber(DalamudService.PluginInterface).Invoke(0, this._plugin.Configuration.PrimaryCollection, true, false);
                        }
                        else
                        {
                            SetCollectionForType.Subscriber(DalamudService.PluginInterface).Invoke(ApiCollectionType.Yourself, this._plugin.Configuration.PrimaryCollection, true, false);
                        }
                        //  RelayCommand($"/penumbra collection {this.Configuration.PenumbraCollectionType} | {this.Configuration.PrimaryCollection} | <me>");
                        DalamudService.Chat.Print($"Your last worn outfit has been cleared and collection set to {this._plugin.Configuration.PrimaryCollection}");
                    }
                    else
                    {
                        DalamudService.Chat.Print($"Your last worn outfit has been cleared.");
                    }
                    this._plugin.Configuration.Save();

                    break;
                case "setcollectiontype":

                    if (args.ToLower().Contains("reset"))
                    {
                        this._plugin.Configuration.PenumbraCollectionType = "Your Character";
                    }
                    else
                    {
                        this._plugin.Configuration.PenumbraCollectionType = originalArgs.Trim().Remove(0, 17).Trim();
                    }
                    this._plugin.Configuration.Save();

                    DalamudService.Chat.Print($"Set penumbra collection type to {this._plugin.Configuration.PenumbraCollectionType}");
                    break;
                case "snapshot":

                    try
                    {
                        this._plugin.OutfitHandler.CreateSnapshot();
                    }
                    catch(Exception ex)
                    {

                    }
                 
                    break;
                case "clearsnapshot":

                    try
                    {
                     
                        this._plugin.OutfitHandler.Snapshot = new Models.OmgOutfit();
                    }
                    catch (Exception ex)
                    {

                    }

                    break;

                //case "generatepreviews":

                //    try
                //    {
                //        if (Environment.OSVersion.Platform == PlatformID.Win32NT)
                //        {
                //          this._plugin.CaptureService = new OutfitCaptureService(this._plugin,this._plugin.OutfitHandler);
                //          this._plugin.CaptureService.CaptureOutfits();
                //        }
                //        else
                //        {
                //            DalamudService.Chat.Print("This is only available to windows users.");
                //        }
                      
                //    }
                //    catch (Exception ex)
                //    {

                //    }

                //    break;
                //case "haltpreviews":

                //    try
                //    {
                //        this._plugin.CaptureService.HaltScreenshots = true;

                //    }
                //    catch (Exception ex)
                //    {

                //    }

                //    break;

            }
        }
    }
}
