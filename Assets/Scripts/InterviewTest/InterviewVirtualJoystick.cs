using UnityEngine;
using UnityEngine.EventSystems;

namespace baodeag.InterviewTest
{
    public class InterviewVirtualJoystick : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
    {
        [Header("References")]
        [SerializeField] private RectTransform background;
        [SerializeField] private RectTransform handle;

        [Header("Settings")]
        [SerializeField] private float handleRange = 0.45f;
        [SerializeField] private float deadZone = 0.08f;

        public Vector2 Direction { get; private set; }

        private Canvas parentCanvas;

        private void Awake()
        {
            parentCanvas = GetComponentInParent<Canvas>();
            ResetHandle();
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            OnDrag(eventData);
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (background == null || handle == null)
            {
                return;
            }

            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                background,
                eventData.position,
                parentCanvas != null ? parentCanvas.worldCamera : null,
                out Vector2 localPoint);

            Vector2 radius = background.sizeDelta * 0.5f;
            Vector2 normalized = new Vector2(localPoint.x / radius.x, localPoint.y / radius.y);
            Direction = Vector2.ClampMagnitude(normalized, 1f);

            if (Direction.magnitude < deadZone)
            {
                Direction = Vector2.zero;
            }

            handle.anchoredPosition = Direction * radius * handleRange;
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            Direction = Vector2.zero;
            ResetHandle();
        }

        private void ResetHandle()
        {
            if (handle != null)
            {
                handle.anchoredPosition = Vector2.zero;
            }
        }
    }
}
