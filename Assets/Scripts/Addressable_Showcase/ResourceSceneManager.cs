using UnityEngine;

namespace BU.Workshop
{
    public class ResourceSceneManager : MonoBehaviour
    {
        [SerializeField]
        private string _resourcePath = "MyResource";

        private void Start()
        {
            LoadResource();
        }

        private void LoadResource()
        {
            // Load the resource from the Resources folder
            GameObject resource = Resources.Load<GameObject>(_resourcePath);

            if (resource != null)
            {
                // Instantiate the loaded resource in the scene
                Instantiate(resource, transform.position, Quaternion.identity);
            }
            else
            {
                Debug.LogError($"Resource at path '{_resourcePath}' could not be found.");
            }
        }
    }
}