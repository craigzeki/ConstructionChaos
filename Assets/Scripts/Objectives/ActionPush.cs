using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Zone;

public class ActionPush : ObjectiveActionBehaviour
{
    private Dictionary<ulong, Ragdoll> _pushingPlayers = new Dictionary<ulong, Ragdoll>();
    private Dictionary<ulong, float> _pushingDistances = new Dictionary<ulong, float>();
    private Vector3 _prevPosition = Vector3.zero;
    private float _distanceTravelled = 0f;
    private List<(ObjectiveCondition, Zone)> activeConditions;
    private List<ulong> _clientsToRemove = new List<ulong>();
    private bool _removeClient = false;
    private Dictionary<ulong, Coroutine> _removeCoroutines = new Dictionary<ulong, Coroutine>();
    private Dictionary<ulong, Coroutine> _removeGroundCoroutines = new Dictionary<ulong, Coroutine>();
    private bool _isOnGround = false;

    private void Awake()
    {
        _prevPosition = transform.position;
    }

    void Update()
    {
        activeConditions = GetActiveConditions();
        
        _distanceTravelled = Vector3.Distance(_prevPosition, transform.position);
        foreach(ulong clientId in _pushingPlayers.Keys)
        {
            _pushingDistances[clientId] += _distanceTravelled;
            //Debug.Log("Pushing distance: " + _pushingDistances[clientId]);
            if(_pushingDistances[clientId] > Objective.Action.RequiredPerformanceDistance)
            {

                if((activeConditions.Count == 0) && (Objective.Condition == null))
                {
                    Objective objective = new Objective(Objective.Action, Objective.Colour, Objective.Object, null, null, Objective.Inverse);
                    if (_pushingPlayers[clientId].ObjectiveActionReporter.CheckAndStartActionObjective(objective, clientId))
                    {
                        _clientsToRemove.Add(clientId);
                        _removeClient = true;
                    }
                }
                else
                {
                    foreach ((ObjectiveCondition condition, Zone zone) in activeConditions)
                    {

                        Objective objective = new Objective(Objective.Action, Objective.Colour, Objective.Object, condition, zone, Objective.Inverse);
                        if (_pushingPlayers[clientId].ObjectiveActionReporter.CheckAndStartActionObjective(objective, clientId))
                        {
                            _clientsToRemove.Add(clientId);
                            _removeClient = true;
                            break;
                        }
                    }
                }
                
            }
        }

        foreach(ulong clientId in _clientsToRemove)
        {
            _pushingDistances.Remove(clientId);
            _pushingPlayers.Remove(clientId);
        }

        _clientsToRemove.Clear();

        _prevPosition = transform.position;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if(collision.gameObject.TryGetComponent<Ragdoll>(out Ragdoll ragdoll))
        {
            
            if (_isOnGround && _pushingPlayers.TryAdd(ragdoll.ClientId, ragdoll))
            {
                _pushingDistances.TryAdd(ragdoll.ClientId, 0);
            }
            else
            {
                // we have briefly let go and reconnected
                // stop the hysteresis
                if (_removeCoroutines.TryGetValue(ragdoll.ClientId, out Coroutine coroutine))
                {
                    if (coroutine != null)
                    {
                        StopCoroutine(coroutine);
                        //Debug.Log("PUSH: Player reconnected - hysteresis cancelled");
                        _removeCoroutines.Remove(ragdoll.ClientId);
                    }
                }
            }
        }

        if(GameManager.Instance.GroundLayers == (GameManager.Instance.GroundLayers |  1 << collision.gameObject.layer))
        {
            _isOnGround = true;
            foreach(ulong clientId in _pushingPlayers.Keys)
            {
                if(_removeGroundCoroutines.TryGetValue(clientId, out Coroutine coroutine))
                {
                    if(coroutine != null)
                    {
                        StopCoroutine(coroutine);
                        //Debug.Log("PUSH: Ground reconnected - hysteresis cancelled");
                        _removeGroundCoroutines.Remove(clientId);
                    }
                }
            }
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.TryGetComponent<Ragdoll>(out Ragdoll ragdoll))
        {
            if(_removeCoroutines.TryGetValue(ragdoll.ClientId, out Coroutine coroutine))
            {
                //Debug.Log("PUSH: Using existing hysteresis coroutine");
                if (coroutine == null) coroutine = StartCoroutine(DoHysteresis(ragdoll.ClientId, "Player"));
            }
            else
            {
                //Debug.Log("PUSH: Creating new hysteresis coroutine");
                _removeCoroutines.Add(ragdoll.ClientId, StartCoroutine(DoHysteresis(ragdoll.ClientId, "Player")));
            }
        }

        if(collision.gameObject.layer == LayerMask.NameToLayer("Ground"))
        {
            _isOnGround = false;
            foreach(ulong clientId in _pushingPlayers.Keys)
            {
                if (_removeGroundCoroutines.TryGetValue(clientId, out Coroutine coroutine))
                {
                    //Debug.Log("PUSH: Using existing ground hysteresis coroutine");
                    if (coroutine == null) coroutine = StartCoroutine(DoHysteresis(clientId, "Ground"));
                }
                else
                {
                    //Debug.Log("PUSH: Creating new ground hysteresis coroutine");
                    _removeGroundCoroutines.Add(clientId, StartCoroutine(DoHysteresis(clientId, "Ground")));
                }
            }
        }
    }

    IEnumerator DoHysteresis(ulong clientId, string reason)
    {
        //Debug.Log("PUSH: " + reason + " disconnected, hysteresis started");
        yield return new WaitForSeconds(0.4f);
        //Debug.Log("PUSH: " + reason + " Hysteresis completed - player disconnected");
        _pushingPlayers.Remove(clientId);
        _pushingDistances.Remove(clientId);
        
        if(reason == "Player")
        {
            _removeCoroutines.Remove(clientId);
            if(_removeGroundCoroutines.TryGetValue(clientId, out Coroutine coroutine))
            {
                StopCoroutine(coroutine);
                _removeGroundCoroutines.Remove(clientId);
            }
        }
        else if(reason == "Ground")
        {
            _removeGroundCoroutines.Remove(clientId);
            if (_removeCoroutines.TryGetValue(clientId, out Coroutine coroutine))
            {
                StopCoroutine(coroutine);
                _removeCoroutines.Remove(clientId);
            }
        }

        
        
    }
}
