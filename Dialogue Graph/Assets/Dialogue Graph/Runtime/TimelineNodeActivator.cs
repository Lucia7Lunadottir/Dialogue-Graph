using PG.DialogueGraph;
using UnityEngine;
using UnityEngine.Playables;

// Запускает таймлайн ноды TimelineNode и ведёт диалог дальше, когда он доиграл.
// Wrap Mode None - конец по stopped. Hold - конец по времени: последний кадр держится (например, под выбором ответа),
// пока не начнётся следующий таймлайн на другом директоре или не закончится диалог.
public class TimelineNodeActivator : MonoBehaviour
{
    [SerializeField] private DialogueManager _manager;
    [SerializeField] private PlayableDirector _playableDirector;

    // Директор, который играет ноду сейчас, и сама нода
    private PlayableDirector _playing;
    private RuntimeTimelineNode _node;
    // Доигравший Hold-таймлайн, который держит последний кадр
    private PlayableDirector _held;

    private void OnEnable()
    {
        _manager.dialogueChanged += UpdateTimeline;
        _manager.dialogueEnded += OnDialogueEnded;
    }
    private void OnDisable()
    {
        _manager.dialogueChanged -= UpdateTimeline;
        _manager.dialogueEnded -= OnDialogueEnded;
        Release();
    }
    void UpdateTimeline(RuntimeBaseNode runtimeBaseNode)
    {
        // Новая нода - старая подписка больше не нужна
        Release();
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

            // Удержанный кадр другого директора отпускаем, иначе два таймлайна держат одни объекты
            if (_held != null && _held != playableDirector)
            {
                _held.Stop();
            }
            _held = null;

            // Подписка после Play: на одном директоре смена таймлайна может прислать stopped от предыдущего
            playableDirector.Play(runtimeTimelineNode.timeline);
            _playing = playableDirector;
            _node = runtimeTimelineNode;
            playableDirector.stopped += OnTimelineStopped;
        }
    }

    // Hold не присылает stopped в конце - доигрывание ловим по времени
    private void Update()
    {
        if (_playing == null || _playing.extrapolationMode != DirectorWrapMode.Hold) return;
        if (_playing.state != PlayState.Playing || _playing.time < _playing.duration) return;
        // До Finish: следующая нода может сразу запустить другой таймлайн и отпустить этот кадр
        _held = _playing;
        Finish();
    }

    void OnTimelineStopped(PlayableDirector director) => Finish();

    void Finish()
    {
        // Дальше - только если диалог всё ещё на этой ноде (не закончен и не перезапущен)
        bool current = _manager.currentBaseNode != null && _manager.currentBaseNode == _node;
        Release();
        if (current)
        {
            _manager.NextDialogue();
        }
    }

    void OnDialogueEnded()
    {
        Release();
        // Диалог кончился - удержанный кадр больше не нужен
        if (_held != null)
        {
            _held.Stop();
            _held = null;
        }
    }

    void Release()
    {
        if (_playing != null)
        {
            _playing.stopped -= OnTimelineStopped;
        }
        _playing = null;
        _node = null;
    }
}
