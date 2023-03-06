using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RTSEngine
{
    /// <summary>
    /// Includes data that will be used to regulate the creation of the assigned unit by an NPC faction.
    /// </summary>
    [CreateAssetMenu(fileName = "NewUnitRegulatorData", menuName = "RTS Engine/NPC Unit Regulator Data", order = 52)]
    public class NPCUnitRegulatorData : NPCRegulatorData
    {
        //define the units amount ratio in relation to the population slots available for the faction
        [SerializeField, Tooltip("Instances of this unit amount to available population slots target ratio.")]
        private FloatRange ratioRange = new FloatRange(0.1f, 0.2f);
        public float GetRatio () { return ratioRange.getRandomValue(); }

        [SerializeField, Tooltip("Automatically create instances of this unit when it meets the above ratio requirements?")]
        private bool autoCreate = true; //Automatically create this unit type to meet the ratio requirements.
        //whether Auto Create is true or false, the minimum amount chosen above must be met.
        public bool CanAutoCreate () { return autoCreate; }
    }
}
