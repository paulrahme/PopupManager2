using UnityEngine.UI;
using UnityEngine;
using UnityEngine.Events;
using System.Collections;
using UnityEngine.EventSystems;

#if UNITY_EDITOR
using UnityEditor;
[CustomEditor(typeof(SuperButton))]
public class SuperButton_Editor : Editor
{
	GUIStyle labelStyle = new GUIStyle();
	GUIContent interactableLabel = new GUIContent("Interactable");
	GUIContent transformLabel = new GUIContent("Transform To Animate");
	GUIContent animTypeLabel = new GUIContent("Animation Type");
	GUIContent animCurveLabel = new GUIContent("Anim Curve");
	GUIContent animDurationLabel = new GUIContent("Anim Duration");
	GUIContent moveDistanceLabel = new GUIContent("Move Distance");
	GUIContent disableTypeLabel = new GUIContent("Disable Type");
	GUIContent disableCanvasGroupLabel = new GUIContent("Canvas Group To Alpha Out");
	GUIContent disableAlphaLabel = new GUIContent("Alpha Amount");
	GUIContent lockOthersDuringAnimLabel = new GUIContent("Lock Others During Anim");
	GUIContent lockCooldownAfterAnimLabel = new GUIContent("Lock Cooldown After Anim");
	SuperButton button;

	public override void OnInspectorGUI()
	{
		if (button == null)
		{
			button = (SuperButton)target;
			labelStyle.fontStyle = FontStyle.Bold;
			button.transition = Selectable.Transition.None;
		}

		EditorGUILayout.Space();

		EditorGUILayout.LabelField("Button", labelStyle);
		bool interactable = EditorGUILayout.Toggle(interactableLabel, button.interactable);
		if (interactable != button.interactable)	// Avoids it refreshing on every frame even when it hasn't changed
			button.interactable = interactable;			

		EditorGUILayout.Space();

		EditorGUILayout.LabelField("Animation", labelStyle);
		button.transformToAnimate = (RectTransform)EditorGUILayout.ObjectField(transformLabel, button.transformToAnimate, typeof(RectTransform), true);
		button.animationType = (SuperButton.AnimationTypes)EditorGUILayout.EnumPopup(animTypeLabel, button.animationType);
		switch (button.animationType)
		{
			case SuperButton.AnimationTypes.Scale:
				button.animCurveScale = EditorGUILayout.CurveField(animCurveLabel, button.animCurveScale);
				break;

			case SuperButton.AnimationTypes.MoveUpDown:
			case SuperButton.AnimationTypes.MoveLeftRight:
				button.animCurveMove = EditorGUILayout.CurveField(animCurveLabel, button.animCurveMove);
				break;
		}
		button.animDuration = EditorGUILayout.FloatField(animDurationLabel, button.animDuration);
		if ((button.animationType == SuperButton.AnimationTypes.MoveLeftRight) || (button.animationType == SuperButton.AnimationTypes.MoveUpDown))
			button.moveDistance = EditorGUILayout.FloatField(moveDistanceLabel, button.moveDistance);

		EditorGUILayout.Space();

		EditorGUILayout.LabelField("Disabled Effect", labelStyle);
		button.disableType = (SuperButton.DisableTypes)EditorGUILayout.EnumPopup(disableTypeLabel, button.disableType);
		switch (button.disableType)
		{
			case SuperButton.DisableTypes.CanvasGroupAlpha:
				button.disableCanvasGroup = (CanvasGroup)EditorGUILayout.ObjectField(disableCanvasGroupLabel, button.disableCanvasGroup, typeof(CanvasGroup), true);
				button.disableCanvasGroupAlpha = EditorGUILayout.FloatField(disableAlphaLabel, button.disableCanvasGroupAlpha);
				break;

			case SuperButton.DisableTypes.HierarchySwap:
				SerializedProperty enableWhenInteractable = serializedObject.FindProperty("enableWhenInteractable");
				EditorGUILayout.PropertyField(enableWhenInteractable, true);
				SerializedProperty enableWhenNotInteractable = serializedObject.FindProperty("enableWhenNotInteractable");
				EditorGUILayout.PropertyField(enableWhenNotInteractable, true);
				break;
		}

		EditorGUILayout.Space();

		EditorGUILayout.LabelField("Cooldown (Locks Out Other Buttons)", labelStyle);

		button.lockOtherButtonsDuringAnim = EditorGUILayout.Toggle(lockOthersDuringAnimLabel, button.lockOtherButtonsDuringAnim);
		button.lockCooldownAfterAnim = EditorGUILayout.FloatField(lockCooldownAfterAnimLabel, button.lockCooldownAfterAnim);

		SerializedProperty onAnimStart = serializedObject.FindProperty("onAnimStart");
		EditorGUILayout.PropertyField(onAnimStart);
		SerializedProperty onAnimFinish = serializedObject.FindProperty("onAnimFinish");
		EditorGUILayout.PropertyField(onAnimFinish);

		if (GUI.changed)
			serializedObject.ApplyModifiedProperties();
	}
}
#endif

