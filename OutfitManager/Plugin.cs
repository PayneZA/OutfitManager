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
using ImGuiScene;
using System.Runtime.CompilerServices;
using XivCommon;
using Dalamud.Game.ClientState.Conditions;
using System.Text;
using static Penumbra.Api.Ipc;
using Penumbra.Api.Enums;
using System.Reflection.Metadata.Ecma335;
using FFXIVClientStructs.FFXIV.Client.Game.Character;
using Dalamud.Game.ClientState.Objects.Types;
using Dalamud.Logging;
using System.Text.RegularExpressions;
using OutfitManager.Ipc;
using Newtonsoft.Json.Linq;
using OutfitManager.Handlers;
using OutfitManager.Models;
using OutfitManager.Services;
using Lumina.Excel.GeneratedSheets;
using Dalamud.Game;

namespace OutfitManager
{
    public sealed class Plugin : IDalamudPlugin
    {
        public string Name => "Outfit Manager";
        private const string CommandName = "/omg";
        public DalamudPluginInterface PluginInterface { get; init; }
        private CommandManager CommandManager { get; init; }
        public Configuration Configuration { get; init; }
        private bool isCommandsEnabled { get; set; }

        public bool PersistOutfit { get; set; }

        public TextureWrap OutfitPreview;
        private ChatGui ChatGui { get; init; }
        public WindowSystem WindowSystem = new("OutfitManager");
        public XivCommonBase Common { get; init; }

   //     private bool _ignoreGsEquip { get;set; }
        public bool PersistGearset { get; set; }

        public bool IgnorePersistCollection { get; set; }
        private bool _transition;

     //   private bool _outfitLock;

        private bool _previousTransition;
        public string OutfitName { get; set; }

        public bool SnapshotActive { get; set; }

        public ChatHandler chatHandler;
        public OutfitHandler OutfitHandler { get; }

        public CommandHandler commandHandler;

        public OutfitCaptureService CaptureService;

        public string CurrentCharacter { get; set; }
        public bool Property
        {
            get { return _transition; }
            set
            {
                _previousTransition = _transition;
                _transition = value;

                // Check if the value has changed
                if (_previousTransition == true)
                {
                    // Trigger the second event
                    OnTransitionChanged();
                }
            }
        }
        public event EventHandler Transition;

