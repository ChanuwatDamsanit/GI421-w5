using UnityEngine;
using UnityEngine.Pool;

namespace BU.Workshop
{
    public sealed class GunWithPooling : Gun
    {
        private IObjectPool<Bullet> _bulletPool;

        private void OnEnable()
        {
            // Initialize the object pool when the gun is enabled
            _bulletPool ??= new ObjectPool<Bullet>(
                createFunc: () => Instantiate(BulletPrefab),
                actionOnGet: bullet => bullet.gameObject.SetActive(true),
                actionOnRelease: bullet => bullet.gameObject.SetActive(false),
                actionOnDestroy: bullet => Destroy(bullet),
                collectionCheck: true,
                defaultCapacity: 10,
                maxSize: 50
            );
        }

        private void OnDisable()
        {
            // Clear the pool when the gun is disabled
            _bulletPool?.Clear();
        }

        protected override void Fire(Vector2 direction)
        {
            var clampedRotation = Mathf.Clamp(Vector2.SignedAngle(Vector2.up, direction), -130f, 130f);
            transform.rotation = Quaternion.Euler(0, 0, clampedRotation);

            // Get a bullet from the pool instead of instantiating
            Bullet bullet = _bulletPool.Get();
            bullet.transform.position = MuzzleRoot.transform.position;
            bullet.transform.rotation = Quaternion.Euler(0, 0, clampedRotation);
            bullet.WhenRequestedToDestroy += OnBulletRequestedToDestroy;
        }

        private void OnBulletRequestedToDestroy(Bullet bullet)
        {
            bullet.WhenRequestedToDestroy -= OnBulletRequestedToDestroy;
            _bulletPool.Release(bullet);
        }
    }
}