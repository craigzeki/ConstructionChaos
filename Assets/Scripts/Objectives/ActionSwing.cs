using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


/// <summary>
/// Class to detect when an object has been grabbed by a player and report this to the ObjectiveManager
/// </summary>
public class ActionSwing : ObjectiveActionBehaviour
{
    //private List<ulong> _grabbingClients = new List<ulong>();
    Dictionary<FixedJoint2D, (ulong clientId, Objective runningObjective, Ragdoll ragdoll)> _prevFixedJoint2Ds;
    Dictionary<FixedJoint2D, (ulong clientId, Objective runningObjective, Ragdoll ragdoll)> _activeFixedJoint2Ds = new Dictionary<FixedJoint2D, (ulong clientId, Objective runningObjective, Ragdoll ragdoll)>();
    List<FixedJoint2D> currentFixedJoint2Ds = new List<FixedJoint2D>();

    private void Update()
    {
        List<(ObjectiveCondition, Zone)> activeConditions = GetActiveConditions();

        // Create a copy of the previous joints
        _prevFixedJoint2Ds = new(_activeFixedJoint2Ds);
        _activeFixedJoint2Ds.Clear();

        //get a list of the connected joints (players)
        currentFixedJoint2Ds = GetComponents<FixedJoint2D>().ToList<FixedJoint2D>();

        //check if connected players has changed and trigger the ActionReporter accordingly
        foreach (FixedJoint2D fixedJoint2D in currentFixedJoint2Ds)
        {
            if (_prevFixedJoint2Ds.ContainsKey(fixedJoint2D))
            {
                // Existing joint
                // Check if it still meets conditions
                if (activeConditions.Contains((_prevFixedJoint2Ds[fixedJoint2D].runningObjective.Condition, _prevFixedJoint2Ds[fixedJoint2D].runningObjective.Zone)))
                {
                    // Still meets conditions
                    // Remove it from the prev List so that it doesn't get cancelled later
                    _activeFixedJoint2Ds.Add(fixedJoint2D, _prevFixedJoint2Ds[fixedJoint2D]);
                    _prevFixedJoint2Ds.Remove(fixedJoint2D);

                }
                else
                {
                    // No longer meets conditions
                    // Remove it from the active list as it will be cancelled later

                }

            }
            else
            {
                // New joint - register action started
                if (fixedJoint2D.connectedBody.gameObject.TryGetComponent<Ragdoll>(out Ragdoll ragdoll))
                {
                    foreach ((ObjectiveCondition condition, Zone zone) in activeConditions)
                    {

                        Objective objective = new Objective(Objective.Action, Objective.Colour, Objective.Object, condition, zone, Objective.Inverse);
                        if (ragdoll.ObjectiveActionReporter.CheckAndStartActionObjective(objective, ragdoll.ClientId))
                        {
                            // Add to the active list
                            _activeFixedJoint2Ds.Add(fixedJoint2D, (ragdoll.ClientId, objective, ragdoll));
                            break;
                        }
                    }
                }
            }
        }

        // Anything left on the prev List is now no longer connected - as all connected joints / joints meeting conditions were removed
        // Cancel these actions
        foreach (FixedJoint2D fixedJoint2D in _prevFixedJoint2Ds.Keys)
        {
            // !NOTE: Cannot access directly the fixedJoint2D as it may have been destroyed by the owner

            if ((_prevFixedJoint2Ds[fixedJoint2D].ragdoll != null) && (_prevFixedJoint2Ds[fixedJoint2D].runningObjective != null))
            {
                // Cancel the ObjectiveAction
                _prevFixedJoint2Ds[fixedJoint2D].ragdoll.ObjectiveActionReporter.CancelActionObjective(_prevFixedJoint2Ds[fixedJoint2D].runningObjective, _prevFixedJoint2Ds[fixedJoint2D].clientId);
            }
        }

        // Clear the dictionary ready for the next pass
        _prevFixedJoint2Ds.Clear();
    }

}
