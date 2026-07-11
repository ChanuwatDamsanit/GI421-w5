using UnityEngine;
using UnityEngine.Pool;

namespace BU.Workshop
{
    public sealed class PoolingManager : MonoBehaviour
    {
        [SerializeField]
        private Bullet _bulletPrefab;

        private static IObjectPool<Bullet> _bulletPool;

        private void OnEnable()
        {
            // Initialize the object pool when the gun is enabled
            _bulletPool ??= new ObjectPool<Bullet>(
                createFunc: () => Instantiate(_bulletPrefab),
                actionOnGet: bullet => bullet.gameObject.SetActive(true),
                actionOnRelease: bullet => bullet.gameObject.SetActive(false),
                actionOnDestroy: bullet => Destroy(bullet),
                collectionCheck: true,
                defaultCapacity: 2000,
                maxSize: 100000000
            );
        }

        private void OnDisable()
        {
            // Clear the pool when the gun is disabled
            _bulletPool?.Clear();
        }

        public static Bullet GetBulletPool()
        {
            return _bulletPool.Get();
        }

        public static void Release(Bullet bullet)
        {
            _bulletPool.Release(bullet);
        }
    }
}