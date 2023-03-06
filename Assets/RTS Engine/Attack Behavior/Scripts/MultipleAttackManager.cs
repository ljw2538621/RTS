using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/* Multiple Attack Manager script created by Oussama Bouanani, SoumiDelRio.
 * This script is part of the Unity RTS Engine */

namespace RTSEngine
{
    [RequireComponent(typeof(AttackEntity))]
    public class MultipleAttackManager : MonoBehaviour
    {
        private FactionEntity factionEntity = null; //the source component handling this comp

        public AttackEntity[] AttackEntities { private set; get; } //holds all the Attack Entity components attached to the faction entity main object
        public int BasicAttackID { private set; get; }//the basic attack that the faction entity uses
        private AttackEntity activeAttack; //the attack entity component that is currently active is held by this ref var

        [SerializeField]
        private bool canSwitchAttacks = true; //enable this to allow the local player to switch between different attack entities attached to the faction entity
        public bool CanSwitchAttacks () { return canSwitchAttacks; }
        [SerializeField]
        private bool revertToBasicAttack = true; //when enabled, everytime the faction entity uses a non-basic attack, it will revert back to the basic attack
        public bool RevertToBasic () { return revertToBasicAttack; }

        [SerializeField]
        private int switchAttackTaskCategory = 0; //the task panel category ID where the attack switch task will be drawn in.
        public int GetTaskPanelCategory () { return switchAttackTaskCategory; }

        public void Init(FactionEntity factionEntity)
        {
            this.factionEntity = factionEntity;

            AttackEntities = GetComponents<AttackEntity>(); //get all attack entity components attached to the faction entity
            if(AttackEntities.Length < 1) //if there's no more than one attack entity component
            {
                Destroy(this); //no need to have this component
                return;
            }

            activeAttack = null;
            for(int i = 0; i < AttackEntities.Length; i++) //go through the attack components
            {
                AttackEntities[i].ID = i; //set the attack entity ID for each attack component (will be used by UI task buttons to refer to the attack components).
                if (AttackEntities[i].IsBasic() == true) //if this is the basic attack
                {
                    BasicAttackID = i;
                    SetActiveAttack(i);
                }
                else
                    AttackEntities[i].Toggle(false);
            }
        }

        //enable an attack entity using its code
        public ErrorMessage EnableAttack (string attackCode)
        {
            for (int i = 0; i < AttackEntities.Length; i++) //go through all the source's attack entities
                if (AttackEntities[i].GetCode() == attackCode) //if the attack code matches
                    return EnableAttack(i);

            return ErrorMessage.attackTypeNotFound;
        }

        //enable an attack entity
        public ErrorMessage EnableAttack (int ID)
        {
            //if the attack type is locked then it can't be activated
            if (AttackEntities[ID].IsLocked)
                return ErrorMessage.attackTypeLocked;
            else if(AttackEntities[ID].CoolDownActive == true) //if the target attack type is in cool down right now:
                return ErrorMessage.attackInCooldown;

            if (GameManager.MultiplayerGame == false) //single player game -> directly enable attack entity
                EnableAttackLocal(ID);
            else if(RTSHelper.IsLocalPlayer(activeAttack.FactionEntity))
            {
                NetworkInput newInput = new NetworkInput()
                {
                    sourceMode = (byte)InputMode.factionEntity,
                    targetMode = (byte)InputMode.multipleAttack,
                    value = ID
                };
                InputManager.SendInput(newInput, factionEntity, null); //send input to input manager
            }

            return ErrorMessage.none;
        }

        public void EnableAttackLocal (int ID)
        {
            //make sure the attack isn't already enabled and that the attack ID is valid
            if (ID < 0 || ID >= AttackEntities.Length || AttackEntities[ID] == activeAttack)
                return;

            if(activeAttack != null) //if there was a previously active attack
            {
                activeAttack.Stop(); //stop if there's a current attack
                activeAttack.Reset(); //reset it
                activeAttack.Toggle(false); //deactivate it
                activeAttack.GetWeapon()?.Toggle(false); //disable the wepaon object
            }

            SetActiveAttack(ID);

            CustomEvents.OnAttackSwitch(activeAttack); //trigger custom event
        }

        //sets the active attack
        public void SetActiveAttack (int ID)
        {
            activeAttack = AttackEntities[ID]; //new active attack
            activeAttack.Toggle(true);
            activeAttack.FactionEntity.UpdateAttackComp(activeAttack);
            activeAttack.GetWeapon()?.Toggle(true); //enable the wepaon object
        }

        //update the lock status of an attack type:
        public void ToggleAttackLock (string attackCode, bool lockAttack)
        {
            foreach(AttackEntity attackType in AttackEntities) //go through all attack types
                if(attackType.GetCode() == attackCode) //if the attack code matches
                {
                    attackType.IsLocked = lockAttack; //update lock
                    return;
                }
        }
    }
}