using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

/* Attack Entity script created by Oussama Bouanani, SoumiDelRio.
 * This script is part of the Unity RTS Engine */

namespace RTSEngine
{
    public abstract class AttackEntity : MonoBehaviour
    {
        //general attributes:
        [SerializeField]
        private bool isLocked = false; //is the attack type locked? when locked, the player can't switch to it
        public bool IsLocked
        {
            set { isLocked = value; }
            get { return isLocked; }
        }

        [SerializeField]
        private bool isActive = true; //is the attack component active?
        public void Toggle (bool active) { isActive = active; }
        public bool IsActive () { return isActive; }

        [SerializeField]
        private string code = "new_attack_code"; //input a unique code for each attack type
        public string GetCode () { return code; }
        [SerializeField]
        private Sprite icon = null; //the icon of the attack that appears on the task panel.
        public Sprite GetIcon() { return icon; }

        [SerializeField]
        private bool basic = true; //is this is the basic attack that is enabled by default for the faction entity?
        public bool IsBasic () { return basic; }
        public int ID { set; get; } //set by the multiple attacks manager in case there are more than one attack components attached to the faction entity

        //what units/buildings can this entity attack:
        [SerializeField]
        private bool engageAllTypes = true; //attack all unit and building types?
        [SerializeField]
        private bool engageUnits = true; //can attack units?
        [SerializeField]
        private bool engageFlyingUnits = true; //can attack flying units?
        [SerializeField]
        private bool engageBuildings = true; //can attack buildings?
        [SerializeField]
        private bool engageInList = false; //if true, attack entity will only be able to attack faction entities whome codes are in the below array, if false then it will be able to attack all entities but the ones whome codes are in the array
        [SerializeField]
        private CodeCategoryField codesList = new CodeCategoryField();

        //attack type-related attributes:
        [SerializeField]
        private bool direct = false; //when true, the faction entity will simply trigger the attack without sending an attack object.
        [SerializeField]
        private bool engageOnAssign = true; //can this attack entity engage an enemy when the local player assings it to?
        public bool CanEngageOnAssign() { return engageOnAssign; }
        [SerializeField]
        private bool engageWhenAttacked = false; //can this attack entity engage units that attack it?
        public bool CanEngageWhenAttacked () { return engageWhenAttacked; }
        [SerializeField]
        private bool engageOnce = false; //does this attack entity trigger one attack then stops? till it's reassigned to engage once again
        [SerializeField]
        private bool engageFriendly = false; //can this attack entity engage friendly faction entites?
        public bool CanEngageFriendly () { return engageFriendly; }

        [SerializeField]
        private bool engageInRange = true; //can the attack entity engage enemy units when they are within a certain range?
        [SerializeField]
        protected float searchRange = 10.0f; //the range where the attack entity can search for enemy units to engage if the above field is enable
        [SerializeField]
        private float searchReload = 1.0f; //how frequent will the attack entity look for targets to engage?
        private float searchTimer;

        //AI-related attributes
        public Border SearchRangeCenter { set; get; } //decides which building center this AI attack entity must protect

        //delay-related attributes:
        [SerializeField]
        private float delayDuration = 0.0f; //how long does the delay last for?
        protected float delayTimer;

        [SerializeField]
        private bool delayTriggerEnabled = false; //if set to true, then another component (other than this component must trigger the attack).
        protected bool triggered = false; //is attack already triggered?
        public void TriggerAttack() { triggered = true; }

        //reload-related attributes:
        [SerializeField]
        private bool useReload = true; //does this attack type have a reload time?
        [SerializeField]
        private float reloadDuration = 2.0f; //duration of the reload
        protected float reloadTimer = 0.0f;

        //cooldown-related attributes:
        [SerializeField]
        private bool coolDownEnabled = false; //enable cooldown before the attack entity can pick another target
        public bool CoolDownActive { private set; get; }
        [SerializeField]
        private float coolDownDuration = 10.0f; //how long would the cooldown last?
        protected float coolDownTimer;

        [SerializeField]
        private bool requireTarget = true; //does this attack type require a target to be assigned in order to launch it?
        public bool RequireTarget() { return requireTarget; }
        private bool terrainAttackActive = false; //if the attacker doesn't require a target and this is enabled -> attacker is about to launch attack to land on terrain

