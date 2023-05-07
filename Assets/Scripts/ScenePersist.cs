using UnityEngine;

/// <summary>
/// Makes the object persist between scenes
/// </summary>
/// <remarks>This should be used for objects in the main scene as just the environment will be loaded in at runtime.</remarks>
public class ScenePersist : MonoBehaviour
{
    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }
}
