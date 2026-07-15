using Fusion;
using Project.Internal.Abstract;
using Project.Internal.Utility;
using Project.Scripts.Game;
using UnityEngine;

public class Ball : NetworkBehaviour
{
    public float speed = 5f;
    public float MaxSpeed = 30;
    [Tooltip("How often does each speed increment happen?")]
    public float SpeedIncrement = 30;
    [Tooltip("Speed Multiplier")]
    public float SpeedIncrementFactor = 1.1f;
    public Rigidbody rb;
    [SerializeField] private NetworkTransform networkTransform;

    [Networked] private TickTimer SpeedIncreaseTimer { get; set; }

    Vector3 _originalPosition;
    Vector3 _pausedVelocity;
    private float _initialSpeed;
    
    public override void Spawned()
    {
        if (!NetworkHandler.Instance.IsHost) return;

        _originalPosition = transform.position;
        _initialSpeed = speed;
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
            speed = Mathf.Min(speed * SpeedIncrementFactor, MaxSpeed);
            Debug.Log($"Increased speed to {speed}");
            
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

        speed = _initialSpeed;
        networkTransform.Teleport(_originalPosition);
        LaunchBall();
    }

    void LaunchBall()
    {
        float xDir;
        do
        {
            xDir = Random.Range(-1f, 1f);
        } while (Mathf.Abs(xDir) < 0.5f);

        float yDir = Random.Range(-0.5f, 0.5f);

        Vector3 launchDir = new Vector3(xDir, yDir, 0).normalized;

        rb.linearVelocity = launchDir * speed;
    }

    void OnCollisionEnter(Collision enterCollider)
    {
        if (!NetworkHandler.Instance.IsHost) return;
        if (GameManager.Instance.IsGameDone) return;

        if (!enterCollider.gameObject.TryGetComponent(out PaddleBase paddle)) return;
        
        float yDir = HitFactor(transform.position, enterCollider.transform.position, enterCollider.collider.bounds.size.y);
        float xDir = rb.linearVelocity.x > 0 ? 1 : -1;
        
        Vector2 direction = new Vector2(xDir, yDir).normalized;

        rb.linearVelocity = direction * speed;
    }

    float HitFactor(Vector2 ballPos, Vector2 playerPos, float playerHeight) => (ballPos.y - playerPos.y) / playerHeight;
}
