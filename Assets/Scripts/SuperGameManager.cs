using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SuperGameManager : MonoBehaviour
{
    private static SuperGameManager _instance;

    [SerializeField] private List<GameObject> _sceneNetworkObjectPrefabs = new List<GameObject>();
    [SerializeField] private List<GameObject> _sceneNetworkObjects = new List<GameObject>();

    private Coroutine _coroutine;

    public static SuperGameManager Instance
    {
        get
        {
            if( _instance == null ) _instance = FindObjectOfType<SuperGameManager>();
            return _instance;
        }
    }

    public void ReloadEntireGame()
    {
        if (_coroutine != null) return;


        MenuUIManager.Instance.ToggleCanvas(MenuUIManager.Instance.DisconnectedCanvas, false);
        MenuUIManager.Instance.ToggleCanvas(MenuUIManager.Instance.LoadingCanvas, true);

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

        yield return new WaitForSeconds(2);
        MenuUIManager.Instance.ToggleCanvas(MenuUIManager.Instance.LoadingCanvas, false);
        Debug.Log("SuperGameManager: Menu Canvas = true");
        MenuUIManager.Instance.ToggleCanvas(MenuUIManager.Instance.MainMenuCanvas, true);
        _coroutine = null;
    }

    
}
