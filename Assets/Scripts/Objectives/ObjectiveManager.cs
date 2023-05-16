using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;
using UnityEngine;
using UnityEngine.InputSystem.LowLevel;
using ZekstersLab.Helpers;

/// <summary>
/// Runs server side only.<br/>
/// Creates random objectives and assigns them to players.<br/>
/// Provides an event driven system to allow players to report when an action has occurred.<br/>
/// Will report back to the player the status of their objective.<br/>
/// </summary>
public class ObjectiveManager : MonoBehaviour
{
	/// <summary>
	/// Dictionary of ObjectiveObjectInstances and their corresponding qty that exist in the current scene<br/>
	/// A dictionary was the best way to quickly check and ignore identical objects (e.g. two purple crates) from being added and instead tracks the duplicate by adding to the qty (int)
	/// </summary>
	[SerializeField] private Dictionary<ObjectiveObjectInstance, int> _objectiveObjects = new Dictionary<ObjectiveObjectInstance, int>();

	/// <summary>
	/// List of all possible objectives based on the objects in the scene
	/// </summary>
	[SerializeField] private List<Objective> _possibleObjectives = new List<Objective>();

	/// <summary>
	/// List of all possible zones based on what is present in the scene
	/// </summary>
	[SerializeField] private Dictionary<Zone.ZONE, List<Zone>> _possibleZones = new Dictionary<Zone.ZONE, List<Zone>>();

	/// <summary>
	/// A dictionary which is hashed based on the objective - this allows very performant lookup of the assigned ClientID<br/>
	/// during gameplay. When an action / condition is performed, a new objective can be created, hashed and used to lookup the ID.
	/// </summary>
	[SerializeField] private Dictionary<Objective, ulong> _playerObjectives = new Dictionary<Objective, ulong>();

	private string _inverseString = "Don't ";

	private string _anyActionString = "Do anything with";

	/// <summary>
	/// Keeps track of the position in the _possibleObjectives list for the next one which hasn't been used.
	/// </summary>
	private int _nextAvailableObjective = 0;

	private static ObjectiveManager _instance;
    public static ObjectiveManager Instance
	{
		get
		{
			if(_instance == null) _instance = FindObjectOfType<ObjectiveManager>();
			return _instance;
		}
	}

	private void Awake()
	{
		UnityEngine.Random.InitState((int)DateTime.Now.Ticks);
    }

	private void OnEnable()
	{
		if (GameManager.Instance == null) return;
        GameManager.Instance.OnPlayerSpawned += OnPlayerSpawned;
		GameManager.Instance.OnSceneLoaded += OnSceneLoaded;
    }

	private void OnDisable()
	{
        if (GameManager.Instance == null) return;
        GameManager.Instance.OnPlayerSpawned -= OnPlayerSpawned;
        GameManager.Instance.OnSceneLoaded -= OnSceneLoaded;
    }

	/// <summary>
	/// Resets the ObjectiveManager to start a new round
	/// </summary>
	public void ResetObjectiveManager()
	{
		_objectiveObjects.Clear();
		_possibleObjectives.Clear();
		_possibleZones.Clear();
		// Clear the player data specific to a round
		foreach(ulong clientId in GameManager.Instance.PlayerData.Keys)
		{
			GameManager.Instance.PlayerData[clientId].Objective = null;
			// TODO: Add any other objective related params that need clearing - objectives cleared this round? etc.
		}

		// Get all the zones in the scene
		List<Zone> zones = FindObjectsOfType<Zone>().ToList();
		foreach(Zone zone in zones)
		{
			RegisterZone(zone);
		}

		// Get all the objectives in the scene
		List<ObjectiveObjectInstance> objectiveObjectInstances = FindObjectsOfType<ObjectiveObjectInstance>().ToList();
		foreach(ObjectiveObjectInstance objectiveObjectInstance in objectiveObjectInstances)
		{
			RegisterObject(objectiveObjectInstance);
		}

        // Shuffle the possible objectives
        _possibleObjectives.Shuffle();

        // Reset the next available objective counter
        _nextAvailableObjective = 0;
    }

