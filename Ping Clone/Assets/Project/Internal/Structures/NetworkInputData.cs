using Fusion;
using UnityEngine;

public struct NetworkInputData : INetworkInput
{
    public NetworkButtons Buttons;
    public Vector3 Direction;
}

public enum Buttons
{
    Up = 0,
    Down = 1,
}
