using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using YleAPI;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace YleAPI.UI
{
	public class UISearchProgramView : MonoBehaviour, IEndDragHandler
	{		
		public GameObject searchProgramItemParent;
		public GameObject searchProgramItemPrefab;
		public ScrollRect scrollRect;
		public Scrollbar scrollbar;

		private List<UISearchProgramItem> searchProgramItems = new List<UISearchProgramItem>();
        private float _minScrollValueForAppendingItems = 0;
        private float _maxScrollValueForPrependingItems = 1;
        private RectTransform _rt;
        private float _itemHeight;
        private List<UISearchProgramItem> _cachedProgramItems = new List<UISearchProgramItem>();

		void Awake()
		{            
            _rt = GetComponent<RectTransform>();

            if(searchProgramItemPrefab != null &&
                searchProgramItemParent != null)
            {
                _itemHeight = searchProgramItemPrefab.GetComponent<LayoutElement>().preferredHeight;
                _itemHeight += searchProgramItemParent.GetComponent<VerticalLayoutGroup>().spacing;
            }
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
            if(searchProgramItems.Count + programInfos.Count > Constants.MaxSearchProgramBufferCount)
            {                
                float sp = _rt.rect.height; // start position (display height)
                float b = _itemHeight; // block size (item height)
                float t = Constants.MaxSearchProgramBufferCount;
                float v = scrollbar.value; // current value of scrollbar
                float c = t*b * (1 - v) + v*sp; // current position of scrollbar
                float n = programInfos.Count; // added block count in the end
                float newV = 1 - ((c - n*b) - sp) / (t*b - sp); // new value of scrollbar

                scrollbar.value = newV;

                int i = 0;
                int deleteCount = searchProgramItems.Count + programInfos.Count - Constants.MaxSearchProgramBufferCount;

                foreach (Transform child in searchProgramItemParent.transform) 
                {
                    if(i < deleteCount)
                    {                    
                        GameObject.Destroy (child.gameObject);
                    }
                    else
                    {
                        break;
                    }

                    i++;
                }

                searchProgramItems.RemoveRange(0, deleteCount);
            }

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

            _minScrollValueForAppendingItems = 1 - (float)(searchProgramItems.Count - Constants.ProgramCountForUpdatingBuffer) / (float)searchProgramItems.Count;

            if(scrollbar.value < 1)                
            {            
                _maxScrollValueForPrependingItems = 1 - _minScrollValueForAppendingItems;
            }
		}  

        public void PrependView(List<ProgramInfo> programInfos)
        {            
            if(searchProgramItems.Count + programInfos.Count > Constants.MaxSearchProgramBufferCount)
            {                
                float sp = _rt.rect.height; // start position (display height)
                float b = _itemHeight; // block size (item height)
                float t = Constants.MaxSearchProgramBufferCount;
                float v = scrollbar.value; // current value of scrollbar
                float c = t*b * (1 - v) + v*sp; // current position of scrollbar
                float n = programInfos.Count; // added block count in the front
                float newV = 1 - ((c + n*b) - sp) / (t*b - sp); // new value of scrollbar

                scrollbar.value = newV;

                int deleteCount = searchProgramItems.Count + programInfos.Count - Constants.MaxSearchProgramBufferCount;
                Transform parentTransform = searchProgramItemParent.transform;
                int childCount = parentTransform.childCount;
                Transform child;

                for(int i = 0; i < deleteCount; i++)
                {
                    child = parentTransform.GetChild(childCount - i - 1);

                    GameObject.Destroy(child.gameObject);
                }

                searchProgramItems.RemoveRange(searchProgramItems.Count - deleteCount, deleteCount);
            }

            if (searchProgramItemPrefab != null) 
            {
                _cachedProgramItems.Clear();
                
                for(int i = 0; i < programInfos.Count; i++) 
                {                   
                    GameObject go = GameObject.Instantiate (searchProgramItemPrefab);

                    if (go != null) 
                    {                   
                        UISearchProgramItem item = go.GetComponent<UISearchProgramItem> ();

                        item.transform.SetParent (searchProgramItemParent.transform);
                        item.transform.SetSiblingIndex(i);

                        _cachedProgramItems.Add(item);
                    }
                }

                searchProgramItems.InsertRange(0, _cachedProgramItems);
            }

            for (int i = 0; i < programInfos.Count; ++i) 
            {               
                var item = searchProgramItems [i];

                item.UpdateItem (programInfos [i]);
            }

            bool scrollbarActive = searchProgramItems.Count > 0 ? true : false;

            scrollbar.gameObject.SetActive (scrollbarActive);

            _maxScrollValueForPrependingItems = 1 - (Constants.ProgramCountForUpdatingBuffer / (float)searchProgramItems.Count);
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            if (scrollbar.value < _minScrollValueForAppendingItems)
            {   
                MessageObjectManager.Instance.SendMessageToAll(eMessage.SearchProgramViewAppend);
            }
            else if(scrollbar.value > _maxScrollValueForPrependingItems)
            {        
                MessageObjectManager.Instance.SendMessageToAll(eMessage.SearchProgramViewPrepend);
            }
        }
	}
}
