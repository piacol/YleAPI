using UnityEngine;
using System.Collections;
using YleAPI;

namespace YleAPI.UI
{
	public class UIProgramListView : MonoBehaviour 
	{
		public GameObject programListItemParent;
		public GameObject programListItemPrefab;
		public UIProgramListItem[] programListItems = new UIProgramListItem[Constants.MaxProgramListItems];

		void Awake()
		{			
		}

		void Start()
		{
			if (programListItemPrefab != null) 
			{
				for (int i = 0; i < programListItems.Length; ++i) 
				{				
					GameObject go = GameObject.Instantiate (programListItemPrefab);

					if (go != null) 
					{						
						go.name = string.Format ("{0}{1:D2}", programListItemPrefab.name, i + 1);

						programListItems [i] = go.GetComponent<UIProgramListItem> ();
						programListItems [i].transform.SetParent (programListItemParent.transform);
					}
				}
			}
		}
	}
}