        private bool HasTarget () { return Target != null || (!requireTarget && terrainAttackActive); }
        public FactionEntity Target {private set; get;} //the current attack target
        //the target position is either the assigned target's current position or the last registered target position
        public Vector3 GetTargetPosition () { return Target ? Target.GetSelection().transform.position : lastTargetPosition; }
        protected Vector3 lastTargetPosition; //where was the attack target when the last attack was triggered?

        protected bool wasInTargetRange = false; //is the attack entity already in its target range?
        protected float initialEngagementDistance = 0.0f; //holds the distance between the target and the attacker when the attacker first enters in range of the target.

        [SerializeField]
        private AttackDamage damage = new AttackDamage(); //handles attack type damage settings
        public AttackDamage GetDamageSettings() { return damage; }
        private int dealtDamage; //amount of damage that the attack entity deal to its target(s)
        public int GetDealtDamage () { return dealtDamage; }
        [SerializeField]
        private bool reloadDealtDamage = false; //everytime the attack entity gets a new target, the above field will be reset

        [SerializeField]
        private AttackWeapon weapon = new AttackWeapon(); //handles the attack's weapon
        public AttackWeapon GetWeapon() { return weapon; }

        [SerializeField]
        private AttackLOS lineOfSight = new AttackLOS(); //handles the LOS settings for this attack type
        public bool IsInLineOfSight() {return lineOfSight.IsInSight(GetTargetPosition(), weapon.GetWeaponObject(), transform); }

        [SerializeField]
        private AttackObjectLauncher attackObjectLauncher = new AttackObjectLauncher(); //handles launching attack objects in case of an indirect attack.

        [SerializeField]
        public UnitComponentTask taskUI = new UnitComponentTask(); //the task that will appear on the task panel when the attack entity is selected.

        //Events: Besides the custom delegate events, you can directly use the event triggers below to further customize the behavior of the attack:
        [SerializeField]
        private UnityEvent attackerInRangeEvent = null;
        [SerializeField]
        private UnityEvent targetLockedEvent = null;
        [SerializeField]
        private UnityEvent attackPerformedEvent = null;
        public void InvokeAttackPerformedEvent() { attackPerformedEvent.Invoke(); }
        [SerializeField]
        private UnityEvent attackDamageDealtEvent = null;
        public void InvokeAttackDamageDealtEvent () { attackDamageDealtEvent.Invoke(); } 

        //Audio:
        [SerializeField]
        private AudioClip orderAudio = null; //played when the attack entity is ordered to attack
        public AudioClip GetOrderAudio () { return orderAudio; }
        [SerializeField]
        private AudioClip attackAudio = null; //played each time the attack entity attacks.

        //other components:
        public FactionEntity FactionEntity { private set; get; } //the main faction entity's component
        private MultipleAttackManager multipleAttackMgr;
        protected GameManager gameMgr;

        public virtual void Init(GameManager gameMgr, FactionEntity factionEntity, MultipleAttackManager multipleAttackMgr)
        {
            this.FactionEntity = factionEntity;
            this.multipleAttackMgr = multipleAttackMgr;
            this.gameMgr = gameMgr;

            CoolDownActive = false;
            coolDownTimer = 0.0f;

            searchTimer = 0.0f;
            reloadTimer = 0.0f;

            damage.Init(gameMgr, this); //initialize the damage settings
            attackObjectLauncher.Init(gameMgr, this);
            weapon.Init();

            reloadDuration /= gameMgr.GetSpeedModifier(); //apply the speed modifier
        }

        public abstract bool CanEngage(); //can the faction entity engage in an attack?

        //a method that cancels an attack
        public virtual void Stop()
        {
            terrainAttackActive = false; //in case this was active before stopping the attack
            //reloadTimer = reloadDuration; //reset the reload timer
            ResetDelay(); //set the delay timer
            Target = null;
            weapon.SetIdleRotation(); //set the weapon back to idle rotation
        }

        //can the param faction be attacked by this attack entity?
        public bool CanAttackFaction(int factionID)
        {
            return (FactionEntity.FactionID != factionID || engageFriendly == true);
        }

