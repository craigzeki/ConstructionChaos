using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoadingIcon : MonoBehaviour
{
    private void OnEnable()
    {
        StartCoroutine(Rotate());
    }

    private void OnDisable()
    {
        StopAllCoroutines();
    }

    private IEnumerator Rotate()
    {
        while (true)
        {
            transform.Rotate(0, 0, -360 * Time.deltaTime);
            yield return null;
        }
    }
}
