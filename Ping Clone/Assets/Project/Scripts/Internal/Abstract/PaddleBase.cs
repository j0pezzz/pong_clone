using Fusion;
using UnityEngine;

namespace Project.Internal.Abstract
{
    public class PaddleBase : NetworkBehaviour
    {
        public float speed = 5;
        public NetworkTransform networkTransform;

        private float _originalYPosition;

        private void Awake()
        {
            _originalYPosition = transform.position.y;
        }

        public virtual void SetToInitPosition()
        {
            Vector3 resetPosition = transform.position;
            resetPosition.y = _originalYPosition;
            networkTransform.Teleport(resetPosition);
        }
    }
}