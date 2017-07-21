// 
// HyperTextEditor.cs
// 
// Copyright (c) 2014-2016, Candlelight Interactive, LLC
// All rights reserved.
// 
// This file is licensed according to the terms of the Unity Asset Store EULA:
// http://download.unity3d.com/assetstore/customer-eula.pdf
// 
// This file contains a custom editor for HyperText.

#if UNITY_4_6 || UNITY_5_0 || UNITY_5_1
#define IS_VBO_UI_VERTEX
#endif

#if UNITY_4_6 || UNITY_4_7 || UNITY_5_0 || UNITY_5_1 || UNITY_5_2
#define NO_ALIGN_BY_GEOMETRY
#endif

using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using System.Collections.Generic;
using System.Linq;

namespace Candlelight.UI
{
	/// <summary>
	/// An editor utility class for <see cref="HyperText"/> objects.
	/// </summary>
	public static class HyperTextUtility
	{
		#region Shared Allocations
		private static readonly List<UnityEngine.EventSystems.IPointerClickHandler> s_ClickHandlers =
			new List<UnityEngine.EventSystems.IPointerClickHandler>();
		private static readonly List<UnityEngine.EventSystems.IPointerDownHandler> s_DownHandlers =
			new List<UnityEngine.EventSystems.IPointerDownHandler>();
		#endregion

		/// <summary>
		/// Clean up obsolete <see cref="HyperTextQuad"/> components in the project.
		/// </summary>
		[MenuItem("Assets/Candlelight/HyperText/Clean Up HyperText Quads")]
		private static void CleanUpQuads()
		{
#pragma warning disable 612
			AssetDatabaseX.ModifyAllComponentsInProject<HyperTextQuad>(
				(quad, assetPath) => Undo.DestroyObjectImmediate(quad), "Clean Up HyperText Quads in Prefabs"
			);
#pragma warning restore 612
		}

		/// <summary>
		/// Upgrades all <see cref="UnityEngine.UI"/> objects in the project to <see cref="HyperText"/>.
		/// </summary>
		[MenuItem("Assets/Candlelight/HyperText/Upgrade All UI Text")]
		private static void UpgradeAllTextObjects()
		{
			if (
				!EditorUtility.DisplayDialog(
					"Upgrade All UI Text?",
					string.Format(
						"This option will scan your project for all {0} instances and replace them with {1} objects. " +
						"All references to objects and serialized values will remain intact.",
						typeof(UnityEngine.UI.Text).FullName, typeof(HyperText).FullName
					), "Yes", "No"
				)
			)
			{
				return;
			}
			HyperText ht = new GameObject("DELETE THIS", typeof(HyperText)).GetComponent<HyperText>();
			MonoScript script = MonoScript.FromMonoBehaviour(ht);
			AssetDatabaseX.ModifyAllComponentsInProject<UnityEngine.UI.Text>(
				delegate(UnityEngine.UI.Text text, string assetPath)
				{
					if (text is HyperText)
					{
						return;
					}
					SerializedObject so = new SerializedObject(text);
					so.FindProperty("m_Script").objectReferenceValue = script;
					so.ApplyModifiedProperties();
				}
			);
			if (ht != null)
			{
				Object.DestroyImmediate(ht.gameObject);
			}
		}

