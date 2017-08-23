using UnityEngine;
using System.Collections;	
using System.Collections.Generic;	
using YleAPI.Net;

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
		private NetClient _netClient;
		private string _keyword;
		private int _currentOffset;
		private List<ProgramInfo> _programInfos = new List<ProgramInfo>();
		
		void Awake()
		{
			_netClient = new NetClient ();
		}
		
		void Start()
		{			
			InitProgramInfos ("hero");
		}

		private void InitProgramInfos(string keyword)
		{			
			_keyword = keyword;
			_currentOffset = 0;

			_programInfos.Clear ();

			var programInfos = _netClient.GetPrograms (_keyword, ref _currentOffset);

			_programInfos.AddRange(programInfos);
		}

		private void AppendProgramInfos()
		{
		}
	}
}