using UnityEngine;

public class UI_MainMenu : MonoBehaviour
{
	AudioSource audioSource;
	
	public void PlayAudio(AudioClip _audioClip)
	{
		if (audioSource == null)
			audioSource = GetComponent<AudioSource>();

		audioSource.clip = _audioClip;
		audioSource.Play();
	}

	/// <summary> Called from the Button's OnClick event </summary>
	public void DoublePopupButtonPressed()
	{
		UIMaster.instance.popups.ShowOrEnqueue(new PopupManager.PopupInfo()
		{
			_popupType = PopupManager.PopupTypes.Default,
			title = "First Popup",
			messageBody = "There should be another popup after this...",
			confirmText = "Show Next",
		});
		UIMaster.instance.popups.ShowOrEnqueue(new PopupManager.PopupInfo()
		{
			_popupType = PopupManager.PopupTypes.Default,
			title = "Second Popup",
			messageBody = "This is the second popup! Next one should slide in...",
		});
		UIMaster.instance.popups.ShowOrEnqueue(new PopupManager.PopupInfo()
		{
			_popupType = PopupManager.PopupTypes.Slide,
			title = "Third Popup",
			messageBody = "This one slid in! It will slide out and another slide-in one will take its place...",
		});
		UIMaster.instance.popups.ShowOrEnqueue(new PopupManager.PopupInfo()
		{
			_popupType = PopupManager.PopupTypes.Slide,
			title = "Fourth Popup",
			messageBody = "This one slid in too! After this one slides out, a Debug Log will appear, then another scaled popup will bouce in...",
			confirmText = "Show Confirm Log!",
			confirmCallback = () => { Debug.Log("Confirm callback works!"); },
			cancelText = "Show Cancel Log!",
			cancelCallback = () => { Debug.Log("Cancel callback works!"); },
		});
		UIMaster.instance.popups.ShowOrEnqueue(new PopupManager.PopupInfo()
		{
			_popupType = PopupManager.PopupTypes.Default,
			title = "Fifth Popup",
			messageBody = "This one bounced again! This is the last popup.",
		});
	}

	/// <summary> Called from the Button's OnClick event </summary>
	public void QuitButtonPressed()
	{
		UIMaster.instance.popups.ShowOrEnqueue(new PopupManager.PopupInfo()
		{
			_popupType = PopupManager.PopupTypes.Default,
			title = "Quit?",
			messageBody = "Are you sure you want to quit?",
			confirmText = "Quit",
			cancelText = "Cancel",
			confirmCallback = Quit,
		});
	}

	/// <summary> Quits & closes the game </summary>
	void Quit()
	{
		Debug.Log("Quit - Closing application!");
		Application.Quit();
	}
}
