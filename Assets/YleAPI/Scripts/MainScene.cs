using UnityEngine;
using System.Collections;	
using System.Collections.Generic;	
using YleAPI.Net;
using YleAPI.UI;

namespace YleAPI
{
	public class ProgramInfo
	{
		public string title;
		public string description;
		public string longDescription;
	}

	public class ProgramDetailsInfo
	{
		
	}

	public class MainScene : MonoBehaviour 
	{
		public UISearchProgramView uiSearchProgramView;
		public UIProgramView uiProgramView;
		
		private NetClient _netClient;
		private string _keyword;
		private int _offset;
		private int _metaCount;
		private List<ProgramInfo> _programInfos = new List<ProgramInfo>();
		
		void Awake()
		{
			_netClient = new NetClient ();
		}
		
		void Start()
		{	
		}

		public void InitProgramInfos(string keyword)
		{			
			_keyword = keyword;
			_offset = 0;
			_metaCount = 0;

			_programInfos.Clear ();

			var programInfos = _netClient.GetPrograms (_keyword, ref _offset, ref _metaCount);

			_programInfos.AddRange(programInfos);

			uiSearchProgramView.UpdateView (_programInfos);
		}

		public void AppendProgramInfos()
		{
			if (_offset == _metaCount) 
			{
				return;
			}

			var programInfos = _netClient.GetPrograms (_keyword, ref _offset, ref _metaCount);

			_programInfos.AddRange(programInfos);

			uiSearchProgramView.AppendView (programInfos);
		}
	}
}