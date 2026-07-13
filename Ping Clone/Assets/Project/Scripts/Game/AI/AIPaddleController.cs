using Project.Internal.Abstract;
using UnityEngine;

namespace Project.Scripts.Game.AI
{
    public class AIPaddleController : PaddleBase
    {
        public float maxTrackingOffset = 1.0f;

        private Ball _ball;
        
        public override void Spawned()
        {
            _ball = FindAnyObjectByType<Ball>();

            if (!_ball)
            {
                Debug.LogError("No ball found!");
            }
        }

        public override void FixedUpdateNetwork()
        {
            if (GameManager.Instance.IsGameDone || GameManager.Instance.IsGamePaused) return;
            if (!_ball) return;

            float ballYPos = _ball.transform.position.y;
    
            float offset = Mathf.Sin(Runner.Tick * 2f) * maxTrackingOffset; // Wobble effect
            float targetY = ballYPos + offset;
    
            float newY = Mathf.Clamp(
                transform.position.y + ((targetY - transform.position.y) * 0.5f * speed) * Runner.DeltaTime, 
                NetworkHandler.Instance.BottomBound, 
                NetworkHandler.Instance.TopBound
            );

            Vector3 position = transform.position;
            position.y = newY;
            transform.position = position;
        }
    }
}