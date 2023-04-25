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
        _countdownText.enabled = false;
        _countdownTimer = (int)_countdownTimerStart;
        if (_cratesText != null) _cratesText.text = _numOfCrates.ToString("00") + " of " + _numOfCratesRequired.ToString("00");
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.gameObject.tag == "Crate")
        {
            _numOfCrates++;
            CheckWinCondition();
        }
    }

    private void CheckWinCondition()
    {
        if (_win) return;
        if (_cratesText != null) _cratesText.text = _numOfCrates.ToString("00") + " of " + _numOfCratesRequired.ToString("00");
        if (_numOfCrates >= _numOfCratesRequired)
        {
            if (_countdownCoroutine != null) StopCoroutine(_countdownCoroutine);
            _countdownCoroutine = StartCoroutine(DoCountdown());
        }
        else
        {
            if (_countdownCoroutine != null) StopCoroutine(_countdownCoroutine);
            _countdownText.enabled = false;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if(collision.gameObject.tag == "Crate")
        {
            _numOfCrates--;
            CheckWinCondition();
        }
    }

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
