using UnityEngine;

namespace BlindOrbit.Gameplay
{
    public sealed class FuelSystem : MonoBehaviour
    {
        public float MaxFuel { get; private set; }
        public float CurrentFuel { get; private set; }
        public float NormalizedFuel => MaxFuel <= 0f ? 0f : Mathf.Clamp01(CurrentFuel / MaxFuel);
        public bool IsEmpty => CurrentFuel <= 0f;
        public float FuelUsed => Mathf.Max(0f, MaxFuel - CurrentFuel);

        public void ResetFuel(float amount)
        {
            MaxFuel = Mathf.Max(1f, amount);
            CurrentFuel = MaxFuel;
        }

        public float Consume(float requestedAmount)
        {
            if (requestedAmount <= 0f || CurrentFuel <= 0f)
            {
                return 0f;
            }

            var consumed = Mathf.Min(CurrentFuel, requestedAmount);
            CurrentFuel -= consumed;
            return consumed;
        }
    }
}
