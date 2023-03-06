using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/* Converter script created by Oussama Bouanani, SoumiDelRio.
 * This script is part of the Unity RTS Engine */

namespace RTSEngine
{
	public class Converter : UnitComponent<Unit> {

        [SerializeField]
        private float stoppingDistance = 5.0f; //when assigned a target unit, this is the stopping distance that the healer will have
        [SerializeField]
        private float maxDistance = 7.0f; //the maximum distance between the healer and the target unit to heal.
        public float GetStoppingDistance() { return stoppingDistance; }

        [SerializeField]
        private float duration = 15.0f; //time (in seconds) in order to complete converting a target unit

        [SerializeField]
        private AudioClip[] conversionAudio = null;

        [SerializeField]
		public EffectObj effect = null; //effect spawned at target unit when the conversion is done

        //a method that stops the unit from converting
        public override bool Stop()
        {
            Unit lastTarget = target;

            if (base.Stop() == false)
                return false;

            CustomEvents.OnUnitStopConverting(unit, lastTarget); //trigger custom event

            return true;
        }

        //update component if the converter has a target unit
        protected override bool OnActiveUpdate(float reloadTime, UnitAnimState activeAnimState, AudioClip[] inProgressAudio, bool breakCondition = false, bool inProgressEnableCondition = true, bool inProgressCondition = true)
        {
            if (base.OnActiveUpdate(
                duration,
                UnitAnimState.converting,
                conversionAudio,
                //breaking condition:
                target.HealthComp.IsDead()
                    || target.FactionID == unit.FactionID
                    || (Vector3.Distance(transform.position, target.transform.position) > maxDistance && inProgress == true) //if the converter and the target have the same faction or the target is outside the max allowed range for conversion -> cancel job
                ) == false)
                return false;

            return true;
        }

        //a method that is called when the converter arrives at the target unit to convert
        protected override void OnInProgressEnabled(float reloadTime, UnitAnimState activeAnimState, AudioClip[] inProgressAudio)
        {
            base.OnInProgressEnabled(reloadTime, activeAnimState, inProgressAudio);

            CustomEvents.OnUnitStartConverting(unit, target); //trigger custom event
        }

        //a method that is called when the converter achieved progress in conversion
        protected override void OnInProgress()
        {
            base.OnInProgress();

            target.Convert(unit, unit.FactionID); //convert target unit
            Stop(); //cancel conversion job
        }

        //update component when the converter doesn't have a target unit
        protected override void OnInactiveUpdate ()
        {
            base.OnInactiveUpdate();
        }

        //a method called when converter searches for a target:
        protected override void OnTargetSearch()
        {
            base.OnTargetSearch();

            foreach (Unit u in unit.FactionMgr.GetEnemyUnits()) //go through the faction's enemy units list and look for a target unit
                if (u.gameObject.activeInHierarchy == true && u.CanBeConverted() == true && Vector3.Distance(u.transform.position, transform.position) < autoBehavior.GetSearchRange()) //if the current unit can be converted and it's inside the search range
                {
                    SetTarget(u); //set as new target
                    break; //leave loop
                }
        }

        //a method that sets the target unit to convert
        public override ErrorMessage SetTarget(Unit newTarget, InputMode targetMode = InputMode.none)
        {
            if (newTarget.CanBeConverted() == false)
                return ErrorMessage.targetNoConversion;
            else if (newTarget.FactionID == unit.FactionID)
                return ErrorMessage.targetSameFaction;
            else if (newTarget.HealthComp.IsDead() == true)
                return ErrorMessage.targetDead;

            return base.SetTarget(newTarget, InputMode.convertOrder);
        }

        //a method that sets the conversion target locally
        public override void SetTargetLocal (Unit newTarget)
		{
			if (newTarget == null || newTarget == target)
				return;

            Stop(); //stop converting the current unit

            //set new target
            inProgress = false;
            target = newTarget;

            gameMgr.MvtMgr.Move(unit, target.transform.position, stoppingDistance, target, InputMode.unit, false); //move the unit towards the target unit
		}

        //a method that spawns the converter's effect when it has successfully converted a unit
        public void EnableConvertEffect ()
        {
            if (effect != null) //only if there's a valid effect object
                gameMgr.EffectPool.SpawnEffectObj(effect, target.transform.position, Quaternion.identity, target.transform); //spawn the conversion effect.
        }
    }
}