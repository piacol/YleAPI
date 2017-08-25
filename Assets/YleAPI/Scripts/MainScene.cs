using UnityEngine;
using System.Collections;	
using System.Collections.Generic;	
using YleAPI.Net;
using YleAPI.UI;

namespace YleAPI
{
	public class ProgramInfo
	{
		public string id;
		public string title;
		public string description;
		public string startTime;
		public string type;
	}

	public class ProgramDetailsInfo
	{
        public string title1;      
		public string title2;
		public string description;
        public Sprite image;
		public int imageWidth;
		public int imageHeight;
		public string duration;
		public string startTime;
		public string type;
		public string region;
	}

	public class MainScene : MonoBehaviour, IMessageObject
	{
		public UISearchInputView uiSearchInputView;
		public UISearchProgramView uiSearchProgramView;
		public UIProgramView uiProgramView;
		
		private NetClient _netClient;
		private string _keyword;
		private int _offset;
		private int _metaCount;
		private List<ProgramInfo> _programInfos = new List<ProgramInfo>();
		private ProgramDetailsInfo _programDetailsInfo;
        private Coroutine _updateProgramDetailsInfoCoroutine;

        public bool MessageProc(eMessage type, object value)
        {
            switch(type)
            {
                case eMessage.SearchButtonClick:
                    {
                        OnSearchButtonClick((string)value);                                          
                        return true;
                    }
                case eMessage.SearchProgramViewEndDrag:
                    {
                        OnSearchProgramViewEndDrag();
                        return true;
                    }
                case eMessage.SearchProgramItemClick:
                    {                 
                        OnSearchProgramItemClick((string)value);
                        return true;
                    }
                case eMessage.ProgramViewBackButtonClick:
                    {
                        OnProgramViewBackButtonClick();
                        return true;
                    }
                   
            }

            return false;
        }
		
		void Awake()
		{
            MessageObjectManager.Instance.Add(eMessage.SearchButtonClick, this);
            MessageObjectManager.Instance.Add(eMessage.SearchProgramViewEndDrag, this);
            MessageObjectManager.Instance.Add(eMessage.SearchProgramItemClick, this);
            MessageObjectManager.Instance.Add(eMessage.ProgramViewBackButtonClick, this);

			_netClient = new NetClient ();
		}
		
		void Start()
		{
			ShowSearchView (true);
			ShowProgramView (false);	
		}

        void Destroy()
        {
            MessageObjectManager.Instance.Remove(this);
        }					        	

        private void OnSearchButtonClick(string keyword)
        {
            _keyword = keyword;
            _offset = 0;
            _metaCount = 0;

            _programInfos.Clear ();

            var programInfos = _netClient.GetPrograms (_keyword, ref _offset, ref _metaCount);

            _programInfos.AddRange(programInfos);

            uiSearchProgramView.UpdateView (_programInfos);
        }

        private void OnSearchProgramViewEndDrag()
        {
            if (_offset == _metaCount) 
            {
                return;
            }

            var programInfos = _netClient.GetPrograms (_keyword, ref _offset, ref _metaCount);

            _programInfos.AddRange(programInfos);

            uiSearchProgramView.AppendView (programInfos);
        }

        private void OnSearchProgramItemClick(string programID)
        {   
            if(_updateProgramDetailsInfoCoroutine != null)
            {
                StopCoroutine(_updateProgramDetailsInfoCoroutine);
                _updateProgramDetailsInfoCoroutine = null;
            }

            _updateProgramDetailsInfoCoroutine = StartCoroutine(UpdatingProgramDetailsInfo(programID));     
        }

        private void OnProgramViewBackButtonClick()
        {
            ShowSearchView (true);
            ShowProgramView (false);    
        }

        private IEnumerator UpdatingProgramDetailsInfo(string programID)
        {
            _programDetailsInfo = new ProgramDetailsInfo();

            yield return StartCoroutine(_netClient.GetProgramDetailsByID (_programDetailsInfo, programID));

            ShowSearchView (false);
            ShowProgramView (true);

            uiProgramView.UpdateView (_programDetailsInfo);

            _updateProgramDetailsInfoCoroutine = null;
        }   

        private void ShowSearchView(bool show)
        {
            uiSearchInputView.gameObject.SetActive (show);
            uiSearchProgramView.gameObject.SetActive (show);
        }

        private void ShowProgramView(bool show)
        {
            uiProgramView.gameObject.SetActive (show);
        }
	}
}