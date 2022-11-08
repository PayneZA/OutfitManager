using Dalamud.Game.Command;
using Dalamud.IoC;
using Dalamud.Plugin;
using System.IO;
using System.Reflection;
using Dalamud.Interface.Windowing;
using OutfitManager.Windows;
using Dalamud.Game.Gui;
using Dalamud.Hooking;
using Newtonsoft.Json;
using System;
using XivCommon;
using Dalamud.Game.Text;
using System.Collections.Generic;
using Dalamud.Game.Text.SeStringHandling;
using System.Threading.Tasks;
using System.Linq;

namespace OutfitManager
{
    public sealed class Plugin : IDalamudPlugin
    {
        public string Name => "Outfit Manager";
        private const string CommandName = "/omg";
        private DalamudPluginInterface PluginInterface { get; init; }
        private CommandManager CommandManager { get; init; }
        public Configuration Configuration { get; init; }
        private bool isCommandsEnabled { get; set; }

        private ChatGui ChatGui { get; init; }
        private WindowSystem WindowSystem = new("OutfitManager");
        private XivCommonBase Common { get; init; }
        public Plugin(DalamudPluginInterface pluginInterface, CommandManager commandManager, ChatGui chatGui)
        {
            Dalamud.Initialize(pluginInterface);
            this.PluginInterface = pluginInterface;
            this.CommandManager = commandManager;

            this.ChatGui = chatGui;
            this.Common = new XivCommonBase(Hooks.None);

            this.Configuration = this.PluginInterface.GetPluginConfig() as Configuration ?? new Configuration();
            this.Configuration.Initialize(this.PluginInterface);
            this.isCommandsEnabled = this.Configuration.ChatControl;
       
            this.CommandManager.AddHandler(CommandName, new CommandInfo(OnCommand)
            {
                HelpMessage = $"No arguments to bring up UI (Will take you to outfits if you have added any otherwise config){Environment.NewLine}config = bring up configuration window{Environment.NewLine}wear OUTFITNAME = wear saved outfit name{Environment.NewLine}random TAGNAME = wear random outfit with tag{Environment.NewLine}"
            });


            if (this.Configuration.MyCharacter == null || string.IsNullOrEmpty(this.Configuration.MyCharacter.Name))
            { 

                if (this.Configuration.Characters.Count > 0)
                {
                    var first = this.Configuration.Characters.First();
                    string key = first.Key;
                    Character val = first.Value;

                    this.Configuration.MyCharacter = val;
                    this.Configuration.MyCharacter.Name = first.Key;
                    this.Configuration.Save();
                }
            }

            if (string.IsNullOrEmpty(this.Configuration.MyCharacter.FullName) && !string.IsNullOrEmpty(this.Configuration.MyCharacter.Name) && !string.IsNullOrEmpty(this.Configuration.MyCharacter.World))
            {
                this.Configuration.MyCharacter.FullName = $"{this.Configuration.MyCharacter.Name}@{this.Configuration.MyCharacter.World}";
                this.Configuration.Save();
            }

            WindowSystem.AddWindow(new ConfigWindow(this));
            WindowSystem.AddWindow(new MainWindow(this));
            WindowSystem.AddWindow(new AllowedCharacterWindow(this));
            WindowSystem.AddWindow(new OutfitListWindow(this));

            this.PluginInterface.UiBuilder.Draw += DrawUI;
            this.PluginInterface.UiBuilder.OpenConfigUi += DrawConfigUI;

            this.ChatGui.ChatMessage += OnChatMessage;
        }

        public void Dispose()
        {
            this.ChatGui.ChatMessage -= OnChatMessage;
            this.WindowSystem.RemoveAllWindows();
            this.CommandManager.RemoveHandler(CommandName);
        }

        private void OnCommand(string command, string args)
        {
            // in response to the slash command, just display our main ui
            args = args.ToLower().Trim();

            if (string.IsNullOrEmpty(args))
            {
               if (this.Configuration.MyCharacter.Name != "" && this.Configuration.MyCharacter.World != "")
                {
                    
                    WindowSystem.GetWindow("OutfitManager Outfit List Window").IsOpen = true;
                }
               else
                {
                    WindowSystem.GetWindow("OutfitManager").IsOpen = true;
                }    
            }
            else
            {
                if (args.StartsWith("config"))
                {
                    WindowSystem.GetWindow("OutfitManager").IsOpen = true;
                }
                else if (args.StartsWith("wear"))
                {
                   args = args.Remove(0, 4).Trim();

                    EquipOutfit(args.ToLower());
                }
                else if (args.StartsWith("random"))
                {
                    args = args.Remove(0, 6).Trim();

                    EquipOutfit("",args);
                }
            }
        }

