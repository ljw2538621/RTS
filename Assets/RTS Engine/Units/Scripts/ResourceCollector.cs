using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Networking;

/* Resource Collector script created by Oussama Bouanani, SoumiDelRio.
 * This script is part of the Unity RTS Engine */

namespace RTSEngine
{
	public class ResourceCollector : UnitComponent<Resource> {

        //drop off attributes:
        [SerializeField]
		private int maxCapacity = 7; //the maximum quantity of each resource that the unit can hold before having to drop it off at the closet building that allows him to do so, only if auto collection is disabled

        public class DropOffResource //this class holds the current amount of each resource the unit is holding:
		{
			public int CurrAmount { set; get; }
            private string name;

            private GameManager gameMgr;

            public void Init (GameManager gameMgr, string name)
            {
                this.gameMgr = gameMgr;
                this.name = name;
                CurrAmount = 0;
            }

            //a method that adds the collected resource here to the faction's resources
            public void AddResource(int factionID)
            {
                gameMgr.ResourceMgr.UpdateResource(factionID, name, CurrAmount);
                CurrAmount = 0;
            }
        }
        private DropOffResource[] dropOffResources;

        private Building dropOffBuilding; //where does the unit drop resources at?
        public enum DropOffStatus {inactive, ready, active, done, goingBack};
        private DropOffStatus dropOffStatus;
        public bool IsDroppingOff () { return dropOffStatus == DropOffStatus.active; }


		[System.Serializable]
		public struct CollectionObject
		{
            public ResourceTypeInfo resourceType; //the resource type that this collection object is associated with

            public GameObject obj; //child object of the unit

            public AnimatorOverrideController animatorOverrideController; //when collecting resources, the unit can have different collection animation for each resource type

            public GameObject dropOffObject; //child object of the unit, activated when the player is dropping off resources.
            public AnimatorOverrideController dropOffOverrideController; //the animator override controller that is active when the unit is dropping off resources
        }
        [SerializeField]
        private CollectionObject[] collectionObjects = new CollectionObject[0];
        //only assigned resource types to the above collectionObjects array can be collected by the unit with this component
        private Dictionary<ResourceTypeInfo, CollectionObject> collectionObjectsDic = new Dictionary<ResourceTypeInfo, CollectionObject>();
        private GameObject currDropOffObject; //the currently active drop off object

        public override void Init(GameManager gameMgr, Unit unit)
        {
            base.Init(gameMgr, unit);

            if(!gameMgr.ResourceMgr.CanAutoCollect()) //if auto resource collection is off and units must drop off resources
            {
                dropOffResources = new DropOffResource[gameMgr.ResourceMgr.GetMapResourcesCount()];
                int i = 0;
                foreach(ResourceManager.MapResource mr in gameMgr.ResourceMgr.GetMapResources())
                {
                    dropOffResources[i] = new DropOffResource(); //initialize the drop off resources
                    dropOffResources[i].Init(gameMgr, mr.GetResourceType().GetName());
                    i++;
                }
            }

            //for fast access time, add all the collection objects info to a dictionary
            collectionObjectsDic.Clear();
            foreach (CollectionObject co in collectionObjects)
            {
                if (co.dropOffObject) //if there's a drop off model has been assigned for this resource type
                    co.dropOffObject.SetActive(false); //hide it initially

                collectionObjectsDic.Add(co.resourceType, co);
            }
		}

        //a method that cancels the resource collection
        public override bool Stop()
        {
            Resource lastTarget = target;

            if (base.Stop() == false || dropOffStatus == DropOffStatus.active)
                return false;

            inProgressObject = null;

            if (currDropOffObject != null) //hide the drop off object
                currDropOffObject.SetActive(false);

            //reset collection settings
            CancelDropOff();

            unit.ResetAnimatorOverrideController(); //set the animator override controller of the collector back to the default one

            if(lastTarget)
            {
                lastTarget.WorkerMgr.Remove (unit);//remove the unit from the worker manager
                CustomEvents.OnUnitStopCollecting(unit, lastTarget); //trigger custom event
            }

            return true;
        }

