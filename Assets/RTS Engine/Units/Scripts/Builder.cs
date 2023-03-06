using UnityEngine;
using System.Collections.Generic;

/* Builder script created by Oussama Bouanani, SoumiDelRio.
 * This script is part of the Unity RTS Engine */

namespace RTSEngine
{
	public class Builder : UnitComponent<Building> {

        [SerializeField]
        private bool constructFreeBuildings = false; //can the builder construct free buildings that do not belong to any faction?
        public bool CanConstructFreeBuilding () { return constructFreeBuildings; }

        [SerializeField]
		private int healthPerSecond = 5; //amount of health that the building will receive per second

        [SerializeField]
		private AudioClip[] constructionAudio = new AudioClip[0]; //played when the unit is constructing a building

        [SerializeField]
        private CodeCategoryField restrictions = new CodeCategoryField(); //if a building is in this list, then the builder won't be able to place it/construct it
        public bool CanBuild (Building building) //checks whether the input building can be constructed by this builder
        {
            return building != null ? !restrictions.Contains(building.GetCode(), building.GetCategory()) : false;
        }

        public override void Init(GameManager gameMgr, Unit unit)
        {
            base.Init(gameMgr, unit);

            healthPerSecond = (int)(healthPerSecond * gameMgr.GetSpeedModifier()); //get the speed modifier and set the health per second accordinly
        }

        //update component if the builder has a target
        protected override bool OnActiveUpdate(float reloadTime, UnitAnimState activeAnimState, AudioClip[] inProgressAudio, bool breakCondition = false, bool inProgressEnableCondition = true, bool inProgressCondition = true)
        {
            return base.OnActiveUpdate(
                1.0f,
                UnitAnimState.building,
                constructionAudio,
                target.HealthComp.CurrHealth >= target.HealthComp.MaxHealth);
        }

        //a method that is called when the builder arrives at the target building to construct
        protected override void OnInProgressEnabled(float reloadTime, UnitAnimState activeAnimState, AudioClip[] inProgressAudio)
        {
            base.OnInProgressEnabled(reloadTime, activeAnimState, inProgressAudio);

            CustomEvents.OnUnitStartBuilding(unit, target); //trigger custom event.
        }

        //a method that is called when the builder achieved progress in construction
        protected override void OnInProgress()
        {
            base.OnInProgress();

            target.HealthComp.AddHealth(healthPerSecond, unit); //add health points to the building.
        }

        //update component if the builder doesn't have a target
        protected override void OnInactiveUpdate()
        {
            base.OnInactiveUpdate();
        }

        //a method called when builder searches for a target:
        protected override void OnTargetSearch()
        {
            base.OnTargetSearch();

            foreach (Building b in unit.FactionMgr.GetBuildings()) //go through the faction's building list as long as the unit doesn't have a target
                if (b.gameObject.activeInHierarchy == true && b.HealthComp.CurrHealth < b.HealthComp.MaxHealth && Vector3.Distance(b.transform.position, transform.position) < autoBehavior.GetSearchRange()) //if the current building doesn't have max health and is in the defined range
                {
                    SetTarget(b); //set as new target
                    break; //leave loop
                }
        }

        //a method that stops the builder from constructing
        public override bool Stop ()
        {
            Building lastTarget = target;

            if (base.Stop() == false)
                return false;

            if(lastTarget) //if there was a target building
            {
                lastTarget.WorkerMgr.Remove(unit);//remove the unit from the worker manager

                CustomEvents.OnUnitStopBuilding(unit, lastTarget); //trigger custom event
            }
            return true;
        }

        //a method that sets the target building to construct
        public override ErrorMessage SetTarget (Building newTarget, InputMode targetMode = InputMode.none)
        {
            if (!newTarget.Placed) //can't construct a building that's not placed yet
                return ErrorMessage.buildingNotPlaced;

            if (restrictions.Contains(newTarget.GetCode(), newTarget.GetCategory())) //if the builder is not allowed to construct the input building
                return ErrorMessage.entityNotAllowed;
            else if (newTarget.FactionID != unit.FactionID) //different faction
                return ErrorMessage.targetDifferentFaction;
            else if (newTarget.HealthComp.CurrHealth >= newTarget.HealthComp.MaxHealth) //max health already
                return ErrorMessage.targetMaxHealth;
            else if (newTarget.WorkerMgr.currWorkers >= newTarget.WorkerMgr.GetAvailableSlots()) //max workers reached
                return ErrorMessage.targetMaxWorkers;

            return base.SetTarget(newTarget, InputMode.builder);
        }

        //a method that sets the target building to construct locally
        public override void SetTargetLocal (Building newTarget)
		{
			if (newTarget == null || target == newTarget || newTarget.WorkerMgr.currWorkers >= newTarget.WorkerMgr.GetAvailableSlots()) //if the new target is invalid or it's already the builder's target
				return; //do not proceed

            Stop(); //stop constructing the current building

            //set new target
            inProgress = false;
            target = newTarget;

            gameMgr.MvtMgr.PrepareMove(unit, target.WorkerMgr.Add(unit), target.GetRadius(), target, InputMode.building, false); //move the unit towards the target building

            CustomEvents.OnUnitBuildingOrder(unit, target); //trigger custom event

		}
	}
}