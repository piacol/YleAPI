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
		public MainScene parent;
		public GameObject searchProgramItemParent;
		public GameObject searchProgramItemPrefab;
		public ScrollRect scrollRect;
		public Scrollbar scrollbar;

		private List<UISearchProgramItem> searchProgramItems = new List<UISearchProgramItem>();

		void Awake()
		{			
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

				int number = i + startIndex + 1;

				item.uiTitle.text = "(" + number + ")" + programInfos [i].title;
			}

			bool scrollbarActive = searchProgramItems.Count > 0 ? true : false;

			scrollbar.gameObject.SetActive (scrollbarActive);
		}

		public void OnEndDrag(PointerEventData data)
		{	
			if (scrollbar.value == 0) 
			{
				parent.AppendProgramInfos ();
			}
		}
	}
}
