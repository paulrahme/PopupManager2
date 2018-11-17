using UnityEngine;

public class UIMaster : MonoBehaviour
{
	public UI_MainMenu mainMenu;
	public PopupManager popups;

	/// <summary> Singleton instance </summary>
	public static UIMaster instance;

	/// <summary> Called when object/script activates </summary>
	void Awake()
	{
		if (instance != null)
			throw new UnityException("Singleton instance already exists");
		instance = this;
	}
}