		/// <summary>
		/// Disable the raycastTarget property for <see cref="HyperText"/> instances that might interfere with parent
		/// objects.
		/// </summary>
		[MenuItem("Assets/Candlelight/HyperText/Validate Raycast Targets")]
		private static void ValidateRaycastTargets()
		{
			if (
				!EditorUtility.DisplayDialog(
					"Validate HyperText Raycast Targets?",
					string.Format(
						"This option will scan your project for all {0} instances that are children of {1} or {2} " +
						"objects (such as e.g., Buttons) but also have the raycastTarget property enabled and have " +
						"links defined. Links on these instances will block input events. Results will be logged to " +
						"the console.\n\nDo you wish to proceed?",
						typeof(HyperText).Name,
						typeof(UnityEngine.EventSystems.IPointerClickHandler).Name,
						typeof(UnityEngine.EventSystems.IPointerDownHandler).Name

					), "Yes", "No"
				)
			)
			{
				return;
			}
			System.Type logEntries =
				ReflectionX.AllTypes.FirstOrDefault(t => t.FullName == "UnityEditorInternal.LogEntries");
			if (logEntries != null)
			{
				System.Reflection.MethodInfo clear = logEntries.GetStaticMethod("Clear");
				if (clear != null)
				{
					clear.Invoke(null, null);
				}
			}
			bool isSilent = HyperText.IsSilent;
			HyperText.IsSilent = true;
			string undoName = "Validate HyperText Raycast Targets";
			System.Reflection.MethodInfo getWarningMessage =
				typeof(HyperText).GetInstanceMethod("GetInputBlockingWarningMessage");
			AssetDatabaseX.ModifyAssetCallback<HyperText> validateRaycastTarget =
				delegate (HyperText hyperText, string assetPath)
			{
				if (!hyperText.raycastTarget)
				{
					return;
				}
#if UNITY_4_6 || UNITY_4_7
				s_ClickHandlers.Clear();
				s_DownHandlers.Clear();
				s_ClickHandlers.AddRange(
					hyperText.GetComponentsInParent(
						typeof(UnityEngine.EventSystems.IPointerClickHandler), true
					).Cast<UnityEngine.EventSystems.IPointerClickHandler>()
				);
				s_DownHandlers.AddRange(
					hyperText.GetComponentsInParent(
						typeof(UnityEngine.EventSystems.IPointerDownHandler), true
					).Cast<UnityEngine.EventSystems.IPointerDownHandler>()
				);
#else
				hyperText.GetComponentsInParent(true, s_ClickHandlers);
				hyperText.GetComponentsInParent(true, s_DownHandlers);
#endif
				s_ClickHandlers.Remove(hyperText);
				s_DownHandlers.Remove(hyperText);
				if (s_ClickHandlers.Count > 0 || s_DownHandlers.Count > 0)
				{
					using (ListPool<HyperText.LinkInfo>.Scope links = new ListPool<HyperText.LinkInfo>.Scope())
					{
						if (hyperText.GetLinks(links.List) > 0)
						{
							string warningMessage = getWarningMessage.Invoke(hyperText, null) as string;
							if (!string.IsNullOrEmpty(warningMessage))
							{
								Debug.LogWarning(
									string.Format(
										"{0}\n<b>{1}</b> at {2}\n", warningMessage, hyperText.name, assetPath
									), hyperText
								);
							}
						}
					}
				}
			};
			AssetDatabaseX.ModifyAllComponentsInProject(validateRaycastTarget, undoName);
			HyperText.IsSilent = isSilent;
		}
	}

	/// <summary>
	/// A custom editor for <see cref="HyperText"/> objects.
	/// </summary>
	[CanEditMultipleObjects, CustomEditor(typeof(HyperText), true), InitializeOnLoad]
	public class HyperTextEditor : BaseEditor<HyperText>
	{
		/// <summary>
		/// An enum with different debug display modes
		/// </summary>
		public enum DebugSceneMode { None, VertexIndices }

		#region Labels
		private static readonly GUIContent s_FixRaycastTargetLabel = new GUIContent(
			"Disable Raycast Target", "Prevent this object from blocking input to buttons or similar controls."
		);
		private static readonly GUIContent s_InputTextSourceLabel =
			new GUIContent("Override Text Source", "Assigning a text input source overrides the text on this object.");
		private static readonly GUIContent s_OpenURLPatternsLabel = new GUIContent(
			"Open URL Patterns",
			"Enable this property to automatically open links in the browser if their name attribute specifies an " +
			"http or https url."
		);
		private static readonly GUIContent s_MaterialLabel = new GUIContent("Material");
		#endregion
		#region Preferences
		private static readonly EditorPreference<DebugSceneMode, HyperTextEditor> s_DebugSceneModePreference =
			new EditorPreference<DebugSceneMode, HyperTextEditor>("debugSceneMode", DebugSceneMode.None);
		private static readonly EditorPreference<Color, HyperTextEditor> s_HitboxColorPreference =
			new EditorPreference<Color, HyperTextEditor>("hitboxesColor", Color.magenta);
		private static readonly EditorPreference<bool, HyperTextEditor> s_HitboxTogglePreference =
			EditorPreference<bool, HyperTextEditor>.ForToggle("hitboxes", true);
		#endregion
		#region Shared Allocations
		private static readonly List<UnityEngine.EventSystems.IPointerClickHandler> s_ClickHandlers =
			new List<UnityEngine.EventSystems.IPointerClickHandler>();
		private static readonly List<UnityEngine.EventSystems.IPointerDownHandler> s_DownHandlers =
			new List<UnityEngine.EventSystems.IPointerDownHandler>();
		private static List<Vector3> s_DebugSceneModeVertices = new List<Vector3>(4096);
		private Vector3[] s_HitboxVertices = new Vector3[4];
		private static readonly GUIContent s_ReusableLabel = new GUIContent();
		#endregion

		/// <summary>
		/// For each index in a quad, the next index.
		/// </summary>
		private static readonly int[] s_QuadIndexWrap = new [] { 1, 2, 3, 0 };

		/// <summary>
		/// Gets the product category.
		/// </summary>
		/// <value>The product category.</value>
		new public static AssetStoreProduct ProductCategory { get { return AssetStoreProduct.HyperText; } }

