using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Glass : MonoBehaviour
{
    [SerializeField] private SpriteRenderer _spriteRenderer;
    [SerializeField] private Sprite[] frames;
    [SerializeField] private float _framesPerSecond;

    private void OnTriggerEnter2D(Collider2D other)
    {
        StartCoroutine(PlayGif(1f / _framesPerSecond));
    }

    IEnumerator PlayGif(float timeBetweenFrames)
    {
        for (int i = 0; i < frames.Length; i++)
        {
            _spriteRenderer.sprite = frames[i];
            yield return new WaitForSeconds(timeBetweenFrames);
        }
        Destroy(gameObject);
    }
}
