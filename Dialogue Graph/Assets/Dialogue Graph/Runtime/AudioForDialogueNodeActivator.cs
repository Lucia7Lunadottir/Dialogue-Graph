using System.Collections.Generic;
using PG.DialogueGraph;
using UnityEngine;

public class AudioForDialogueNodeActivator : MonoBehaviour
{
    [SerializeField] private DialogueManager _manager;
    [SerializeField] private AudioSource _baseAudioSource;
    private readonly HashSet<AudioSource> _usedAudioSources = new();

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

    void UpdateAudio(RuntimeBaseNode runtimeBaseNode)
    {
        if (runtimeBaseNode is not RuntimeDialogueNode runtimeDialogueNode || runtimeDialogueNode.audioResource == null)
        {
            return;
        }

        StopAllAudio();

        AudioSource audioSource = runtimeDialogueNode.audioSource.Resolve(_manager);
        if (audioSource == null)
        {
            audioSource = _baseAudioSource;
        }

        audioSource.resource = runtimeDialogueNode.audioResource;
        audioSource.Play();
        _usedAudioSources.Add(audioSource);
    }

    void StopAllAudio()
    {
        _baseAudioSource.Stop();
        foreach (var audioSource in _usedAudioSources)
        {
            audioSource.Stop();
        }
    }
}
