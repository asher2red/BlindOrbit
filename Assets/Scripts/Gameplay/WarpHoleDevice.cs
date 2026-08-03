using System.Collections.Generic;
using UnityEngine;

namespace BlindOrbit.Gameplay
{
    public sealed class WarpHoleDevice : PlayerTriggerEffect
    {
        static readonly Dictionary<PlayerController, float> nextWarpTimes = new Dictionary<PlayerController, float>();
        Vector2 destination;
        float cooldown = 0.65f;

        public void Configure(Vector2 target, float reentryCooldown = 0.65f)
        {
            destination = target;
            cooldown = Mathf.Max(0.1f, reentryCooldown);
        }

        public override void OnPlayerEnter(PlayerController player)
        {
            if (nextWarpTimes.TryGetValue(player, out var nextTime) && Time.time < nextTime)
            {
                return;
            }

            nextWarpTimes[player] = Time.time + cooldown;
            player.Body.position = destination;
            player.transform.position = destination;
        }
    }
}
