using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;

/* RTSEditorHelper component created by Oussama Bouanani,  SoumiDelRio
 * This script is part of the RTS Engine */

namespace RTSEngine
{
    /// <summary>
    /// Defines methods that help build the RTS Engine custom editors.
    /// </summary>
    [InitializeOnLoad] //constructor of class is ran as soon as the project is open.
    public static class RTSEditorHelper
    {
        /// <summary>
        /// Constructor that attempts to cache asset files as soon as the project is open.
        /// </summary>
        static RTSEditorHelper()
        {
            //cache asset files
            GetFactionTypes(); 
            GetResourceTypes();
            GetNPCTypes();

            //Debug.Log("[RTSEngine] Cached faction type, resource type and NPC type asset files.");
        }

        /// <summary>
        /// Attempts to get asset files of a given scriptable object and filter.
        /// </summary>
        /// <typeparam name="T">Type that extends ScriptableObject</typeparam>
        /// <param name="assets">The found assets will be added to this list.</param>
        /// <param name="filter">Filter string can contain search data of the asset files.</param>
        /// <returns>True if at least one asset file is found, otherwise false.</returns>
        private static bool TryGetAllAssetFiles <T>(out List<T> assets, string filter = "DefaultAsset l:noLabel t:noType") where T : ScriptableObject
        {
            assets = new List<T>();
            assets.Add(null); //for the unassigned asset file option
            string[] guids = AssetDatabase.FindAssets(filter);

            if (guids.Length > 0)
            {
                foreach (string guid in guids)
                {
                    string assetPath = AssetDatabase.GUIDToAssetPath(guid);
                    assets.Add(AssetDatabase.LoadAssetAtPath(assetPath, typeof(T)) as T);
                }

                return true;
            }
            else
                return false;
        }

        /// <summary>
        /// Searches and caches asset files of a given scriptable object and a filter.
        /// </summary>
        /// <typeparam name="T">Type that extends ScriptableObject</typeparam>
        /// <param name="assets">The found assets will be added assigned to this IEnumerable instance.</param>
        /// <param name="filter">Filter string can contain search data of the asset files.</param>
        /// <returns>True if at least one asset file is found and successfully assigned, otherwise false.</returns>
        private static bool CacheAssetFiles <T> (out IEnumerable<T> targetEnumerable, string filter) where T : ScriptableObject
        {
            targetEnumerable = null;
            if (TryGetAllAssetFiles<T>(out List<T> assetsList, filter))
            {
                targetEnumerable = assetsList;
                return true;
            }

            return false;
        }

        private static IEnumerable<FactionTypeInfo> factionTypes = null; //holds currently available FactionTypeInfo asset files.
        /// <summary>
        /// Gets the cached FactionTypeInfo assets in the project and refreshes the cache if necessary.
        /// </summary>
        /// <param name="requireTest">When true, a test will determine whether testFactionType is cached or not. If already cached, the cache will not be refreshed.</param>
        /// <param name="testFactionType">The FactionTypeInfo instance to test.</param>
        /// <returns>Diciontary instance where each key is the code cached FactionTypeInfo assets and each value is the actual asset instance that matches the key type</returns>
        public static Dictionary<string, FactionTypeInfo> GetFactionTypes (bool requireTest = false, FactionTypeInfo testFactionType = null)
        {
            //only refresh if..
            if (factionTypes == null //cache hasn't been assigned yet.
                || (requireTest == true && !factionTypes.Contains(testFactionType)) ) //or test is required while test faction type is not in the cached list
                CacheAssetFiles(out factionTypes, "t:FactionTypeInfo");

            return factionTypes.ToDictionary(type => type == null ? "Unassigned" : type.GetCode());
        }

        private static IEnumerable<NPCTypeInfo> npcTypes = null; //holds currently available NPCTypeInfo asset files.
        /// <summary>
        /// Gets the cached NPCTypeInfo assets in the project and refreshes the cache if necessary.
        /// </summary>
        /// <param name="requireTest">When true, a test will determine whether testNPCType is cached or not. If already cached, the cache will not be refreshed.</param>
        /// <param name="testNPCType">The NPCTypeInfo instance to test.</param>
        /// <returns>Diciontary instance where each key is the code cached NPCTypeInfo assets and each value is the actual asset instance that matches the key type</returns>
        public static Dictionary<string, NPCTypeInfo> GetNPCTypes (bool requireTest = false, NPCTypeInfo testNPCType = null)
        {
            //only refresh if..
            if (npcTypes == null //cache hasn't been assigned yet.
                || (requireTest == true && !npcTypes.Contains(testNPCType)) ) //or test is required while test resource type is not in the cached list
                CacheAssetFiles(out npcTypes, "t:NPCTypeInfo");

            return npcTypes.ToDictionary(type => type == null ? "Unassigned" : type.GetCode());
        }

        private static IEnumerable<ResourceTypeInfo> resourceTypes = null; //holds currently available ResourceTypeInfo asset files.
        /// <summary>
        /// Gets the cached ResourceTypeInfo assets in the project and refreshes the cache if necessary.
        /// </summary>
        /// <param name="requireTest">When true, a test will determine whether testResourceType is cached or not. If already cached, the cache will not be refreshed.</param>
        /// <param name="testResourceType">The ResourceTypeInfo instance to test.</param>
        /// <returns>Diciontary instance where each key is the code cached ResourceTypeInfo assets and each value is the actual asset instance that matches the key type</returns>
        public static Dictionary<string, ResourceTypeInfo> GetResourceTypes (bool requireTest = false, ResourceTypeInfo testResourceType = null)
        {
            //only refresh if..
            if (resourceTypes == null //cache hasn't been assigned yet.
                || (requireTest == true && !resourceTypes.Contains(testResourceType)) ) //or test is required while test resource type is not in the cached list
                CacheAssetFiles(out resourceTypes, "t:ResourceTypeInfo");

            return resourceTypes.ToDictionary(type => type == null ? "Unassigned" : type.GetName());
        }

        /// <summary>
        /// Allows to move an index in the interval: [0, MAX_VALUE]
        /// </summary>
        /// <param name="index">The index to move.</param>
        /// <param name="step">Specifies the size and direction of the index movement.</param>
        /// <param name="max">The maximum value that the index can have.</param>
        public static void Navigate (ref int index, int step, int max)
        {
            if (index + step >= 0 && index + step < max)
                index += step;
        }
    }
}