		/// <summary>
		/// Creates a new HyperText in the scene.
		/// </summary>
		/// <param name="menuCommand">The menu command being executed.</param>
		[MenuItem("GameObject/UI/Candlelight/HyperText")]
		public static void CreateNew(MenuCommand menuCommand)
		{
			EditorApplication.ExecuteMenuItem("GameObject/UI/Text");
			UnityEngine.UI.Text text = Selection.activeGameObject.GetComponent<UnityEngine.UI.Text>();
			Color color = text.color;
			GameObject.DestroyImmediate(Selection.activeGameObject.GetComponent<UnityEngine.UI.Shadow>(), true);
			GameObject.DestroyImmediate(text, true);
			Selection.activeGameObject.name = "HyperText";
			HyperText hyperText = Selection.activeGameObject.AddComponent<HyperText>();
			hyperText.color = color;
			hyperText.text = "New <a name=\"link\">HyperText</a>";
			// BUG: for some reason parenting behavior is not inherited when executing built-in menu command
			GameObject parent = menuCommand.context as GameObject;
			if (parent != null && parent.GetComponentInParent<Canvas>() != null)
			{
#if !UNITY_4_6 && !UNITY_4_7
				hyperText.gameObject.name =
					GameObjectUtility.GetUniqueNameForSibling(parent.transform, hyperText.gameObject.name);
#endif
				GameObjectUtility.SetParentAndAlign(hyperText.gameObject, parent);
			}
		}

		/// <summary>
		/// Displays a font property field.
		/// </summary>
		/// <param name="position">Position.</param>
		/// <param name="fontProperty">Font property.</param>
		/// <param name="inheritingFrom">Style sheet from which the font is potentially inheriting.</param>
		public static void DisplayFontProperty(
			Rect position, SerializedProperty fontProperty, HyperTextStyles inheritingFrom
		)
		{
			ValidationStatus status = ValidationStatus.None;
			s_ReusableLabel.text = fontProperty.displayName;
			s_ReusableLabel.tooltip = string.Empty;
			if (fontProperty.objectReferenceValue == null)
			{
				if (inheritingFrom != null && inheritingFrom.CascadedFont != null)
				{
					s_ReusableLabel.tooltip = string.Format(
						"Inheriting Font {0} from {1}.", inheritingFrom.CascadedFont.name, inheritingFrom.name
					);
				}
				else
				{
					s_ReusableLabel.tooltip = "Font cannot be null.";
					status = ValidationStatus.Error;
				}
			}
			else if (!(fontProperty.objectReferenceValue as Font).dynamic)
			{
				s_ReusableLabel.tooltip = "Font size and style settings are only supported for dynamic fonts. " +
					"Only colors and offsets will be applied.";
				status = ValidationStatus.Warning;
			}
			else if (inheritingFrom != null && inheritingFrom.CascadedFont != null)
			{
				s_ReusableLabel.tooltip = string.Format(
					"Overriding Font {0} inherited from {1}.", inheritingFrom.CascadedFont.name, inheritingFrom.name
				);
				status = ValidationStatus.Warning;
			}
			if (
				string.IsNullOrEmpty(s_ReusableLabel.tooltip) &&
				inheritingFrom != null &&
				inheritingFrom.CascadedFont != null
			)
			{
				s_ReusableLabel.tooltip = string.Format(
					"Assign a value to override Font {0} inherited from {1}",
					inheritingFrom.CascadedFont.name,
					inheritingFrom.name
				);
			}
			switch (status)
			{
			case ValidationStatus.None:
				EditorGUI.PropertyField(position, fontProperty, s_ReusableLabel);
				break;
			default:
				EditorGUIX.DisplayPropertyFieldWithStatus(
					position, fontProperty, status, s_ReusableLabel, false, s_ReusableLabel.tooltip
				);
				break;
			}
		}

		/// <summary>
		/// Displays the handle preferences. They will be displayed in the preference menu and the top of the inspector.
		/// </summary>
		new protected static void DisplayHandlePreferences()
		{
			EditorGUIX.DisplayHandlePropertyEditor<HyperTextEditor>(
				"Hitboxes", s_HitboxTogglePreference, s_HitboxColorPreference
			);
			EditorGUI.BeginChangeCheck();
			{
				s_DebugSceneModePreference.CurrentValue = (DebugSceneMode)EditorGUILayout.EnumPopup(
					"Debug Scene Mode", s_DebugSceneModePreference.CurrentValue
				);
			}
			if (EditorGUI.EndChangeCheck())
			{
				SceneView.RepaintAll();
			}
		}

		/// <summary>
		/// Displays a property field with an override property checkbox and status icon if styles are assigned.
		/// </summary>
		/// <param name="position">Position.</param>
		/// <param name="property">Property to display.</param>
		/// <param name="overrideProperty">
		/// Property specifying whether the other property is overriding an inherited one. Object reference properties
		/// are assumed to override if they have a value assigned.
		/// </param>
		/// <param name="stylesProperty">Property with the reference to a style sheet.</param>
		public static void DisplayOverridableProperty(
			Rect position,
			SerializedProperty property,
			SerializedProperty overrideProperty,
			SerializedProperty stylesProperty
		)
		{
			DisplayOverridableProperty(
				position: position,
				property: property,
				overrideProperty: overrideProperty,
				displayCheckbox:
					stylesProperty.objectReferenceValue != null || stylesProperty.hasMultipleDifferentValues,
				inheritTooltip: "controlled by styles on this object",
				overrideTooltip: "overridden by this object"
			);
		}