        //update component if the collector has a target
        protected override bool OnActiveUpdate(float reloadTime, UnitAnimState activeAnimState, AudioClip[] inProgressAudio, bool breakCondition = false, bool inProgressEnableCondition = true, bool inProgressCondition = true)
        {
            if (base.OnActiveUpdate(
                target.GetCollectOneUnitDuration(),
                UnitAnimState.collecting,
                target.GetResourceType().GetCollectionAudio(),
                target.IsEmpty(), //target resource must not be empty
                (gameMgr.ResourceMgr.CanAutoCollect() == true || (dropOffStatus == DropOffStatus.goingBack || dropOffStatus == DropOffStatus.inactive)), //auto collection must be enabled or the unit must currently not be dropping off resources
                dropOffStatus != DropOffStatus.active //in order to be able to collect resources, unit must not be dropping off resources
                ) == false)
                return false;

            if (dropOffBuilding != null && dropOffStatus == DropOffStatus.active && unit.MovementComp.DestinationReached) //unit is currently dropping off resources while having a valid drop off building
            {
                DropOff();
                inProgress = false; //unit is no longer collecting => needs to go back to the resource to collect

                if (currDropOffObject != null) //hide the collection object
                    currDropOffObject.SetActive(false);
            }

            return true;
        }

        //a method that is called when the collector arrives at the target resource to collect
        protected override void OnInProgressEnabled(float reloadTime, UnitAnimState activeAnimState, AudioClip[] inProgressAudio)
        {
            base.OnInProgressEnabled(reloadTime, activeAnimState, inProgressAudio);

            if (collectionObjectsDic.TryGetValue(target.GetResourceType(), out CollectionObject collectionObject))
                unit.SetAnimatorOverrideController(collectionObject.animatorOverrideController); //update the runtime animator controller

            if (gameMgr.ResourceMgr.CanAutoCollect()) //if the collector can auto collect, no drop off required
                CustomEvents.OnUnitStartCollecting(unit, target); //trigger custom event
            else
            {
                if (dropOffStatus != DropOffStatus.goingBack) //only if the unit wasn't coming back after a drop off -> first time collecting
                {
                    UpdateDropOffResources(target.ID, 0); //check if the unit needs to drop off resources
                    CustomEvents.OnUnitStartCollecting(unit, target); //trigger custom event
                }

                //if the unit was coming back after a drop off, then that is done & unit is no longer dropping off resources
                if (dropOffStatus == DropOffStatus.goingBack)
                    CancelDropOff();
            }
        }

        //a method that is called when the builder achieved progress in collection
        protected override void OnInProgress()
        {
            base.OnInProgress();

            if (target.TreasureComp != null) //if the resource has a treasure component
            {
                target.DestroyResource(unit); //destroy the resource which will trigger opening the treasure
            }
            else if (target.IsEmpty() == false) //we're dealing with a normal resource that's not empty yet
            {
                target.AddAmount(-1, unit); //take one unit of the resource
            }
        }

        //update component if the collector doesn't have a target
        protected override void OnInactiveUpdate()
        {
            base.OnInactiveUpdate();
        }

        //a method called when collector searches for a target:
        protected override void OnTargetSearch()
        {
            base.OnTargetSearch();

            foreach (Resource r in gameMgr.ResourceMgr.GetAllResources()) //go through all resources list
                if (r != null && r.gameObject.activeInHierarchy == true && r.FactionID == unit.FactionID && r.IsEmpty() == false && Vector3.Distance(r.transform.position, transform.position) < autoBehavior.GetSearchRange()) //if the resource is in the collector's faction territory, it's not empty yet and it's inside the search range
                {
                    SetTarget(r); //set as new target
                    break; //leave loop
                }
        }

