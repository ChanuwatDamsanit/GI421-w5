using UnityEngine;

namespace BU.Workshop
{
    public class ResourceSceneManager : MonoBehaviour
    {
        [SerializeField]
        private string[] _resourcePaths;

        [SerializeField]
        private float _spawnOffset = 5f;

        private float _currentOffset;

        private void Start()
        {
            _currentOffset = 0f;

            LoadResource();
        }

        private void LoadResource()
        {
            foreach (string _resourcePath in _resourcePaths)
            {
                // Load the resource from the Resources folder
                GameObject resource = Resources.Load<GameObject>(_resourcePath);

                Vector3 spawnPosition = transform.position + new Vector3(_currentOffset, 0f, 0f);

                if (resource != null)
                {
                    // Instantiate the loaded resource in the scene
                    Instantiate(resource, spawnPosition, Quaternion.identity);
                }
                else
                {
                    Debug.LogError($"Resource at path '{_resourcePath}' could not be found.");
                }

                _currentOffset += _spawnOffset;
            }
        }
    }
}