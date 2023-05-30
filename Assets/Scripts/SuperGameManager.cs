using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SuperGameManager : MonoBehaviour
{
    private static SuperGameManager _instance;

    [SerializeField] private List<GameObject> _sceneNetworkObjectPrefabs = new List<GameObject>();
    [SerializeField] private List<GameObject> _sceneNetworkObjects = new List<GameObject>();
    [SerializeField] private float _loadingScreenMinTime = 2.0f;

    private Coroutine _coroutine;

    private void Start()
    {
        ReloadEntireGame(true);
    }

    public static SuperGameManager Instance
    {
        get
        {
            if( _instance == null ) _instance = FindObjectOfType<SuperGameManager>();
            return _instance;
        }
    }

    public void ReloadEntireGame(bool splashScreen = false)
    {
        if (_coroutine != null) return;


        MenuUIManager.Instance.ToggleAllCanvasesOff();
        if (splashScreen)
        {
            MenuUIManager.Instance.ToggleCanvas(MenuUIManager.Instance.SplashScreenCanvas, true);
        }
        else
        {
            MenuUIManager.Instance.ToggleCanvas(MenuUIManager.Instance.LoadingCanvas, true);
        }
        

        _coroutine = StartCoroutine(RecreateNetworkObjects());
    }

    IEnumerator RecreateNetworkObjects()
    {
        foreach (GameObject obj in _sceneNetworkObjects)
        {
            if (obj != null) Destroy(obj);
            yield return new WaitForEndOfFrame();
        }

        _sceneNetworkObjects.Clear();
        yield return new WaitForEndOfFrame();

        foreach (GameObject obj in _sceneNetworkObjectPrefabs)
        {
            if(obj != null) _sceneNetworkObjects.Add(Instantiate(obj));
            yield return new WaitForEndOfFrame();
        }

        yield return new WaitForSeconds(_loadingScreenMinTime);
        MenuUIManager.Instance.ToggleAllCanvasesOff();
        Debug.Log("SuperGameManager: Menu Canvas = true");
        MenuUIManager.Instance.ToggleCanvas(MenuUIManager.Instance.MainMenuCanvas, true);
        GameManager.Instance.StartGameManager();
        _coroutine = null;
    }

    
}
