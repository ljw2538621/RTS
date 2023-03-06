using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/* CustomAttributes script created by Oussama Bouanani, SoumiDelRio.
 * This script is part of the Unity RTS Engine */

namespace RTSEngine
{
    /// <summary>
    /// Converts a FactionTypeInfo property into a Popup property of available faction type code in the inspector
    /// </summary>
    public class FactionTypeAttribute : PropertyAttribute { }

    /// <summary>
    /// Converts a NPCTypeInfo property into a Popup property of available NPC type codes in the inspector
    /// </summary>
    public class NPCTypeAttribute : PropertyAttribute { }

    /// <summary>
    /// Converts a string property into a Popup property of available ResourceTypeInfo codes in the inspector
    /// </summary>
    public class ResourceTypeAttribute : PropertyAttribute { }
}
