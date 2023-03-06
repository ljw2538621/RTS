using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/* Unit Attack script created by Oussama Bouanani, SoumiDelRio.
 * This script is part of the Unity RTS Engine */

namespace RTSEngine
{
    [RequireComponent(typeof(Unit))]
    public class UnitAttack : AttackEntity
    {
        Unit unit; //the building's main component

        [SerializeField]
        private string rangeTypeCode = "shortrange"; //input the attack range's type in this field (attack ranges can be defined in the attack manager).
        [SerializeField, Tooltip("Defines the unit's stopping distance when engaging in an attack.")]
        private UnitAttackRange range = new UnitAttackRange(); //defines how the unit's stopping distance when engaging in an attack.
        /// <summary>
        /// Gets the UnitAttackRange instance that defines how the unit's stopping distance when engaging in an attack.
        /// </summary>
        /// <returns>UnitAttackRange instance of the attack unit.</returns>
        public UnitAttackRange GetRange () { return range; }

        [SerializeField]
        private bool moveOnAttack = false; //is the unit allowed to trigger its attack while moving?
        [SerializeField]
        private float followDistance = 15.0f; //if the attack target's leaves the attack entity range (defined in the attack manager), then this is max distance between this and the target where the attack entity can follow its target before stopping the attack

        //animation related attributes:
        [SerializeField]
        private AnimatorOverrideController attackAnimOverrideController = null; //so that each attack component can have a different attack animation
        private bool canTriggerAnimation = true; //play the unit's attack animation?
        [SerializeField]
        private bool triggerAnimationInDelay = false; //true => the attack animation is triggered when the delay starts. if false, it will only be triggered when the attack is triggered

        public override void Init(GameManager gameMgr, FactionEntity factionEntity, MultipleAttackManager multipleAttackMgr)
        {
            base.Init(gameMgr, factionEntity, multipleAttackMgr);
            this.unit = (Unit) factionEntity;
        }

        //can the unit engage in an attack:
        public override bool CanEngage() //make sure the unit is not dead
        {
            return unit.HealthComp.IsDead() == false && coolDownTimer <= 0.0f;
        }

        //check whether the unit is in idle mode or not:
        public override bool IsIdle()
        {
            return unit.IsIdle();
        }

        //check whether the unit is in range of its target or not:
        public override bool IsTargetInRange()
        {
            if (!unit.MovementComp.CanMove()) //if the source unit can't move
                return Vector3.Distance(transform.position, GetTargetPosition()) <= searchRange; //we use the search range as the allowed attack range
            else
            {
                if (moveOnAttack == true) //if the can move on attack, keep calculating whether unit is inside its movement on attack range or not
                    return Vector3.Distance(transform.position, GetTargetPosition())
                        <= gameMgr.MvtMgr.GetStoppingDistance()
                            + range.GetStoppingDistance(Target) + range.GetMoveOnAttackOffset();
                else //can not move and attack, rely on unit's movement component to let us know when the destination is reached
                    return unit.MovementComp.DestinationReached;
            }
        }

        //update in case the unit has an attack target:
        protected override void OnTargetUpdate()
        {
            if (unit.MovementComp.CanMove()) //only if the unit is able to move
            {
                if (Target?.Type == EntityTypes.unit) //if there's a target unit
                {
                    //if this is not a AI unit defending a building and there's a target unit (not building) and it was already once inside the attack range of the target but the target moved away (distance is higher than the allowed follow distance) and the 
                    if (SearchRangeCenter == null && wasInTargetRange == true && Vector3.Distance(transform.position, GetTargetPosition()) > Mathf.Max(followDistance, initialEngagementDistance))
                    {
                        Stop(); //stop the attack.
                        return; //and do not proceed
                    }

                    //if the attack target unit changed its position before this unit reached it
                    if(range.CanUpdateMvt(lastTargetPosition, GetTargetPosition()))
                    {
                        unit.MovementComp.DestinationReached = false; //Destination is not marked as reached anymore
                                                                      //launch the attack again so that the unit moves closer to its target
                        gameMgr.MvtMgr.LaunchAttack(unit, Target, Target.GetSelection().transform.position, MovementManager.AttackModes.change, false);
                    }
                }


                //if the unit didn't reach its destination yet but it's not really moving
                if (unit.MovementComp.DestinationReached == false && unit.MovementComp.IsMoving() == false)
                    gameMgr.MvtMgr.LaunchAttack(unit, Target, GetTargetPosition(), MovementManager.AttackModes.change, false); //make the unit move towards the target
            }
            //if the source unit can not move
            //check if it was already in target range and if the target leaves the attacking range, then stop the attack
            else if(SearchRangeCenter == null && wasInTargetRange && !IsTargetInRange()) 
            {
                Stop();
                return;
            }

            //if the unit is not in los or it can't attack while moving or it can but it's not in the target's range yet
            if (IsInLineOfSight() == false || (moveOnAttack == false && unit.MovementComp.IsMoving() == true) || IsTargetInRange() == false)
                return;

            //if the reload timer is done and we can play the attack animation and if the delay conditions are met
            if (reloadTimer <= 0.0f && canTriggerAnimation && (triggerAnimationInDelay || (delayTimer <= 0.0 && triggered)))
                TriggerAnimation();

            base.OnTargetUpdate();
        }

        //a method that triggers the unit's attack animation
        public void TriggerAnimation()
        {
            if(attackAnimOverrideController)
                unit.SetAnimatorOverrideController(attackAnimOverrideController); //set the anim override controller if there's one

            unit.SetAnimState(UnitAnimState.attacking);

            canTriggerAnimation = false; //can only play attack animation again after the attack is done
        }

        //a method called when the unit attack is over:
        public override void Stop()
        {
            base.Stop();
            //unit.ResetAnimatorOverrideController(); //reset the animator override controller in case it has been changed
        }

        //a method called when an attack is complete:
        public override void OnAttackComplete()
        {
            base.OnAttackComplete();
            canTriggerAnimation = true; //attack animation can be triggered for the next attack
        }

        public override void SetTarget(FactionEntity newTarget, Vector3 newTargetPosition)
        {
            gameMgr.MvtMgr.LaunchAttack(unit, newTarget, newTargetPosition, MovementManager.AttackModes.none, false);
        }

        //set the attack target locally
        public override void SetTargetLocal(FactionEntity newTarget, Vector3 newTargetPosition)
        {
            unit.MovementComp.DestinationReached = false;
            base.SetTargetLocal(newTarget, newTargetPosition);
        }
    }
}