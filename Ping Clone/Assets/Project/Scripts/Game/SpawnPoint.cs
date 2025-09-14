using System;
using UnityEngine;

namespace Project.Scripts.Game
{
    public class SpawnPoint : MonoBehaviour
    {
        public Team team;
        
        public Vector3 Position => _transform.position;
        
        Transform _transform;

        void Awake()
        {
            _transform = transform;
        }
    }
}