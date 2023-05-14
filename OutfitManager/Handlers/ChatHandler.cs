using Dalamud.Game.Text;
using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Interface.Windowing;
using Dalamud.Plugin;
using OutfitManager.Models;
using OutfitManager.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OutfitManager.Handlers
{
    public class ChatHandler
    {
        private readonly Plugin _plugin;
        public ChatHandler(Plugin plugin)
        {
            _plugin = plugin;
        }

        public void ProcessChatMessage(XivChatType type, uint id, ref SeString sender, ref SeString message, ref bool handled)
        {
            if (!this._plugin.Configuration.ChatControl) return;

            string messageTextLower = message.TextValue.ToLower();

            if (type == XivChatType.TellIncoming &&
                (messageTextLower.StartsWith("wear:") || messageTextLower.StartsWith("random:") || messageTextLower.StartsWith("forceoutfit:")))
            {
                string name = this._plugin.Configuration.MyCharacter.Name;

                var payloads = sender.Payloads[0].ToString();
                var payloadElements = payloads.Split(",").ToList();
                var playername = payloadElements[0].Split(":")[1].Trim();
                var world = payloadElements[2].Split(":")[1].Trim();
                string playerKey = $"{playername}@{world}";

                if (this._plugin.Configuration.SafeSenders.Keys.Contains(playerKey) || this._plugin.Configuration.SafeSenders.ContainsKey("everyone@everywhere"))
                {
                    OmgOutfit outfit = null;
                    string textValue = message.TextValue.Substring(message.TextValue.IndexOf(":") + 1).Trim();
                    string textValueLower = textValue.ToLower();

                    if (messageTextLower.StartsWith("random:"))
                    {
                        this._plugin.OutfitHandler.EquipOutfit("", textValueLower);
                    }
                    else if (messageTextLower.StartsWith("forceoutfit:") && this._plugin.Configuration.SafeSenders[playerKey].canOutfitLock)
                    {
                        if (textValueLower == "toggle")
                        {
                            this._plugin.OutfitHandler.ToggleLock();
                        }
                    }
                    else
                    {
                        if (this._plugin.OutfitHandler.Outfits.ContainsKey(textValueLower))
                        {
                            outfit = this._plugin.OutfitHandler.Outfits[textValueLower];
                            this._plugin.OutfitHandler.EquipOutfit(outfit.Name);
                        }
                        else
                        {
                            DalamudService.Chat.Print("Outfit not found");
                        }
                    }
                }
            }
            else if (type == XivChatType.SystemMessage && messageTextLower.Contains("equipped"))
            {
                if (this._plugin.OutfitHandler.IgnoreGsEquip)
                {
                    this._plugin.OutfitHandler.IgnoreGsEquip = false;
                }
                else if (this._plugin.OutfitHandler.OutfitLock || this._plugin.Configuration.Persist)
                {
                    if (!string.IsNullOrEmpty(this._plugin.Configuration.OutfitName?.Trim()))
                    {
                        if (this._plugin.OutfitHandler.OutfitLock)
                        {
                            this._plugin.OutfitHandler.EquipOutfit(this._plugin.Configuration.OutfitName);
                        }
                        else if (this._plugin.Configuration.PersistGearset)
                        {
                            this._plugin.OutfitHandler.EquipOutfit(this._plugin.Configuration.OutfitName, "", false);
                        }
                    }
                }
            }
        }


        // You can move other chat-related methods from Plugin.cs to this class
    }

}
