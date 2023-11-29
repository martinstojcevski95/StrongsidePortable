namespace GameCreator.ModuleManager
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    	 #if UNITY_EDITOR 
 using UnityEditor;
 #endif 

    [System.Serializable]
    public class Dependency
    {
        public static Dependency NONE
        {
            get
            {
                return new Dependency("", Version.NONE);
            }
        }

        // PROPERTIES: ----------------------------------------------------------------------------

        public string moduleID;
        public Version version;

        // INITIALIZERS: --------------------------------------------------------------------------

        public Dependency(string name, Version version)
        {
            this.moduleID = name;
            this.version = version;
        }
    }
}