		/// <summary>
		/// Displays a property field with an optional override property checkbox and status icon as needed.
		/// </summary>
		/// <param name="position">Position.</param>
		/// <param name="property">Property.</param>
		/// <param name="overrideProperty">
		/// Property specifying whether the other property is overriding an inherited one. Object reference properties
		/// are assumed to override if they have a value assigned.
		/// </param>
		/// <param name="displayCheckbox">If set to <see langword="true"/> display checkbox/tooltip as needed.</param>
		/// <param name="inheritTooltip">Predicate fragment of the tooltip when property is inheriting.</param>
		/// <param name="overrideTooltip">Predicate fragment of the tooltip when property is overridden.</param>
		public static void DisplayOverridableProperty(
			Rect position,
			SerializedProperty property,
			SerializedProperty overrideProperty,
			bool displayCheckbox,
			string inheritTooltip,
			string overrideTooltip
		)
		{
			if (displayCheckbox)
			{
				ValidationStatus status = ValidationStatus.None;
				if (property.propertyType == SerializedPropertyType.ObjectReference)
				{
					status = property.objectReferenceValue == null ? ValidationStatus.None : ValidationStatus.Warning;
				}
				else
				{
					status = overrideProperty.hasMultipleDifferentValues || overrideProperty.boolValue ?
						ValidationStatus.Warning : ValidationStatus.None;
				}
				s_ReusableLabel.text = property.displayName;
				s_ReusableLabel.tooltip = string.Format(
					property.serializedObject.isEditingMultipleObjects ?
						"{0} {1} on at least one selected object." : "{0} {1}.",
					property.displayName,
					status == ValidationStatus.Warning ? overrideTooltip : inheritTooltip
				);
				if (status != ValidationStatus.None)
				{
					Rect iconPosition = position;
					iconPosition.x += position.width - EditorGUIUtility.singleLineHeight;
					iconPosition.width = iconPosition.height = EditorGUIUtility.singleLineHeight;
					if (property.propertyType != SerializedPropertyType.Generic)
					{
						position.width -= iconPosition.width;
					}
					EditorGUIX.DisplayValidationStatusIcon(iconPosition, status, s_ReusableLabel.tooltip);
				}
				if (property.propertyType == SerializedPropertyType.ObjectReference)
				{
					EditorGUI.PropertyField(position, property, s_ReusableLabel);
				}
				else
				{
					EditorGUIX.DisplayPropertyWithToggle(position, s_ReusableLabel, overrideProperty, property);
				}
			}
			else
			{
				EditorGUI.PropertyField(position, property);
			}
		}

		#region Serialized Properties
#if !NO_ALIGN_BY_GEOMETRY
		[RelativeProperty("m_FontData")]
#endif
		#pragma warning disable 649
		private SerializedProperty m_AlignByGeometry;
		#pragma warning restore 649
		[RelativeProperty("m_FontData")] private SerializedProperty m_Alignment;
		[RelativeProperty("m_FontData")] private SerializedProperty m_BestFit;
		[SerializedProperty] private SerializedProperty m_ClickedLink;
		[SerializedProperty] private SerializedProperty m_Color;
		[SerializedProperty] private SerializedProperty m_EnteredLink;
		[SerializedProperty] private SerializedProperty m_ExitedLink;
		[RelativeProperty("m_FontData")] private SerializedProperty m_Font;
		[RelativeProperty("m_FontData")] private SerializedProperty m_FontSize;
		[RelativeProperty("m_FontData")] private SerializedProperty m_FontStyle;
		[RelativeProperty("m_FontData")] private SerializedProperty m_HorizontalOverflow;
		[RelativeProperty("m_TextProcessor")] private SerializedProperty m_InputTextSourceObject;
		[SerializedProperty] private SerializedProperty m_Interactable;
		[SerializedProperty] private SerializedProperty m_LinkHitboxPadding;
		[RelativeProperty("m_FontData")] private SerializedProperty m_LineSpacing;
		[RelativeProperty("m_TextProcessor")] private ReorderableList m_LinkKeywordCollections;
		[SerializedProperty] private SerializedProperty m_Material;
		[RelativeProperty("m_FontData")] private SerializedProperty m_MaxSize;
		[RelativeProperty("m_FontData")] private SerializedProperty m_MinSize;
		[SerializedProperty] private SerializedProperty m_OpenURLPatterns;
		[SerializedProperty] private SerializedProperty m_PressedLink;
		[RelativeProperty("m_TextProcessor")] private ReorderableList m_QuadKeywordCollections;
		[SerializedProperty] private SerializedProperty m_QuadMaterial;
		[SerializedProperty] private SerializedProperty m_RaycastTarget;
		[SerializedProperty] private SerializedProperty m_ReleasedLink;
		[RelativeProperty("m_FontData")] private SerializedProperty m_RichText;
		[SerializedProperty] private SerializedProperty m_Script;
		[SerializedProperty] private SerializedProperty m_ShouldOverrideStylesFontColor;
		[RelativeProperty("m_TextProcessor")] private SerializedProperty m_ShouldOverrideStylesFontSize;
		[SerializedProperty] private SerializedProperty m_ShouldOverrideStylesFontStyle;
		[SerializedProperty] private SerializedProperty m_ShouldOverrideStylesLineSpacing;
		[SerializedProperty] private SerializedProperty m_ShouldOverrideStylesLinkHitboxPadding;
		[RelativeProperty("m_TextProcessor")] private SerializedProperty m_Styles;
		private List<SerializedProperty> m_SubclassProperties = new List<SerializedProperty>();
		[RelativeProperty("m_TextProcessor")] private ReorderableList m_TagKeywordCollections;
		[SerializedProperty] private SerializedProperty m_Text;
		[SerializedProperty] private SerializedProperty m_TextProcessor;
		[RelativeProperty("m_FontData")] private SerializedProperty m_VerticalOverflow;
		#endregion

