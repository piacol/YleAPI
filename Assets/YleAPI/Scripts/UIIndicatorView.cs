using UnityEngine;
using System.Collections;
using UnityEngine.UI;

namespace YleAPI.UI
{    
	public class UIIndicatorView : MonoBehaviour 
	{		
        public enum Mode
        {            
            Networking,
            LogMessage,
        }

		public Text uiIndicator;
        		
		private string[] _displayTexts = { "Networking", "Networking.", "Networking..", "Networking..."};
        private Mode _mode = Mode.Networking;
        private string _logMessage;

        public void Show(bool show, UIIndicatorView.Mode mode, string logMessage)
        {
            gameObject.SetActive(show);

            _mode = mode;
            _logMessage = logMessage;

            StopAllCoroutines();

            if(show == true)
            {
                if(_mode == Mode.Networking)
                {
                    StartCoroutine(ShowingNetworking());
                }
                else
                {
                    StartCoroutine(ShowingLogMessage());
                }
            }
        }

        private IEnumerator ShowingNetworking()
        {
            float time = 0;
            float duration = 0.5f;
            int displayIndex = 0;

            if (uiIndicator != null) 
            {
                uiIndicator.text = _displayTexts [displayIndex];           
            }

            do
            {
                yield return null;                

                time += Time.deltaTime;

                if (time > duration) 
                {
                    time -= duration;

                    displayIndex++;
                    displayIndex %= _displayTexts.Length;

                    if (uiIndicator != null) 
                    {
                        uiIndicator.text = _displayTexts [displayIndex];           
                    }
                }

            }while(true);
        }

        private IEnumerator ShowingLogMessage()
        {
            float time = 0;
            float duration = 3;

            if (uiIndicator != null) 
            {
                uiIndicator.text = _logMessage;
            }

            do
            {
                yield return null;                

                time += Time.deltaTime;

            }while(time < duration);

            gameObject.SetActive(false);
        }
	}
}