using Dalamud.Interface.Windowing;
using ImGuiNET;
using OutfitManager.Models;
using System;
using System.Numerics;

namespace OutfitManager.Windows
{
    public class NoticeWindow : Window, IDisposable
    {
        private Plugin plugin;
        private bool shouldClose;

        public void Dispose()
        {
        }

        public NoticeWindow(Plugin plugin) : base(
           "OutfitManager Notice Window", ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse | ImGuiWindowFlags.NoResize)
        {
            this.plugin = plugin;
            this.SizeConstraints = new WindowSizeConstraints
            {
                MinimumSize = new Vector2(675, 630),
                MaximumSize = new Vector2(float.MaxValue, float.MaxValue)
            };
            this.Size = new Vector2(675, 630);
            this.PositionCondition = ImGuiCond.Appearing;
            this.Position = ImGui.GetMainViewport().GetCenter() - this.Size / 2;
        }

        public override void Draw()
        {
            if (shouldClose)
            {
                this.IsOpen = false;
                return;
            }

            ImGui.TextColored(new Vector4(0.26f, 0.59f, 0.98f, 1.0f), "NOTE: Outfit Manager Will Not Be Updated Post-Dawntrail !");
            ImGui.Spacing();
            ImGui.TextWrapped("What does this mean?");
            ImGui.TextWrapped("I have been unable to work on Outfit Manager due to my real life, beyond just 'keeping it working'. However, larger changes to Penumbra and Glamourer are making the 'keeping it working' part less and less viable.");
            ImGui.Spacing();
            ImGui.TextWrapped("Unfortunately, as one person who has no privy to Penumbra, Glamourer, or Dalamud development, I have to work with what I have in a reactionary capacity.");
            ImGui.TextWrapped("As it is, Penumbra and Glamourer's latest updates have potentially broken functionality of Outfit Manager, but it can still semi-function.");
            ImGui.TextWrapped("The changes anticipated for keeping it working Post-Dawntrail will most likely make me unable to spare the time to update it.");
            ImGui.Spacing();
            ImGui.TextWrapped("Maybe one day I will have the time and a decent overhaul can be done to bring it up to date. I really don't know.");
            ImGui.TextWrapped("I am sorry to disappoint you if you have been relying on this.");
            ImGui.Spacing();

            // Center the button
            float buttonWidth = 300;
            float buttonHeight = 30;
            ImGui.SetCursorPosX((ImGui.GetWindowSize().X - buttonWidth) / 2);
            ImGui.SetCursorPosY(ImGui.GetCursorPosY() + buttonHeight);

            if (ImGui.Button("I acknowledge reading the above.", new Vector2(buttonWidth, buttonHeight)))
            {
                this.plugin.Configuration.HasShowNotice = true;
                this.plugin.Configuration.Save();
                shouldClose = true; // Set the flag to close the window
            }
        }
    }
}
