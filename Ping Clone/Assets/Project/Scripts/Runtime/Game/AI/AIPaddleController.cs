using Project.Internal.Abstract;
using Project.Internal.Utility;
using UnityEngine;

namespace Project.Scripts.Game.AI
{
    /// <summary>
    /// Handles AI movement. Basically follows the ball on the Y-Axis.
    /// </summary>
    public class AIPaddleController : PaddleBase
    {
        private Ball _ball;
        private AISetting _aiSetting;
        
        public override void Spawned()
        {
            _aiSetting = GameData.Instance.aiGameSettings.GetAISettings(Runner.SessionInfo.GetGameSettings().AIDifficulty);
            
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
    
            float offset = Mathf.Sin(Runner.Tick * 2f) * _aiSetting.maxTrackingOffset; // Wobble effect
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