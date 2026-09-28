using TMPro;
using UnityEngine;
using UnityEngine.UI;
namespace PG.DialogueGraph
{
    using LocalizationSystem;
    public class LocalizationDialogueManager : DialogueManager
    {

        public override void ShowUI(string speaker, string dialogue)
        {
            speaker = LocalizationSystem.instance.GetLocalizedValue(speaker);
            dialogue = LocalizationSystem.instance.GetLocalizedValue(dialogue);
            base.ShowUI(speaker, dialogue);
        }

        public override void RebuildChoices(RuntimeChoiceNode runtimeChoiceNode)
        {
            foreach (var choice in runtimeChoiceNode.choices)
            {
                Button button = Instantiate(_choiceButtonPrefab, _choiceButtonContainer);

                TMP_Text buttonText = button.GetComponentInChildren<TMP_Text>();
                if (buttonText != null)
                {
                    buttonText.text = LocalizationSystem.instance.GetLocalizedValue(choice.choiceText);
                }

                if (button != null)
                {
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
        }
    }
}
