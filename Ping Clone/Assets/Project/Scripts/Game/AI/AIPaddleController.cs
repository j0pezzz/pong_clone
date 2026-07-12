using Project.Internal.Abstract;
using UnityEngine;

namespace Project.Scripts.Game.AI
{
    public class AIPaddleController : PaddleBase
    {
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

            //BUG: bot when following ball on Y-axis, just goes to the Top or Bottom bounds.
            float ballYPos = _ball.transform.position.y;
            
            Debug.Log(ballYPos);
            
            //TODO: we need to move this AI either slow or fast depending on difficulty.
            float newY = Mathf.Clamp(transform.position.y + (ballYPos * speed) * Runner.DeltaTime, NetworkHandler.Instance.BottomBound, NetworkHandler.Instance.TopBound);

            Vector3 position = transform.position;
            position.y = newY;
            transform.position = position;
            //transform.position = new Vector3(transform.position.x, newY, transform.position.z);
        }
    }
}