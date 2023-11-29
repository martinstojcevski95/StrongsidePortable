namespace GameCreator.Variables
{
    using System.Collections;
    using System.Collections.Generic;
	using UnityEngine;
    #if UNITY_EDITOR
	using UnityEditor;
    #endif

    [CustomPropertyDrawer(typeof(Texture2DProperty))]
    public class Texture2DPropertyPD : BasePropertyPD
    {
        protected override int GetAllowTypesMask()
        {
            return 1 << (int)Variable.DataType.Texture2D;
        }
    }
}