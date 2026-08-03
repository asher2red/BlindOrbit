using UnityEngine;

namespace BlindOrbit.Gameplay
{
    public sealed class BoosterDevice : PlayerTriggerEffect
    {
        float boost = 6f;

        public void Configure(float amount)
        {
            boost = Mathf.Max(0f, amount);
        }

        public override void OnPlayerEnter(PlayerController player)
        {
            player.Body.AddForce((Vector2)transform.up * boost, ForceMode2D.Impulse);
        }
    }
}
