using UnityEngine.UI;
using UnityEngine;
using UnityEngine.Events;
using System.Collections;
using UnityEngine.EventSystems;

#if UNITY_EDITOR
using UnityEditor;
[CustomEditor(typeof(ButtonWithAnim))]
public class ButtonWithAnim_Editor : Editor
{
	GUIStyle labelStyle = new GUIStyle();
	GUIContent transformLabel = new GUIContent("Transform To Animate");
	GUIContent animTypeLabel = new GUIContent("Animation Type");
	GUIContent animCurveLabel = new GUIContent("Anim Curve");
	GUIContent animDurationLabel = new GUIContent("Anim Duration");
	GUIContent moveDistanceLabel = new GUIContent("Move Distance");
	ButtonWithAnim button;

	public override void OnInspectorGUI()
	{
		if (button == null)
		{
			button = (ButtonWithAnim)target;
			labelStyle.fontStyle = FontStyle.Bold;
			button.transition = Selectable.Transition.None;
		}

		EditorGUILayout.Space();

		EditorGUILayout.LabelField("Hierarchy", labelStyle);
		button.transformToAnimate = (RectTransform)EditorGUILayout.ObjectField(transformLabel, button.transformToAnimate, typeof(RectTransform), true);

		EditorGUILayout.Space();

		EditorGUILayout.LabelField("Animation", labelStyle);
		button.animationType = (ButtonWithAnim.AnimationTypes)EditorGUILayout.EnumPopup(animTypeLabel, button.animationType);
		if (button.animationType == ButtonWithAnim.AnimationTypes.Scale)
			button.animCurveScale = EditorGUILayout.CurveField(animCurveLabel, button.animCurveScale);
		else
			button.animCurveMove = EditorGUILayout.CurveField(animCurveLabel, button.animCurveMove);
		button.animDuration = EditorGUILayout.FloatField(animDurationLabel, button.animDuration);
		if ((button.animationType == ButtonWithAnim.AnimationTypes.MoveLeftRight) || (button.animationType == ButtonWithAnim.AnimationTypes.MoveUpDown))
			button.moveDistance = EditorGUILayout.FloatField(moveDistanceLabel, button.moveDistance);
		EditorGUILayout.Space();

		SerializedProperty onAnimStart = serializedObject.FindProperty("onAnimStart");
		EditorGUILayout.PropertyField(onAnimStart);
		SerializedProperty onAnimFinish = serializedObject.FindProperty("onAnimFinish");
		EditorGUILayout.PropertyField(onAnimFinish);
		if (GUI.changed)
			serializedObject.ApplyModifiedProperties();
	}
}
#endif

public class ButtonWithAnim : Button
{
	public enum AnimationTypes { Scale, MoveUpDown, MoveLeftRight };
	static readonly Vector2 vec2Up = Vector2.up;
	static readonly Vector2 vec2Right = Vector2.right;

	#region Inspector variables

	[Header("Hierarchy")]
	public RectTransform transformToAnimate = null;

	[Header("Animation")]
	public AnimationTypes animationType = AnimationTypes.Scale;
	public AnimationCurve animCurveScale = new AnimationCurve(new Keyframe(0f, 1f, 1f, 1f), new Keyframe(0.1f, 1.2f), new Keyframe(1f, 1f, -0.5f, -0.5f));
	public AnimationCurve animCurveMove = new AnimationCurve(new Keyframe(0f, 0f, 1f, 1f), new Keyframe(0.1f, 1f), new Keyframe(1f, 0f, -0.5f, -0.5f));
	public float animDuration = 0.2f;
	public float moveDistance = 10f;

	[Header("Events")]
	public UnityEvent onAnimStart = null;
	public UnityEvent onAnimFinish = null;

	#endregion // Inspector variables

	float animSpeed;

	/// <summary> Called when object/script is disabled in the hierarchy </summary>
	protected override void OnDisable()
	{
		StopAllCoroutines();
	}

	/// <summary> Wrapper for Button.OnPointerClick </summary>
	public override void OnPointerClick(PointerEventData eventData)
	{
		OnClick();
	}

	/// <summary> Wrapper for Button.OnClick </summary>
	public void OnClick()
	{
		if (onAnimStart != null)
			onAnimStart.Invoke();

		animSpeed = 1f / animDuration;

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
