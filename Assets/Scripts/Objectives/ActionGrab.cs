using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// Class to detect when an object has been grabbed by a player and report this to the ObjectiveManager
/// </summary>
public class ActionGrab : ObjectiveActionBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(blinky());
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    IEnumerator blinky()
    {
        while(true)
        {
            test = !test;
            yield return new WaitForSeconds(0.5f);
        }
    }

    
}
