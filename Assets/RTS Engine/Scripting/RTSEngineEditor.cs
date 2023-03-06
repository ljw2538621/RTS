using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RTSEngine
{
    [ExecuteAlways]
    public class RTSEngineEditor : MonoBehaviour
    {
        public static RTSEngineEditor instance = null;

        private void Awake()
        {
            if (instance == null)
                instance = this;
            else if (instance != this)
                DestroyImmediate(this);
        }

        void Update()
        {
            print("Editor caused this.");
        }
    }
}