		/// <summary>
		/// All keyword collections assigned to this object in some list or other.
		/// </summary>
		private IEnumerable<KeywordCollection> m_AssignedCollections = null;
		/// <summary>
		/// The index of the currently highlighted vertex.
		/// </summary>
		private int m_CurrentlyHighlightedVertexIndex = 0;
		/// <summary>
		/// The link hitboxes.
		/// </summary>
		private Dictionary<HyperText.LinkInfo, List<Rect>> m_LinkHitboxes =
			new Dictionary<HyperText.LinkInfo, List<Rect>>();
		/// <summary>
		/// The warning message for the raycastTarget property, if applicable.
		/// </summary>
		private string m_RaycastTargetWarningMessage = null;
		/// <summary>
		/// The warning icon to use for the raycastTarget property.
		/// </summary>
		private ValidationStatus m_RaycastTargetWarningStatus = ValidationStatus.None;

		/// <summary>
		/// Gets a value indicating whether this <see cref="HyperTextEditor"/> implements scene GUI handles.
		/// </summary>
		/// <value><see langword="true"/> if implements scene GUI handles; otherwise, <see langword="false"/>.</value>
		protected override bool ImplementsSceneGUIHandles { get { return true; } }
		/// <summary>
		/// Gets a value indicating whether this <see cref="HyperTextEditor"/> implements scene GUI overlay.
		/// </summary>
		/// <value><see langword="true"/> if implements scene GUI overlay; otherwise, <see langword="false"/>.</value>
		protected override bool ImplementsSceneGUIOverlay { get { return false; } }

		/// <summary>
		/// Displays the specified event property, respecting the current indent level.
		/// </summary>
		/// <param name="eventProperty">Event property.</param>
		private void DisplayEventProperty(SerializedProperty eventProperty)
		{
			// NOTE: for some reason, need to add 2 more pixels to get same height as when using GUILayout
			Rect position =
				EditorGUI.IndentedRect(GUILayoutUtility.GetRect(0f, EditorGUI.GetPropertyHeight(eventProperty) + 2f));
			EditorGUI.PropertyField(position, eventProperty);
		}

