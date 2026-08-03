using UnityEngine;

namespace BlindOrbit.Gameplay
{
    public sealed class BlackHoleDevice : PlayerTriggerEffect
    {
        float pullStrength = 8f;

        public void Configure(float strength)
        {
            pullStrength = Mathf.Max(0f, strength);
        }

        public override void OnPlayerStay(PlayerController player)
        {
            var offset = (Vector2)transform.position - player.Body.position;
            var distance = Mathf.Max(0.75f, offset.magnitude);
            var falloff = 1f / Mathf.Max(1f, distance * 0.35f);
            player.Body.AddForce(offset.normalized * pullStrength * falloff, ForceMode2D.Force);
        }
    }
}
