namespace GameCreator.Variables
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
        #if UNITY_EDITOR
	using UnityEditor;
    #endif

    [CustomPropertyDrawer(typeof(Vector3Property))]
    public class Vector3PropertyPD : BasePropertyPD
    {
        protected override int GetAllowTypesMask()
        {
            return 1 << (int)Variable.DataType.Vector3;
        }
    }
}