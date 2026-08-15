using UnityEngine;
using UnityEngine.EventSystems;

namespace baodeag.InterviewTest
{
    public class InterviewAttackButton : MonoBehaviour, IPointerClickHandler
    {
        [Header("References")]
        [SerializeField] private InterviewPlayerController playerController;

        public void OnPointerClick(PointerEventData eventData)
        {
            if (InterviewGameManager.instance == null || !InterviewGameManager.instance.IsGameplayActive)
            {
                return;
            }

            playerController.AttemptAttack();
        }
    }
}
