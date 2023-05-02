using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// An instance of an objective object.<br/>
/// This is the object that will be placed in the world.
/// </summary>
public class ObjectiveObjectInstance : MonoBehaviour
{
    /// <summary>
    /// The objective object that this instance is based on.
    /// </summary>
    [SerializeField]
    private ObjectiveObject objectiveObject;
    public ObjectiveObject ObjectiveObject => objectiveObject;

    /// <summary>
    /// The colour of the objective object.
    /// </summary>
    [SerializeField]
    [Dropdown("objectiveObject.PossibleColours")]
    private ObjectiveColour objectiveColour;
    public ObjectiveColour ObjectiveColour => objectiveColour;

    private void Awake()
    {
        // The object should be an actual colour, not any coloured
        while ((objectiveColour?.FriendlyString == "any coloured") || (objectiveColour == null))
        {
            // Pick a random colour from the list of possible colours and assign it to the objective colour
            objectiveColour = objectiveObject.PossibleColours[Random.Range(0, objectiveObject.PossibleColours.Count)];
        }
        

        // Set the colour of the object to the colour of the objective colour
        //! This is temporary, will need to be adapted for different types of objects
        GetComponent<SpriteRenderer>().color = objectiveColour.Colour;

        // TODO: Tell the objective manager that this object exists

        // TODO: For each of the possible actions, add a component to detect that action
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
