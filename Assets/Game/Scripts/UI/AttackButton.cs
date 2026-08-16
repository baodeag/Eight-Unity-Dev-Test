using UnityEngine;
using UnityEngine.EventSystems;

namespace baodeag.Game
{
    public class AttackButton : MonoBehaviour, IPointerClickHandler
    {
        [Header("References")]
        [SerializeField] private PlayerController playerController;

        public void OnPointerClick(PointerEventData eventData)
        {
            if (playerController == null || GameManager.instance == null || !GameManager.instance.IsGameplayActive)
            {
                return;
            }

            playerController.AttemptAttack();
        }
    }
}
