using System.Collections.Generic;
using PG.DialogueGraph;
using UnityEngine;
using UnityEngine.Audio;

public class AudioForDialogueNodeActivator : MonoBehaviour
{
    [SerializeField] private DialogueManager _manager;
    [SerializeField] private bool _checkAudioTag;
    [SerializeField] private AudioSource _baseAudioSource;
    [SerializeField] private AudioData[] _datas;
    private Dictionary<string, AudioSource> _audioSources = new Dictionary<string, AudioSource>();
    public struct AudioData
    {
        public string name;
        public AudioSource audioSource;
    }

    private void Awake()
    {
        foreach (var data in _datas)
        {
            _audioSources.Add(data.name, data.audioSource);
        }
    }

    private void OnEnable()
    {
        _manager.dialogueChanged += UpdateAudio;
        _manager.dialogueEnded += StopAllAudio;
    }
    private void OnDisable()
    {
        _manager.dialogueChanged -= UpdateAudio;
        _manager.dialogueEnded -= StopAllAudio;
    }
    // Update is called once per frame
    void UpdateAudio(RuntimeDialogueNode runtimeDialogueNode)
    {
        if (runtimeDialogueNode.audioResource == null)
        {
            return;
        }
        if (_checkAudioTag)
        {
            StopAllAudio();
            if (_audioSources.TryGetValue(runtimeDialogueNode.audioKey, out AudioSource audioSource))
            {
                audioSource.resource = runtimeDialogueNode.audioResource;
                audioSource.Play();
            }
            else
            {
                PlayBaseAudio(runtimeDialogueNode.audioResource);
            }
        }
        else
        {
            PlayBaseAudio(runtimeDialogueNode.audioResource);
        }
    }

    void PlayBaseAudio(AudioResource audioResource)
    {
        _baseAudioSource.Stop();
        _baseAudioSource.resource = audioResource;
        _baseAudioSource.Play();
    }
    void StopAllAudio()
    {
        _baseAudioSource.Stop();
        foreach (var data in _datas)
        {
            if (data.audioSource == null)
            {
                continue;
            }
            data.audioSource.Stop();
        }
    }
}