using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


/// <summary>
/// Class to detect when an object has been grabbed by a player and report this to the ObjectiveManager
/// </summary>
public class ActionThrow : ObjectiveActionBehaviour
{
    private Dictionary<ulong, Ragdoll> _currentHolders = new Dictionary<ulong, Ragdoll>();
    private List<(ObjectiveCondition initialCondition, Zone initialZone)> _prevConditions = new List<(ObjectiveCondition initialCondition, Zone initialZone)>();
    private Ragdoll _prevHolder;
    private bool _thrown = false;

    List<FixedJoint2D> currentFixedJoint2Ds = new List<FixedJoint2D>();

    private void Update()
    {
        List<(ObjectiveCondition, Zone)> activeConditions = GetActiveConditions();

        _currentHolders.Clear();

        //get a list of the connected joints (players)
        currentFixedJoint2Ds = GetComponents<FixedJoint2D>().ToList<FixedJoint2D>();
        // Create Dictionary of unique players holding (ignoring if held in 2 hands)
        foreach(FixedJoint2D fj in currentFixedJoint2Ds)
        {
            if (fj.connectedBody.gameObject.TryGetComponent<Ragdoll>(out Ragdoll ragdoll))
            {
                _currentHolders.TryAdd(ragdoll.ClientId, ragdoll);
            }
        }

        if (_thrown)
        {
            if(_currentHolders.Count > 0)
            {
                // someone has grabbed it!
                _thrown = false;
                _prevConditions.Clear();
                _prevHolder = null;
                // If this list is > 1 - break early as can only be thrown if only one person holding
                if (_currentHolders.Count > 1) return;
                if (_currentHolders.Count == 1)
                {
                    // Keep track of being held
                    _prevHolder = _currentHolders.ElementAt(0).Value;
                }
            }
            // Check for conditions to be met
            foreach ((ObjectiveCondition condition, Zone zone) in activeConditions)
            {
                // Check if this condition
                if (condition.RequiresObjectToBeInZone && _prevConditions.Contains((condition, zone))) return;

                Objective objective = new Objective(Objective.Action, Objective.Colour, Objective.Object, condition, zone, Objective.Inverse);
                if (_prevHolder.ObjectiveActionReporter.CheckAndStartActionObjective(objective, _prevHolder.ClientId))
                {
                    // Objective reported - reset
                    _prevHolder = null;
                    _thrown = false;
                    _prevConditions.Clear();
                    break;
                }
            }
            _prevConditions = new(activeConditions);
        }
        else
        {
            // If this list is > 1 - break early as can only be thrown if only one person holding
            if (_currentHolders.Count > 1) return;
            if (_currentHolders.Count == 1)
            {
                // Keep track of being held
                _prevHolder = _currentHolders.ElementAt(0).Value;
            }
            else if (_prevHolder != null)
            {
                // we've been thrown
                _thrown = true;
                _prevConditions = new(activeConditions);
            }
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        LayerMask layerMask = (LayerMask.NameToLayer("Ground") | LayerMask.NameToLayer("Player"));
        // colliding with the ground or player will cancel the throw
        if ((collision.gameObject.layer & layerMask) != 0)
        {
            // collided with player or ground - cancel
            _thrown = false;
            _prevHolder = null;
        }
    }

}