    /// <summary>
    /// Triggered by GameManager event once a player has spawned on the server.<br/>
	/// Sets the player's objective to the next one on the possible objectives list.
    /// </summary>
    /// <param name="clientId">The network client ID of the player</param>
    /// <returns>True if successful, False if out of objectives or clientId not found in PlayerData</returns>
    public bool AssignPlayerObjective(ulong clientId)
	{
        if (!GameManager.Instance.PlayerData.TryGetValue(clientId, out NetPlayerData playerData)) return false;

		if(_nextAvailableObjective < _possibleObjectives.Count)
		{
            _playerObjectives.Add(_possibleObjectives[_nextAvailableObjective], clientId);
            playerData.Objective = _possibleObjectives[_nextAvailableObjective];
            _nextAvailableObjective++;
        }
		else
		{
			playerData.Objective = null;
		}
        

        return true;
    }

	/// <summary>
	/// To be called if a player disconnects intentionally.<br/>
	/// Removes the player from the ObjectiveManager<br/>
	/// The player's ClientRPC will no longer be called to send new Objectives
	/// </summary>
	/// <param name="clientId">The clientId of the disconnecting player</param>
	public void RemovePlayer(ulong clientId)
	{
		if (GameManager.Instance.PlayerData.TryGetValue(clientId, out NetPlayerData objectivePlayerData))
		{
            if (objectivePlayerData.Objective != null)
            {
                _playerObjectives.Remove(objectivePlayerData.Objective);
            }
        }
	}

    /// <summary>
    /// To be called if a player has rejoined following a disconnection.<br/>
    /// This will re-register the player with the ObjectiveManager.<br/>
    /// If the player's old clientID is recognized, it will be replaced with the new one.<br/>
    /// The player's previous ObjectivePlayerData will be retained, if not a new one will be created <br/>
    /// </summary>
    /// <param name="currentClientId">The new / current clientId</param>
    /// <param name="previousClientId">The old / previous clientId</param>
    /// <returns>True if successful, False if not successful</returns>
    public bool ReconnectPlayer(ulong currentClientId)
	{
		if (!GameManager.Instance.PlayerData.TryGetValue(currentClientId, out NetPlayerData netPlayerData)) return false;

		// If an old objective does not exist, create one. If it does, remap it. 
		if (netPlayerData.Objective == null)
		{
			AssignPlayerObjective(currentClientId);
		}
		else
		{
			_playerObjectives.Add(netPlayerData.Objective, currentClientId);
		}

        netPlayerData.NetPlayer?.SetObjectiveStringClientRpc(netPlayerData.Objective?.ObjectiveString, netPlayerData.ClientRpcParams);
        
		return true;
    }

	/// <summary>
	/// Registers the objectiveObjectInstance with the Objective Manager<br/>
	/// This allows the Objective Manager to use this object when creating objectives
	/// </summary>
	/// <param name="objectiveObjectInstance"></param>
	public void RegisterObject(ObjectiveObjectInstance objectiveObjectInstance)
	{
		if(!_objectiveObjects.TryAdd(objectiveObjectInstance, 1))
		{
			// objectiveObjectInstance already exists - add to the qty
			_objectiveObjects[objectiveObjectInstance]++;
		}
		else
		{
			// First time adding this object type - create all possible objectives
			foreach(ObjectiveAction action in objectiveObjectInstance.ObjectiveObject.PossibleActions)
			{
				foreach(ObjectiveCondition condition in action.PossibleConditions)
				{
					Zone zone = null;
					if(condition.RequiresObjectToBeInZone)
					{
						// Condition requires a zone, try and get a random one from the list stored in the dictionary
						if(_possibleZones.TryGetValue(condition.RequiredZone, out List<Zone> zones))
						{
							//if(zones.Count > 0) zone = zones[UnityEngine.Random.Range((int)0, zones.Count)];
							foreach(Zone possibleZone in zones)
							{
                                Objective newObjective = new Objective(action, objectiveObjectInstance.ObjectiveColour, objectiveObjectInstance.ObjectiveObject, condition, possibleZone, false);
                                _possibleObjectives.Add(newObjective);
                            }
						}
					}
					
				}
			}
		}
	}


