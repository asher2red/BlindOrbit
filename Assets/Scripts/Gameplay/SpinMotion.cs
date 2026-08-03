using UnityEngine;

namespace BlindOrbit.Gameplay
{
    public sealed class SpinMotion : MonoBehaviour
    {
        float degreesPerSecond;
        Rigidbody2D body;

        void Awake()
        {
            body = GetComponent<Rigidbody2D>();
        }

        public void Configure(float speed)
        {
            degreesPerSecond = speed;
        }

        void FixedUpdate()
        {
            var delta = degreesPerSecond * Time.fixedDeltaTime;
            if (body != null)
            {
                body.MoveRotation(body.rotation + delta);
                return;
            }

            transform.Rotate(0f, 0f, delta);
        }
    }
}
