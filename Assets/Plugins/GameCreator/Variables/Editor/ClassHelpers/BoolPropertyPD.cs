namespace GameCreator.Variables
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
     #if UNITY_EDITOR 
 using UnityEditor;
 #endif 

    [CustomPropertyDrawer(typeof(BoolProperty))]
    public class BoolPropertyPD : BasePropertyPD
    {
        protected override int GetAllowTypesMask()
        {
            return 1 << (int)Variable.DataType.Bool;
        }
    }
}