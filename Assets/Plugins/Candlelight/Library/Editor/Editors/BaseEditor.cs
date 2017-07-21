// 
// BaseEditor.cs
// 
// Copyright (c) 2015-2016, Candlelight Interactive, LLC
// All rights reserved.
// 
// This file is licensed according to the terms of the Unity Asset Store EULA:
// http://download.unity3d.com/assetstore/customer-eula.pdf

using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Candlelight
{
	/// <summary>
	/// A utility class to register all <see cref="BaseEditor"/> classes in the editor preferences menu.
	/// </summary>
	[InitializeOnLoad]
	public static class BaseEditorUtility
	{
		/// <summary>
		/// Initializes the <see cref="BaseEditorUtility"/> class.
		/// </summary>
		static BaseEditorUtility()
		{
			foreach (System.Type type in ReflectionX.AllTypes)
			{
				if (!type.IsAbstract && typeof(BaseEditor).IsAssignableFrom(type))
				{
					MethodInfo initializeMethod = type.GetStaticMethod("InitializeClass");
					if (initializeMethod != null)
					{
						initializeMethod.Invoke(null, null);
					}
					PropertyInfo featureGroupProp = type.GetStaticProperty("ProductCategory");
					if (featureGroupProp == null)
					{
						continue;
					}
					AssetStoreProduct product = (AssetStoreProduct)featureGroupProp.GetValue(null, null);
					if (product == AssetStoreProduct.None)
					{
						continue;
					}
					MethodInfo prefMenuMethod = type.GetStaticMethod("DisplayHandlePreferences");
					if (prefMenuMethod == null || prefMenuMethod.DeclaringType != type)
					{
						continue;
					}
					EditorPreferenceMenu.AddPreferenceMenuItem(product, prefMenuMethod);
				}
			}
		}
	}

	/// <summary>
	/// Base editor class for objects to register preferences and scene GUI callbacks.
	/// </summary>
	[InitializeOnLoad]
	public abstract class BaseEditor : Editor, ISceneGUIContext
	{
		/// <summary>
		/// Initializes the <see cref="BaseEditor"/> class.
		/// </summary>
		static BaseEditor()
		{
			List<SerializedPropertyAttribute> attrs = new List<SerializedPropertyAttribute>();
			Dictionary<FieldInfo, string> problemFields = new Dictionary<FieldInfo, string>();
			foreach (System.Type editorType in ReflectionX.AllTypes)
			{
				if (editorType == s_BaseType || !typeof(BaseEditor).IsAssignableFrom(editorType))
				{
					continue;
				}
				System.Type targetType = GetBaseEditorTargetType(editorType);
				if (targetType == null)
				{
					continue;
				}
				s_SerializedPropertyFields[editorType] = new HashSet<FieldInfo>();
				foreach (
					FieldInfo editorField in editorType.GetFields(ReflectionX.instanceBindingFlags).Where(
						f => f.GetCustomAttributes(attrs) > 0
					)
				)
				{
					s_SerializedPropertyFields[editorType].Add(editorField);
					string targetFieldName;
					if (!attrs[0].GetPropertyPath(editorField, targetType, out targetFieldName))
					{
						problemFields.Add(editorField, string.Format("{0}.{1}", targetType, targetFieldName));
					}
				}
			}
			foreach (KeyValuePair<System.Type, HashSet<FieldInfo>> kv in s_SerializedPropertyFields)
			{
				System.Type t = kv.Key.BaseType;
				while (t != null && t.BaseType != s_BaseType)
				{
					System.Type lookupType = t;
					if (lookupType.IsGenericType)
					{
						lookupType = t.GetGenericTypeDefinition();
					}
					if (s_SerializedPropertyFields.ContainsKey(lookupType))
					{
						if (t.IsGenericType)
						{
							foreach (FieldInfo field in s_SerializedPropertyFields[lookupType])
							{
								kv.Value.Add(t.GetInstanceField(field.Name));
							}
						}
						else
						{
							foreach (FieldInfo field in s_SerializedPropertyFields[lookupType])
							{
								kv.Value.Add(field);
							}
						}
					}
					t = t.BaseType;
				}
			}
			if (problemFields.Count > 0)
			{
				Debug.LogError(
					string.Format(
						"The following fields are decorated with {0}, but no serializable field with the specified " +
						"path could be found on their target classes:\n\n{1}\n",
						typeof(SerializedPropertyAttribute),
						"\n".Join(
							from kv in problemFields select string.Format(
								" - {0}.{1} ({2})", kv.Key.DeclaringType, kv.Key.Name, kv.Value
							)
						)
					)
				);
			}
		}

		/// <summary>
		/// An attribute to decorate a serialized property or reorderable list field to have it automatically assigned
		/// in <see cref="BaseEditor.OnEnable()"/> 
		/// </summary>
		/// <remarks>
		/// This class is just a convenience for nested properties. For example, a property with the path "m_Prop.m_Int"
		/// could be indicated on a field with any name decorated with [SerializedProperty("m_Prop.m_Int")], or it could
		/// be indicated on a field named "m_Int" decorated with [RelativeProperty("m_Prop")].
		/// </remarks>
		protected class RelativePropertyAttribute : SerializedPropertyAttribute
		{
			/// <summary>
			/// The parent path.
			/// </summary>
			private readonly string m_ParentPath = null;

			/// <summary>
			/// Initializes a new instance of the <see cref="BaseEditor.RelativePropertyAttribute"/> class.
			/// </summary>
			/// <param name="parentPath">Parent path for the property.</param>
			public RelativePropertyAttribute(string parentPath)
			{
				m_ParentPath = parentPath;
			}

			/// <summary>
			/// Gets the property path indicated on the decorated field.
			/// </summary>
			/// <returns>
			/// <see cref="true"/>, if <paramref name="propertyPath"/> refers to an actual serializable field;
			/// otherwise, <see langword="false"/>.
			/// </returns>
			/// <param name="decoratedField">The field decorated with this attribute.</param>
			/// <param name="targetType">Target type specified on the <see cref="BaseEditor{T}"/>.</param>
			/// <param name="propertyPath">Property path.</param>
			public override bool GetPropertyPath(
				FieldInfo decoratedField, System.Type targetType, out string propertyPath
			)
			{
				return GetPropertyPath(decoratedField, targetType, out propertyPath, m_ParentPath);
			}
		}

		/// <summary>
		/// An attribute to decorate a serialized property or reorderable list field to have it automatically assigned
		/// in <see cref="BaseEditor.OnEnable()"/> 
		/// </summary>
		protected class SerializedPropertyAttribute : System.Attribute
		{
			/// <summary>
			/// The path of the property. If none is specified, then the name of the decorated field will be used.
			/// </summary>
			private readonly string m_PropertyPath = null;

			/// <summary>
			/// Initializes a new instance of the <see cref="BaseEditor.SerializedPropertyAttribute"/> class. Use this
			/// constructor to decorate a field on a <see cref="BaseEditor{T}"/> whose name matches the path of the
			/// <see cref="UnityEditor.SerializedProperty"/> to which it refers on the target object.
			/// </summary>
			public SerializedPropertyAttribute() {}

			/// <summary>
			/// Initializes a new instance of the <see cref="BaseEditor.SerializedPropertyAttribute"/> class, allowing
			/// you to manually specify the property path if it is different from the name of the decorated field.
			/// </summary>
			/// <param name="propertyPath">Property path.</param>
			public SerializedPropertyAttribute(string propertyPath)
			{
				m_PropertyPath = propertyPath;
			}

			/// <summary>
			/// Gets the property path indicated on the decorated field.
			/// </summary>
			/// <returns>
			/// <see cref="true"/>, if <paramref name="propertyPath"/> refers to an actual serializable field;
			/// otherwise, <see langword="false"/>.
			/// </returns>
			/// <param name="decoratedField">The field decorated with this attribute.</param>
			/// <param name="targetType">Target type specified on the <see cref="BaseEditor{T}"/>.</param>
			/// <param name="propertyPath">Property path.</param>
			public virtual bool GetPropertyPath(
				FieldInfo decoratedField, System.Type targetType, out string propertyPath
			)
			{
				return GetPropertyPath(decoratedField, targetType, out propertyPath, null);
			}

			/// <summary>
			/// Gets the property path indicated on the decorated field.
			/// </summary>
			/// <returns>
			/// <see cref="true"/>, if <paramref name="propertyPath"/> refers to an actual serializable field;
			/// otherwise, <see langword="false"/>.
			/// </returns>
			/// <param name="decoratedField">The field decorated with this attribute.</param>
			/// <param name="targetType">Target type specified on the <see cref="BaseEditor{T}"/>.</param>
			/// <param name="propertyPath">Property path.</param>
			/// <param name="parentPath">A parent path to prefix, if any.</param>
			protected bool GetPropertyPath(
				FieldInfo decoratedField, System.Type targetType, out string propertyPath, string parentPath
			)
			{
				propertyPath = string.Format(
					"{0}{1}",
					string.IsNullOrEmpty(parentPath) ?
						null : (parentPath.StartsWith(".") ? parentPath : string.Format("{0}.", parentPath)),
					string.IsNullOrEmpty(m_PropertyPath) ? decoratedField.Name : m_PropertyPath
				);
				return SerializedPropertyX.HasSerializedProperty(targetType, propertyPath);
			}
		}

		#region Labels
		private static readonly GUIContent[] s_SingleButtonLabel = new GUIContent[1];
		#endregion

		/// <summary>
		/// The base type for editors with known targets.
		/// </summary>
		private static readonly System.Type s_BaseType = typeof(BaseEditor<>);
		/// <summary>
		/// For each <see cref="BaseEditor{T}"/> type, a list of the fields decorated with
		/// <see cref="BaseEditor.SerializedPropertyAttribute"/>.
		/// </summary>
		private static readonly Dictionary<System.Type, HashSet<FieldInfo>> s_SerializedPropertyFields =
			new Dictionary<System.Type, HashSet<FieldInfo>>();
		/// <summary>
		/// A control identifier for a single button.
		/// </summary>
		private static int[] s_SingleButtonControlID = new int[1];

		/// <summary>
		/// Gets the product category. Replace this property in a subclass to specify a location in the preference menu.
		/// </summary>
		/// <value>The product category.</value>
		protected static AssetStoreProduct ProductCategory { get { return AssetStoreProduct.None; } }

		/// <summary>
		/// Displays an error message with an optional button to fix the error.
		/// </summary>
		/// <returns>
		/// <see langword="true"/> if the fix button was pressed; otherwise, <see langword="false"/>.
		/// </returns>
		/// <param name="errorMessage">Error message to display.</param>
		/// <param name="buttonText">Button text. If null or empty then no button will be displayed.</param>
		/// <param name="messageType">Message type.</param>
		/// <param name="controlId">
		/// Control identifier. If left unspecified, then the text of the label will be used to generate one.
		/// </param>
		protected static bool DisplayErrorMessageWithFixButton(
			string errorMessage, GUIContent buttonText, MessageType messageType = MessageType.Error, int controlId = 0
		)
		{
			s_SingleButtonControlID[0] = controlId;
			s_SingleButtonLabel[0] = buttonText;
			return DisplayErrorMessageWithFixButtons(
				errorMessage, s_SingleButtonLabel, messageType, s_SingleButtonControlID
			) == 0;
		}

		/// <summary>
		/// Displays an error message with optional buttons to fix errors.
		/// </summary>
		/// <returns>
		/// <see langword="true"/> if the fix button was pressed; otherwise, <see langword="false"/>.
		/// </returns>
		/// <param name="errorMessage">Error message to display.</param>
		/// <param name="buttonTexts">Button texts. If null or empty then no buttons will be displayed.</param>
		/// <param name="messageType">Message type.</param>
		/// <param name="controlIds">
		/// Control identifiers. If left unspecified, then the text of each label will be used to generate them.
		/// </param>
		protected static int DisplayErrorMessageWithFixButtons(
			string errorMessage,
			GUIContent[] buttonTexts,
			MessageType messageType = MessageType.Error,
			int[] controlIds = null
		)
		{
			int result = -1;
			Color oldColor = GUI.color;
			switch (messageType)
			{
			case MessageType.Error:
				GUI.color = Color.red;
				break;
			case MessageType.Warning:
				GUI.color = Color.yellow;
				break;
			}
			if (controlIds != null && controlIds.Length != buttonTexts.Length)
			{
				Debug.LogError(
					"Different number of button labels and control identifiers specified. " +
					"Specified control identifiers will be ignored."
				);
				controlIds = null;
			}
			EditorGUILayout.BeginVertical(EditorStylesX.Box);
			{
				GUI.color = oldColor;
				EditorGUILayout.HelpBox(errorMessage, messageType);
				if (buttonTexts != null)
				{
					for (int i = 0; i < buttonTexts.Length; ++i)
					{
						if (
							buttonTexts[i] != null &&
							EditorGUIX.DisplayButton(buttonTexts[i], controlId: controlIds == null ? 0 : controlIds[i])
						)
						{
							result = i;
						}
					}
				}
			}
			EditorGUILayout.EndVertical();
			return result;
		}

		/// <summary>
		/// Displays the handle preferences. They will be displayed in the preference menu and the top of the inspector.
		/// </summary>
		protected static void DisplayHandlePreferences()
		{

		}

		/// <summary>
		/// Gets the target type of the <see cref="BaseEditor{T}"/> type.
		/// </summary>
		/// <returns>The target type for the specified <paramref name="baseEditorType"/>.</returns>
		/// <param name="baseEditorType">A <see cref="BaseEditor{T}"/> type.</param>
		private static System.Type GetBaseEditorTargetType(System.Type baseEditorType)
		{
			while (baseEditorType != null && baseEditorType != typeof(object))
			{
				System.Type current =
					baseEditorType.IsGenericType ? baseEditorType.GetGenericTypeDefinition() : baseEditorType;
				if (s_BaseType == current)
				{
					return baseEditorType.GetGenericArguments().FirstOrDefault();
				}
				baseEditorType = baseEditorType.BaseType;
			}
			return null;
		}

		/// <summary>
		/// Initializes the class. Override this method to perform any special functions when the class is loaded.
		/// </summary>
		protected static void InitializeClass()
		{
			
		}

		/// <summary>
		/// Static method for displaying handle preferences.
		/// </summary>
		private MethodInfo m_DisplayHandlePreferencesMethod;
		/// <summary>
		/// A table of serialized object representations of the currently selected objects.
		/// </summary>
		private readonly Dictionary<Object, SerializedObject> m_InspectedObjects =
			new Dictionary<Object, SerializedObject>();

		#region Backing Fields
		private Object m_FirstTarget;
		#endregion

		/// <summary>
		/// Gets the first target. This should be a value cached in OnEnable(), as invoking Editor.targets inside of the
		/// OnSceneGUI() callback logs an error message.
		/// </summary>
		/// <value>The first target.</value>
		public Object FirstTarget { get { return m_FirstTarget; } }
		/// <summary>
		/// Gets the handle matrix.
		/// </summary>
		/// <value>The handle matrix.</value>
		protected virtual Matrix4x4 HandleMatrix { get { return Matrix4x4.identity; } }
		/// <summary>
		/// Gets a value indicating whether this <see cref="BaseEditor{T}"/> implements a scene GUI handles.
		/// </summary>
		/// <value><see langword="true"/> if implements a scene GUI handles; otherwise, <see langword="false"/>.</value>
		protected abstract bool ImplementsSceneGUIHandles { get; }
		/// <summary>
		/// Gets a value indicating whether this <see cref="BaseEditor{T}"/> implements a scene GUI overlay.
		/// </summary>
		/// <value><see langword="true"/> if implements a scene GUI overlay; otherwise, <see langword="false"/>.</value>
		protected abstract bool ImplementsSceneGUIOverlay { get; }
		/// <summary>
		/// The Editor calling SceneGUI.Display().
		/// </summary>
		/// <value>The Editor calling SceneGUI.Display().</value>
		public Editor SceneGUIContext { get { return this; } }
		/// <summary>
		/// The current target object represented as a serialized object. Use this property when interacting with
		/// serialized properties from within OnSceneGUI() to prevent errors related to accessing targets array.
		/// </summary>
		/// <value>The serialized target.</value>
		protected SerializedObject SerializedTarget { get { return m_InspectedObjects[this.target]; } }

		/// <summary>
		/// Applies any pending modifications and updates GUI contents.
		/// </summary>
		protected void ApplyModificationsAndUpdateGUIContents()
		{
			this.serializedObject.ApplyModifiedProperties();
			this.serializedObject.Update();
			UpdateGUIContents();
		}

		/// <summary>
		/// Assigns all <see cref="UnityEditor.SerializedProperty"/> and
		/// <see cref="UnityEditorInternal.ReorderableList"/> fields defined on this instance if they are decorated with
		/// <see cref="SerializedPropertyAttribute"/>.
		/// </summary>
		/// <param name="targetType">Target type.</param>
		protected void AssignDecoratedFields(System.Type targetType)
		{
			List<SerializedPropertyAttribute> attrs = new List<SerializedPropertyAttribute>();
			string propertyPath;
			foreach (FieldInfo propertyField in s_SerializedPropertyFields[GetType()])
			{
				bool isSerializedProperty = propertyField.FieldType == typeof(SerializedProperty);
				if (!isSerializedProperty && propertyField.FieldType != typeof(ReorderableList))
				{
					continue;
				}
				propertyField.GetCustomAttributes(attrs);
				if (attrs[0].GetPropertyPath(propertyField, targetType, out propertyPath))
				{
					SerializedProperty property = this.serializedObject.FindProperty(propertyPath);
					if (property == null)
					{
						continue;
					}
					if (isSerializedProperty)
					{
						propertyField.SetValue(this, property);
					}
					else
					{
						ReorderableList list = new ReorderableList(property.serializedObject, property);
						string displayName = property.displayName;
						list.onAddCallback = delegate(ReorderableList lst) {
							++lst.serializedProperty.arraySize;
							lst.serializedProperty.serializedObject.ApplyModifiedProperties();
						};
						list.drawHeaderCallback = rect => EditorGUI.LabelField(rect, displayName);
						list.drawElementCallback = (rect, index, isActive, isFocused) =>
						{
							rect.height -= EditorGUIUtility.standardVerticalSpacing;
							EditorGUIUtility.labelWidth -= EditorGUIX.ReorderableListThumbWidth;
							EditorGUI.PropertyField(rect, list.serializedProperty.GetArrayElementAtIndex(index));
							EditorGUIUtility.labelWidth += EditorGUIX.ReorderableListThumbWidth;
						};
#if !UNITY_4_6 && !UNITY_4_7 && !UNITY_5_0 && !UNITY_5_1 && !UNITY_5_2
						list.elementHeightCallback = delegate(int index) {
							return EditorGUI.GetPropertyHeight(list.serializedProperty.GetArrayElementAtIndex(index)) +
								EditorGUIUtility.standardVerticalSpacing;
						};
#endif
						propertyField.SetValue(this, list);
					}
				}
			}
		}
		
		/// <summary>
		/// Displays the inspector.
		/// </summary>
		protected virtual void DisplayInspector()
		{
			base.OnInspectorGUI();
		}
		
		/// <summary>
		/// Displays the scene GUI controls. This group appears after handle toggles.
		/// </summary>
		protected virtual void DisplaySceneGUIControls()
		{

		}
		
		/// <summary>
		/// Displays the scene GUI handle toggles. This group appears at the top of the scene GUI overlay.
		/// </summary>
		protected virtual void DisplaySceneGUIHandleToggles()
		{
			
		}
		
		/// <summary>
		/// Displays the scene GUI handles.
		/// </summary>
		protected virtual void DisplaySceneGUIHandles()
		{
			
		}

		/// <summary>
		/// Displays a field for a property in the scene GUI.
		/// </summary>
		/// <param name="propertyPath">Property path.</param>
		protected void DisplaySceneGUIPropertyField(string propertyPath)
		{
			EditorGUI.BeginChangeCheck();
			{
				EditorGUIX.DisplayPropertyField(m_InspectedObjects[this.target].FindProperty(propertyPath));
			}
			if (EditorGUI.EndChangeCheck())
			{
				SerializedObject so = new SerializedObject(m_InspectedObjects.Keys.ToArray());
				switch (so.FindProperty(propertyPath).propertyType)
				{
				case SerializedPropertyType.AnimationCurve:
					so.FindProperty(propertyPath).animationCurveValue =
						m_InspectedObjects[this.target].FindProperty(propertyPath).animationCurveValue;
					break;
				case SerializedPropertyType.ArraySize:
				case SerializedPropertyType.Integer:
				case SerializedPropertyType.LayerMask:
				case SerializedPropertyType.Character:
					so.FindProperty(propertyPath).intValue =
						m_InspectedObjects[this.target].FindProperty(propertyPath).intValue;
					break;
				case SerializedPropertyType.Boolean:
					so.FindProperty(propertyPath).boolValue =
						m_InspectedObjects[this.target].FindProperty(propertyPath).boolValue;
					break;
				case SerializedPropertyType.Bounds:
					so.FindProperty(propertyPath).boundsValue =
						m_InspectedObjects[this.target].FindProperty(propertyPath).boundsValue;
					break;
				case SerializedPropertyType.Color:
					so.FindProperty(propertyPath).colorValue =
						m_InspectedObjects[this.target].FindProperty(propertyPath).colorValue;
					break;
				case SerializedPropertyType.Enum:
					so.FindProperty(propertyPath).enumValueIndex =
						m_InspectedObjects[this.target].FindProperty(propertyPath).enumValueIndex;
					break;
				case SerializedPropertyType.Float:
					so.FindProperty(propertyPath).floatValue =
						m_InspectedObjects[this.target].FindProperty(propertyPath).floatValue;
					break;
				case SerializedPropertyType.Generic:
					Debug.LogError("Generic properties not implemented.");
					break;
				case SerializedPropertyType.Gradient:
					Debug.LogError("Gradient properties not implemented");
					break;
				case SerializedPropertyType.ObjectReference:
					so.FindProperty(propertyPath).objectReferenceValue =
						m_InspectedObjects[this.target].FindProperty(propertyPath).objectReferenceValue;
					break;
				case SerializedPropertyType.Quaternion:
					so.FindProperty(propertyPath).quaternionValue =
						m_InspectedObjects[this.target].FindProperty(propertyPath).quaternionValue;
					break;
				case SerializedPropertyType.Rect:
					so.FindProperty(propertyPath).rectValue =
						m_InspectedObjects[this.target].FindProperty(propertyPath).rectValue;
					break;
				case SerializedPropertyType.String:
					so.FindProperty(propertyPath).stringValue =
						m_InspectedObjects[this.target].FindProperty(propertyPath).stringValue;
					break;
				case SerializedPropertyType.Vector2:
					so.FindProperty(propertyPath).vector2Value =
						m_InspectedObjects[this.target].FindProperty(propertyPath).vector2Value;
					break;
				case SerializedPropertyType.Vector3:
					so.FindProperty(propertyPath).vector3Value =
						m_InspectedObjects[this.target].FindProperty(propertyPath).vector3Value;
					break;
				case SerializedPropertyType.Vector4:
					so.FindProperty(propertyPath).vector4Value =
						m_InspectedObjects[this.target].FindProperty(propertyPath).vector4Value;
					break;
				}
				so.ApplyModifiedProperties();
			}
		}

		/// <summary>
		/// Gets the cached targets. Use this method when interacting with serialized properties from within
		/// OnSceneGUI() to prevent errors related to accessing targets array.
		/// </summary>
		/// <returns>The cached targets.</returns>
		protected Object[] GetCachedTargets()
		{
			return m_InspectedObjects.Keys.ToArray();
		}

		/// <summary>
		/// Raises the disable event.
		/// </summary>
		protected virtual void OnDisable()
		{
			SceneGUI.DeregisterObjectGUICallback(this as ISceneGUIContext);
			Undo.undoRedoPerformed -= ApplyModificationsAndUpdateGUIContents;
			Undo.postprocessModifications -= OnModifyProperty;
		}

		/// <summary>
		/// Raises the enable event.
		/// </summary>
		protected virtual void OnEnable()
		{
			m_DisplayHandlePreferencesMethod = GetType().GetStaticMethod("DisplayHandlePreferences");
			m_FirstTarget = this.target;
			foreach (Object t in this.targets)
			{
				if (t != null)
				{
					m_InspectedObjects.Add(t, new SerializedObject(t));
				}
			}
			if (this.ImplementsSceneGUIOverlay)
			{
				SceneGUI.RegisterObjectGUICallback(this as ISceneGUIContext, OnSceneGUIOverlay);
			}
			Undo.undoRedoPerformed += ApplyModificationsAndUpdateGUIContents;
			Undo.postprocessModifications += OnModifyProperty;
		}
		
		/// <summary>
		/// Raises the inspector GUI event.
		/// </summary>
		public override void OnInspectorGUI()
		{
			// early out if the target is null, e.g., if it was destroyed in an earlier callback this frame
			if (this.target == null)
			{
				return;
			}
			if (this.ImplementsSceneGUIOverlay || this.ImplementsSceneGUIHandles)
			{
				if (EditorGUIX.BeginSceneGUIControlsArea())
				{
					m_DisplayHandlePreferencesMethod.Invoke(null, null);
				}
				EditorGUIX.EndSceneGUIControlsArea();
			}
			DisplayInspector();
		}

		/// <summary>
		/// Triggers <see cref="UpdateGUIContents"/> when a property is modified (e.g., reset to prefab value).
		/// </summary>
		/// <param name="modifications">Modifications.</param>
		private UndoPropertyModification[] OnModifyProperty(UndoPropertyModification[] modifications)
		{
			UpdateGUIContents();
			return modifications;
		}

		/// <summary>
		/// Raises the scene GUI event.
		/// </summary>
		protected virtual void OnSceneGUI()
		{
			// early out if the target is null, e.g., if it was destroyed in an earlier callback this frame or if scene gui is disabled
			if (this.target == null || !SceneGUI.IsEnabled)
			{
				return;
			}
			if (this.ImplementsSceneGUIHandles)
			{
				Color oldColor = Handles.color;
				Matrix4x4 oldMatrix = Handles.matrix;
				Handles.matrix = this.HandleMatrix;
				DisplaySceneGUIHandles();
				Handles.color = oldColor;
				Handles.matrix = oldMatrix;
			}
			if (this.ImplementsSceneGUIOverlay)
			{
				SceneGUI.Display(this);
			}
		}

		/// <summary>
		/// Raises the scene GUI overlay event.
		/// </summary>
		private void OnSceneGUIOverlay()
		{
			DisplaySceneGUIHandleToggles();
			m_InspectedObjects[this.target].Update();
			DisplaySceneGUIControls();
			m_InspectedObjects[this.target].ApplyModifiedProperties();
		}

		/// <summary>
		/// Updates any necessary GUI contents when something has changed.
		/// </summary>
		protected virtual void UpdateGUIContents()
		{

		}
	}

	/// <summary>
	/// Base editor class for objects of a particular type to register preferences and scene GUI callbacks.
	/// </summary>
	public abstract class BaseEditor<T> : BaseEditor where T : Object
	{
		/// <summary>
		/// A flag indicating whether the inspected type is a component.
		/// </summary>
		private bool m_IsComponentType = false;

		/// <summary>
		/// Gets the handle matrix.
		/// </summary>
		/// <value>The handle matrix.</value>
		protected override Matrix4x4 HandleMatrix
		{
			get
			{
				return m_IsComponentType ? (this.Target as Component).transform.localToWorldMatrix : Matrix4x4.identity;
			}
		}
		/// <summary>
		/// Gets the target.
		/// </summary>
		/// <value>The target.</value>
		protected T Target { get { return this.target as T; } }

		/// <summary>
		/// For a specified target, gets all objects that are dirtied by its property changes.
		/// </summary>
		/// <returns>An array of objects to record for undoing.</returns>
		/// <param name="obj">Target.</param>
		protected virtual Object[] GetUndoObjects(T obj)
		{
			return obj == null ? new Object[0] : new Object[] { obj };
		}

		/// <summary>
		/// Raises the enable event.
		/// </summary>
		protected override void OnEnable()
		{
			base.OnEnable();
			m_IsComponentType = typeof(Component).IsAssignableFrom(typeof(T));
			AssignDecoratedFields(typeof(T));
		}
	}
}