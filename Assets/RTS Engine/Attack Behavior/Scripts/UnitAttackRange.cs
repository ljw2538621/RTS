using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/* UnitAttackRange script created by Oussama Bouanani, SoumiDelRio.
 * This script is part of the Unity RTS Engine */

namespace RTSEngine
{
    [System.Serializable]
    /// <summary>
    /// Defines the stopping distances and the range in which a certain attack unit can launch its attacks in.
    /// </summary>
    public class UnitAttackRange
    {
        [SerializeField, Tooltip("Minimum and maximum stopping distance when targeting a unit.")]
        private FloatRange unitStoppingDistance = new FloatRange(2.0f, 6.0f); //stopping distance for target units
        [SerializeField, Tooltip("Minimum and maximum stopping distance when targeting a building.")]
        private FloatRange buildingStoppingDistance = new FloatRange(5.0f, 10.0f); //stopping distance when the unit has a target building to attack
        [SerializeField, Tooltip("Minimum and maximum stopping distance when not targeting a specific target.")]
        private FloatRange noTargetStoppingDistance = new FloatRange(5.0f, 10.0f);  //stopping distance when the unit is launching an attack without a target assigned.

        [SerializeField, Tooltip("Include the target's unit/building radius when determining the attack's stopping distance.")]
        private bool includeTargetRadius = true;

        [SerializeField, Tooltip("If the unit is allowed to move and attack, the attack range is increased by this offset."), Min(0)]
        private float moveOnAttackOffset = 3.0f; //when the attack unit can move and attack, the range of attack increases by this value
        /// <summary>
        /// Gets the stopping distance offset for units that can move on attack.
        /// </summary>
        /// <returns>The attack stopping distance offset for units that can move on attack.</returns>
        public float GetMoveOnAttackOffset () { return moveOnAttackOffset; }

        [SerializeField, Tooltip("How far does the attack target need to move in order to recalculate the attacker's unit movement."), Min(0)]
        private float updateMvtDistance = 2.0f; //if the unit is moving towards a target and it changes its position by more than this distance, the attacker's movement will be recalculated
        /// <summary>
        /// Check if the current attack target has moved too far from its position when the attack is initiated.
        /// </summary>
        /// <param name="lastTargetPosition">The attack target's position when the attack was last initiated.</param>
        /// <param name="currTargetPosition">The current attack target's position.</param>
        /// <returns>True if the distance between the last and current attack target's position is greater or equal to the allowed update movement distance, otherwise false.</returns>
        public bool CanUpdateMvt (Vector3 lastTargetPosition, Vector3 currTargetPosition)
        {
            return Vector3.Distance(lastTargetPosition, currTargetPosition) >= updateMvtDistance;
        }

        [SerializeField, Tooltip("How would a group of units of this type move in formationmove in formation??")]
        private MovementManager.Formations attackFormation = MovementManager.Formations.circular; //the movement formation that units from this range type will have when moving to attack
        /// <summary>
        /// Gets the attack movement formation for the unit type associated with the attack range.
        /// </summary>
        /// <returns>Attack movement formation type.</returns>
        public MovementManager.Formations GetAttackFormation () { return attackFormation; }

        /// <summary>
        /// Get the appropriate stopping distance for an attack depending on the target type.
        /// </summary>
        /// <param name="targetType">Type of the attack target.</param>
        /// <param name="targetRadius">Size of the target's radius.</param>
        /// <returns>Stopping distance for the unit's movement to launch an attack.</returns>
        public float GetStoppingDistance (FactionEntity target)
        {
            float stoppingDistance = 0.0f;

            EntityTypes targetType = target ? target.Type : EntityTypes.none;
            switch(targetType)
            {
                case EntityTypes.unit:
                    stoppingDistance = unitStoppingDistance.min;
                    break;
                case EntityTypes.building:
                    stoppingDistance = buildingStoppingDistance.min;
                    break;
                default:
                    stoppingDistance = noTargetStoppingDistance.min;
                    break;
            }

            return stoppingDistance + (target && includeTargetRadius ? target.GetRadius() : 0.0f);
        }
    }
}
