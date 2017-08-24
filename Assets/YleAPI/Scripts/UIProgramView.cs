using UnityEngine;
using UnityEngine.UI;
using System.Collections;

namespace YleAPI.UI
{
	public class UIProgramView : MonoBehaviour 
	{		
		public Button uiBackButton;
		public Text uiTitle1;
        public Text uiTitle2;
		public Image uiImage;
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

			if (uiTitle1 != null) 
			{
                if(info.title1 != null)
                {
                    uiTitle1.gameObject.SetActive(true);

				    uiTitle1.text = info.title1;
                }
                else
                {
                    uiTitle1.gameObject.SetActive(false);
                }
			}

            if (uiTitle2 != null) 
            {
                uiTitle2.text = info.title2;
            }

			if (uiImage != null) 
			{		
                if(info.sprite != null)
                {
                    uiImage.gameObject.SetActive (true);
                    uiImage.sprite = info.sprite;
                }
                else
                {
				    uiImage.gameObject.SetActive (false);
                }
			}

			if (uiDescription != null) 
			{
				uiDescription.text = info.description;
			}
		}

		private void OnBackButtonClick()
		{
            MessageObjectManager.Instance.SendMessageToAll(eMessage.ProgramViewBackButtonClick);
		}
	}
}
