using Dalamud.Interface.Windowing;
using Dalamud.Logging;

using OutfitManager.Models;
using OutfitManager.Services;
using Penumbra.Api.Enums;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;


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
            if (!this._plugin.Configuration.HasShowNotice)
            {
                this._plugin.ShowOrHideWindow("OutfitManager Notice Window", true);


                return;
            }

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
                    this._plugin.ShowOrHideWindow("OutfitManager Outfit List Window", true);
                }
                else
                {
                    this._plugin.ShowOrHideWindow("OutfitManager", true);
                }
                return;
            }

            string[] splitArgs = args.Split(' ', 2);
            string action = splitArgs[0];
            string actionArgs = splitArgs.Length > 1 ? splitArgs[1].Trim() : "";

            switch (action)
            {
                case "config":
                    this._plugin.ShowOrHideWindow("Outfit Manager Configuration Window", true);
              //      this._plugin.WindowSystem.GetWindow("Outfit Manager Configuration Window").IsOpen = true;
                    break;
                case "menu":
                    this._plugin.ShowOrHideWindow("OutfitManager", true);
                    break;
                case "wear":
                    this._plugin.OutfitHandler.EquipOutfit(actionArgs);
                    break;
                case "random":
                    this._plugin.OutfitHandler.EquipOutfit("", actionArgs);
                    break;
                case "other":
                    this._plugin.ShowOrHideWindow("OutfitManager Other Character Window", true);
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

                    this._plugin.RelayCommand("/penumbra bulktag inherit clothing | b:-=CLOTHING=-");
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
                        DalamudService.Chat.Print($"No longer supported.");
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
            }
        }
    }
}
