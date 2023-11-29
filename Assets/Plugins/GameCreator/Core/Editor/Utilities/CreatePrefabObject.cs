namespace GameCreator.Core
{
	using System.Collections;
	using System.Collections.Generic;
	using UnityEngine;
	 #if UNITY_EDITOR 
 using UnityEditor;
 #endif 

	public abstract class CreatePrefabObject
	{
		public static T AddGameObjectToPrefab<T>(GameObject prefabRoot, string name) where T : MonoBehaviour
		{
            GameObject instanceChild = new GameObject(name);
            T componentChild = instanceChild.AddComponent<T>();

            instanceChild.transform.SetParent(prefabRoot.transform);
            PrefabUtility.SavePrefabAsset(prefabRoot);
            return componentChild;
        }

		public static void RemoveGameObjectFromPrefab(GameObject prefabRoot, GameObject prefabChild)
		{
            Object.DestroyImmediate(prefabChild);
            PrefabUtility.SavePrefabAsset(prefabRoot);
		}
	}
}