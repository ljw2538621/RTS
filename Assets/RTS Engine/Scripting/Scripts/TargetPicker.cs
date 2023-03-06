using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/* TargetPicker script created by Oussama Bouanani, SoumiDelRio.
 * This script is part of the Unity RTS Engine */

namespace RTSEngine
{
    //the available types to pick a target from
    /* all: all targets
     * allInList: all targets defined in the list
     * allButInList: all targets but the ones defined in the list
     * */
    public enum TargetPickerType {all, allInList, allButInList}

    /// <summary>
    /// Generic data type that allows to pick all types of a target T, just a list of elements of type T or everything but a list of elements of type T
    /// </summary>
    [System.Serializable]
    public abstract class TargetPicker<T, V>
    {
        /* T is the type of target
         * V is the type of the list that defines the possible (or not) targets 
         * T can be the same as V */

        [SerializeField]
        protected TargetPickerType type = TargetPickerType.all;
        [SerializeField]
        protected List<V> list = new List<V>();

        /// <summary>
        /// Determines whether a target 't' can be picked as a valid target.
        /// </summary>
        /// <param name="t">The target to test its validity.</param>
        /// <returns>True if the target 't' can be picked, otherwise false.</returns>
        public bool IsValidTarget (T t)
        {
            return type == TargetPickerType.all
                || (type == TargetPickerType.allInList && IsInList(t))
                || (type == TargetPickerType.allButInList && !IsInList(t));
        }

        /// <summary>
        /// Is the target 't' in the list?
        /// </summary>
        /// <param name="t">Target instance to test</param>
        /// <returns>True if the target 't' is in the list, otherwise false.</returns>
        protected abstract bool IsInList(T t);
    }
}
