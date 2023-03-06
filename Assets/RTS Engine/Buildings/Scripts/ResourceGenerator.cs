using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;

/* Resource Generator script created by Oussama Bouanani, SoumiDelRio.
 * This script is part of the Unity RTS Engine */

namespace RTSEngine
{
    [RequireComponent(typeof(Building))]
    public class ResourceGenerator : MonoBehaviour {

        public Building building { private set; get; } //the main building component for which this component opeartes
        public bool IsActive { private set; get; } //is this component active or not?

        [System.Serializable]
		public class Generator
		{
            [SerializeField]
            private ResourceTypeInfo resourceType = null; //the resource type to collect
            public string GetResourceName() { return resourceType.GetName(); }
            [SerializeField]
            private Sprite taskIcon = null; //when the maximum amount is reached, a task appears on the task panel to collect the gathered resource when the generator is selected. This is the task's icon.
            public Sprite GetTaskIcon() { return taskIcon; }
            [SerializeField]
            private bool autoCollect = false; //if true, resources produced by this generator will be collected automatically. If false, then the player will have to collect them manually

            [SerializeField]
            private bool isActive = true; //is this generator active or not?

            [SerializeField]
            protected float collectOneUnitTime = 1.5f; //time required to collect one unit of this resource type.

            private float timer; //each generator is assigned a timer
            private void ReloadTimer () { timer = collectOneUnitTime; } //reloads the production timer

            [SerializeField]
			private int maxAmount = 50; //the maximum amount of this resource type that this generator can store.
			private int currAmount = 0; //the current amount of produced resource in this generator.
            public int GetCurrAmount() { return currAmount; }
            public bool IsMaxAmountReached () { return currAmount >= maxAmount; } //did this generator reach its max amount?

            //other components
            GameManager gameMgr;

            //Initialize the generator's settings:
            public void Init (GameManager gameMgr)
            {
                this.gameMgr = gameMgr;

                currAmount = 0;
                ReloadTimer();
            }

            //Update the production in this generator:
            public bool OnProductionUpdate (Building building)
            {
                if (isActive == false) //if this resource is not active, then do not proceed
                    return false;

                if (IsMaxAmountReached() == false && isActive == true) //as long as the maximum amount is not yet reached and this generator is active
                {
                    if (timer > 0) //timer
                        timer -= Time.deltaTime;
                    else //timer is done
                    {
                        currAmount++; //increment current amount
                        if (IsMaxAmountReached() == true) //if the maximum allowed amount is reached
                        {
                            if (autoCollect || gameMgr.GetFaction(building.FactionID).IsNPCFaction() == true) //if auto collect is on, this is a NPC faction
                                OnResourceCollected(building.FactionID); //collect resources directly
                            else
                                return true; //only return true when the generator is maxed out for the first time
                        }

                        ReloadTimer(); //reload timer
                    }
                }

                return false;
            }

            //a method to collect resources and reset production settings
            public void OnResourceCollected (int factionID)
            {
                gameMgr.ResourceMgr.UpdateResource(factionID, resourceType.GetName(), currAmount); //add resources
                currAmount = 0; //reset the current amount
            }
		}
        [SerializeField]
        private Generator[] generators = new Generator[0]; //an array of the generators available in this component
        public int GetGeneratorsLength () { return generators.Length; }
        public Generator GetGenerator (int id) { return generators[id];  }

        [SerializeField]
        private int taskPanelCategory = 0; //task panel category at which the collection button will be shown in case Auto Collect is turned off.
        public int GetTaskPanelCategory () { return taskPanelCategory; }
        [SerializeField]
        private AudioClip collectionAudio = null; //played when the player collects the resources produced by this generator.

        //other components
        private GameManager gameMgr;
        
        public void Init(GameManager gameMgr, Building building)
        {
            this.gameMgr = gameMgr;
            this.building = building;

            foreach (Generator g in generators) //go through all the available generators
                g.Init(gameMgr); //init the generator's settings

			if (GameManager.MultiplayerGame == true && !RTSHelper.IsLocalPlayer(building)) //if it's a multiplayer game and this does not belong to the local player's faction.
                enabled = false; //disable this component

            IsActive = true;
        }
        
		void Update ()
		{
            if (IsActive) //in order for the building to generate resources, it must be built.
                for (int i = 0; i < generators.Length; i++)
                {
                    if (generators[i].OnProductionUpdate(building) == true) //update the production of the resources, if return value is true then this is maxed out for the first time
                        CustomEvents.OnResourceGeneratorFull(this, i);
                }
        }

        //a method to collect resources.
        public void CollectResources (int generatorID)
        {
            generators[generatorID].OnResourceCollected(building.FactionID); //collect the resources and reset the generator's settings

            if (RTSHelper.IsLocalPlayer(building)) //if this is the local player:
            {
                CustomEvents.OnResourceGeneratorCollected(this, generatorID); //trigger custom event
                
                AudioManager.Play(gameMgr.GetGeneralAudioSource(), collectionAudio, false); //plau the collection audio
            }
        }
	}
}