using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace PvPSelector
{
    class PluginUI : IDisposable
    {
        public Configuration Configuration { get; set; }
        private Dalamud.Plugin.DalamudPluginInterface DalamudPluginInterface { get; set; }

        public PluginUI(Configuration configuration, Dalamud.Plugin.DalamudPluginInterface dalamudPluginInterface)
        {
            this.Configuration = configuration;
            this.DalamudPluginInterface = dalamudPluginInterface;
        }

        public void Draw()
        {
            DrawSettingWindow();
        }

        

        private void DrawSettingWindow()
        {
            if (!Configuration.ConfigWindowVisible)
            {
                return;
            }
            ImGui.SetNextWindowSize(Configuration.ConfigWindowSize, ImGuiCond.FirstUseEver);
            ImGui.SetNextWindowPos(Configuration.ConfigWindowPos, ImGuiCond.FirstUseEver);
            ImGui.SetNextWindowBgAlpha(Configuration.ConfigWindowBgAlpha);
            if (ImGui.Begin("Setting Window", ref Configuration.ConfigWindowVisible, ImGuiWindowFlags.AlwaysAutoResize))
            {
                ImGui.TextColored(new Vector4(127f / 255f, 255f / 255f, 212f / 255f, 1f), "欢迎使用 PvP_Selector");

                ImGui.Checkbox("Auto Select Enermy", ref Configuration.AutoSelect);
                ImGui.Checkbox("Includ NPC", ref Configuration.Includ_NPC);

                ImGui.SliderFloat("Select Distance", ref Configuration.SelectDistance, 0, 50, "%.1f");
                

                //ImGui.SliderFloat("Auto Select Interval", ref Configuration.SelectInterval, 20f,5000f, "%.0f");
                //ImGui.InputFloat("Auto Select Interval", ref Configuration.SelectInterval, 50);
                ImGui.DragFloat("Auto Select Interval", ref Configuration.SelectInterval, 50, 0, 5000, "%.0f");
                Configuration.Save();
            }
            ImGui.End();



        }


        public void Dispose()
        {
            
        }
    }
}
