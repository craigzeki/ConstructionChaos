using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class AudioManager : MonoBehaviour
{
    [SerializeField] private List<AudioClip> _audioClips = new List<AudioClip>();

    [SerializeField] private AudioSource _audioSource;

    [SerializeField] private Toggle _muteToggle;

    private List<int> _audioClipIndexes = new List<int>();

    private int _previousClipIndex = -1;
    private int _previousPreviousClipIndex = -1;

    private void Awake()
    {
        if (_audioSource == null) _audioSource = GetComponent<AudioSource>();
        ResetAudioClipIndexes();
        StartCoroutine(BackgroundMusicLoop());
        if (_muteToggle != null) _muteToggle.onValueChanged.AddListener(MuteToggle);
    }

    private void OnDisable()
    {
        StopAllCoroutines();
        if (_muteToggle != null) _muteToggle.onValueChanged.RemoveListener(MuteToggle);
    }

    private void ResetAudioClipIndexes()
    {
        _audioClipIndexes.Clear();
        _audioClipIndexes = Enumerable.Range(0, _audioClips.Count).ToList();
    }

    private IEnumerator BackgroundMusicLoop()
    {
        while (true)
        {
            if (_audioClipIndexes.Count == 0) ResetAudioClipIndexes();

            int index = Random.Range(0, _audioClipIndexes.Count);
            while (index == _previousClipIndex || index == _previousPreviousClipIndex)
            {
                index = Random.Range(0, _audioClipIndexes.Count);
            }
            _previousPreviousClipIndex = _previousClipIndex;
            _previousClipIndex = index;
            _audioSource.Stop();
            _audioSource.clip = _audioClips[_audioClipIndexes[index]];
            _audioSource.Play();
            _audioClipIndexes.RemoveAt(index);

            yield return new WaitForSeconds(_audioSource.clip.length);
        }
    }

    private void MuteToggle(bool isOn)
    {
        _audioSource.mute = isOn;
    }
}
