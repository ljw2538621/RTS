using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/* Building Attack script created by Oussama Bouanani, SoumiDelRio.
 * This script is part of the Unity RTS Engine */

namespace RTSEngine
{
    [RequireComponent(typeof(Building))]
    public class BuildingAttack : AttackEntity
    {
        Building building; //the building's main component

        public override void Init(GameManager gameMgr, FactionEntity factionEntity, MultipleAttackManager multipleAttackMgr)
        {
            base.Init(gameMgr, factionEntity, multipleAttackMgr);

            this.building = (Building)factionEntity;
        }

        //can the building engage in an attack:
        public override bool CanEngage() //only if the building has health and is not in construction phase
        {
            return building.HealthComp.IsDead() == false && building.IsBuilt == true && coolDownTimer <= 0.0f;
        }

        //the building is always marked as in idle mode
        public override bool IsIdle() { return true; }

        //check whether the unit is in range of its target or not:
        public override bool IsTargetInRange()
        {
            return Vector3.Distance(transform.position, GetTargetPosition()) <= searchRange;
        }

        //update in case the building has an attack target:
        protected override void OnTargetUpdate()
        {
            if (IsTargetInRange() == false) //if the building's target is no longer in range
            {
                Stop(); //stop the attack.
                return; //and do not proceed
            }

            base.OnTargetUpdate();
        }

        //called when the building picks a target
        public override void SetTarget(FactionEntity newTarget, Vector3 newTargetPosition)
        {
            if (GameManager.MultiplayerGame == false) //single player game, go ahead
                SetTargetLocal(newTarget, newTargetPosition);
            else if(RTSHelper.IsLocalPlayer(building) == true) //only if this is a local player
            {
                NetworkInput newInput = new NetworkInput()
                {
                    sourceMode = (byte)InputMode.building,
                    targetMode = (byte)InputMode.attack,
                    targetPosition = newTargetPosition,
                };
                InputManager.SendInput(newInput, building, newTarget); //send input
                return;
            }
        }
    }
}