		/// <summary>
		/// Displays the scene GUI handles.
		/// </summary>
		protected override void DisplaySceneGUIHandles()
		{
			base.DisplaySceneGUIHandles();
			if (s_HitboxTogglePreference.CurrentValue)
			{
				Color oldColor = Handles.color;
				Handles.color = s_HitboxColorPreference.CurrentValue;
				this.Target.GetLinkHitboxes(m_LinkHitboxes);
				foreach (KeyValuePair<HyperText.LinkInfo, List<Rect>> linkHitboxes in m_LinkHitboxes)
				{
					Vector2 center = Vector2.zero;
					foreach (Rect hitbox in linkHitboxes.Value)
					{
						s_HitboxVertices[0] = Vector3.right * hitbox.xMin + Vector3.up * hitbox.yMax;
						s_HitboxVertices[1] = Vector3.right * hitbox.xMax + Vector3.up * hitbox.yMax;
						s_HitboxVertices[2] = Vector3.right * hitbox.xMax + Vector3.up * hitbox.yMin;
						s_HitboxVertices[3] = Vector3.right * hitbox.xMin + Vector3.up * hitbox.yMin;
						// draw a box around each hitbox
						for (int i = 0; i < s_HitboxVertices.Length; ++i)
						{
							Handles.DrawLine(s_HitboxVertices[i], s_HitboxVertices[s_QuadIndexWrap[i]]);
						}
						center += hitbox.center;
					}
					center /= linkHitboxes.Value.Count;
					// indicate the name for each link
					Handles.Label(
						center,
						string.Format(
							"{0}{1}",
							linkHitboxes.Key.Name,
							string.IsNullOrEmpty(linkHitboxes.Key.ClassName) ?
							"" : string.Format(" ({0})", linkHitboxes.Key.ClassName)
						)
					);
				}
				Handles.color = oldColor;
			}
			if (s_DebugSceneModePreference.CurrentValue == DebugSceneMode.VertexIndices)
			{
				int scrollAmt = 1;
				m_CurrentlyHighlightedVertexIndex =
					Mathf.Clamp(m_CurrentlyHighlightedVertexIndex, 0, s_DebugSceneModeVertices.Count);
				s_DebugSceneModeVertices.Clear();
#if IS_VBO_UI_VERTEX
				s_DebugSceneModeVertices.AddRange(
					from v in this.Target.GetFieldValue<List<UIVertex>>("m_UIVertices") select v.position
				);
#else
				s_DebugSceneModeVertices.AddRange(this.Target.GetFieldValue<Mesh>("m_GlyphMesh").vertices);
#endif
				for (int i = 0; i < s_DebugSceneModeVertices.Count; ++i)
				{
					Handles.Label(s_DebugSceneModeVertices[i], i.ToString());
					if (i == m_CurrentlyHighlightedVertexIndex)
					{
						HighlightIndex(s_DebugSceneModeVertices[i], i);
					}
				}
				if (Event.current.isKey && Event.current.type == EventType.KeyDown)
				{
					switch (Event.current.keyCode)
					{
					case KeyCode.Comma:
					case KeyCode.Less:
						scrollAmt *= -1 * (Event.current.shift ? 12 : 1);
						break;
					case KeyCode.Period:
					case KeyCode.Greater:
						scrollAmt *= 1 * (Event.current.shift ? 12 : 1);
						break;
					default:
						scrollAmt = 0;
						break;
					}
					Event.current.Use();
					m_CurrentlyHighlightedVertexIndex = ArrayX.ScrollArrayIndex(
						m_CurrentlyHighlightedVertexIndex, s_DebugSceneModeVertices.Count, scrollAmt
					);
				}
			}
		}

		/// <summary>
		/// Encircle the specified world position and label the index at the top of the scene view.
		/// </summary>
		/// <param name="position">Position.</param>
		/// <param name="index">Index.</param>
		private void HighlightIndex(Vector3 position, int index)
		{
			Color oldColor = Handles.color;
			Handles.color = Color.green;
			Handles.DrawWireDisc(position, Vector3.forward, 2f);
			Handles.color = oldColor;
			Handles.BeginGUI();
			{
				oldColor = GUI.color;
				GUI.color = Color.green;
				Rect rect = Camera.current.pixelRect;
				rect.x += rect.width * 0.5f - 20f;
				rect.width = 40f;
				GUI.Label(rect, index.ToString(), EditorStylesX.BoldLabel);
				GUI.color = oldColor;
			}
			Handles.EndGUI();
		}

		/// <summary>
		/// Raises the enable event.
		/// </summary>
		protected override void OnEnable()
		{
			base.OnEnable();
			m_LinkKeywordCollections.drawElementCallback = (position, index, isActive, isFocused) =>
				HyperTextProcessorDrawer.OnDrawLinkKeywordCollectionsEntry(
					position, index, m_TextProcessor, () => m_AssignedCollections
				);
			m_QuadKeywordCollections.drawElementCallback = (position, index, isActive, isFocused) =>
				HyperTextProcessorDrawer.OnDrawQuadKeywordCollectionsEntry(
					position, index, m_TextProcessor, () => m_AssignedCollections
				);
			m_TagKeywordCollections.drawElementCallback = (position, index, isActive, isFocused) =>
				HyperTextProcessorDrawer.OnDrawTagKeywordCollectionsEntry(
					position, index, m_TextProcessor, () => m_AssignedCollections
				);
			SerializedPropertyX.GetRemainingVisibleProperties<HyperText>(
				this.target as HyperText, m_SubclassProperties
			);
			UpdateGUIContents();
		}

