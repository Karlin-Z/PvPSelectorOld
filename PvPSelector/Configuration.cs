using Dalamud.Configuration;
using Dalamud.Plugin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace PvPSelector
{
    [Serializable]
    class Configuration : IPluginConfiguration
    {
        public int Version { get; set; } = 0;


        public bool ConfigWindowVisible = false;
        public Vector2 ConfigWindowPos = new Vector2(20, 20);
        public Vector2 ConfigWindowSize = new Vector2(300, 300);
        public float ConfigWindowBgAlpha = 1;


        public float SelectDistance = 25;
        public float SelectInterval = 50;
        public bool Includ_NPC = false;

        [NonSerialized]
        public bool AutoSelect=false;

        [NonSerialized]
        public DalamudPluginInterface pluginInterface;
        [NonSerialized]
        public List<Dalamud.Game.ClientState.Actors.Types.Actor> EnermyActors = new List<Dalamud.Game.ClientState.Actors.Types.Actor>();
        [NonSerialized]
        public Dalamud.Game.ClientState.Actors.Types.Actor LocalPlayer;

        public void Initialize(DalamudPluginInterface pluginInterface)
        {
            this.pluginInterface = pluginInterface;
        }

        public void Save()
        {
            if (this.pluginInterface != null)
            {
                this.pluginInterface.SavePluginConfig(this);
            }

        }
    }
}