        /// <summary>
        /// Check whether a resource type (defined by its ResourceTypeInfo instance) can be collected by the resource collector.
        /// </summary>
        /// <param name="resourceType">ResourceTypeInfo instance that defines the resource type.</param>
        /// <param name="useDic">Set to true only if the source unit has been already initialized, when calling this on a prefab, set to false.</param>
        /// <returns>True if the resource type can be collected, otherwise false.</returns>
        public bool CanCollectResourceType (ResourceTypeInfo resourceType, bool useDic = true)
        {
            if (useDic) //the dictionary can be only used if the source unit has already been initialized.
                return collectionObjectsDic.ContainsKey(resourceType);
            else
            {
                foreach (CollectionObject co in collectionObjects)
                    if (co.resourceType == resourceType)
                        return true;

                return false;
            }
        }

        //a method that sets the target resource to collect
        public override ErrorMessage SetTarget(Resource newTarget, InputMode targetMode = InputMode.none)
        {
            if (target != newTarget) //if the resource wasn't being collected by this collector:
            {
                if (!CanCollectResourceType(newTarget.GetResourceType())) //resource type can't be collected by unit
                    return ErrorMessage.entityNotAllowed; 
                else if (newTarget.IsEmpty() == true)
                    return ErrorMessage.targetEmpty;
                else if (newTarget.WorkerMgr.currWorkers >= newTarget.WorkerMgr.GetAvailableSlots()) //max workers reached
                    return ErrorMessage.targetMaxWorkers;
                else if (newTarget.CanCollectOutsideBorder() == false && newTarget.FactionID != unit.FactionID)
                    return ErrorMessage.targetOutsideTerritory;
            }

            return base.SetTarget(newTarget, InputMode.collect);
        }

        //a method that sets the target resource to collect locally
        public override void SetTargetLocal (Resource newTarget)
		{
            if (newTarget.IsEmpty() || newTarget == null || (target == newTarget && dropOffStatus != DropOffStatus.done)) //if the new target is invalid or it's already the collector's target (if the collector is not coming back after dropping off resources)
                return; //do not proceed

            if(target != newTarget) //if the resource wasn't being collected by this collector:
            {
                if (newTarget.WorkerMgr.currWorkers < newTarget.WorkerMgr.GetAvailableSlots() && (newTarget.CanCollectOutsideBorder() == true || newTarget.FactionID == unit.FactionID)) //does the new target has empty slots in its worker manager? and can this be collected outside the border or this under the collector's territory
                {
                    CancelDropOff(); //cancel drop off if it was pending

                    Stop(); //stop collecting from the last resource (if there was one).

                    //set new target
                    inProgress = false;
                    target = newTarget;

                    inProgressObject = null; //initially set to nothing

                    if (collectionObjectsDic.TryGetValue(target.GetResourceType(), out CollectionObject collectionObject))
                        inProgressObject = collectionObject.obj; //update the current collection object

                    //Search for the nearest drop off building if we are not automatically collecting:
                    if (!gameMgr.ResourceMgr.CanAutoCollect())
                        UpdateDropOffBuilding();

                    //Move the unit to the resource and add the collector to the workers list in the worker manager
                    gameMgr.MvtMgr.PrepareMove(unit, target.WorkerMgr.Add(unit), target.GetRadius(), target, InputMode.resource, false);

                    CustomEvents.OnUnitCollectionOrder(unit, target); //trigger custom event
                }
            }
            else if(dropOffStatus == DropOffStatus.done) //collector is going back to the resource after a drop off
            {
                gameMgr.MvtMgr.PrepareMove(unit, target.WorkerMgr.GetWorkerPos(unit.LastWorkerPosID), target.GetRadius(), target, InputMode.resource, false);
                dropOffStatus = DropOffStatus.goingBack;
            }
		}

		//a method that finds the closest drop off building to an input position
		public void UpdateDropOffBuilding ()
		{
            if (target == null) //if there's no target resource
                return;

            Building closestBuilding = null;
            float minDistance = 0.0f;

            foreach(Building b in unit.FactionMgr.GetDropOffBuildings()) //go through all faction's drop off buildings
                if (b.IsBuilt == true && b.DropOffComp.CanDrop(target.GetResourceType().GetName())) //if this drop off building is completely built and it accepts the target resource
                    if (closestBuilding == null || Vector3.Distance(target.transform.position, b.transform.position) < minDistance) //is this the closest drop off building?
                    {
                        closestBuilding = b;
                        minDistance = Vector3.Distance(target.transform.position, b.transform.position);
                    }

            if (closestBuilding == null && unit.FactionID == GameManager.PlayerFactionID) //no drop off building has been found
                ErrorMessageHandler.OnErrorMessage(ErrorMessage.dropoffBuildingMissing, unit);

            dropOffBuilding = closestBuilding; //update the drop off building

            if (dropOffStatus == DropOffStatus.ready && dropOffBuilding != null) //if the unit is already waiting to drop off resources and a valid drop off destination has been assigned
                SendToDropOff(); //send the unit to drop off building then
        }

