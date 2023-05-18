using SolidUtilities;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using ZekstersLab.Helpers;

public class SpawnManager : MonoBehaviour
{
    [SerializeField][ReadOnly] private List<SpawnPoint> _spawnPoints = new List<SpawnPoint>();
    [SerializeField][ReadOnly] private int _currentSpawnPoint = 0;

    private const float MAX_WAIT_TIME = 0.5f;

    private bool _ready = false;

    private static SpawnManager _instance;

    public static SpawnManager Instance
    {
        get
        {
            if(_instance == null) _instance = FindObjectOfType<SpawnManager>();
            return _instance;
        }
    }

    private void OnEnable()
    {
        if(GameManager.Instance != null)
        {
            GameManager.Instance.OnGameStateChanged += OnGameManagerStateChange;
        }
    }

    private void OnDisable()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnGameStateChanged -= OnGameManagerStateChange;
        }
    }

    public void ReloadSpawnPoints()
    {
        _spawnPoints.Clear();
        _spawnPoints = FindObjectsOfType<SpawnPoint>().ToList<SpawnPoint>();
        _spawnPoints.Shuffle();
        _currentSpawnPoint = 0;
        _ready = true;
    }

    public void ResetSpawnManager()
    {
        _spawnPoints.Clear();
        _currentSpawnPoint = 0;
        _ready = false;
    }

    public void SpawnPlayer(GameObject player)
    {
        if (!_ready) ReloadSpawnPoints();
        if (_spawnPoints.Count == 0) return;

        // There should always be at least 1 spawn point open as even with 5 players loaded, they can't cover all 6 spawn points

        for(int i = 0; i < _spawnPoints.Count; i++)
        {
            _currentSpawnPoint %= _spawnPoints.Count;
            if (_spawnPoints[_currentSpawnPoint].SpawnPointIsClear())
            {
                player.transform.position = _spawnPoints[_currentSpawnPoint].gameObject.transform.position;
                break;
            }
            else
            {
                _currentSpawnPoint++;
            }

        }


        //StartCoroutine(SpawnWhenClear(player, _spawnPoints[_currentSpawnPoint]));

        _currentSpawnPoint++;
    }

    //IEnumerator SpawnWhenClear(GameObject player, SpawnPoint spawnPoint)
    //{
    //    if (spawnPoint == null) yield break;
    //    if (player == null) yield break;

    //    float duration = 0;

    //    player.SetActive(false);

    //    while(true)
    //    {
    //        if(spawnPoint.SpawnPointIsClear())
    //        {
    //            //Move the player to this spawn point
    //            player.transform.position = spawnPoint.transform.position;
    //            player.SetActive(true);
    //            yield break;
    //        }
    //        else
    //        {
    //            yield return new WaitForEndOfFrame();
    //            duration += Time.deltaTime;

    //            if (duration > MAX_WAIT_TIME)
    //            {
    //                // try another spawn location
    //                SpawnPlayer(player);
    //                yield break;
    //            }
    //        }
    //    }
    //}

    private void OnGameManagerStateChange(object sender, GameManager.GAMESTATE state)
    {
        if((state == GameManager.GAMESTATE.LOADING_LOBBY) ||
            (state == GameManager.GAMESTATE.LOADING_ROUND))
        {
            ResetSpawnManager();
        }
    }
}
