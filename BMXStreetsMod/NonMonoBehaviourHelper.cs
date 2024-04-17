using Il2Cpp;
using System.Collections;
using System.Reflection;
using UnityEngine;

namespace MapDLLInjector
{
    public class NonMonoBehaviourHelper : MonoBehaviour
    {
        public static NonMonoBehaviourHelper Instance { get; private set; }

        public UGCSceneSO SceneInstance { get; set; }

        void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(this.gameObject);
            }
            else
            {
                Destroy(this.gameObject);
            }
        }

    }

}