		/// <summary>
		/// Raises the inspector GUI event.
		/// </summary>
		public override void OnInspectorGUI()
		{
			this.serializedObject.Update();
			m_AssignedCollections = HyperTextProcessorDrawer.GetAllCollections(m_TextProcessor);
			EditorGUILayout.PropertyField(m_Script);
			if (
				m_RaycastTargetWarningStatus != ValidationStatus.None &&
				DisplayErrorMessageWithFixButton(
					m_RaycastTargetWarningMessage,
					s_FixRaycastTargetLabel,
					m_RaycastTargetWarningStatus == ValidationStatus.Warning ? MessageType.Warning : MessageType.Error
				)
			)
			{
				m_RaycastTarget.boolValue = !m_RaycastTarget.boolValue;
				this.serializedObject.ApplyModifiedProperties();
			}
			EditorGUILayout.PropertyField(m_Interactable);
			++EditorGUI.indentLevel;
			EditorGUI.BeginDisabledGroup(!m_Interactable.boolValue);
			{
				EditorGUILayout.PropertyField(m_OpenURLPatterns, s_OpenURLPatternsLabel);
			}
			EditorGUI.EndDisabledGroup();
			--EditorGUI.indentLevel;
			Rect position = EditorGUILayout.GetControlRect(true, EditorGUI.GetPropertyHeight(m_LinkHitboxPadding));
			DisplayOverridableProperty(
				position, m_LinkHitboxPadding, m_ShouldOverrideStylesLinkHitboxPadding, m_Styles
			);
			bool hadStyles = m_Styles.objectReferenceValue != null;
			if (EditorGUIX.DisplayScriptableObjectPropertyFieldWithButton<HyperTextStyles>(m_Styles))
			{
				HyperTextStyles newStyles = m_Styles.objectReferenceValue as HyperTextStyles;
				if (newStyles != null)
				{
					if (m_Font.objectReferenceValue != null)
					{
						newStyles.Font = m_Font.objectReferenceValue as Font;
					}
					newStyles.DefaultFontStyle = (FontStyle)m_FontStyle.enumValueIndex;
					newStyles.DefaultTextColor = m_Color.colorValue;
					newStyles.FontSize = m_FontSize.intValue;
				}
			}
			if (
				!hadStyles &&
				m_Styles.objectReferenceValue != null &&
				(m_Styles.objectReferenceValue as HyperTextStyles).CascadedFont != null
			)
			{
				m_Font.objectReferenceValue = null;
			}
			// NOTE: LayoutList() doesn't use proper vertical spacing
			++EditorGUI.indentLevel;
			int indent = EditorGUI.indentLevel;
			Rect rect =
				EditorGUI.IndentedRect(EditorGUILayout.GetControlRect(false, m_LinkKeywordCollections.GetHeight()));
			EditorGUI.indentLevel = 0;
			m_LinkKeywordCollections.DoList(rect);
			EditorGUI.indentLevel = indent;
			rect =
				EditorGUI.IndentedRect(EditorGUILayout.GetControlRect(false, m_TagKeywordCollections.GetHeight()));
			EditorGUI.indentLevel = 0;
			m_TagKeywordCollections.DoList(rect);
			EditorGUI.indentLevel = indent;
			rect =
				EditorGUI.IndentedRect(EditorGUILayout.GetControlRect(false, m_QuadKeywordCollections.GetHeight()));
			EditorGUI.indentLevel = 0;
			m_QuadKeywordCollections.DoList(rect);
			EditorGUI.indentLevel = indent;
			--EditorGUI.indentLevel;
			bool isTextInputSourceAssigned =
				m_InputTextSourceObject.objectReferenceValue != null ||
				(this.target as HyperText).InputTextSource != null;
			EditorGUI.BeginDisabledGroup(isTextInputSourceAssigned);
			{
				EditorGUI.BeginChangeCheck();
				{
					EditorGUILayout.PropertyField(m_Text);
				}
				if (EditorGUI.EndChangeCheck())
				{
					ApplyModificationsAndUpdateGUIContents();
				}
			}
			EditorGUI.EndDisabledGroup();
			if (isTextInputSourceAssigned)
			{
				EditorGUIX.DisplayPropertyFieldWithStatus(
					m_InputTextSourceObject,
					ValidationStatus.Warning,
					s_InputTextSourceLabel,
					false,
					s_InputTextSourceLabel.tooltip
				);
			}
			else
			{
				EditorGUILayout.PropertyField(m_InputTextSourceObject, s_InputTextSourceLabel);
			}
			EditorGUILayout.LabelField("Character", EditorStyles.boldLabel);
			++EditorGUI.indentLevel;
			position = EditorGUILayout.GetControlRect();
			DisplayFontProperty(position, m_Font, m_Styles.objectReferenceValue as HyperTextStyles);
			position = EditorGUILayout.GetControlRect();
			DisplayOverridableProperty(position, m_FontStyle, m_ShouldOverrideStylesFontStyle, m_Styles);
			position = EditorGUILayout.GetControlRect();
			DisplayOverridableProperty(position, m_FontSize, m_ShouldOverrideStylesFontSize, m_Styles);
			position = EditorGUILayout.GetControlRect();
			DisplayOverridableProperty(
				position, m_LineSpacing, m_ShouldOverrideStylesLineSpacing, m_Styles
			);
			EditorGUILayout.PropertyField(m_RichText);
			--EditorGUI.indentLevel;
			EditorGUILayout.LabelField("Paragraph", EditorStyles.boldLabel);
			++EditorGUI.indentLevel;
			EditorGUILayout.PropertyField(m_Alignment);
			if (m_AlignByGeometry != null)
			{
				EditorGUILayout.PropertyField(m_AlignByGeometry);
			}
			EditorGUILayout.PropertyField(m_HorizontalOverflow);
			EditorGUILayout.PropertyField(m_VerticalOverflow);
			EditorGUILayout.PropertyField(m_BestFit);
			if (m_BestFit.boolValue)
			{
				++EditorGUI.indentLevel;
				EditorGUILayout.PropertyField(m_MinSize);
				EditorGUILayout.PropertyField(m_MaxSize);
				--EditorGUI.indentLevel;
			}
			--EditorGUI.indentLevel;
			position = EditorGUILayout.GetControlRect();
			DisplayOverridableProperty(position, m_Color, m_ShouldOverrideStylesFontColor, m_Styles);
			EditorGUILayout.PropertyField(m_Material, s_MaterialLabel);
			EditorGUILayout.PropertyField(m_QuadMaterial);
			EditorGUI.BeginChangeCheck();
			{
				EditorGUILayout.PropertyField(m_RaycastTarget);
				if (!string.IsNullOrEmpty(m_RaycastTargetWarningMessage))
				{
					position = GUILayoutUtility.GetLastRect();
					position.x += position.width - EditorGUIUtility.singleLineHeight;
					position.width = EditorGUIUtility.singleLineHeight;
					EditorGUIX.DisplayValidationStatusIcon(
						position, m_RaycastTargetWarningStatus, m_RaycastTargetWarningMessage
					);
				}
			}
			if (EditorGUI.EndChangeCheck())
			{
				ApplyModificationsAndUpdateGUIContents();
			}
			if (m_SubclassProperties.Count > 0)
			{
				EditorGUILayout.LabelField("Other Properties", EditorStyles.boldLabel);
				++EditorGUI.indentLevel;
				for (int i = 0; i < m_SubclassProperties.Count; ++i)
				{
					EditorGUILayout.PropertyField(m_SubclassProperties[i]);
				}
				--EditorGUI.indentLevel;
			}
			EditorGUILayout.LabelField("Events", EditorStyles.boldLabel);
			++EditorGUI.indentLevel;
			DisplayEventProperty(m_ClickedLink);
			DisplayEventProperty(m_EnteredLink);
			DisplayEventProperty(m_ExitedLink);
			DisplayEventProperty(m_PressedLink);
			DisplayEventProperty(m_ReleasedLink);
			--EditorGUI.indentLevel;
			this.serializedObject.ApplyModifiedProperties();
		}

