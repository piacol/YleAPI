using UnityEngine;
using System.Collections;
using UnityEngine.UI;

namespace YleAPI.UI
{
	public class UISearchProgramItem : MonoBehaviour 
	{
		public MainScene parent;
		public Button uiButton;
		public Text uiTitle;

		private string _programID;

		void Awake()
		{
			uiButton.onClick.AddListener (OnButtonClick);
		}

		private void OnButtonClick()
		{
			parent.UpdateProgramDetailsInfo (_programID);
		}

		public void SetProgramID(string id)
		{
			_programID = id;
		}
	}
}
