using UnityEngine;

public class UI_MainMenu : MonoBehaviour
{
	/// <summary> Called from the Button's OnClick event </summary>
	public void DoublePopupButtonPressed()
	{
		UIMaster.instance.popups.ShowOrEnqueue(new PopupManager.PopupInfo()
		{
			_popupType = PopupManager.PopupTypes.Default,
			title = "First Popup",
			messageBody = "There should be another popup after this...",
			confirmText = "Show Next",
			cancelText = "Show Next",
		});
		UIMaster.instance.popups.ShowOrEnqueue(new PopupManager.PopupInfo()
		{
			_popupType = PopupManager.PopupTypes.Default,
			title = "Second Popup",
			messageBody = "This is the second popup!",
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
