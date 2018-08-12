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
    public static class ValidatorProcess
    {
        #region PostProcess

        [PostProcessBuild]
        public static void OnPostprocessBuild(BuildTarget target, string pathToBuiltProject)
        {
            var checkPassed = CheckAllEnabledScenes() & CheckAllPrefabs();
            if (!checkPassed)
            {
                throw new Exception(string.Format("[*] Null or Missing Reference check failed!"));
            }
        }

        #endregion

        #region Check
      
        private static bool CheckAllEnabledScenes()
        {
            var checkPassed = true;
            foreach (var scene in GetAllEnabledScenes())
            {
                EditorSceneManager.OpenScene(scene.path);
                Debug.Log(string.Format("[!] Start checking Scene: {0}", scene.path));
                var gameObjects = GetAllGameObjectsFromScene();
                if (!CheckGameObjects(gameObjects))
                {
                    checkPassed = false;
                    Debug.LogError(string.Format("[*] Null Check Failed! Scene: {0}", scene.path));
                }
                else
                {
                    Debug.Log(string.Format("[*] Null Check Success! Scene: {0}", scene.path));
                }
            }

            return checkPassed;
        }

        private static bool CheckAllPrefabs()
        {
            var checkPassed = true;
            foreach (var prefab in GetAllPrefabs())
            {
                Debug.Log(string.Format("[!] Start checking Prefab: {0}", prefab.name));
                var gameObjects = GetAllGameObjectsFromPrefab(prefab);
                if(!CheckGameObjects(gameObjects))
                {
                    checkPassed = false;
                    Debug.LogError(string.Format("[*] Null Check Failed! Prefab: {0}", prefab.name));
                }
                else
                {
                    Debug.Log(string.Format("[*] Null Check Success! Prefab: {0}", prefab.name));
                }
            }

            return checkPassed;
        }

        private static bool CheckGameObjects(IList<GameObject> gameObjects)
        {
            bool checkPassed = true;
            foreach (var gameObject in gameObjects)
            {
                foreach (var component in GetAllComponents(gameObject))
                {
                    if (!CheckNullValueOrMissingReference(component))
                    {
                        checkPassed = false;
                    }
                }
            }

            return checkPassed;
        }

        private static bool CheckNullValueOrMissingReference(Component component)
        {
            var result = true;
            var serialObject = new SerializedObject(component);
            var type = component.GetType();
            var fieldInfos = type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            foreach (var fieldInfo in fieldInfos)
            {
                var fieldValue = fieldInfo.GetValue(component);            
                if (IsMissingReference(serialObject, fieldInfo, fieldValue))
                {
                    Debug.LogError(string.Format("    [Miss] {0} in {1}", fieldInfo.Name, component.gameObject.name));
                    result = false;
                }

                if (IsNotNullValidationFailed(fieldInfo, fieldValue))
                {
                    Debug.LogError(string.Format("    [Null] {0} in {1}", fieldInfo.Name, component.gameObject.name));
                    result = false;
                }
            }

            return result;
        }

        private static bool IsMissingReference(SerializedObject serialObject, FieldInfo fieldInfo, object fieldValue)
        {
            var serialProperty = serialObject.FindProperty(fieldInfo.Name);
            if (serialProperty != null && serialProperty.propertyType == SerializedPropertyType.ObjectReference)
            {
                if (fieldValue.Equals(null) && serialProperty.objectReferenceInstanceIDValue != 0)
                {
                    return true;
                }
            }

            return false;
        }

        private static bool IsNotNullValidationFailed(FieldInfo fieldInfo, object fieldValue)
        {
            var attributes = fieldInfo.GetCustomAttributes(typeof(NotNullAttribute), false);
            if (attributes.Any())
            {
                if (fieldValue.Equals(null))
                {
                    return true;
                }
            }

            return false;
        }

        #endregion

        #region Get GameObjects and Components

        private static IEnumerable<EditorBuildSettingsScene> GetAllEnabledScenes()
        {
            return EditorBuildSettings.scenes.Where(s => s.enabled);
        }

        private static IEnumerable<GameObject> GetAllPrefabs()
        {
            return AssetDatabase.GetAllAssetPaths()
                .Where(p => p.StartsWith("Assets/", StringComparison.CurrentCulture))
                .Select(p => AssetDatabase.LoadAssetAtPath<GameObject>(p))
                .Where(a => a != null);
        }

        private static IEnumerable<Component> GetAllComponents(GameObject gameObject)
        {
            return gameObject.GetComponents<Component>().AsEnumerable();
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

        private static IList<GameObject> GetAllChildren(GameObject root)
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

        #endregion
    }
}
