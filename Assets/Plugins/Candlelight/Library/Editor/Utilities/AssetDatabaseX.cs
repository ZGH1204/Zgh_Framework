// 
// AssetDatabaseX.cs
// 
// Copyright (c) 2012-2016, Candlelight Interactive, LLC
// All rights reserved.
// 
// Redistribution and use in source and binary forms, with or without
// modification, are permitted provided that the following conditions are met:
// 
// 1. Redistributions of source code must retain the above copyright notice,
// this list of conditions and the following disclaimer.
// 
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS"
// AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE
// IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE
// ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE
// LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR
// CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF
// SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS
// INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN
// CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE)
// ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE
// POSSIBILITY OF SUCH DAMAGE.
// 
// This file contains a class with static methods for working with the asset
// database.

#if UNITY_4_6 || UNITY_4_7 || UNITY_5_0 || UNITY_5_1 || UNITY_5_2
#define NO_SCENE_MANAGER
#endif

using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace Candlelight
{
	/// <summary>
	/// A utility class for working with the asset database.
	/// </summary>
	public static class AssetDatabaseX
	{
		/// <summary>
		/// A predicate for determining whether a specified asset at a specified path satisfies a criterion.
		/// </summary>
		/// <remarks>The asset path may be the path to a scene if the asset is a non-prefab in a scene.</remarks>
		public delegate bool AssetMatchPredicate<T>(T asset, string assetPath) where T : Object;
		/// <summary>
		/// A callback for modifying a specified asset at a specified path.
		/// </summary>
		/// <remarks>The asset path may be the path to a scene if the asset is a non-prefab in a scene.</remarks>
		public delegate void ModifyAssetCallback<T>(T asset, string assetPath) where T : Object;

		/// <summary>
		/// Adds and loads the asset.
		/// </summary>
		/// <returns>The new asset, imported from the asset database.</returns>
		/// <param name="asset">Asset to add to the existing path.</param>
		/// <param name="path">A project-relative path to an asset in the form "Assets/MyTextures/hello.png".</param>
		/// <typeparam name="T">The asset's type.</typeparam>
		public static T AddAndLoadAsset<T>(T asset, string path) where T: Object
		{
			CreateFolderIfNecessary(Path.GetDirectoryName(path));
			AssetDatabase.AddObjectToAsset(asset, path);
			AssetDatabase.ImportAsset(path);
			return LoadAssetAtPath<T>(path);
		}
		
		/// <summary>
		/// Creates and loads the asset.
		/// </summary>
		/// <returns>The new asset, imported from the asset database.</returns>
		/// <param name="asset">Asset to add to the database.</param>
		/// <param name="path">A project-relative path to an asset in the form "Assets/MyTextures/hello.png".</param>
		/// <typeparam name="T">The asset's type.</typeparam>
		public static T CreateAndLoadAsset<T>(T asset, string path) where T: Object
		{
			return CreateAndLoadAsset(asset as Object, path) as T;
		}

		/// <summary>
		/// Creates and loads the asset.
		/// </summary>
		/// <returns>The new asset, imported from the asset database.</returns>
		/// <param name="asset">Asset to add to the database.</param>
		/// <param name="path">A project-relative path to an asset in the form "Assets/MyTextures/hello.png".</param>
		public static Object CreateAndLoadAsset(Object asset, string path)
		{
			CreateFolderIfNecessary(Path.GetDirectoryName(path));
			path = AssetDatabase.GenerateUniqueAssetPath(path);
			AssetDatabase.CreateAsset(asset, path);
			AssetDatabase.ImportAsset(path);
			return AssetDatabase.LoadAssetAtPath(path, asset.GetType());
		}
		
		/// <summary>
		/// Creates the specified folder if it does not exist.
		/// </summary>
		/// <param name="folder">A project-relative path to a folder in the form "Assets/MyTextures".</param>
		public static void CreateFolderIfNecessary(string folder)
		{
			string parentFolder = "";
			string[] folders = new Regex("[\\/]").Split(folder);
			string fullPath = Directory.GetParent(Application.dataPath).FullName;
			foreach (string f in folders)
			{
				fullPath = Path.Combine(fullPath, f);
				if (!Directory.Exists(fullPath))
				{
					AssetDatabase.CreateFolder(parentFolder, f);
				}
				parentFolder = Path.Combine(parentFolder, f);
			}
		}
		
		/// <summary>
		/// Creates a new scriptable object asset in the current project folder. Use this method when adding menu items
		/// to Assets/Create.
		/// </summary>
		/// <returns>The new scriptable object asset, imported from the asset database.</returns>
		/// <typeparam name="T">The asset's type.</typeparam>
		public static T CreateNewAssetInCurrentProjectFolder<T>() where T: ScriptableObject
		{
			T newAsset = ScriptableObject.CreateInstance<T>();
			newAsset =
				CreateNewAssetInCurrentProjectFolder<T>(newAsset, string.Format("{0}.asset", typeof(T).Name.ToWords()));
			Selection.activeObject = newAsset;
			return newAsset;
		}

		/// <summary>
		/// Creates a new scriptable object asset in a user-specified path.
		/// </summary>
		/// <returns>The new scriptable object asset, imported from the asset database.</returns>
		/// <param name="title">Title.</param>
		/// <param name="defaultName">Default name.</param>
		/// <typeparam name="T">The 1st type parameter.</typeparam>
		public static T CreateNewAssetInUserSpecifiedPath<T>(
			string title = null, string defaultName = null
		) where T: ScriptableObject
		{
			return CreateNewAssetInUserSpecifiedPath(typeof(T), title, defaultName) as T;
		}

		/// <summary>
		/// Creates a new scriptable object asset in a user-specified path.
		/// </summary>
		/// <returns>The new scriptable object asset, imported from the asset database.</returns>
		/// <param name="scriptableObjectType">A type assignable from UnityEngine.ScriptableObject.</param>
		/// <param name="title">Title.</param>
		/// <param name="defaultName">Default name.</param>
		public static ScriptableObject CreateNewAssetInUserSpecifiedPath(
			System.Type scriptableObjectType, string title = null, string defaultName = null
		)
		{
			if (!typeof(ScriptableObject).IsAssignableFrom(scriptableObjectType))
			{
				Debug.LogException(
					new System.ArgumentException(
						string.Format("Type must inherit from {0}.", typeof(ScriptableObject).FullName),
						"scriptableObjectType"
					)
				);
				return null;
			}
			string typeWords = scriptableObjectType.Name.ToWords();
			title = title ?? string.Format("Create new {0}", typeWords);
			defaultName = defaultName ?? string.Format("{0}.asset", typeWords);
			string pathToNewAsset = EditorUtility.SaveFilePanelInProject(
				title, defaultName, "asset", string.Format("Please enter a file name for the new {0}", typeWords)
			);
			return string.IsNullOrEmpty(pathToNewAsset) ?
				null : CreateAndLoadAsset(ScriptableObject.CreateInstance(scriptableObjectType), pathToNewAsset);
		}

		/// <summary>
		/// Creates the new asset in the current project folder. Use this method when adding menu items to
		/// Assets/Create.
		/// </summary>
		/// <returns>The new asset, imported from the asset database.</returns>
		/// <param name="newAsset">New asset.</param>
		/// <param name="newAssetFileName">New asset file name, such as "new asset.asset".</param>
		/// <typeparam name="T">The asset's type.</typeparam>
		public static T CreateNewAssetInCurrentProjectFolder<T>(T newAsset, string newAssetFileName) where T: Object
		{
			string folderName = "Assets";
			if (
				Selection.activeObject != null &&
				(AssetDatabase.IsMainAsset(Selection.activeObject) || AssetDatabase.IsSubAsset(Selection.activeObject))
			)
			{
				folderName = AssetDatabase.GetAssetPath(Selection.activeObject);
				folderName = Directory.Exists(folderName) ?
					folderName : Path.GetDirectoryName(folderName);
			}
			return CreateAndLoadAsset<T>(newAsset, Path.Combine(folderName, newAssetFileName));
		}

		/// <summary>
		/// Tests whether the asset has one of the possible required file extensions.
		/// </summary>
		/// <returns>
		/// <see langword="true"/> if the asset has a possible required file extension;
		/// otherwise, <see langword="false"/>.
		/// </returns>
		/// <param name="assetPath">Asset path.</param>
		/// <param name="possibleExtensions">Possible extensions in the form ".png".</param>
		public static bool DoesAssetHaveRequiredFileExtension(
			string assetPath, ReadOnlyCollection<string> possibleExtensions
		)
		{
			string cmp = Path.GetExtension(assetPath).ToLower();
			foreach (string ext in possibleExtensions)
			{
				if (cmp == ext.ToLower())
				{
					return true;
				}
			}
			return false;
		}

		/// <summary>
		/// Finds all assets in the project of the specified type.
		/// </summary>
		/// <param name="assets">The asset and its path for each match found in the project.</param>
		/// <param name="isMatch">Optional predicate to determine whether the found asset should be included.</param>
		/// <typeparam name="T">A <see cref="UnityEngine.Object"/> type.</typeparam>
		public static void FindAllAssets<T>(Dictionary<T, string> assets, AssetMatchPredicate<T> isMatch = null)
			where T : Object
		{
			assets.Clear();
			AssetDatabase.SaveAssets();
			bool hasMatchPredicate = isMatch != null;
			foreach (string guid in AssetDatabase.FindAssets(string.Format("t:{0}", typeof(T).Name)))
			{
				string assetPath = AssetDatabase.GUIDToAssetPath(guid);
				T asset = AssetDatabase.LoadAssetAtPath(assetPath, typeof(T)) as T;
				if (asset != null && (!hasMatchPredicate || isMatch(asset, assetPath)))
				{
					assets.Add(asset, assetPath);
				}
			}
		}

		/// <summary>
		/// Finds all assets in the project with the specified component type in their hierarchies.
		/// </summary>
		/// <param name="components">
		/// For each asset in the project where <typeparamref name="T"/> were found, all <typeparamref name="T"> found
		/// in the asset's hierarchy as well as its path.
		/// </param>
		/// <typeparam name="T">A <see cref="UnityEngine.Component"/> type.</typeparam>
		public static void FindAllAssetsWithComponent<T>(ref Dictionary<List<T>, string> components) where T : Component
		{
			Dictionary<List<T>, string> internalComponents = new Dictionary<List<T>, string>();
			Dictionary<GameObject, string> gameObjects = new Dictionary<GameObject, string>();
			AssetMatchPredicate<GameObject> hasComponents = delegate(GameObject asset, string assetPath)
			{
				if (asset == null)
				{
					return false;
				}
				List<T> comps = new List<T>();
				asset.GetComponentsInChildren(true, comps);
				if (comps.Count > 0)
				{
					internalComponents.Add(comps, assetPath);
					return true;
				}
				return false;
			};
			FindAllAssets(gameObjects, hasComponents);
			components = components ?? new Dictionary<List<T>, string>();
			components.Clear();
			foreach (KeyValuePair<List<T>, string> kv in internalComponents)
			{
				components.Add(kv.Key, kv.Value);
			}
		}

		/// <summary>
		/// Gets the folder containing the script that defines the specified <see cref="System.Type"/>.
		/// </summary>
		/// <returns>
		/// The folder containing the script that defines the specified <see cref="System.Type"/>. if it could be found;
		/// otherwise, <see langword="null"/>.
		/// </returns>
		/// <param name="folderToSearch">
		/// Optional folder to search in the asset database. Otherwise, entire asset database will be searched.
		/// </param>
		/// <typeparam name="T">The <see cref="System.Type"/> whose script is being sought.</typeparam>
		public static string GetFolderContainingScript<T>(string folderToSearch = "Assets")
		{
			return GetFolderContainingScript(typeof(T), folderToSearch);
		}

		/// <summary>
		/// Gets the folder containing the script that defines the specified <see cref="System.Type"/>.
		/// </summary>
		/// <returns>
		/// The folder containing the script that defines the specified <see cref="System.Type"/>. if it could be found;
		/// otherwise, <see langword="null"/>.
		/// </returns>
		/// <param name="type">The <see cref="System.Type"/> whose script is being sought.</param>
		/// <param name="folderToSearch">
		/// Optional folder to search in the asset database. Otherwise, entire asset database will be searched.
		/// </param>
		public static string GetFolderContainingScript(System.Type type, string folderToSearch = "Assets")
		{
			folderToSearch = string.IsNullOrEmpty(folderToSearch) ? "Assets" : folderToSearch;
			MonoScript script = null;
			if (!Directory.Exists(folderToSearch))
			{
				return null;
			}
			foreach (string filePath in Directory.GetFiles(folderToSearch, "*.cs", SearchOption.AllDirectories))
			{
				script = AssetDatabase.LoadAssetAtPath(filePath, typeof(MonoScript)) as MonoScript;
				if (script != null && script.GetClass() == type)
				{
					return Path.GetDirectoryName(filePath);
				}
			}
			return null;
		}

		/// <summary>
		/// Gets the name of the folder containing the specified asset.
		/// </summary>
		/// <returns>
		/// The name of the folder containing the specified asset. For instance, "Asssets/MyTextures/hello.png" returns
		/// "MyTextures".
		/// </returns>
		/// <param name="assetPath">
		/// A project-relative path to an asset in the form "Assets/MyTextures/hello.png".
		/// </param>
		public static string GetFolderName(string assetPath)
		{
			return Path.GetFileNameWithoutExtension(Path.GetDirectoryName(assetPath));
		}
		
		/// <summary>
		/// Determines if an asset with the specified path is in a folder with the specified name.
		/// </summary>
		/// <returns>
		/// <see langword="true"/> if the asset at the specified path is in a folder with folderName;
		/// otherwise, <see langword="false"/>.
		/// </returns>
		/// <param name="assetPath">
		/// A project-relative path to an asset in the form "Assets/MyTextures/hello.png".
		/// </param>
		/// <param name="folderName">
		/// The name of the folder in which the asset is expected. For instance, "Assets/MyTextures/hello.png" would be
		/// "MyTextures".
		/// </param>
		public static bool IsAssetPathInFolderWithName(string assetPath, string folderName)
		{
			return GetFolderName(assetPath) == folderName;
		}

		/// <summary>
		/// Loads the asset at path with the specified type.
		/// </summary>
		/// <returns>The asset at path with the specified type.</returns>
		/// <param name="assetPath">
		/// A project-relative path to an asset in the form "Assets/MyTextures/hello.png".
		/// </param>
		/// <typeparam name="T">The asset's type.</typeparam>
		public static T LoadAssetAtPath<T>(string assetPath) where T: Object
		{
			return AssetDatabase.LoadAssetAtPath(assetPath, typeof(T)) as T;
		}

		/// <summary>
		/// Performs the specified modification on all assets in the project of the specified type.
		/// </summary>
		/// <param name="assets">The assets to modify and their respective paths.</param>
		/// <param name="onModifyAsset">The callback to invoke for each asset found.</param>
		/// <param name="undoMessage">Undo message to use.</param>
		/// <typeparam name="T">A <see cref="UnityEngine.Object"/> type.</typeparam>
		private static void ModifyAllAssetsInProject<T>(
			Dictionary<T, string> assets, ModifyAssetCallback<T> onModifyAsset, string undoMessage
		) where T : Object
		{
			int undoGroup = Undo.GetCurrentGroup();
			undoMessage = string.IsNullOrEmpty(undoMessage) ?
				string.Format("Modify All {0} in Project", typeof(T)) : undoMessage;
			int assetIndex = 0;
			string formatString =
				typeof(GameObject).IsAssignableFrom(typeof(T)) || typeof(Component).IsAssignableFrom(typeof(T)) ?
					"Processing Prefab {0} / {1}" : "Processing Asset {0} / {1}";
			foreach (KeyValuePair<T, string> kv in assets)
			{
				string message = string.Format(formatString, assetIndex, assets.Count);
				EditorUtility.DisplayProgressBar(message, message, (float)assetIndex / assets.Count);
#if !UNITY_4_6 && !UNITY_4_7
				Undo.SetCurrentGroupName(undoMessage);
#endif
				onModifyAsset(kv.Key, kv.Value);
				++assetIndex;
			}
			Undo.CollapseUndoOperations(undoGroup);
			EditorUtility.ClearProgressBar();
			AssetDatabase.SaveAssets();
			if (
				!EditorUtility.DisplayDialog(
					"Search Scenes?",
					"Prefab hierarchies have been modified.\n\n" +
					string.Format(
						"Do you also wish to modify {0} in scenes that are not in prefab hierarchies? " +
						"(This operation is not undoable and will clear your undo queue.)", typeof(T).Name
					), "Yes", "No"
				)
			)
			{
				return;
			}
			List<string> scenePaths = new List<string>(
				from guid in AssetDatabase.FindAssets("t:Scene") select AssetDatabase.GUIDToAssetPath(guid)
			);
			scenePaths.RemoveAll(p => !System.IO.File.Exists(p));
			for (int i = 0; i < scenePaths.Count; ++i)
			{
				string scenePath = scenePaths[i];
				string message = string.Format("Processing Scene {0} / {1}", i, scenePaths.Count);
				EditorUtility.DisplayProgressBar(message, message, (float)i / scenePaths.Count);
#if NO_SCENE_MANAGER
				EditorApplication.OpenScene(scenePath);
#else
				UnityEditor.SceneManagement.EditorSceneManager.OpenScene(
					scenePath, UnityEditor.SceneManagement.OpenSceneMode.Single
				);
#endif
				HashSet<T> allObjects = new HashSet<T>();
				if (!typeof(Component).IsAssignableFrom(typeof(T)))
				{
					foreach (T obj in Object.FindObjectsOfType<T>())
					{
						allObjects.Add(obj);
					}
				}
				else
				{
					foreach (GameObject go in Object.FindObjectsOfType<GameObject>())
					{
						foreach (Component comp in go.GetComponentsInChildren(typeof(T), true))
						{
							allObjects.Add(comp as T);
						}
					}
				}
				undoGroup = Undo.GetCurrentGroup();
				foreach (T obj in allObjects)
				{
					onModifyAsset(obj, scenePaths[i]);
				}
				Undo.CollapseUndoOperations(undoGroup);
				if (allObjects.Count > 0)
				{
#if NO_SCENE_MANAGER
					EditorApplication.SaveScene();
#else
					UnityEditor.SceneManagement.EditorSceneManager.SaveOpenScenes();
#endif
				}
			}
			EditorUtility.ClearProgressBar();
#if NO_SCENE_MANAGER
			EditorApplication.NewScene();
#else
			UnityEditor.SceneManagement.EditorSceneManager.NewScene(
				UnityEditor.SceneManagement.NewSceneSetup.DefaultGameObjects,
				UnityEditor.SceneManagement.NewSceneMode.Single
			);
#endif
		}

		/// <summary>
		/// Performs the specified modification on all assets in the project of the specified type.
		/// </summary>
		/// <param name="onModifyAsset">The callback to invoke for each asset found.</param>
		/// <param name="undoMessage">Optional undo message to use.</param>
		/// <param name="isMatch">Optional predicate to determine whether the found asset should be included.</param>
		/// <typeparam name="T">A <see cref="UnityEngine.Object"/> type.</typeparam>
		public static void ModifyAllAssetsInProject<T>(
			ModifyAssetCallback<T> onModifyAsset, string undoMessage = null, AssetMatchPredicate<T> isMatch = null
		) where T : Object
		{
			Dictionary<T, string> assets = null;
			FindAllAssets(assets, isMatch);
			ModifyAllAssetsInProject(assets, onModifyAsset, undoMessage);
		}

		/// <summary>
		/// Performs the specified modification on all components of the specified type on assets in the project.
		/// </summary>
		/// <param name="onModifyComponent">The callback to invoke for each component found.</param>
		/// <param name="undoMessage">Optional undo message to use.</param>
		/// <typeparam name="T">A <see cref="UnityEngine.Component"/> type.</typeparam>
		public static void ModifyAllComponentsInProject<T>(
			ModifyAssetCallback<T> onModifyComponent, string undoMessage = null
		) where T : Component
		{
			Dictionary<List<T>, string> assetsWithComponent = null;
			FindAllAssetsWithComponent(ref assetsWithComponent);
			Dictionary<T, string> assets = new Dictionary<T, string>();
			foreach (KeyValuePair<List<T>, string> comps in assetsWithComponent)
			{
				foreach (T comp in comps.Key)
				{
					assets[comp] = comps.Value;
				}
			}
			ModifyAllAssetsInProject(assets, onModifyComponent, undoMessage);
		}

		/// <summary>
		/// Prints the selected objects' asset paths and types.
		/// </summary>
		[MenuItem("Assets/Candlelight/Print Asset Path")]
		private static void PrintSelected()
		{
			foreach (Object obj in Selection.objects)
			{
				if (AssetDatabase.Contains(obj))
				{
					Debug.Log(string.Format("{0} ({1})", AssetDatabase.GetAssetPath(obj), obj.GetType()));
				}
				else
				{
					Debug.LogWarning(string.Format("{0} is not a source asset.", obj));
				}
			}
		}
	}
}