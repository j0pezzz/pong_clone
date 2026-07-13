using Fusion;
using Project.Internal.Abstract;
using Project.Internal.Utility;
using Project.Scripts.Game;
using UnityEngine;

public class Ball : NetworkBehaviour
{
    public float Speed = 5f;
    public float MaxSpeed = 30;
    [Tooltip("How often does each speed increment happen?")]
    public float SpeedIncrement = 30;
    [Tooltip("Speed Multiplier")]
    public float SpeedIncrementFactor = 1.1f;
    public Rigidbody rb;

    [Networked] private TickTimer SpeedIncreaseTimer { get; set; }

    Vector3 _originalPosition;
    Vector3 _pausedVelocity;
    float _xDir, _yDir;
    private float _originalSpeed;
    
    public override void Spawned()
    {
        if (!NetworkHandler.Instance.IsHost) return;

        _originalPosition = transform.position;

        _originalSpeed = Speed;
        SpeedIncreaseTimer = TickTimer.CreateFromSeconds(Runner, SpeedIncrement);
        LaunchBall();
        bl_EventHandler.Match.OnGlobalGamePause += OnGamePaused;
    }

    public override void Despawned(NetworkRunner runner, bool hasState)
    {
        bl_EventHandler.Match.OnGlobalGamePause -= OnGamePaused;
    }

    public override void FixedUpdateNetwork()
    {
        // If match is finished, stop the ball.
        if (GameManager.Instance.IsGameDone)
        {
            rb.linearVelocity = Vector3.zero;
            return;
        }
        
        if (SpeedIncreaseTimer.Expired(Runner))
        {
            Speed = Mathf.Min(Speed * SpeedIncrementFactor, MaxSpeed);
            Debug.Log($"Increased speed to {Speed}");
            
            SpeedIncreaseTimer = TickTimer.CreateFromSeconds(Runner, SpeedIncrement);
        }
    }

    void OnGamePaused(bool paused)
    {
        float speedIncrementTime = 0;
        
        if (paused)
        {
            speedIncrementTime = SpeedIncreaseTimer.GetSecondsFloat(Runner);
            SpeedIncreaseTimer = new TickTimer();
            _pausedVelocity = rb.linearVelocity;
            rb.linearVelocity = Vector3.zero;
        }
        else
        {
            if (_pausedVelocity == Vector3.zero) return;

            SpeedIncreaseTimer = TickTimer.CreateFromSeconds(Runner, speedIncrementTime);
            rb.linearVelocity = _pausedVelocity;
        }
    }

    public void SetBallToInit()
    {
        if (!NetworkHandler.Instance.IsHost) return;
        if (GameManager.Instance.IsGameDone) return;

        Speed = _originalSpeed;
        transform.position = _originalPosition;
        LaunchBall();
    }

    void LaunchBall()
    {
        do
        {
            _xDir = Random.Range(-1f, 1f);
        } while (Mathf.Abs(_xDir) < 0.5f);

        _yDir = Random.Range(-0.5f, 0.5f);

        Vector3 launchDir = new Vector3(_xDir, _yDir, 0).normalized;

        rb.linearVelocity = launchDir * Speed;
    }

    void OnCollisionEnter(Collision enterCollider)
    {
        if (!NetworkHandler.Instance.IsHost) return;
        if (GameManager.Instance.IsGameDone) return;

        if (!enterCollider.gameObject.TryGetComponent(out PaddleBase paddle)) return;
        
        float yDir = HitFactor(transform.position, enterCollider.transform.position, enterCollider.collider.bounds.size.y);
        float xDir = rb.linearVelocity.x > 0 ? 1 : -1;
        
        Vector2 direction = new Vector2(xDir, yDir).normalized;

        rb.linearVelocity = direction * Speed;
    }

    float HitFactor(Vector2 ballPos, Vector2 playerPos, float playerHeight) => (ballPos.y - playerPos.y) / playerHeight;
}
