using UnityEngine;
using System.Collections;
using UnityEngine.UI;

namespace YleAPI.UI
{
	public class UISearchInputView : MonoBehaviour 
	{
		public MainScene parent;
		public InputField uiSearchInputField;
		public Button uiSearchButton;

		void Awake()
		{
			uiSearchButton.onClick.AddListener (OnSearchButtonClick);
		}

		private void OnSearchButtonClick()
		{	
			if (string.IsNullOrEmpty (uiSearchInputField.text) == true) 
			{
				return;
			}

			parent.InitProgramInfos (uiSearchInputField.text);
		}
	}
}
