using PG.Localization;
using TMPro;
using UnityEngine.UI;

namespace PG.DialogueGraph
{
    // DialogueManager, у которого имя говорящего, реплика и варианты ответа - ключи PG Localization System
    public class LocalizationDialogueManager : DialogueManager
    {
        public override void ShowUI(string speaker, string dialogue)
        {
            base.ShowUI(Localize(speaker), Localize(dialogue));
        }

        public override void RebuildChoices(RuntimeChoiceNode runtimeChoiceNode)
        {
            foreach (var choice in runtimeChoiceNode.choices)
            {
                Button button = Instantiate(_choiceButtonPrefab, _choiceButtonContainer);

                TMP_Text buttonText = button.GetComponentInChildren<TMP_Text>();
                if (buttonText != null)
                {
                    buttonText.text = Localize(choice.choiceText);
                }

                button.onClick.AddListener(() =>
                {
                    if (!string.IsNullOrEmpty(choice.desinationNodeID))
                    {
                        ShowNode(choice.desinationNodeID);
                    }
                    else
                    {
                        EndDialogue();
                    }
                });
            }
        }

        // Нет системы локализации или ключа - показываем текст как есть
        private static string Localize(string key)
        {
            if (string.IsNullOrEmpty(key) || LocalizationSystem.instance == null) return key;
            return LocalizationSystem.instance.GetLocalizedValue(key, key);
        }
    }
}