        //check if the attacker can engage target:
        public ErrorMessage CanEngageTarget (FactionEntity potentialTarget)
        {
            if (gameMgr.InPeaceTime() == true)
                return ErrorMessage.peaceTime;
            else if (potentialTarget == null) //if there's no target assigned
                return !requireTarget ? ErrorMessage.none : ErrorMessage.attackTargetRequired; //see if the attack entity can attack without target
            else if (potentialTarget.FactionID == FactionEntity.FactionID && engageFriendly == false)
                return ErrorMessage.targetSameFaction;
            else if (potentialTarget.EntityHealthComp.CanBeAttacked == false) //peace time, same faction ID or target can't be attacked? -> nope
                return ErrorMessage.targetNoAttack;

            if (engageAllTypes) //attack all types then yes!
                return ErrorMessage.none;

            //if the target can't attack units/buildings and this is the case then nope
            if ((potentialTarget.Type == EntityTypes.building && engageBuildings == false)
                || (potentialTarget.Type == EntityTypes.unit //also check whether the attacker can engage flying units
                    && (engageUnits == false || ((potentialTarget as Unit).MovementComp.CanFly() && !engageFlyingUnits)) ))
                return ErrorMessage.entityNotAllowed;
            
            return codesList.Contains(potentialTarget.GetCode(), potentialTarget.GetCategory()) == engageInList ? ErrorMessage.none : ErrorMessage.entityNotAllowed;
        }

        public abstract bool IsIdle(); //is the faction entity idle?

        //is the current target in range of the attacker?
        public abstract bool IsTargetInRange();

        //method that updates the cooldown
        void UpdateCoolDown()
        {
            if (coolDownTimer > 0) //cooldown timer
                coolDownTimer -= Time.deltaTime;
            else
            {
                CoolDownActive = false;

                CustomEvents.OnAttackCooldownUpdated(this, Target);
            }
        }

        protected virtual void Update ()
        {
            if (CoolDownActive == true) //cooldown:
                UpdateCoolDown();

            if (useReload == true && reloadTimer > 0) //reload timer update
                reloadTimer -= Time.deltaTime;

            //if the attack component is not active or is locked, the attack entity can't attack
            if (isActive == false || isLocked || CanEngage() == false) 
                return;

            if(!HasTarget()) //if there's a target assigned and no terrain attack is active
                OnNoTargetUpdate();
            else
            {  
                if (Target?.EntityHealthComp.IsDead() == true) //if the target is already dead
                {
                    Stop(); //stop the attack
                    return;
                }
                OnTargetUpdate();
            }
        }

        //update the attack entity when there's no target
        protected virtual void OnNoTargetUpdate ()
        {
            weapon.UpdateIdleRotation();

            if (gameMgr.InPeaceTime() == true) //game is still in peace time
                return;

            //if the attacker belongs to the local faction, the attacker can attack in range and it is in idle mode
            if (engageInRange == true && (GameManager.MultiplayerGame == false || RTSHelper.IsLocalPlayer(FactionEntity)) && IsIdle() == true)
            {
                if (searchTimer > 0)
                    searchTimer -= Time.deltaTime;
                else
                {
                    SearchTarget();
                    searchTimer = searchReload; //reload search timer
                }
            }
        }

        //search for a target to attack
        private void SearchTarget ()
        {
            //first pick the search center and size/range (if this is an AI unit defending a range center, the search range and size are that of the building center).
            float searchSize = (SearchRangeCenter == null) ? searchRange : SearchRangeCenter.Size;
            Vector3 searchCenter = (SearchRangeCenter == null) ? transform.position : SearchRangeCenter.transform.position;

            FactionEntity potentialTarget = null; //this will hold the potential target faction entity
            float currDistance = 0.0f;

            //go through the enemy faction entities of the attack entity's faction
            foreach (FactionEntity enemyEntity in (FactionEntity.IsFree() == true || engageFriendly) ? gameMgr.UnitMgr.GetAllUnits() : FactionEntity.FactionMgr.GetEnemyFactionEntities())
            {
                if (enemyEntity == null || enemyEntity.enabled == false || !enemyEntity.Interactable
                    || enemyEntity.EntityHealthComp.IsDead() == true || enemyEntity == FactionEntity as Unit)
                    continue; //move to the next enemy faction entity in case the Unit's component is not enabled or the entity isn't alive

                //if the enemy faction entity is inside the current search size and it can be attacked
                if((currDistance = Vector3.Distance(enemyEntity.transform.position, searchCenter)) < searchSize && CanEngageTarget(enemyEntity) == ErrorMessage.none)
                {
                    searchSize = currDistance; //this will always decrease the search size to the closest enemy entity
                    potentialTarget = enemyEntity;
                }
            }

            if (potentialTarget != null)
                SetTarget(potentialTarget, potentialTarget.GetSelection().transform.position); //if there's a potential target found, set it as next target
        }

