using UnityEngine;

namespace BlindOrbit.Gameplay
{
    /// <summary>Base contract for non-lethal devices that react to the player.</summary>
    public abstract class PlayerTriggerEffect : MonoBehaviour
    {
        public virtual void OnPlayerEnter(PlayerController player) { }
        public virtual void OnPlayerStay(PlayerController player) { }
        public virtual void OnPlayerExit(PlayerController player) { }
    }
}
