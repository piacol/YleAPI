using UnityEngine;
using System.Collections;
using UnityEngine.UI;

namespace YleAPI.UI
{
	public class UISearchInputView : MonoBehaviour 
	{
		public InputField uiSearchInputField;
		public Button uiSearchButton;

		void Awake()
		{
			uiSearchButton.onClick.AddListener (OnSearchButtonClick);
		}

		private void OnSearchButtonClick()
		{			
			Debug.Log ("(uiSearchInputField.text)" + uiSearchInputField.text);
		}
	}
}
