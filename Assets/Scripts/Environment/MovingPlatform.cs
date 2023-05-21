using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovingPlatform : MonoBehaviour
{
    [System.Serializable]
    public enum MovementType
    {
        LOOP,
        PING_PONG
    }

    [SerializeField] private GameObject _platform;

    [SerializeField] private List<Transform> _waypoints = new List<Transform>();

    [SerializeField] private MovementType _movementType;

    [SerializeField] private float _speed = 1f;

    [SerializeField] private float _waitTime = 1f;

    private int _currentWaypointIndex = 0;

    // Start is called before the first frame update
    void Start()
    {
        if (_waypoints.Count == 0)
        {
            Debug.LogError("No waypoints set for moving platform");
            return;
        }
        _platform.transform.position = _waypoints[_currentWaypointIndex].position;
        MovementLoop();
    }

    private void MovementLoop()
    {
        float distance = Vector2.Distance(_platform.transform.position, _waypoints[_currentWaypointIndex].position);
        float moveTime = distance / _speed;
        LeanTween.move(_platform, _waypoints[_currentWaypointIndex].position, moveTime).setOnComplete(() =>
        {
            _currentWaypointIndex++;
            if (_currentWaypointIndex >= _waypoints.Count)
            {
                _currentWaypointIndex = 0;

                // Reverse the list if ping pong
                if (_movementType == MovementType.PING_PONG)
                {
                    _waypoints.Reverse();
                }
            }
            StartCoroutine(Wait());
        });
    }

    private IEnumerator Wait()
    {
        yield return new WaitForSeconds(_waitTime);
        MovementLoop();
    }
}
