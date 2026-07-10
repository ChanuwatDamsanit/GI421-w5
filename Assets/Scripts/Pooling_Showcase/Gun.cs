using System;
using UnityEngine;

namespace BU.Workshop
{
    public class Gun : MonoBehaviour
    {
        [SerializeField]
        private Bullet _bulletPrefab;
        public Bullet BulletPrefab => _bulletPrefab;

        [SerializeField]
        private GameObject _muzzleRoot;
        public GameObject MuzzleRoot => _muzzleRoot;

        [SerializeField]
        private float _fireRate = 3f;

        [SerializeField]
        private float _randomRotateRangeMin = -130f;

        [SerializeField]
        private float _randomRotateRangeMax = 130f;

        [SerializeField]
        private bool _useAutoFire = true;

        private float _nextFireTime;

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.F1))
            {
                _useAutoFire = !_useAutoFire;
            }

            if (Input.GetMouseButtonDown(0) && !_useAutoFire)
            {
                var mousePosition = Input.mousePosition;
                var worldPosition = Camera.main.ScreenToWorldPoint(new Vector3(mousePosition.x, mousePosition.y, Camera.main.nearClipPlane));
                var direction = (worldPosition - transform.position).normalized;

                Fire(direction);
            }

            if (Time.time >= _nextFireTime && _useAutoFire)
            {
                var randomAngle = UnityEngine.Random.Range(_randomRotateRangeMin, _randomRotateRangeMax);
                var direction = Quaternion.Euler(0, 0, randomAngle) * Vector2.up;

                Fire(direction);

                _nextFireTime = Time.time + 1f / _fireRate;
            }
        }

        protected virtual void Fire(Vector2 direction)
        {
            var clampedRotation = Mathf.Clamp(Vector2.SignedAngle(Vector2.up, direction), _randomRotateRangeMin, _randomRotateRangeMax);
            transform.rotation = Quaternion.Euler(0, 0, clampedRotation);

            Bullet bullet = Instantiate(_bulletPrefab, _muzzleRoot.transform.position, Quaternion.identity);
            bullet.WhenRequestedToDestroy += OnBulletRequestedToDestroy;
            bullet.transform.rotation = Quaternion.Euler(0, 0, clampedRotation);
        }

        private void OnBulletRequestedToDestroy(Bullet bullet)
        {
            bullet.WhenRequestedToDestroy -= OnBulletRequestedToDestroy;
            Destroy(bullet.gameObject);
        }
    }
}