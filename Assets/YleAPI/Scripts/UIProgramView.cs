using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Text;

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
		public Text uiDetails;

		private LayoutElement _imageLayoutElement;
		private StringBuilder _sb = new StringBuilder();
        private int _screenWidth;
        private int _screenHeight;

		void Awake()
		{
			if (uiBackButton != null) 
			{
				uiBackButton.onClick.AddListener (OnBackButtonClick);
			}

			if (uiImage != null) 
			{
				_imageLayoutElement = uiImage.GetComponent<LayoutElement> ();
			}

            _screenWidth = Screen.width;
            _screenHeight = Screen.height;
		}

        void Update()
        {
            if(_screenWidth != Screen.width || _screenHeight != Screen.height)
            {
                _screenWidth = Screen.width;
                _screenHeight = Screen.height;   

                UpdateImageLayoutElement();
            }
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
                if(info.image != null)
                {
                    uiImage.gameObject.SetActive (true);
                    uiImage.sprite = info.image;

                    UpdateImageLayoutElement();
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

			if (uiDetails != null) 
			{
				_sb.Length = 0;

				if (string.IsNullOrEmpty (info.duration) == false) 
				{
					_sb.Append (info.duration);
					_sb.Append (" | ");
				}

				if (string.IsNullOrEmpty (info.startTime) == false) 
				{
					_sb.Append (info.startTime);
					_sb.Append (" | ");
				}

				if (string.IsNullOrEmpty (info.type) == false) 
				{
					_sb.Append (info.type);
					_sb.Append (" | ");
				}

				_sb.Append (info.region);

				uiDetails.text = _sb.ToString ();
			}
		}

		private void OnBackButtonClick()
		{
            MessageObjectManager.Instance.SendMessageToAll(eMessage.ProgramViewBackButtonClick);
		}       

        private void UpdateImageLayoutElement()
        {
            if(uiImage == null ||
                uiImage.sprite == null ||
                _imageLayoutElement == null)
            {
                return;
            }
            
            float height = (float)Screen.width * (float)uiImage.sprite.texture.height / (float)uiImage.sprite.texture.width;
            int marginY = 15;

            _imageLayoutElement.preferredHeight = (int)height + marginY;
        }
	}
}
