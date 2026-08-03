using UnityEngine;

namespace BlindOrbit.Gameplay
{
    public sealed class FuelDrainDevice : PlayerTriggerEffect
    {
        float fuelPerSecond = 5f;

        public void Configure(float drainRate)
        {
            fuelPerSecond = Mathf.Max(0f, drainRate);
        }

        public override void OnPlayerStay(PlayerController player)
        {
            player.Fuel.Consume(fuelPerSecond * Time.fixedDeltaTime);
        }
    }
}
