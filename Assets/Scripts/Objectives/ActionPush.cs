using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActionPush : ObjectiveActionBehaviour
{
    private Dictionary<ulong, Ragdoll> _pushingPlayers = new Dictionary<ulong, Ragdoll>();
    private Dictionary<ulong, float> _pushingDistances = new Dictionary<ulong, float>();
    private Vector3 _prevPosition = Vector3.zero;
    private float _distanceTravelled = 0f;
    private List<(ObjectiveCondition, Zone)> activeConditions;
    private ulong _clientToRemove;
    private bool _removeClient = false;

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
            if(_pushingDistances[clientId] > Objective.Action.RequiredPerformanceDistance)
            {
                foreach ((ObjectiveCondition condition, Zone zone) in activeConditions)
                {

                    Objective objective = new Objective(Objective.Action, Objective.Colour, Objective.Object, condition, zone, Objective.Inverse);
                    if (_pushingPlayers[clientId].ObjectiveActionReporter.CheckAndStartActionObjective(objective, clientId))
                    {
                        _clientToRemove = clientId;
                        _removeClient = true;
                        break;
                    }
                }
            }
        }

        if(_removeClient)
        {
            _pushingDistances.Remove(_clientToRemove);
            _pushingPlayers.Remove(_clientToRemove);
        }

        _prevPosition = transform.position;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if(collision.gameObject.TryGetComponent<Ragdoll>(out Ragdoll ragdoll))
        {
            if(_pushingPlayers.TryAdd(ragdoll.ClientId, ragdoll))
            {
                _pushingDistances.TryAdd(ragdoll.ClientId, 0);
            }
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.TryGetComponent<Ragdoll>(out Ragdoll ragdoll))
        {
            _pushingPlayers.Remove(ragdoll.ClientId);
            _pushingDistances.Remove(ragdoll.ClientId);
        }
    }
}
