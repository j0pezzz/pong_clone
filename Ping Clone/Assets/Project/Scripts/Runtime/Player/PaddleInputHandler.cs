using Fusion;

namespace Project.Scripts.Runtime.Player
{
    public class PaddleInputHandler : NetworkBehaviour
    {
        public SInputs SInputs;

        public override void Spawned()
        {
            if (!HasInputAuthority) return;
            
            bl_EventHandler.Match.Player.OnMobilePlayerMove += OnPlayerMove;
        }

        public override void Despawned(NetworkRunner runner, bool hasState)
        {
            if (!HasInputAuthority) return;
            
            bl_EventHandler.Match.Player.OnMobilePlayerMove -= OnPlayerMove;
        }

        void OnPlayerMove(float direction)
        {
            SInputs.MoveUpwards = direction > 0;
            SInputs.MoveDownwards = direction < 0;
        }
    }

    public struct SInputs
    {
        public bool MoveUpwards;
        public bool MoveDownwards;
    }
}