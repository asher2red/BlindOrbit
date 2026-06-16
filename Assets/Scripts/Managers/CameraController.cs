using BlindOrbit.Gameplay;
using UnityEngine;

namespace BlindOrbit.Managers
{
    public sealed class CameraController : MonoBehaviour
    {
        [SerializeField] float gameplaySize = 8.5f;
        [SerializeField] float followSmoothing = 7f;
        [SerializeField] float zoomSmoothing = 4.5f;

        Camera targetCamera;
        Transform followTarget;
        Vector2 stageBounds;
        Vector2 revealFocus;
        float desiredSize;
        bool revealMode;

        public Camera Camera => targetCamera;

        public void Initialize(Camera cameraToControl)
        {
            targetCamera = cameraToControl;
            targetCamera.orthographic = true;
            targetCamera.orthographicSize = gameplaySize;
            targetCamera.backgroundColor = new Color(0.015f, 0.018f, 0.035f, 1f);
            desiredSize = gameplaySize;
        }

        public void Follow(PlayerController player, Vector2 bounds)
        {
            followTarget = player.transform;
            stageBounds = bounds;
            desiredSize = gameplaySize;
            revealMode = false;
            targetCamera.orthographicSize = gameplaySize;
            transform.position = new Vector3(followTarget.position.x, followTarget.position.y, -10f);
        }

        public void RevealFullStage(Vector2 bounds)
        {
            stageBounds = bounds;
            followTarget = null;
            revealMode = true;
            revealFocus = Vector2.zero;
            desiredSize = Mathf.Max(bounds.y * 0.53f, bounds.x * 0.53f / Mathf.Max(targetCamera.aspect, 0.1f));
        }

        public void RevealDeathArea(Vector2 deathPosition, Vector2 bounds)
        {
            stageBounds = bounds;
            followTarget = null;
            revealMode = true;
            revealFocus = deathPosition;
            desiredSize = gameplaySize * 1.5f;
        }

        void LateUpdate()
        {
            if (targetCamera == null)
            {
                return;
            }

            Vector3 targetPosition;
            if (revealMode)
            {
                targetPosition = ClampToBounds(revealFocus);
            }
            else if (followTarget != null)
            {
                targetPosition = ClampToBounds(followTarget.position);
            }
            else
            {
                targetPosition = transform.position;
            }

            transform.position = Vector3.Lerp(transform.position, targetPosition, 1f - Mathf.Exp(-followSmoothing * Time.unscaledDeltaTime));
            targetCamera.orthographicSize = Mathf.Lerp(targetCamera.orthographicSize, desiredSize, 1f - Mathf.Exp(-zoomSmoothing * Time.unscaledDeltaTime));
        }

        Vector3 ClampToBounds(Vector3 target)
        {
            var aspect = Mathf.Max(targetCamera.aspect, 0.1f);
            var halfHeight = targetCamera.orthographicSize;
            var halfWidth = halfHeight * aspect;
            var limitX = Mathf.Max(0f, stageBounds.x * 0.5f - halfWidth);
            var limitY = Mathf.Max(0f, stageBounds.y * 0.5f - halfHeight);
            return new Vector3(Mathf.Clamp(target.x, -limitX, limitX), Mathf.Clamp(target.y, -limitY, limitY), -10f);
        }
    }
}
