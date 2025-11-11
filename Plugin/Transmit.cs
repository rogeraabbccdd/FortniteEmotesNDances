using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;

namespace FortniteEmotes;

public partial class Plugin
{
    public void Transmit_OnLoad()
    {
        if (Config.EmoteHidePlayers != 0)
        {
            RegisterListener<Listeners.CheckTransmit>(Hook_CheckTransmit);
        }
    }

    public void Transmit_OnUnload()
    {
        if (Config.EmoteHidePlayers != 0)
        {
            RemoveListener<Listeners.CheckTransmit>(Hook_CheckTransmit);
        }
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
                if (target.IsHLTV || target.Slot == player.Slot)
                    continue;

                var pawn = target.PlayerPawn.Value!;

                if (player.Pawn.Value?.As<CCSPlayerPawnBase>().PlayerState == CSPlayerState.STATE_OBSERVER_MODE)
                    continue;

                if (pawn == null)
                    continue;

                if (!g_PlayerSettings.ContainsKey(steamID))
                    continue;

                if (!g_PlayerSettings[steamID].IsDancing)
                    continue;

                switch (Config.EmoteHidePlayers)
                {
                    case 1:
                        if (player.Team != target.Team)
                            info.TransmitEntities.Remove((int)pawn.Index);
                        break;
                    case 2:
                        if (player.Team == target.Team)
                            info.TransmitEntities.Remove((int)pawn.Index);
                        break;
                    case 3:
                        info.TransmitEntities.Remove((int)pawn.Index);
                        break;
                }
            }
        }
    }
}