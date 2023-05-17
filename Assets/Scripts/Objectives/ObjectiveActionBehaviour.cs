using SolidUtilities;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This is the base class for any action behaviour - e.g. hold<br/>
/// It provides condition and zone detection to its concrete class
/// </summary>
public abstract class ObjectiveActionBehaviour : MonoBehaviour
{
    /// <summary>
    /// Required list of conditions for this Objective
    /// </summary>
    [SerializeField][ReadOnly] public List<ObjectiveCondition> Conditions = new List<ObjectiveCondition>();

    /// <summary>
    /// List to store the zone(s) that the object is in
    /// </summary>
    [SerializeField] [ReadOnly] private List<Zone> _currentZones = new List<Zone>();

    /// <summary>
    /// An instance of Objective used to construct a possible objective that was achieved<br/>
    /// This is passed to the ObejctiveManager for checking
    /// </summary>
    public Objective Objective;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Check if the object is in any zone
        if (collision.gameObject.TryGetComponent<Zone>(out Zone zone))
        {
            if (!_currentZones.Contains(zone)) _currentZones.Add(zone);
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        // Check if the object is no longer in a zone
        if (collision.gameObject.TryGetComponent<Zone>(out Zone zone))
        {
            _currentZones.Remove(zone);
        }
    }

    /// <summary>
    /// Provide a list of all conditions which are currently being met by this object
    /// </summary>
    /// <returns>
    /// A list of Tuples (ObjectiveCondition, Zone)<br/>
    /// ObjectiveCondition - The condition that is met
    /// Zone - The zone that it is in
    /// </returns>
    protected List<(ObjectiveCondition, Zone)> GetActiveConditions()
    {
        List<(ObjectiveCondition, Zone)> validConditions = new List<(ObjectiveCondition, Zone)>();

        foreach(ObjectiveCondition condition in Conditions)
        {

            if(condition.DuringCountdown && !condition.RequiresObjectToBeInZone)
            {
                // TODO detect countdown (from gamemanager? / goalzone? etc) and check if active
                // TODO and add to the validConditions list
            }
            else if(!condition.DuringCountdown && condition.RequiresObjectToBeInZone)
            {
                foreach(Zone zone in _currentZones)
                {
                    if(zone.ZoneType == condition.RequiredZone) validConditions.Add((condition, zone));
                }
            }
            else if(condition.DuringCountdown && condition.RequiresObjectToBeInZone)
            {
                // TODO detect countdown (from gamemanager? / goalzone? etc) and check if active
                // TODO if not, break

                foreach (Zone zone in _currentZones)
                {
                    if (zone.ZoneType == condition.RequiredZone) validConditions.Add((condition, zone));
                }
            }
            else
            {
                // No condition required
                validConditions.Add((condition, null));
            }

        }

        return validConditions;
    }
}
