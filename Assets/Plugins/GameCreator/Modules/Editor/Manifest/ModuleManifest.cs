namespace GameCreator.ModuleManager
{
    using UnityEngine;
     #if UNITY_EDITOR 
 using UnityEditor;
 #endif 

    [System.Serializable]
    public class ModuleManifest
    {
        // PROPERTIES: ----------------------------------------------------------------------------

        public Module module;

        // INITIALIZERS: --------------------------------------------------------------------------

        public ModuleManifest()
        {
            this.module = new Module();
        }

        public ModuleManifest(Module module)
        {
            this.module = module;
        }
    }
}