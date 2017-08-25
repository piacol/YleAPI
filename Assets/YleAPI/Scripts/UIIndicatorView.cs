using UnityEngine;
using System.Collections;
using UnityEngine.UI;

namespace YleAPI.UI
{
	public class UIIndicatorView : MonoBehaviour 
	{		
		public Text uiIndicator;

		private const float _delayTime = 0.5f;
		private float _time = 0;
		private string[] _displayTexts = { "Networking", "Networking.", "Networking..", "Networking..."};
		private int _displayIndex = 0;

		void OnEnable()
		{
			_time = 0;
			_displayIndex = 0;

			UpdateIndicatorText ();
		}

		void Start()
		{
			UpdateIndicatorText ();
		}

		void Update()
		{
			_time += Time.deltaTime;

			if (_time > _delayTime) 
			{
				_time -= _delayTime;

				_displayIndex++;
				_displayIndex %= _displayTexts.Length;

				UpdateIndicatorText ();
			}
		}

		void UpdateIndicatorText()
		{
			if (uiIndicator != null) 
			{
				uiIndicator.text = _displayTexts [_displayIndex];			
			}
		}
	}
}