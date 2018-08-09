using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Camus.Validators
{
	public static class NotNullPostProcess
	{
		[PostProcessScene]
		public static void OnPostProcessScene()
		{
			var checkPassed = true;
			var gameObjects = GetAllGameObjectsFromScene();
			checkPassed = Check(gameObjects, checkPassed);

			if (!checkPassed)
			{
				var scene = SceneManager.GetActiveScene();
				throw new Exception(string.Format("Null Check Failed! Scene: {0}", scene.name));
			}
		}

		[PostProcessBuild]
		public static void OnPostprocessBuild(BuildTarget target, string pathToBuiltProject)
		{
			var checkPassed = true;
			var allAssets = AssetDatabase.GetAllAssetPaths().Where(path => path.StartsWith("Assets/")).ToArray();
			var prefabs = allAssets.Select(a => AssetDatabase.LoadAssetAtPath(a, typeof(GameObject)) as GameObject).Where(a => a != null).ToArray();

			foreach (var prefab in prefabs)
			{
				var gameObjects = GetAllGameObjectsFromPrefab(prefab);
				checkPassed = Check(gameObjects, checkPassed);
			}

			if (!checkPassed)
			{
				throw new Exception(string.Format("Null Check Failed!"));
			}
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

		private static bool Check(IList<GameObject> gameObjects, bool checkPassed)
		{
            foreach (var gameObject in gameObjects)
            {
				var components = GetAllComponents(gameObject);
                foreach (var component in components)
				{
                    checkPassed &= HasNullField(component, gameObject);
                }
            }

			return checkPassed;
		}

        private static List<Component> GetAllComponents(GameObject gameObject)
        {
            return gameObject.GetComponents<Component>().ToList();
        }
      
        private static bool HasNullField(Component component, GameObject instance)
        {
            var result = true;
            var type = component.GetType();
            var fieldInfos = type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            foreach (var fieldInfo in fieldInfos)
            {
                var attributes = fieldInfo.GetCustomAttributes(typeof(NotNullAttribute), false);
                if (attributes.Any())
                {
                    var fieldValue = fieldInfo.GetValue(component);
                    if (fieldValue.Equals(null))
                    {
                        Debug.LogError(string.Format("{1} is Null in {0}", instance.name, fieldInfo.Name), instance);
                        result = false;
                    }
                }
            }

            return result;
        }
    }
}
