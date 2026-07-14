using Project.Internal.Utility;
using UnityEngine;

namespace Project.Scripts.Game
{
    public class Goal : MonoBehaviour
    {
        public Team playerTeam;

        private void OnTriggerEnter(Collider enterCollider)
        {
            if (!NetworkHandler.Instance.IsHost) return;
            if (!enterCollider.gameObject.TryGetComponent(out Ball ball)) return;
               
            bl_EventHandler.Match.DispatchPointAddition(playerTeam.GetOppositeTeam());
            NetworkHandler.Instance.ResetGame();
        }
    }
}