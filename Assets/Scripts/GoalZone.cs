using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class GoalZone : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _cratesText;
    [SerializeField] private TextMeshProUGUI _countdownText;
    [SerializeField] private uint _numOfCratesRequired = 3;
    [SerializeField] private uint _countdownTimerStart = 3;

    private uint _numOfCrates = 0;
    private int _countdownTimer = 0;
    private Coroutine _countdownCoroutine;
    private bool _win = false;
   
    
    private void Awake()
    {
        //hide the goal countdown timer and set it to its value reeady for countdown
        _countdownText.enabled = false;
        _countdownTimer = (int)_countdownTimerStart;

        //reset the crate counter
        if (_cratesText != null) _cratesText.text = _numOfCrates.ToString("00") + " of " + _numOfCratesRequired.ToString("00");
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        //if crate has entered the area, increment the number of crates for the UI to update and check if the win conditions are met
        if(collision.gameObject.tag == "Crate")
        {
            _numOfCrates++;
            CheckWinCondition();
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "Crate")
        {
            //a crate has been removed from the area, reduce the number of crates
            _numOfCrates--;
            //update the UI and check the win conditions
            CheckWinCondition();
        }

    }


    /// <summary>
    /// Checks to see if all win conditions are met:<br/>
    /// 1. Required number of crates present in the area<br/>
    /// 2. Countdown timer has reached zero
    /// </summary>
    private void CheckWinCondition()
    {
        if (_win) return; //already won - don't bother
        if (_cratesText != null) _cratesText.text = _numOfCrates.ToString("00") + " of " + _numOfCratesRequired.ToString("00"); //update the crates UI text
        if (_numOfCrates >= _numOfCratesRequired)
        {
            //total number of crates criteria met
            //start the countdown timer
            if (_countdownCoroutine != null) StopCoroutine(_countdownCoroutine);
            _countdownCoroutine = StartCoroutine(DoCountdown());
        }
        else
        {
            //not enough crates yet, stop countdown if it was previously running (crate has been removed)
            if (_countdownCoroutine != null) StopCoroutine(_countdownCoroutine);
            //hide the countdown timer
            _countdownText.enabled = false;
        }
    }

    /// <summary>
    /// Runs the countdown and triggers the win condition if countdown completes<br/>
    /// <i>Start as a coroutine</i>
    /// </summary>
    /// <returns></returns>
    IEnumerator DoCountdown()
    {
        _countdownTimer = (int)_countdownTimerStart;
        _countdownText.enabled = true;
        
        while(_countdownTimer > -1)
        {
            _countdownText.text = _countdownTimer.ToString();
            _countdownTimer--;
            yield return new WaitForSeconds(1);
            
        }

        _countdownText.text = "WIN";
        _win = true;
        _countdownCoroutine = null;
    }

    
}
