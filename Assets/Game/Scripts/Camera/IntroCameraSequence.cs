using System;
using System.Collections;
using UnityEngine;

namespace baodeag.Game
{
    public class IntroCameraSequence : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private CameraController followCamera;
        [SerializeField] private Transform mapCenter;
        [SerializeField] private Transform player;

        [Header("Intro")]
        [SerializeField] private float orbitDuration = 3.2f;
        [SerializeField] private float blendDuration = 1.4f;
        [SerializeField] private float orbitRadius = 28f;
        [SerializeField] private float orbitHeight = 18f;
        [SerializeField] private AnimationCurve ease = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

        private Coroutine introRoutine;

        public void PlayIntro(Action onComplete)
        {
            if (introRoutine != null)
            {
                StopCoroutine(introRoutine);
            }

            introRoutine = StartCoroutine(PlayIntroRoutine(onComplete));
        }

        private IEnumerator PlayIntroRoutine(Action onComplete)
        {
            followCamera.SetCameraControlEnabled(false);
            followCamera.SetFollowEnabled(false);
            Transform cameraTransform = followCamera.transform;
            Vector3 center = mapCenter != null ? mapCenter.position : Vector3.zero;

            for (float time = 0f; time < orbitDuration; time += Time.deltaTime)
            {
                float t = ease.Evaluate(time / orbitDuration);
                float angle = Mathf.Lerp(-55f, 265f, t) * Mathf.Deg2Rad;
                float height = Mathf.Lerp(orbitHeight + 4f, orbitHeight, t);
                Vector3 position = center + new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle)) * orbitRadius + Vector3.up * height;
                cameraTransform.position = position;
                cameraTransform.rotation = Quaternion.LookRotation(center - position + Vector3.up * 1.5f, Vector3.up);
                yield return null;
            }

            Vector3 startPosition = cameraTransform.position;
            Quaternion startRotation = cameraTransform.rotation;
            followCamera.SnapBehindTarget();
            Vector3 endPosition = cameraTransform.position;
            Quaternion endRotation = cameraTransform.rotation;

            cameraTransform.SetPositionAndRotation(startPosition, startRotation);

            for (float time = 0f; time < blendDuration; time += Time.deltaTime)
            {
                float t = ease.Evaluate(time / blendDuration);
                cameraTransform.position = Vector3.Lerp(startPosition, endPosition, t);
                cameraTransform.rotation = Quaternion.Slerp(startRotation, endRotation, t);
                yield return null;
            }

            followCamera.SnapBehindTarget();
            followCamera.SetFollowEnabled(true);
            onComplete?.Invoke();
        }
    }
}
