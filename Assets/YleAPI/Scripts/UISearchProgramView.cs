using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using YleAPI;

namespace YleAPI.UI
{
	public class UISearchProgramView : MonoBehaviour 
	{
		public GameObject programListItemParent;
		public GameObject programListItemPrefab;

		private List<UISearchProgramItem> searchProgramItems = new List<UISearchProgramItem>();

		void Awake()
		{			
		}

		void Start()
		{			
		}

		public void UpdateView(List<ProgramInfo> programInfos)
		{
			searchProgramItems.Clear ();

			while (programListItemParent.transform.childCount > 0) 
			{
				Destroy (programListItemParent.transform.GetChild (0));
			}

			AppendView (programInfos);
		}

		public void AppendView(List<ProgramInfo> programInfos)
		{
			int startIndex = searchProgramItems.Count;

			if (programListItemPrefab != null) 
			{
				for(int i = 0; i < programInfos.Count; i++) 
				{					
					GameObject go = GameObject.Instantiate (programListItemPrefab);

					if (go != null) 
					{					
						UISearchProgramItem item = go.GetComponent<UISearchProgramItem> ();
						item.transform.SetParent (programListItemParent.transform);
						searchProgramItems.Add (item);
					}
				}
			}

			for (int i = startIndex; i < searchProgramItems.Count; ++i) 
			{
				var item = searchProgramItems [i];

				item.uiTitle.text = programInfos [i].title;
			}
		}
	}
}