	public void RegisterZone(Zone zone)
	{

		if (_possibleZones.TryGetValue(zone.ZoneType, out List<Zone> zones))
		{
			if (!zones.Contains(zone)) zones.Add(zone);
		}
		else
		{
			List<Zone> newZones = new List<Zone>();
			newZones.Add(zone);
			_possibleZones.Add(zone.ZoneType, newZones);
		}
	}

	/// <summary>
	/// Creates a structred string which can be used to display the objective to the player
	/// </summary>
	/// <param name="objective">The objective to convert to a string</param>
	/// <returns>The objective as a string</returns>
	public string CreateObjectiveString(Objective objective)
	{
		if (objective == null) return String.Empty;

		string str = string.Empty;
		str = objective.Action.FriendlyString;

		// If the action is "Do anything with" then we don't need to add the colour, object or condition
		if (str != _anyActionString)
		{
			str = str.Replace("<COLOUR>", objective.Colour.FriendlyString);
			str = str.Replace("<OBJECT>", objective.Object.FriendlyString);
			str = str.Replace("<CONDITION>", objective.Condition.FriendlyString);
			if(objective.Condition.RequiresObjectToBeInZone)
			{
				str = str.Replace("<ZONE>", objective.Zone.FriendlyString);
			}
		}
		
		if(objective.Inverse)
		{
			// Prepend the inverse string and force first letter of original string to be lower case
			str = _inverseString + char.ToLower(str[0]) + str[1..];
		}

		return str;
	}
	
	/// <summary>
	/// Triggered when GameManager sends the OnPlayerSpawned event
	/// </summary>
	/// <param name="sender"></param>
	/// <param name="clientId"></param>
	private void OnPlayerSpawned(object sender, ulong clientId)
	{
		AssignPlayerObjective(clientId);
	}


	/// <summary>
	/// Triggered when GameManager sends the OnPlayerDisconnect event
	/// </summary>
	/// <param name="sender"></param>
	/// <param name="clientId"></param>
	private void OnPlayerDisconnect(object sender, ulong clientId)
	{
		RemovePlayer(clientId);
	}

	/// <summary>
	/// Triggered when GameManager sends the OnPlayerReconnect event
	/// </summary>
	/// <param name="sender"></param>
	/// <param name="playerReconnectData"></param>
	private void OnPlayerReconnect(object sender, PlayerReconnectData playerReconnectData)
	{
		ReconnectPlayer(playerReconnectData.CurrentClientID);
	}

	/// <summary>
	/// Triggered when GameManager sends OnSceneLoaded event
	/// </summary>
	/// <param name="sender"></param>
	/// <param name="e"></param>
	private void OnSceneLoaded(object sender, EventArgs e)
	{
		// Assign each players objective
		foreach(ulong clientId in GameManager.Instance.PlayerData.Keys)
		{
			if(GameManager.Instance.PlayerData[clientId].Objective == null)
			{
				AssignPlayerObjective(clientId);
			}
		}
	}

	/// <summary>
	/// Interface to allow objects to report objective actions occuring and have them checked
	/// </summary>
	/// <param name="objectiveOccured">The possible objective that occured</param>
	/// <param name="playerClientId">The player Client ID that initiated the action</param>
	public void ReportObjectiveAction(Objective objectiveOccured, ulong playerClientId)
	{
		// Check if the objective is one that is assigned to a player
		if(_playerObjectives.TryGetValue(objectiveOccured, out ulong clientId))
		{
			// Check if the matching objective's assigned player is the same as the one performing the action
			if (playerClientId != clientId) return;

			// The objective that occured matches one of that is assigned to a player
			// TODO increment player score, below is temporary for testing
			GameManager.Instance.PlayerData[playerClientId].Score += 100;

			// Remove this objective from the player list
			_playerObjectives.Remove(objectiveOccured);

			// Assign a new objective
			AssignPlayerObjective(clientId);

			GameManager.Instance.PlayerData[clientId].NetPlayer.SetObjectiveStringClientRpc(GameManager.Instance.PlayerData[clientId].Objective?.ObjectiveString, GameManager.Instance.PlayerData[clientId].ClientRpcParams);
		}
	}
}