        public Plugin(DalamudPluginInterface pluginInterface, CommandManager commandManager, ChatGui chatGui)
        {

            this.PluginInterface = pluginInterface;
            this.CommandManager = commandManager;
            this.ChatGui = chatGui;
            try
            {
                this.Common = new XivCommonBase(Hooks.None);
            }
            catch (NullReferenceException ex)
            {
                // Log the error
                PluginLog.Error(ex, "Failed to initialize XivCommonBase due to a NullReferenceException.");
            }
            catch (Exception ex)
            {
                // Catch any other unexpected exceptions
                PluginLog.Error(ex, "An unexpected error occurred while initializing XivCommonBase.");
            }


            this.Configuration = this.PluginInterface.GetPluginConfig() as Configuration ?? new Configuration();
            this.Configuration.Initialize(this.PluginInterface);
          

            this.isCommandsEnabled = this.Configuration.ChatControl;
            pluginInterface.Create<DalamudService>();
          
            this.chatHandler = new ChatHandler(this);
            this.OutfitHandler = new OutfitHandler(this);
            this.commandHandler = new CommandHandler(this);
       
    
            this.CommandManager.AddHandler(CommandName, new CommandInfo(OnCommand)
            {
                HelpMessage = $"No arguments to bring up UI (Will take you to outfits if you have added any otherwise config){Environment.NewLine}" +
                $"config = bring up configuration window{Environment.NewLine}" +
                $"menu = bring up main menu window{Environment.NewLine}" +
                $"wear OUTFITNAME = wear saved outfit name{Environment.NewLine}" +
                $"random TAGNAME = wear random outfit with tag{Environment.NewLine}" +
                $"other = bring up remote outfit control.{Environment.NewLine}{Environment.NewLine}" +
                $"The outfit preview system requires images with the same name as your outfit to exist in the preview directory you set in config. (Experimental){Environment.NewLine}{Environment.NewLine}" +
                $"persist - will re-apply your outfit whenever it would be needed zone change, login.{Environment.NewLine}" +
                $"lockoutfit SECONDS (optional) - will lock you into your last worn outfit including gearset, if seconds are specified then for that amount of seconds." +
                $"reset - will clear your last equipped outfit and if present set your primary penumbra collection you set." +
                $"setcollectiontype Individual - will set your penumbra collection type away from the default 'Your Character' to Individual, if you want to go back to 'Your Character' just put Reset instead of individual." +
                $"snapshot - will have a temporary penumbra / glamourer combination that will re-apply if re-apply is enabled and be lost on wear outfit or restart." +
                $"clearsnapshot - will manually clear the snapshot."
            });
       

            if (this.Configuration.MyCharacter == null || string.IsNullOrEmpty(this.Configuration.MyCharacter.Name))
            { 

                if (this.Configuration.Characters.Count > 0)
                {
                    var first = this.Configuration.Characters.First();
                    string key = first.Key;
                    OmgCharacter val = first.Value;

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

            if (this.Configuration.Persist)
            {
                this.Configuration.Persist = false;
                this.Configuration.Save();
            }
            WindowSystem.AddWindow(new ConfigWindow(this));
            WindowSystem.AddWindow(new MainWindow(this));
            WindowSystem.AddWindow(new AllowedCharacterWindow(this));
            WindowSystem.AddWindow(new OutfitListWindow(this));
            WindowSystem.AddWindow(new OtherCharactersWindow(this));
            WindowSystem.AddWindow(new OutfitPreviewWindow(this));

            this.PluginInterface.UiBuilder.Draw += DrawUI;
            this.PluginInterface.UiBuilder.OpenConfigUi += DrawConfigUI;

            DalamudService.Conditions.ConditionChange += OnTransitionChange;
            DalamudService.ClientState.Login += OnLogin;

            SetChatMonitoring(this.isCommandsEnabled);

            if (this.Configuration.OutfitName == null)
            {
                this.Configuration.OutfitName = "";
            }


            SetCharacterAndWorld();
        

   
        }

        public void OnLogin(object? sender, EventArgs e)
        {
            SetCharacterAndWorld();
        }

        public void HideAllWindows()
        {
            this.WindowSystem.GetWindow("OutfitManager Outfit List Window").IsOpen = false;
            this.WindowSystem.GetWindow("OutfitManager Allowed Character Window").IsOpen = false;
            this.WindowSystem.GetWindow("OutfitManager").IsOpen = false;
            this.WindowSystem.GetWindow("Outfit Preview Window").IsOpen = false;
        }

        protected void OnTransitionChanged()
        {
            if (this.Configuration.Persist || this.OutfitHandler.OutfitLock)
            {
              
                if ((this.Configuration.LastOutfits.ContainsKey(DalamudService.ClientState.LocalPlayer.Name.TextValue) && this.Configuration.LastOutfits[DalamudService.ClientState.LocalPlayer.Name.TextValue] != "") || this.OutfitHandler.Snapshot.IsSnapshot)
                {
                    if (this.OutfitHandler.Snapshot.IsSnapshot)
                    {
                        this.OutfitHandler.ApplySnapshot();
                    }
                    else
                    {

                        if (this.Configuration.LastOutfits[DalamudService.ClientState.LocalPlayer.Name.TextValue] != "")
                        {

                            this.OutfitHandler.EquipOutfit(this.Configuration.LastOutfits[DalamudService.ClientState.LocalPlayer.Name.TextValue], "", false, this.Configuration.IgnorePersistCollection);
                        }
                    }
                }
            }


        }

        private void SetCharacterAndWorld()
        {
            try
            {
           
                this.CurrentCharacter = DalamudService.ClientState?.LocalPlayer?.Name?.TextValue ?? "";

                if (this.Configuration.MyCharacter == null)
                {
                    this.Configuration.MyCharacter = new OmgCharacter
                    {
                        Name = this.CurrentCharacter,
                        World = DalamudService.ClientState.LocalPlayer.HomeWorld.GameData.Name,
                        FullName = $"{this.CurrentCharacter}@{DalamudService.ClientState.LocalPlayer.HomeWorld.GameData.Name}"
                    };
                }


                if (this.CurrentCharacter != "")
                {
                    if (this.Configuration.LastOutfits == null)
                    {
                        this.Configuration.LastOutfits = new Dictionary<string, string> { { this.CurrentCharacter, "" } };
                    }
                    else if (!this.Configuration.LastOutfits.ContainsKey(this.CurrentCharacter))
                    {
                        this.Configuration.LastOutfits.Add(this.CurrentCharacter, "");

                    }

                    this.Configuration.Save();
                }
            }
            catch (Exception ex)
            {
           
            }
        }


        
        private void OnTransitionChange(ConditionFlag flag, bool value)
        {
            if (flag == ConditionFlag.BetweenAreas51)
            {
                Property = !Property;
            }
        }
        public void SetChatMonitoring(bool active)
        {
            if (active)
            {
                this.isCommandsEnabled = true;
                this.Configuration.ChatControl = true;
                this.Configuration.Save();
                this.ChatGui.ChatMessage -= OnChatMessage;
                this.ChatGui.ChatMessage += OnChatMessage;
            }
            else
            {
                this.isCommandsEnabled = false;
                this.Configuration.ChatControl = false; ;
                this.Configuration.Save();
                this.ChatGui.ChatMessage -= OnChatMessage;
            }
        }
        public void Dispose()
        {
            DalamudService.ClientState.Login -= OnLogin;
            DalamudService.Conditions.ConditionChange -= OnTransitionChange;
            this.ChatGui.ChatMessage -= OnChatMessage;
            this.WindowSystem.RemoveAllWindows();
            this.CommandManager.RemoveHandler(CommandName);
        }

        private void OnCommand(string command, string args)
        {
            this.commandHandler.OnCommand(command,args);
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

        public List<string> GetAvailableCollections()
        {
            var collections = GetCollections.Subscriber(PluginInterface).Invoke().ToList();


            return collections;
        }

        public string GetCurrentCollection()
        {
            if (this.Configuration.PenumbraCollectionType != "Your Character")
            {

                return GetCollectionForType.Subscriber(PluginInterface).Invoke(ApiCollectionType.Current);
            }
            else
            {
                return GetCollectionForType.Subscriber(PluginInterface).Invoke(ApiCollectionType.Yourself);
            }

           
        }

        public async Task SendEquipOutfit(string character, string characterFirstname, string outfit)
        {
            this.Common.Functions.Chat.SendMessage($"/tell {character} wear:{outfit}");
        }

        public async Task SendLockOutfit(string character, string characterFirstname, string outfitlock = "off")
        {
            this.Common.Functions.Chat.SendMessage($"/tell {character} forceoutfit:{outfitlock}");
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

        public void SetImagePreview(string name)
        {
            var imagePath = Path.Combine(this.Configuration.PreviewDirectory, $"{name}.png");

            if (this.OutfitPreview != null)
            {
                this.OutfitPreview.Dispose();
                this.OutfitPreview = null;
            }

            if (File.Exists(imagePath))
            {
                ShowOrHideWindow("Outfit Preview Window", true);
                this.OutfitPreview = this.PluginInterface.UiBuilder.LoadImage(imagePath);
            }

            else
            {
                this.OutfitPreview = null;
            }
        }

        public void ShowOrHideWindow(string name, bool visible)
        {
            WindowSystem.GetWindow(name).IsOpen = visible;


            if(name == "Outfit Preview Window" && !visible)
            {
                this.OutfitPreview = null;
            }
        }
        private void OnChatMessage(XivChatType type, uint id, ref SeString sender, ref SeString message,
    ref bool handled)
        {
            try
            {
                this.chatHandler.ProcessChatMessage(type, id, ref sender, ref message, ref handled);

            }
            catch (Exception ex)
            {
                isCommandsEnabled = false;
            }
        }
    }
}
