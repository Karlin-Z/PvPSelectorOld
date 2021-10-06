using Dalamud.Game.Command;
using Dalamud.Plugin;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace PvPSelector
{
    public class Plugin : IDalamudPlugin
    {
        public string Name => "PvPSelector";
        private DalamudPluginInterface DalamudPluginInterface { get; set; }
        private Configuration Configuration { get; set; }
        private PluginUI PluginUI { set; get; }

        DateTime LastSelectTime;

        private string PluginCommandStr = "/PvPSelector";

        #region 更新地址
        private CanAttackDelegate CanAttack;
        private delegate int CanAttackDelegate(int arg, IntPtr objectAddress);
        private const int CanAttackOffset = 0x802840;//Struct121_IntPtr_17

        private IntPtr TargetPtr;
        private const int TargetOffset = 0x1DB9550;//Struct121_IntPtr_5
        private IntPtr TargetIdPtr;
        private const int TargetIdOffset = 0x1DB9610;
        #endregion


        public void Initialize(DalamudPluginInterface pluginInterface)
        {
            this.DalamudPluginInterface = pluginInterface;
            this.Configuration = this.DalamudPluginInterface.GetPluginConfig() as Configuration ?? new Configuration();
            this.Configuration.Initialize(this.DalamudPluginInterface);
            this.PluginUI = new PluginUI(this.Configuration, this.DalamudPluginInterface);

            this.DalamudPluginInterface.UiBuilder.OnBuildUi += DrawUI;
            this.DalamudPluginInterface.UiBuilder.OnOpenConfigUi += (sender, args) => DrawConfigUI();

            this.DalamudPluginInterface.CommandManager.AddHandler(PluginCommandStr, new CommandInfo(OnCommand)
            {
                HelpMessage = $"/PvPSelector Help :Show Help."
            });


            CanAttack = Marshal.GetDelegateForFunctionPointer<CanAttackDelegate>(Process.GetCurrentProcess().MainModule.BaseAddress + CanAttackOffset);
            TargetPtr = Process.GetCurrentProcess().MainModule.BaseAddress + TargetOffset;
            TargetIdPtr = Process.GetCurrentProcess().MainModule.BaseAddress + TargetIdOffset;

            this.DalamudPluginInterface.Framework.OnUpdateEvent += ReFreshEnermyActors_And_AutoSelect;
        }

        private char[] para = { ' ' };
        private void OnCommand(string command, string arguments)
        {
            if (command== "/PvPSelector")
            {
                string[] args = arguments.Split(para,StringSplitOptions.RemoveEmptyEntries);
                if (args.Length == 0 | args.Length > 2)
                {
                    return;
                }
                if (args[0]=="Help")
                {
                    ShowHelp();
                }
                if (args[0]=="Once")
                {
                    float olddis = Configuration.SelectDistance;
                    if (args.Length == 2 && float.TryParse(args[1], out float result))
                    {
                        Configuration.SelectDistance = result;
                    }
                    SelectEnermyOnce();
                    Configuration.SelectDistance = olddis;
                }
                if (args[0] == "Auto")
                {
                    if (args.Length == 2 && float.TryParse(args[1], out float result))
                    {
                        Configuration.SelectDistance = result;
                    }
                    Configuration.AutoSelect = !Configuration.AutoSelect;
                }
                if (args[0] == "Setting")
                {
                    Configuration.ConfigWindowVisible = true;
                }
            }
        }

        private void SelectEnermyOnce()
        {
            if (Configuration.LocalPlayer==null || Configuration.EnermyActors==null)
            {
                return;
            }
            Dalamud.Game.ClientState.Actors.Types.Chara selectActor = null;
            foreach (Dalamud.Game.ClientState.Actors.Types.Chara actor in Configuration.EnermyActors)
            {
                try
                {
                    var distance2D = Math.Sqrt(Math.Pow(Configuration.LocalPlayer.Position.X - actor.Position.X, 2) + Math.Pow(Configuration.LocalPlayer.Position.Y - actor.Position.Y, 2)) - 1;
                    if (distance2D <= Configuration.SelectDistance & actor.CurrentHp != 0 & (selectActor == null || actor.CurrentHp < selectActor.CurrentHp))
                    {
                        selectActor = actor;
                    }
                }
                catch (Exception)
                {
                    continue;
                }
            }
            SelectTarget(selectActor);
            
        }


        private void SelectTarget(Dalamud.Game.ClientState.Actors.Types.Chara chara)
        {
            if (chara==null)
            {
                return;
            }
            if (Marshal.ReadInt32(this.TargetIdPtr)!=chara.ActorId)
            {
                Marshal.WriteInt64(TargetPtr, chara.Address.ToInt64());
            }
            
        }

        private void ShowHelp()
        {
            SendChat("/PvPSelector Once [distance] :SelectEnermy Once Ever");
            SendChat("/PvPSelector Auto [distance]:Auto SelectEnermy In Setting During");
            SendChat("/PvPSelector Setting :Show Setting Window");
        }
        private void SendChat(string message)
        {
            this.DalamudPluginInterface.Framework.Gui.Chat.Print(message);
        }
        private void ReFreshEnermyActors_And_AutoSelect(Dalamud.Game.Internal.Framework framework)
        {
            #region 刷新敌对列表

            
            if (DalamudPluginInterface.ClientState.LocalPlayer == null)
            {
                return;
            }
            Configuration.LocalPlayer = DalamudPluginInterface.ClientState.LocalPlayer;
            lock (Configuration.EnermyActors)
            {
                Configuration.EnermyActors.Clear();
                var actorTable = this.DalamudPluginInterface.ClientState.Actors;
                if (actorTable==null)
                {
                    return;
                }

                foreach (var actor in actorTable)
                {
                    try
                    {
                        if ((actor.ObjectKind == Dalamud.Game.ClientState.Actors.ObjectKind.Player|Configuration.Includ_NPC) &actor.Address.ToInt64()!=0 && CanAttack(142, actor.Address) == 1)
                        {
                            if (actor.ActorId != DalamudPluginInterface.ClientState.LocalPlayer.ActorId)
                            {
                                Configuration.EnermyActors.Add(actor);
                            }

                        }
                    }
                    catch (Exception)
                    {

                        continue;
                    }
                }
            }
            #endregion

            #region 自动选择
            if (Configuration.AutoSelect)
            {
                DateTime now = DateTime.Now;
                if (LastSelectTime == null || (now - LastSelectTime).TotalMilliseconds > Configuration.SelectInterval)
                {
                    SelectEnermyOnce();
                    LastSelectTime = now;
                }
            }
            
            #endregion
        }

        private void DrawUI()
        {
            this.PluginUI.Draw();
        }
        private void DrawConfigUI()
        {
            this.Configuration.ConfigWindowVisible = true;
        }
        public void Dispose()
        {
            this.PluginUI.Dispose();
            this.DalamudPluginInterface.Dispose();
            this.DalamudPluginInterface.Framework.OnUpdateEvent -= ReFreshEnermyActors_And_AutoSelect;
            this.DalamudPluginInterface.CommandManager.RemoveHandler(PluginCommandStr);
        }
    }
}
