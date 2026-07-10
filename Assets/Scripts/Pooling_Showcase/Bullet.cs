using System;
using UnityEngine;

namespace BU.Workshop
{
    public sealed class Bullet : MonoBehaviour
    {
        [SerializeField]
        private float _speed = 10f;

        [SerializeField]
        private Rigidbody _rigidbody;

        [SerializeField]
        private LayerMask _collisionLayerMask;

        public event Action<Bullet> WhenRequestedToDestroy;

        private void Update()
        {
            _rigidbody.linearVelocity = transform.up * _speed;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (((1 << other.gameObject.layer) & _collisionLayerMask) != 0)
            {
                WhenRequestedToDestroy?.Invoke(this);
            }
        }
    }
}