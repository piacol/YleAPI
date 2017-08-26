using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using YleAPI;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace YleAPI.UI
{
	public class UISearchProgramView : MonoBehaviour
	{		
		public GameObject searchProgramItemParent;
		public GameObject searchProgramItemPrefab;
		public ScrollRect scrollRect;
		public Scrollbar scrollbar;

		private List<UISearchProgramItem> searchProgramItems = new List<UISearchProgramItem>();
        private float _maxScrollValueForAppendingItems;
        private float _lastScrollValue = 0;

		void Awake()
		{
            scrollbar.onValueChanged.AddListener(OnScroll);
		}

		void Start()
		{			
			scrollbar.gameObject.SetActive (false);
		}

		public void UpdateView(List<ProgramInfo> programInfos)
		{
			searchProgramItems.Clear ();
			scrollbar.value = 1;

			foreach (Transform child in searchProgramItemParent.transform) 
			{
				GameObject.Destroy (child.gameObject);
			}

			AppendView (programInfos);
		}

		public void AppendView(List<ProgramInfo> programInfos)
		{
			int startIndex = searchProgramItems.Count;

			if (searchProgramItemPrefab != null) 
			{
				for(int i = 0; i < programInfos.Count; i++) 
				{					
					GameObject go = GameObject.Instantiate (searchProgramItemPrefab);

					if (go != null) 
					{					
						UISearchProgramItem item = go.GetComponent<UISearchProgramItem> ();

						item.transform.SetParent (searchProgramItemParent.transform);						

						searchProgramItems.Add (item);
					}
				}
			}

			for (int i = 0; i < programInfos.Count; ++i) 
			{				
				var item = searchProgramItems [i + startIndex];

				item.UpdateItem (programInfos [i]);
			}

			bool scrollbarActive = searchProgramItems.Count > 0 ? true : false;

			scrollbar.gameObject.SetActive (scrollbarActive);

            _maxScrollValueForAppendingItems = 1 - (float)(searchProgramItems.Count - 3) / (float)searchProgramItems.Count;
		}     

        private void OnScroll(float value)
        {
            if(value < _lastScrollValue)
            {       
                return;
            }
            else
            {
                _lastScrollValue = 0;
            }
            
            if (value < _maxScrollValueForAppendingItems)
            {      
                _lastScrollValue = value;
                _maxScrollValueForAppendingItems = 0; 

                MessageObjectManager.Instance.SendMessageToAll(eMessage.SearchProgramViewEndDrag);
            }
        }
	}
}