public class SuperButton : Button
{
	public enum AnimationTypes { Scale, MoveUpDown, MoveLeftRight };
	public enum DisableTypes { CanvasGroupAlpha, HierarchySwap };
	static readonly Vector2 vec2Up = Vector2.up;
	static readonly Vector2 vec2Right = Vector2.right;

	#region Inspector variables

	/// <summary> Wrapper for Button's "interactable" property, which also refreshes elements </summary>
	public new bool interactable
	{
		get { return base.interactable; }
		set
		{
			base.interactable = value;
			RefreshInteractable(value);
		}
	}

	[Header("Animation")]
	public RectTransform transformToAnimate = null;
	public AnimationTypes animationType = AnimationTypes.Scale;
	public AnimationCurve animCurveScale = new AnimationCurve(new Keyframe(0f, 1f, 1f, 1f), new Keyframe(0.1f, 1.2f), new Keyframe(1f, 1f, -0.5f, -0.5f));
	public AnimationCurve animCurveMove = new AnimationCurve(new Keyframe(0f, 0f, 1f, 1f), new Keyframe(0.1f, 1f), new Keyframe(1f, 0f, -0.5f, -0.5f));
	public float animDuration = 0.2f;
	public float moveDistance = 10f;

	[Header("Disabling")]
	public DisableTypes disableType = DisableTypes.CanvasGroupAlpha;
	public CanvasGroup disableCanvasGroup = null;
	[Range(0, 1)] public float disableCanvasGroupAlpha = 0.5f;
	public GameObject[] enableWhenInteractable = null;
	public GameObject[] enableWhenNotInteractable = null;

	[Header("Events")]
	public UnityEvent onAnimStart = null;
	public UnityEvent onAnimFinish = null;

	[Header("Cooldown")]
	public bool lockOtherButtonsDuringAnim = true;
	public float lockCooldownAfterAnim = 0.3f;

	#endregion // Inspector variables

	float animSpeed;
	static float cooldownEndTime;

	/// <summary> Called when object/script is disabled in the hierarchy </summary>
	protected override void OnDisable()
	{		
		StopAllCoroutines();
	}

	/// <summary> Refreshes elements for interactable/disabled state </summary>
	/// <param name="_interactable"> True if button's interactable, false to show disabled state </param>
	void RefreshInteractable(bool _interactable)
	{
		switch (disableType)
		{
			case DisableTypes.CanvasGroupAlpha:
				disableCanvasGroup.alpha = (_interactable ? 1.0f : disableCanvasGroupAlpha);
				break;

			case DisableTypes.HierarchySwap:
				for (int i = 0; i < enableWhenInteractable.Length; ++i)
					enableWhenInteractable[i].SetActive(_interactable);
				for (int i = 0; i < enableWhenNotInteractable.Length; ++i)
					enableWhenNotInteractable[i].SetActive(!_interactable);
				break;
		}
	}

	/// <summary> Wrapper for Button.OnPointerClick </summary>
	public override void OnPointerClick(PointerEventData eventData)
	{
		OnClick();
	}

	/// <summary> Wrapper for Button.OnClick </summary>
	public void OnClick()
	{
		if (!interactable || (Time.time < cooldownEndTime))
			return;

		if (onAnimStart != null)
			onAnimStart.Invoke();

		animSpeed = 1f / animDuration;

		if (lockOtherButtonsDuringAnim)
			cooldownEndTime = Time.time + animDuration + 0.05f;

		switch (animationType)
		{
			case AnimationTypes.Scale:
				StartCoroutine(StartAnimScale());
				break;

			case AnimationTypes.MoveUpDown:
			case AnimationTypes.MoveLeftRight:
				StartCoroutine(StartAnimMove());
				break;

			default:
				throw new UnityException("Unhandled animation type " + animationType);
		}
	}

	/// <summary> Called when animation has completed </summary>
	public void OnAnimComplete()
	{
		cooldownEndTime = Time.time + lockCooldownAfterAnim;

		if (onAnimFinish != null)
			onAnimFinish.Invoke();
	}

	/// <summary> Performs the button's animation, triggering its callback when complete </summary>
	IEnumerator StartAnimScale()
	{
		Vector3 origScale = transformToAnimate.localScale;

		float progress = 0f;
		while (progress < 1f)
		{
			progress += animSpeed * Time.deltaTime;
			transformToAnimate.localScale = origScale * animCurveScale.Evaluate(progress);
			yield return null;
		}

		OnAnimComplete();
	}

	/// <summary> Performs the button's animation, triggering its callback when complete </summary>
	IEnumerator StartAnimMove()
	{
		Vector2 origPos = transformToAnimate.anchoredPosition;
		Vector2 direction = moveDistance * ((animationType == AnimationTypes.MoveUpDown) ? vec2Up : vec2Right);

		float progress = 0f;
		while (progress < 1f)
		{
			progress += animSpeed * Time.deltaTime;
			transformToAnimate.localPosition = origPos + (direction * animCurveMove.Evaluate(progress));
			yield return null;
		}

		OnAnimComplete();
	}
}
