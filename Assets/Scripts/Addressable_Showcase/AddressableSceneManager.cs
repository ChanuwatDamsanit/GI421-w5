using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace BU.Workshop
{
    public class AddressableSceneManager : MonoBehaviour
    {
        [SerializeField]
        private AssetReferenceGameObject _playerAddressable;

        private void Start()
        {
            // Loads the data without placing anything in the hierarchy
            _playerAddressable.LoadAssetAsync().Completed += OnPlayerLoaded;
        }

        private void OnDisable()
        {
            // Release the loaded asset when the scene is disabled
            _playerAddressable.ReleaseAsset();
        }

        private void OnPlayerLoaded(AsyncOperationHandle<GameObject> handle)
        {
            if (handle.Status == AsyncOperationStatus.Succeeded)
            {
                GameObject playerPrefab = handle.Result;
                Instantiate(playerPrefab, Vector3.zero, Quaternion.identity);

                Debug.Log("Player prefab loaded and instantiated successfully.");
            }
            else
            {
                Debug.LogError("Failed to load player prefab.");
            }
        }
    }
}