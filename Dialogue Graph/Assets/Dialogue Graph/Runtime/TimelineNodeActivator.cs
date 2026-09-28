using PG.DialogueGraph;
using UnityEngine;
using UnityEngine.Playables;

public class TimelineNodeActivator : MonoBehaviour
{
    [SerializeField] private DialogueManager _manager;
    [SerializeField] private PlayableDirector _playableDirector;

    private void OnEnable()
    {
        _manager.dialogueChanged += UpdateTimeline;
    }
    private void OnDisable()
    {
        _manager.dialogueChanged -= UpdateTimeline;
    }
    void UpdateTimeline(RuntimeBaseNode runtimeBaseNode)
    {
        if (runtimeBaseNode is RuntimeTimelineNode runtimeTimelineNode)
        {
            PlayableDirector playableDirector = runtimeTimelineNode.director.Resolve(_manager);
            if (playableDirector == null)
            {
                playableDirector = _playableDirector;
            }
            if (playableDirector == null)
            {
                return;
            }

            playableDirector.stopped += OnTimelineFinished;
            playableDirector.Play(runtimeTimelineNode.timeline);
        }
    }

    void OnTimelineFinished(PlayableDirector director)
    {
        director.stopped -= OnTimelineFinished;
        _manager.NextDialogue();
    }
}
