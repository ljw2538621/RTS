using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RTSEngine
{
    public enum UnitAnimState { idle, building, collecting, moving, attacking, healing, converting, takingDamage, dead } //all the possible animations states

    public class Unit : FactionEntity
    {
        public override EntityTypes Type { get { return EntityTypes.unit; } }

        [SerializeField]
        private int populationSlots = 1; //how many population slots will this unit occupy?
        public int GetPopulationSlots() { return populationSlots; }
        public void SetPopulationSlots (int value) { populationSlots = value; }

        [SerializeField]
        private int apcSlots = 1; //this defines the capacity of this unit when it enters an APC
        public int GetAPCSlots () { return apcSlots; }

        [SerializeField]
        private bool canBeConverted = true; //can this unit be converted?
        public bool CanBeConverted() { return canBeConverted; }

        public Building Creator { private set; get; } //the building that produced this unit

        public int LastWorkerPosID { set; get; } //if this unit was constructing/collecting resource, this would the last worker position it had.

        [SerializeField]
        private Animator animator = null; //the animator component
        private UnitAnimState currAnimatorState; //holds the current animator state
        public UnitAnimState GetCurrAnimatorState() { return currAnimatorState; }
        [SerializeField]
        private AnimatorOverrideController animatorOverrideController = null; //the unit's main animator override controller component
        public bool LockAnimState { set; get; }//When true, it won't be possible to change the animator state using the SetAnimState method.

        //NPC Related:
        [SerializeField, Tooltip("Data required to manage the creation of this unit in a NPC faction.")]
        private NPCUnitRegulatorDataSingle regulatorData = new NPCUnitRegulatorDataSingle();
        /// <summary>
        /// Gets a NPCUnitRegulatorData instance that suits the input requirements.
        /// </summary>
        /// <param name="factionType">FactionTypeInfo instance that defines the faction type of the regulator data.</param>
        /// <param name="npcManagerCode">The NPCManager instance code that defines the NPC Manager type.</param>
        /// <returns>NPCUnitRegulatorData instance if both requirements are met.</returns>
        public NPCUnitRegulatorData GetRegulatorData (FactionTypeInfo factionType, string npcManagerCode) {
            return regulatorData.Filter(factionType, npcManagerCode); }

        //Unit components:
        public UnitHealth HealthComp { private set; get; }
        public Converter ConverterComp { private set; get; }
        public UnitMovement MovementComp { private set; get; }
        public Wander WanderComp { private set; get; }
        public EscapeOnAttack EscapeComp { private set; get; }
        public Builder BuilderComp { private set; get; }
        public ResourceCollector CollectorComp { private set; get; }
        public Healer HealerComp { private set; get; }

        public UnitAttack AttackComp { private set; get; }
        private UnitAttack[] AllAttackComp = new UnitAttack[0]; //holds all of the attack components attached to this unit
        public override void UpdateAttackComp(AttackEntity attackEntity) { AttackComp = (UnitAttack)attackEntity; }

        public void Init(GameManager gameMgr, int fID, bool free, Building createdBy, Vector3 gotoPosition)
        {
            base.Init(gameMgr, fID, free);

            //get the unit's components
            HealthComp = GetComponent<UnitHealth>();
            ConverterComp = GetComponent<Converter>();
            MovementComp = GetComponent<UnitMovement>();
            WanderComp = GetComponent<Wander>();
            EscapeComp = GetComponent<EscapeOnAttack>();
            BuilderComp = GetComponent<Builder>();
            CollectorComp = GetComponent<ResourceCollector>();
            HealerComp = GetComponent<Healer>();
            AllAttackComp = GetComponents<UnitAttack>();

            //initialize them:
            if (ConverterComp)
                ConverterComp.Init(gameMgr, this);
            if (MovementComp)
                MovementComp.Init(gameMgr, this);
            if (WanderComp)
                WanderComp.Init(gameMgr, this);
            if (EscapeComp)
                EscapeComp.Init(gameMgr, this);
            if (BuilderComp)
                BuilderComp.Init(gameMgr, this);
            if (CollectorComp)
                CollectorComp.Init(gameMgr, this);
            if (HealerComp)
                HealerComp.Init(gameMgr, this);
            foreach(UnitAttack comp in AllAttackComp) //init all attached attack components
            {
                if (AttackComp == null)
                    AttackComp = comp;

                comp.Init(gameMgr, this, MultipleAttackMgr);
            }
            if (MultipleAttackMgr)
                MultipleAttackMgr.Init(this);
            if (TaskLauncherComp) //if the entity has a task launcher component
                TaskLauncherComp.Init(gameMgr, this); //initialize it

            if (animator == null) //no animator component?
                Debug.LogError("[Unit] The " + GetName() + "'s Animator hasn't been assigned to the 'animator' field");

            if (animator != null) //as long as there's an animator component
            {
                if (animatorOverrideController == null) //if there's no animator override controller assigned..
                    animatorOverrideController = gameMgr.UnitMgr.GetDefaultAnimController();
                ResetAnimatorOverrideController(); //set the default override controller
                //Set the initial animator state to idle
                SetAnimState(UnitAnimState.idle);
            }

            Rigidbody rigidbody = GetComponent<Rigidbody>();
            if (rigidbody == null) //no rigidbody component?
                Debug.LogError("[Unit] The " + GetName() + "'s main object is missing a rigidbody component");

            //rigidbody settings:
            rigidbody.isKinematic = true;
            rigidbody.useGravity = false;

            //set the radius value:
            radius = MovementComp.GetAgentRadius();

            //if this is a free unit
            if (this.free)
                UpdateFactionColors(gameMgr.UnitMgr.GetFreeUnitColor()); //set the free unit color

            gameMgr.MinimapIconMgr?.Assign(selection); //ask the minimap icon manager to create the a minimap icon for this unit

            CustomEvents.OnUnitCreated(this); //trigger custom event

            SetInitialTargetPosition(createdBy, gotoPosition); //set creator building and make unity move to its goto position
        }

        //a method that is used to move the unit to its initial position after it spawns:
        public void SetInitialTargetPosition(Building source, Vector3 gotoPosition)
        {
            Creator = source; //set the building creator

            //only if the is owned by the local player or this is not a multiplayer game
            if (RTSHelper.IsLocalPlayer(this) || GameManager.MultiplayerGame == false)
            {
                if (Creator != null) //if the creator is assigned
                    Creator.SendUnitToRallyPoint(this); //send unit to rally point
                else if (Vector3.Distance(gotoPosition, transform.position) > gameMgr.MvtMgr.GetStoppingDistance()) //only if the goto position is not within the stopping distance of this unit
                {
                    gameMgr.MvtMgr.Move(this, gotoPosition, 0.0f, null, InputMode.movement, false); //no creator building? move player to its goto position
                }
            }
        }

        //a method that converts this unit to the converter's faction
        public void Convert(Unit converter, int targetFactionID)
        {
            if (targetFactionID == FactionID) //if the converter and this unit have the same faction, then, what a waste of time and resources.
                return;

            if (GameManager.MultiplayerGame == false) //if this is a single player game
                ConvertLocal(converter, targetFactionID); //convert unit directly
            else if (RTSHelper.IsLocalPlayer(this)) //online game and this is the local player
            {
                NetworkInput newInput = new NetworkInput()
                {
                    sourceMode = (byte)InputMode.unit,
                    targetMode = (byte)InputMode.convert,
                    initialPosition = transform.position,
                    value = targetFactionID
                };

                InputManager.SendInput(newInput, this, converter); //send conversion input to the input manager
            }
        }

        //a method that converts this unit to the converter's faction, locally
        public void ConvertLocal(Unit converter, int targetFactionID)
        {
            CustomEvents.OnUnitConversionStart(converter, this);

            Disable(false); //remove it first from its current faction

            AssignFaction(gameMgr.GetFaction(targetFactionID).FactionMgr); //assign the new faction

            if(converter) //if there's a source converter
                converter.ConverterComp.EnableConvertEffect(); //enable the conversion effect on the converter

            //deselect the unit if it was selected:
            if (selection.IsSelected)
                gameMgr.SelectionMgr.Selected.Remove(this);

            CustomEvents.OnUnitConversionComplete(converter, this); //trigger the custom event
        }

        public enum jobType { attack, building, collecting, healing, converting, all} //these are the components that the unit is allowed to have.

        //this method allows to cancel one or more jobs.
        public void CancelJob (jobType[] jobs)
        {
            foreach (jobType job in jobs)
                CancelJob(job);
        }

        public void CancelJob (jobType job)
        {
            if (AttackComp && (job == jobType.all || job == jobType.attack))
                AttackComp.Stop();
            if (BuilderComp && (job == jobType.all || job == jobType.building))
                BuilderComp.Stop();
            if (CollectorComp && (job == jobType.all || job == jobType.collecting))
            {
                CollectorComp.CancelDropOff();
                CollectorComp.Stop();
            }
            if (HealerComp && (job == jobType.all || job == jobType.healing))
                HealerComp.Stop();
            if (ConverterComp && (job == jobType.all || job == jobType.converting))
                ConverterComp.Stop();
        }

        //a method that assings a new faction for the unit
        public void AssignFaction(FactionManager factionMgr)
        {
            FactionMgr = factionMgr; //set the new faction
            FactionID = FactionMgr.FactionID; //set the new faction ID
            free = false; //if this was a free unit then not anymore

            Creator = gameMgr.GetFaction(FactionID).GetCapitalBuilding(); //make the unit's producer, the capital of the new faction

            UpdateFactionColors(gameMgr.GetFaction(FactionID).GetColor()); //set the new faction colors
            selection.UpdateMinimapIconColor(); //assign the new faction color for the unit in the minimap icon

            if (TaskLauncherComp != null) //if the unit has a task launcher 
                TaskLauncherComp.Init(gameMgr, this); //update the task launcher info
        }

        //a method that removes this unit from its current faction
        public override void Disable(bool destroyed)
        {
            base.Disable(destroyed);

            if(!free)
                gameMgr.GetFaction(FactionID).UpdateCurrentPopulation(-GetPopulationSlots());

            if(destroyed) //if the unit is supposed to be completely destroyed
                MovementComp.DestroyTargetPositionCollider(); //destroy the target collider position

            MovementComp.Stop(); //stop the unit's movement

            CancelJob(jobType.all); //cancel all jobs
        }

        //See if the unit is in idle state or not:
        public bool IsIdle()
        {
            return !(MovementComp.IsMoving()
                || (BuilderComp && BuilderComp.IsActive())
                || (CollectorComp && CollectorComp.IsActive())
                || (AttackComp && AttackComp.IsActive() && AttackComp.Target != null)
                || (HealerComp && HealerComp.IsActive())
                || (ConverterComp && ConverterComp.IsActive()));
        }

        //a method to change the animator state
        public void SetAnimState(UnitAnimState newState)
        {
            if (LockAnimState == true || animator == null) //if our animation state is locked or there's no animator assigned then don't proceed.
                return;

            if (currAnimatorState == UnitAnimState.dead //if the current animation state is not the death one
                || (newState == UnitAnimState.takingDamage && HealthComp.IsDamageAnimationEnabled() == false) //if taking damage animation is disabled
                || (HealthComp.IsDamageAnimationActive() && newState != UnitAnimState.dead) ) //or if it's enabled and it's in progress and the requested animator state is not a death one
                return;

            currAnimatorState = newState; //update the current animator state

            animator.SetBool("TookDamage", currAnimatorState == UnitAnimState.takingDamage);
            animator.SetBool("IsIdle", currAnimatorState==UnitAnimState.idle); //stop the idle animation in case take damage animation is played since the take damage animation is broken by the idle anim

            if (currAnimatorState == UnitAnimState.takingDamage) //because we want to get back to the last anim state after the taking damage anim is done
                return;

            animator.SetBool("IsBuilding", currAnimatorState == UnitAnimState.building);
            animator.SetBool("IsCollecting", currAnimatorState == UnitAnimState.collecting);
            animator.SetBool("IsMoving", currAnimatorState == UnitAnimState.moving);
            animator.SetBool("IsAttacking", currAnimatorState == UnitAnimState.attacking);
            animator.SetBool("IsHealing", currAnimatorState == UnitAnimState.healing);
            animator.SetBool("IsConverting", currAnimatorState == UnitAnimState.converting);
            animator.SetBool("IsDead", currAnimatorState == UnitAnimState.dead);
        }

        //using a parameter that determines whether the unit is currently in the moving animator state or not
        public bool IsInMvtState () { return animator.GetBool("InMvtState"); }

        //a method to change the animator override controller:
        public void SetAnimatorOverrideController(AnimatorOverrideController newOverrideController)
        {
            if (newOverrideController == null)
                return;

            animator.runtimeAnimatorController = newOverrideController; //set the runtime controller to the new override controller
            animator.Play(animator.GetCurrentAnimatorStateInfo(0).fullPathHash, -1, 0f); //reload the runtime animator controller
        }

        //a method that changes the animator override controller back to the default one
        public void ResetAnimatorOverrideController ()
        {
            SetAnimatorOverrideController(animatorOverrideController);
        }
    }
}
