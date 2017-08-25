using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Text;

namespace YleAPI.UI
{
	public class UISearchProgramItem : MonoBehaviour 
	{		
		public Button uiButton;
		public Text uiTitle;
		public Text uiDetails;
		public Text uiDescription;

		private string _programID;
		private StringBuilder _sb = new StringBuilder();

		void Awake()
		{
			uiButton.onClick.AddListener (OnButtonClick);
		}

		private void OnButtonClick()
		{
			MessageObjectManager.Instance.SendMessageToAll(eMessage.SearchProgramItemClick, _programID);
		}

		public void UpdateItem(ProgramInfo info)
		{
			_programID = info.id;

			if (uiTitle != null) 
			{
				uiTitle.text = info.title;
			}

			if (uiDetails != null) 
			{
				_sb.Length = 0;

				if (string.IsNullOrEmpty (info.startTime) == false) 
				{
					_sb.Append (info.startTime);
					_sb.Append (" | ");
				}

				if (string.IsNullOrEmpty (info.type) == false) 
				{
					_sb.Append (info.type);
				}

				uiDetails.text = _sb.ToString ();
			}

			if (uiDescription != null) 
			{
				uiDescription.text = info.description;
			}
		}
	}
}
