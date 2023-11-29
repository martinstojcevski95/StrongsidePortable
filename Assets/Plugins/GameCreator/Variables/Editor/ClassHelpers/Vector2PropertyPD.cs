namespace GameCreator.Variables
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
        #if UNITY_EDITOR
	using UnityEditor;
    #endif

    [CustomPropertyDrawer(typeof(Vector2Property))]
    public class Vector2PropertyPD : BasePropertyPD
    {
        protected override int GetAllowTypesMask()
        {
            return 1 << (int)Variable.DataType.Vector2;
        }
    }
}