using UnityEngine;

namespace BlindOrbit.Gameplay
{
    public sealed class OrbitMotion : MonoBehaviour
    {
        Vector2 center;
        float angularSpeed;
        Rigidbody2D body;

        void Awake()
        {
            body = GetComponent<Rigidbody2D>();
        }

        public void Configure(Vector2 orbitCenter, float degreesPerSecond)
        {
            center = orbitCenter;
            angularSpeed = degreesPerSecond;
        }

        void FixedUpdate()
        {
            var offset = (Vector2)transform.position - center;
            var nextPosition = center + (Vector2)(Quaternion.Euler(0f, 0f, angularSpeed * Time.fixedDeltaTime) * offset);
            if (body != null)
            {
                body.MovePosition(nextPosition);
                return;
            }

            transform.position = nextPosition;
        }
    }
}
