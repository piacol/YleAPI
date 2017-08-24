using UnityEngine;
using UnityEngine.UI;
using System.Collections;

namespace YleAPI.UI
{
	public class UIProgramView : MonoBehaviour 
	{
		public MainScene parent;
		public Button uiBackButton;
		public Text uiTitle;
		public RawImage uiImage;
		public Text uiDescription;
		public Scrollbar uiScrollbar;

		void Awake()
		{
			if (uiBackButton != null) 
			{
				uiBackButton.onClick.AddListener (OnBackButtonClick);
			}
		}

		void Start()
		{			
		}

		public void UpdateView(ProgramDetailsInfo info)
		{			
			uiScrollbar.value = 1;

			if (uiTitle != null) 
			{
				uiTitle.text = info.title;
			}

			if (uiImage != null) 
			{		
				uiImage.gameObject.SetActive (false);
			}

			if (uiDescription != null) 
			{
				uiDescription.text = info.description;
			}
		}

		private void OnBackButtonClick()
		{
			parent.OnProgramViewBackButtonClick ();
		}
	}
}