		/// <summary>
		/// Updates any necessary GUI contents when something has changed.
		/// </summary>
		protected override void UpdateGUIContents()
		{
			base.UpdateGUIContents();
			m_RaycastTargetWarningMessage = null;
			m_RaycastTargetWarningStatus = ValidationStatus.None;
			if (m_RaycastTarget.boolValue || m_RaycastTarget.hasMultipleDifferentValues)
			{
				foreach (HyperText hyperText in this.targets)
				{
					if (hyperText == null)
					{
						continue;
					}
					using (ListPool<HyperText.LinkInfo>.Scope links = new ListPool<HyperText.LinkInfo>.Scope())
					{
						if (hyperText.GetLinks(links.List) == 0)
						{
							continue;
						}
					}
#if UNITY_4_6 || UNITY_4_7
					s_ClickHandlers.Clear();
					s_DownHandlers.Clear();
					s_ClickHandlers.AddRange(
						hyperText.GetComponentsInParent(
							typeof(UnityEngine.EventSystems.IPointerClickHandler), true
						).Cast<UnityEngine.EventSystems.IPointerClickHandler>()
					);
					s_DownHandlers.AddRange(
						hyperText.GetComponentsInParent(
							typeof(UnityEngine.EventSystems.IPointerDownHandler), true
						).Cast<UnityEngine.EventSystems.IPointerDownHandler>()
					);
#else
					hyperText.GetComponentsInParent(true, s_ClickHandlers);
					hyperText.GetComponentsInParent(true, s_DownHandlers);
#endif
					s_ClickHandlers.Remove(hyperText);
					s_DownHandlers.Remove(hyperText);
					if (s_ClickHandlers.Count == 0 || s_DownHandlers.Count == 0)
					{
						continue;
					}
					m_RaycastTargetWarningMessage = string.Format(
						"One or more {0} or {1} objects found upstream in hierarchy of {2}selected object with at least one link. " +
						"Links will block pointer input unless you disable the raycastTarget property.",
						typeof(UnityEngine.EventSystems.IPointerClickHandler).Name,
						typeof(UnityEngine.EventSystems.IPointerDownHandler).Name,
						this.targets.Length == 1 ? "" : "at least one "
					);
					m_RaycastTargetWarningStatus = ValidationStatus.Warning;
				}
			}
		}
	}
}