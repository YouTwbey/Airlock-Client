using Il2CppSG.Airlock;
using System.Collections.Generic;

namespace AirlockClient.Attributes
{
    public class QueuedWin
    {
        public List<PlayerState> WinningPlayers = new List<PlayerState>();
        public int Reason = 0;
        public int Authority = 0;
        public GameplayStates RunInState = GameplayStates.NotSet;
    }
}
