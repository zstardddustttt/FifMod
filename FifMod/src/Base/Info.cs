using System;

namespace FifMod.Base
{
    public static class FifModGameInfo
    {
        public static bool IsMansion
        {
            get
            {
                var everyTeleport = UnityEngine.Object.FindObjectsOfType<EntranceTeleport>();
                var referenceTeleport = Array.Find(everyTeleport, current => current.isEntranceToBuilding);

                if (!referenceTeleport) return false;
                return referenceTeleport.dungeonFlowId == 1;
            }
        }
    }
}