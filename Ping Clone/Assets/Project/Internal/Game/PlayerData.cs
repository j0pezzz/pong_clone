using Fusion;

namespace Project.Internal.Game
{
    public struct PlayerData : INetworkStruct
    {
        public NetworkString<_128> Nickname;
        public Team Team;
    }
}