        //a method that sends the unit to drop off resources at the drop off building
        public void SendToDropOff ()
        {
            if (GameManager.MultiplayerGame == false) //if this is a singleplayer game then go ahead directly
                SendToDropOffLocal();
            else if (RTSHelper.IsLocalPlayer(unit)) //multiplayer game and this is the collector's owner
            {
                NetworkInput newInput = new NetworkInput()
                {
                    sourceMode = (byte)InputMode.unit,
                    targetMode = (byte)InputMode.dropoff,
                    initialPosition = transform.position
                };

                InputManager.SendInput(newInput, unit, dropOffBuilding);
            }
        }

        //a method that sends the unit to drop off resources at the drop off building locally
        public void SendToDropOffLocal ()
        {
            if (dropOffBuilding == null && unit.FactionID == GameManager.PlayerFactionID) //no drop off building and this is the player's faction
            {
                ErrorMessageHandler.OnErrorMessage(ErrorMessage.dropoffBuildingMissing, unit);
                return;
            }

            AudioManager.Stop(unit.AudioSourceComp); //stop the collection audio clip.

            dropOffStatus = DropOffStatus.active;

            if (collectionObjectsDic.TryGetValue(target.GetResourceType(), out CollectionObject collectionObject))
                unit.SetAnimatorOverrideController(collectionObject.dropOffOverrideController); //enable the drop off override controller.

            gameMgr.MvtMgr.PrepareMove(unit, dropOffBuilding.DropOffComp.GetDropOffPosition(), dropOffBuilding.DropOffComp.GetDropOffRadius(), dropOffBuilding, InputMode.building, false);

            CustomEvents.OnUnitDropOffStart(unit, target); //trigger custom event
        }

        //drop off resources at the drop off building
        public void DropOff ()
		{
            foreach (DropOffResource dor in dropOffResources) //go through the drop off resources
                dor.AddResource(unit.FactionID);

            //make the unit go back to the resource he's collecting from:
            dropOffStatus = DropOffStatus.done;
            SetTarget(target);

            unit.ResetAnimatorOverrideController(); //in case the animator override controller has been modified, reset it

            CustomEvents.OnUnitDropOffComplete(unit, target); //trigger custom event
		}

        //a method that updates the drop off resources that the collector is holding
        public void UpdateDropOffResources(int resourceID, int value)
        {
            dropOffResources[resourceID].CurrAmount += value; //update the current amount of this drop off resource

            if (dropOffResources[resourceID].CurrAmount >= maxCapacity) //if the maximum capacity has been reached
            {
                dropOffStatus = DropOffStatus.ready; //the collector is now in drop off mode
                unit.SetAnimState(UnitAnimState.idle); //move to idle state
                SendToDropOff(); //send the unit to drop off resources
                AudioManager.Stop(unit.AudioSourceComp); //stop the collection audio.

                if (inProgressObject != null)
                    inProgressObject.SetActive(false); //hide the collection object

                //look if there's a drop off object for the current taget's resource type
                if (collectionObjectsDic.TryGetValue(target.GetResourceType(), out CollectionObject collectionObject))
                {
                    currDropOffObject = collectionObject.dropOffObject;
                    if (currDropOffObject != null) //activate the drop off object
                        currDropOffObject.SetActive(true);
                }

                ToggleSourceTargetEffect(false); //hide the source and target effect objects during drop off
            }
        }

        //a method that cancels drop off:
        public void CancelDropOff ()
        {
            dropOffStatus = DropOffStatus.inactive;
        }
    }
}