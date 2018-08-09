using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Camus.Validators
{
	public static class NotNullPostProcess
	{      
		[PostProcessBuild]
		public static void OnPostprocessBuild(BuildTarget target, string pathToBuiltProject)
		{
            var checkPassed = true;
			checkPassed |= CheckAllScenes();
			checkPassed |= CheckAllPrefabs();

            if (!checkPassed)
            {
				throw new Exception(string.Format("[#] Null Check Failed!"));
            }
		}

        private static bool CheckAllScenes()
		{
            var checkPassed = true;
			foreach (var scene in EditorBuildSettings.scenes.Where(s => s.enabled))
			{
				EditorSceneManager.OpenScene(scene.path);
				var gameObjects = GetAllGameObjectsFromScene();            
				if (!CheckGameObjects(gameObjects, checkPassed))
                {
					checkPassed = false;
					Debug.LogError(string.Format("[*] Null Check Failed! Scene: {0}", scene.path));
                }
            }

			return checkPassed;
		}

        private static bool CheckAllPrefabs()
		{
            var checkPassed = true;
            var allAssets = AssetDatabase.GetAllAssetPaths().Where(path => path.StartsWith("Assets/")).ToArray();
            var prefabs = allAssets.Select(a => AssetDatabase.LoadAssetAtPath(a, typeof(GameObject)) as GameObject).Where(a => a != null).ToArray();

            foreach (var prefab in prefabs)
            {
                var gameObjects = GetAllGameObjectsFromPrefab(prefab);
				if(!CheckGameObjects(gameObjects, checkPassed))
				{
					checkPassed = false;
					Debug.LogError(string.Format("[*] Null Check Failed! Prefab: {0}", prefab.name));
				}
            }

            return checkPassed;
		}

		private static IList<GameObject> GetAllGameObjectsFromScene()
		{
			var scene = SceneManager.GetActiveScene();
			var rootGameObjects = scene.GetRootGameObjects();
			var gameObjects = new List<GameObject>();
			foreach (var rootGameObject in rootGameObjects)
			{
				gameObjects.Add(rootGameObject);
				var subGameObjects = GetAllChildren(rootGameObject);
				gameObjects.AddRange(subGameObjects);
			}

			return gameObjects;
		}

		private static IList<GameObject> GetAllGameObjectsFromPrefab(GameObject prefab)
		{
			var gameObjects = new List<GameObject>();
			gameObjects.Add(prefab);
			var subGameObjects = GetAllChildren(prefab);
			gameObjects.AddRange(subGameObjects);

			return gameObjects;
		}

		private static List<GameObject> GetAllChildren(GameObject root)
		{
			var gameObjects = new List<GameObject>();
			var count = root.transform.childCount;
			for (int i = 0; i < count; ++i)
			{
				var child = root.transform.GetChild(i);
				gameObjects.Add(child.gameObject);

				var subGameObjects = GetAllChildren(child.gameObject);
				gameObjects.AddRange(subGameObjects);
			}

			return gameObjects;
		}

		private static bool CheckGameObjects(IList<GameObject> gameObjects, bool checkPassed)
		{
            foreach (var gameObject in gameObjects)
            {
				var components = GetAllComponents(gameObject);
                foreach (var component in components)
				{
					checkPassed &= CheckNullValueOrMissingReference(component, gameObject);
                }
            }

			return checkPassed;
		}

        private static List<Component> GetAllComponents(GameObject gameObject)
        {
            return gameObject.GetComponents<Component>().ToList();
        }
      
		private static bool CheckNullValueOrMissingReference(Component component, GameObject instance)
        {
            var result = true;
			var serialObject = new SerializedObject(component);
            var type = component.GetType();
            var fieldInfos = type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            foreach (var fieldInfo in fieldInfos)
            {
				var fieldValue = fieldInfo.GetValue(component);

				var serialProperty = serialObject.FindProperty(fieldInfo.Name);
				if (serialProperty.propertyType == SerializedPropertyType.ObjectReference)
                {
					if (fieldValue.Equals(null) && serialProperty.objectReferenceInstanceIDValue != 0)
                    {
						Debug.LogError(string.Format("[Miss] {1} in {0}", instance.name, fieldInfo.Name), instance);
                        result = false;
                    }
                }

                var attributes = fieldInfo.GetCustomAttributes(typeof(NotNullAttribute), false);
                if (attributes.Any())
                {
                    if (fieldValue.Equals(null))
                    {
						Debug.LogError(string.Format("[Null] {1} in {0}", instance.name, fieldInfo.Name), instance);
                        result = false;
                    }
				}
            }

            return result;
        }
    }
}
