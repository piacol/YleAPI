using UnityEngine;
using System.Collections;
using UnityEngine.UI;

namespace YleAPI.UI
{
	public class UISearchProgramItem : MonoBehaviour 
	{		
		public Button uiButton;
		public Text uiTitle;

		private string _programID;

		void Awake()
		{
			uiButton.onClick.AddListener (OnButtonClick);
		}

		private void OnButtonClick()
		{
			MessageObjectManager.Instance.SendMessageToAll(eMessage.SearchProgramItemClick, _programID);
		}

		public void SetProgramID(string id)
		{
			_programID = id;
		}
	}
}
