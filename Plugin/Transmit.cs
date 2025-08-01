using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API.Modules.Memory.DynamicFunctions;

namespace FortniteEmotes;
public partial class Plugin
{
    //Client Crash Fix From: https://github.com/qstage/CS2-HidePlayers
    private static readonly MemoryFunctionVoid<CCSPlayerPawn, CSPlayerState> StateTransition = new(GameData.GetSignature("StateTransition"));
    private readonly INetworkServerService networkServerService = new();

    public void Transmit_OnLoad()
    {
        if(Config.EmoteHidePlayers != 0)
        {
            RegisterListener<Listeners.CheckTransmit>(Hook_CheckTransmit);
            StateTransition.Hook(Hook_StateTransition, HookMode.Post);
        }
    }

    public void Transmit_OnUnload()
    {
        if(Config.EmoteHidePlayers != 0)
        {
            RemoveListener<Listeners.CheckTransmit>(Hook_CheckTransmit);
            StateTransition.Unhook(Hook_StateTransition, HookMode.Post);
        }
    }

    [GameEventHandler(HookMode.Pre)]
    public HookResult OnPlayerDeathPre(EventPlayerDeath @event, GameEventInfo @info)
    {
        CCSPlayerController? player = @event.Userid;
        if (player != null && player.IsValid)
        {
            player.DesiredObserverMode = (int)ObserverMode_t.OBS_MODE_ROAMING;
            Server.NextFrame(() =>
            {
                if (player != null && player.IsValid)
                {
                    player.ObserverPawn.Value!.ObserverServices!.ForcedObserverMode = true;
                    player.ObserverPawn.Value!.ObserverServices!.ObserverMode = (byte)ObserverMode_t.OBS_MODE_ROAMING;
                    player.ObserverPawn.Value!.ObserverServices!.ObserverLastMode = ObserverMode_t.OBS_MODE_ROAMING;
                }
            });
        }
        return HookResult.Continue;
    }

    private HookResult Hook_StateTransition(DynamicHook hook)
    {
        var player = hook.GetParam<CCSPlayerPawn>(0).OriginalController.Value;
        var state = hook.GetParam<CSPlayerState>(1);

        if (player is null) return HookResult.Continue;

        if (state != player.Pawn.Value?.As<CCSPlayerPawnBase>().PlayerState)
        {
            ForceFullUpdate(player);
        }

        return HookResult.Continue;
    }

    private void Hook_CheckTransmit(CCheckTransmitInfoList infoList)
    {
        foreach ((CCheckTransmitInfo info, CCSPlayerController? player) in infoList)
        {
            if (!player.IsValidPlayer())
                continue;

            var steamID = player!.SteamID;

            foreach (var target in Utilities.GetPlayers())
            {
                if (target == null || target.IsHLTV || target.Slot == player.Slot)
                    continue;

                var pawn = target.PlayerPawn.Value!;

                if (player.Pawn.Value?.As<CCSPlayerPawnBase>().PlayerState == CSPlayerState.STATE_OBSERVER_MODE)
                    continue;

                if (pawn == null)
                    continue;

                if ((LifeState_t)pawn.LifeState != LifeState_t.LIFE_ALIVE)
                {
                    info.TransmitEntities.Remove((int)pawn.Index);
                    continue;
                }

                if(!g_PlayerSettings.ContainsKey(steamID))
                    continue;

                if(!g_PlayerSettings[steamID].IsDancing)
                    continue;

                switch(Config.EmoteHidePlayers)
                {
                    case 1:
                        if(player.Team != target.Team)
                            info.TransmitEntities.Remove((int)pawn.Index);
                        break;
                    case 2:
                        if(player.Team == target.Team)
                            info.TransmitEntities.Remove((int)pawn.Index);
                        break;
                    case 3:
                        info.TransmitEntities.Remove((int)pawn.Index);
                        break;
                }
            }
        }
    }
    private void ForceFullUpdate(CCSPlayerController? player)
    {
        if (player is null || !player.IsValid) return;

        var networkGameServer = networkServerService.GetIGameServer();
        networkGameServer.GetClientBySlot(player.Slot)?.ForceFullUpdate();

        player.PlayerPawn.Value?.Teleport(null, player.PlayerPawn.Value.EyeAngles, null);
    }
}