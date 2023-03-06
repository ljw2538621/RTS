using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

/* AttackManager script created by Oussama Bouanani, SoumiDelRio.
 * This script is part of the Unity RTS Engine */

namespace RTSEngine
{
    public class AttackManager : MonoBehaviour
    {
        //when attack units which do not require a target are selected and the following key is held down by the player, a terrain attack can be launched
        [SerializeField]
        private bool terrainAttackEnabled = true;
        [SerializeField]
        private KeyCode terrainAttackKey = KeyCode.T;

        private GameManager gameMgr;

        public void Init (GameManager gameMgr)
        {
            this.gameMgr = gameMgr;
        }

        //a method called to launch a terrain attack
        public bool LaunchTerrainAttack (List<Unit> units, Vector3 attackPosition, bool direct = false)
        {
            //when direct is set to true, it will ignore whether or not the player is holding down the terrain attack key
            //if the terrain attack feature is disabled or the trigger key isn't pressed by the player
            if (!terrainAttackEnabled || ( !direct && !Input.GetKey(terrainAttackKey) ))
                return false;

            //get the units which do have an attack component and which do not require a target to be assigned
            List<Unit> attackUnits = units.Where(unit => unit.AttackComp != null && !unit.AttackComp.RequireTarget()).ToList();

            if (attackUnits.Count > 0) //if there are still units allowed to launch a terrain attack
            {
                gameMgr.MvtMgr.LaunchAttack(attackUnits, null, attackPosition, MovementManager.AttackModes.full, true);
                return true;
            }

            return false;
        }
    }
}