        //update the attack entity when there's a target
        protected virtual void OnTargetUpdate()
        {
            //the override of this method in both the UnitAttack and BuildingAttack components are responsible for checking the conditions for which an attack can be continued or not
            //reaching this stage means that all conditions have been met and it's safe to continue with the attack

            if (wasInTargetRange == false) //if the attacker never was in the target's range but just entered it:
            {
                attackerInRangeEvent.Invoke();
                CustomEvents.OnAttackerInRange(this, Target, GetTargetPosition()); //launch custom event

                wasInTargetRange = true; //mark as in target's range.
                initialEngagementDistance = Vector3.Distance(transform.position, GetTargetPosition());
            }

            weapon.UpdateActiveRotation(GetTargetPosition());

            if (reloadTimer > 0 || IsInLineOfSight() == false) //still in reload time or not facing target? do not proceed
                return;

            if (delayTimer > 0) //delay timer
            {
                delayTimer -= Time.deltaTime;
                return;
            }

            //If the attack delay is over, the attack is triggered and LOS conditions are met
            if (triggered)
            {
                if (direct == true) //Is this a direct attack (no use of attack objects)?
                {
                    attackPerformedEvent.Invoke();
                    //not an area attack:
                    CustomEvents.OnAttackPerformed(this, Target, GetTargetPosition());

                    damage.TriggerAttack(Target, GetTargetPosition());
                    AudioManager.Play(FactionEntity.AudioSourceComp, attackAudio, false);
                    OnAttackComplete();
                }
                else if (attackObjectLauncher.OnIndirectAttackUpdate()) //indirect attack ? -> must launch attack objects
                    AudioManager.Play(FactionEntity.AudioSourceComp, attackAudio, false);
            }
        }

        //method called when an attack is complete
        public virtual void OnAttackComplete()
        {
            terrainAttackActive = false; //disable if this was active
            reloadTimer = reloadDuration; //reset the reload timer

            StartCoolDown(); //start the cooldown

            //If this not the basic attack then revert back to the basic attack after done if that's the condition
            if (basic == false && multipleAttackMgr != null && multipleAttackMgr.RevertToBasic() == true)
                multipleAttackMgr.EnableAttack(multipleAttackMgr.BasicAttackID);

            //Attack once? cancel attack to prevent source from attacking again
            if (engageOnce == true)
                Stop();

            ResetDelay();
        }

        //a method that resets the attack's delay options
        void ResetDelay()
        {
            delayTimer = delayDuration; //reset the attack delay timer
            triggered = !delayTriggerEnabled; //does the attack need to be triggered from an external component?
        }

        //a method that starts the attack cooldown:
        public void StartCoolDown()
        {
            if (coolDownEnabled == true) //only if the cooldown option is enabled
            {
                coolDownTimer = coolDownDuration;
                CoolDownActive = true;

                CustomEvents.OnAttackCooldownUpdated(this, Target);
            }
        }

        //set the attack target
        public abstract void SetTarget(FactionEntity newTarget, Vector3 newTargetPosition);

        //set the attack target locally
        public virtual void SetTargetLocal(FactionEntity newTarget, Vector3 newTargetPosition)
        {
            lastTargetPosition = newTargetPosition; //mark the last position of the target

            lineOfSight.IsActive = false; //initially mark the attacker as not inside the LOS of the target

            if (newTarget != null) //if there's an attack target
            {
                if (newTarget != Target) //if this is a different target than the last one assigned
                {
                    Target = newTarget; //set new target
                    damage.UpdateCurrentDamage(Target); //update the current damage value to apply to the target in an attack
                }
            }
            else if (!requireTarget) //no target assigned but this is allowed for this attacker
                terrainAttackActive = true; //incoming terrain attack

            if (direct == false) //if the attack type is in-direct (attack is done by launching attack objects)
                attackObjectLauncher.Activate();

            wasInTargetRange = false;

            //new target, check to reload damage dealt:
            if (reloadDealtDamage == true)
                dealtDamage = 0;

            //events:
            targetLockedEvent.Invoke();
            CustomEvents.OnAttackTargetLocked(this, Target, GetTargetPosition());

            ResetDelay(); //reload the attack delay
        }

        //reset the attack attributes
        public void Reset()
        {
            reloadTimer = reloadDuration; //reset the reload timer
            ResetDelay();
            Target = null;
            wasInTargetRange = false;
        }

        //increase/decrease the dealt damage
        public void AddDamageDealt(int value)
        {
            dealtDamage += value;
        }

        //get the current damage dealt.
        public float GetDamageDealt()
        {
            return dealtDamage;
        }
    }
}