        private void DrawUI()
        {
            this.WindowSystem.Draw();
        }

        public void DrawConfigUI()
        {
            WindowSystem.GetWindow("OutfitManager").IsOpen = true;
        }
        public void DrawAllowedUserUI()
        {
            WindowSystem.GetWindow("OutfitManager Allowed Character Window").IsOpen = true;
        }
        public void DrawOutfitListUI()
        {
            WindowSystem.GetWindow("OutfitManager Outfit List Window").IsOpen = true;
        }


        public void EquipOutfit(string outfitName = "", string tag = "")
        {
            Outfit outfit = null;

            if (!string.IsNullOrEmpty(tag))
            {
                List<Outfit> outfits = this.Configuration.MyCharacter.Outfits.Values.Where(x => x.Tags.Contains(tag)).ToList();

                if (outfits.Count > 0)
                {
                    Random random = new Random();
                    int index = random.Next(outfits.Count);
                    outfit = outfits[index];
                }
            }
            else
            {
                outfit = this.Configuration.MyCharacter.Outfits[outfitName];
            }

            if (outfit != null)
            {

                List<RecievedCommand> commands = new List<RecievedCommand>();

                if (!string.IsNullOrEmpty(outfit.CollectionName.Trim()))
                {
                    commands.Add(new RecievedCommand { CommandType = "plugin", Command = $"/penumbra collection yourself {outfit.CollectionName}" });
                }
                if (!string.IsNullOrEmpty(outfit.DesignPath.Trim()))
                {
                    commands.Add(new RecievedCommand { CommandType = "plugin", Command = $"/glamour apply,{this.Configuration.MyCharacter.Name},{outfit.DesignPath}" });
                }
                int delay = 0;
                if (!string.IsNullOrEmpty(outfit.GearSet))
                {
                    this.Common.Functions.Chat.SendMessage("/gearset change " + outfit.GearSet.Trim());

                    delay = 300;
                }

                foreach (RecievedCommand recievedCommand in commands)
                {
                    RelayCommand(recievedCommand.Command, delay += 100);
                }
            }
        }

        public async Task RelayCommand(string command, int delay = 100)
        {
            if (!string.IsNullOrEmpty(command.Trim()))
            {
                await DelayTask(delay);

                CommandManager.ProcessCommand(command);
            }
        }

        static async Task DelayTask(int delay)
        {
            Task task = Task.Delay(delay);
            await task;
        }

        private void OnChatMessage(XivChatType type, uint id, ref SeString sender, ref SeString message,
    ref bool handled)
        {
            try
            {
                if (this.Configuration.ChatControl)
                {
                    if (type == XivChatType.TellIncoming)
                    {
                        string name = this.Configuration.MyCharacter.Name.ToLower();
                        if (message.TextValue.ToLower().StartsWith(name + " wear:") || message.TextValue.ToLower().StartsWith(name + " random:"))
                        {

                            var payloads = sender.Payloads[0].ToString();
                            var payloadElements = payloads.Split(",").ToList();
                            var playername = payloadElements[0].Split(":")[1].Trim();
                            var world = payloadElements[2].Split(":")[1].Trim();

              
                            if (this.Configuration.SafeSenders.Keys.Contains($"{playername}@{world}") || this.Configuration.SafeSenders.ContainsKey("*"))
                            {
                                Outfit outfit = null;
                                string textValue = message.TextValue.Remove(0, message.TextValue.Substring(0, message.TextValue.IndexOf(":")).Length + 1).Trim();

                                if (message.TextValue.ToLower().StartsWith(name + " random:"))
                                {
                                    EquipOutfit("", textValue.Trim().ToLower());
                                }
                                else
                                {

                                    if (this.Configuration.MyCharacter.Outfits.ContainsKey(textValue.Trim().ToLower()))
                                    {
                                        outfit = this.Configuration.MyCharacter.Outfits[textValue.Trim().ToLower()];

                                        EquipOutfit(outfit.Name);
                                    }
                                    else
                                    {
                                        Dalamud.Chat.Print("Outfit not found");
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                isCommandsEnabled = false;
            }
            //if (!this.config.Enabled) return;
            //var chatMessageHandler = this.services.GetService<ChatMessageHandler>();
            //chatMessageHandler.ProcessMessage(type, id, ref sender, ref message, ref handled);
        }
    }
}
