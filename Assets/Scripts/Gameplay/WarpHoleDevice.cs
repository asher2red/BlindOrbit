using UnityEngine;

namespace BlindOrbit.Gameplay
{
    public sealed class WarpHoleDevice : PlayerTriggerEffect
    {
        Transform pairedHole;
        WarpPairState pairState;
        float cooldown = 1.5f;
        SpriteRenderer[] renderers;
        Color[] activeColors;
        bool showingCooldown;

        public void Configure(Transform partner, WarpPairState sharedState, float pairCooldown)
        {
            pairedHole = partner;
            pairState = sharedState;
            cooldown = Mathf.Max(0.1f, pairCooldown);
            renderers = GetComponentsInChildren<SpriteRenderer>();
            activeColors = new Color[renderers.Length];
            for (var i = 0; i < renderers.Length; i++)
            {
                activeColors[i] = renderers[i].color;
            }
        }

        public override void OnPlayerEnter(PlayerController player)
        {
            if (pairedHole == null || pairState == null || pairState.IsCoolingDown)
            {
                return;
            }

            pairState.BeginCooldown(cooldown);
            var destination = (Vector2)pairedHole.position;
            player.Body.position = destination;
            player.transform.position = destination;
        }

        void Update()
        {
            if (pairState == null || renderers == null)
            {
                return;
            }

            var coolingDown = pairState.IsCoolingDown;
            if (coolingDown == showingCooldown)
            {
                return;
            }

            showingCooldown = coolingDown;
            for (var i = 0; i < renderers.Length; i++)
            {
                renderers[i].color = coolingDown
                    ? Color.Lerp(activeColors[i], new Color(0.14f, 0.17f, 0.2f, activeColors[i].a), 0.72f)
                    : activeColors[i];
            }
        }
    }

    public sealed class WarpPairState
    {
        float readyTime;

        public bool IsCoolingDown => Time.time < readyTime;

        public void BeginCooldown(float seconds)
        {
            readyTime = Time.time + Mathf.Max(0.1f, seconds);
        }
    }
}
