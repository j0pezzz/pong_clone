using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Policies;
using Unity.MLAgents.Sensors;
using UnityEngine;

public class AIController : Agent
{
    public float Speed = 5f;
    public bool IsLeftSide = false;
    public BehaviorParameters behavior;

    Ball ball;
    Rigidbody ballRigidbody;

    Vector3 initPos;

    public override void Initialize()
    {
        initPos = transform.localPosition;

        UseNormalBall();
    }

    void UseNormalBall()
    {
        ball = GameController.Instance.cacheBall;
        if (!ball.TryGetComponent(out ballRigidbody))
        {
            Debug.LogError("No rigidbody attached to Ball");
        }
    }

    public void SetToInit()
    {
        transform.localPosition = initPos;
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation(transform.localPosition);

        CollectNormalObservations(sensor);
    }

    void CollectNormalObservations(VectorSensor sensor)
    {
        if (ball != null)
        {
            sensor.AddObservation(ball.transform.localPosition);
            sensor.AddObservation(ballRigidbody.velocity);
        }
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        float move = actions.ContinuousActions[0];

        Vector3 velocity = new(0, move * Speed);

        Vector3 smoothedVelocity = Vector3.Lerp(transform.localPosition, transform.localPosition + velocity, Time.deltaTime);

        transform.localPosition = smoothedVelocity;
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        ActionSegment<float> continuousActions = actionsOut.ContinuousActions;

        continuousActions[0] = Input.GetAxisRaw("Vertical");
    }
}
