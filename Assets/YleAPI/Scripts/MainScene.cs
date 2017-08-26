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
		//public int imageWidth;
		//public int imageHeight;
		public string duration;
		public string startTime;
		public string type;
		public string region;
	}

	public class SearchProgramParameter
	{		
		public string keyword;
		public int offset;
		public int metaCount;
		public int searchCount;
		public List<ProgramInfo> programInfos = new List<ProgramInfo>();
	}

	public class MainScene : MonoBehaviour, IMessageObject
	{
		public UISearchInputView uiSearchInputView;
		public UISearchProgramView uiSearchProgramView;
		public UIProgramView uiProgramView;
		public UIIndicatorView uiIndicatorView;
		
		private NetClient _netClient;
		private string _keyword;
		private int _offset;
		private int _metaCount;
		private List<ProgramInfo> _programInfos = new List<ProgramInfo>();
		private ProgramDetailsInfo _programDetailsInfo;
		private Coroutine _updatingProgramsCoroutine;
		private Coroutine _appendingProgramsCoroutine;
		private Coroutine _updatingProgramDetailsInfoCoroutine;

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
			ShowIndicatorView (false);
		}

        void Destroy()
        {
			_netClient.Release ();
            MessageObjectManager.Instance.Remove(this);
        }	

        private void OnSearchButtonClick(string keyword)
        {
			if (_updatingProgramsCoroutine != null) 
			{
				return;
			}
			
            _keyword = keyword;
            _offset = 0;
            _metaCount = 0;
            _programInfos.Clear ();

			SearchProgramParameter parameter = new SearchProgramParameter ();

			parameter.keyword = _keyword;

			_updatingProgramsCoroutine = StartCoroutine (UpdatingPrograms (parameter));
        }

        private void OnSearchProgramViewEndDrag()
        {
            if(_programInfos.Count == 0)
            {
                return;
            }
            
            if (_offset == _metaCount) 
            {
                return;
            }

			if (_appendingProgramsCoroutine != null) 
			{
				return;
			}

			SearchProgramParameter parameter = new SearchProgramParameter ();

			parameter.keyword = _keyword;
			parameter.offset = _offset;
			parameter.metaCount = _metaCount;

			_appendingProgramsCoroutine = StartCoroutine (AppendingPrograms (parameter));            
        }

        private void OnSearchProgramItemClick(string programID)
        {   
			if (_updatingProgramDetailsInfoCoroutine != null) 
			{
				return;
			}

            _updatingProgramDetailsInfoCoroutine = StartCoroutine(UpdatingProgramDetailsInfo(programID));     
        }

        private void OnProgramViewBackButtonClick()
        {
            ShowSearchView (true);
            ShowProgramView (false);    
        }

		private IEnumerator UpdatingPrograms(SearchProgramParameter parameter)
		{
			ShowIndicatorView (true);

			yield return null;

			int maxSearchCount = Constants.MaxSearchProgramCount;

			do
			{
				yield return StartCoroutine(_netClient.GetPrograms(parameter, maxSearchCount));

			} while(parameter.offset < parameter.metaCount && parameter.searchCount < maxSearchCount);

            _offset = parameter.offset;
            _metaCount = parameter.metaCount;
			_programInfos.AddRange(parameter.programInfos);

			uiSearchProgramView.UpdateView (_programInfos);

			_updatingProgramsCoroutine = null;

            if(parameter.searchCount != 0)
            {
                ShowIndicatorView (false);
            }
            else
            {
                ShowIndicatorView (true, UIIndicatorView.Mode.LogMessage, "No results found.");
            }

            Resources.UnloadUnusedAssets();
		}

		private IEnumerator AppendingPrograms(SearchProgramParameter parameter)
		{
			ShowIndicatorView (true);

			yield return null;

			int maxSearchCount = Constants.MaxSearchProgramCount;

			do
			{
				yield return StartCoroutine(_netClient.GetPrograms(parameter, maxSearchCount));

			} while(parameter.offset < parameter.metaCount && parameter.searchCount < maxSearchCount);

            _offset = parameter.offset;
            _metaCount = parameter.metaCount;
			_programInfos.AddRange(parameter.programInfos);

			uiSearchProgramView.AppendView (parameter.programInfos);

			_appendingProgramsCoroutine = null;

            ShowIndicatorView (false);

            Debug.Log("(AppendingPrograms()) - (programCount)" + _programInfos.Count);
		}

        private IEnumerator UpdatingProgramDetailsInfo(string programID)
        {
			ShowIndicatorView (true);

			yield return null;

            _programDetailsInfo = new ProgramDetailsInfo();

            yield return StartCoroutine(_netClient.GetProgramDetailsByID (_programDetailsInfo, programID));

            ShowSearchView (false);
            ShowProgramView (true);

            uiProgramView.UpdateView (_programDetailsInfo);

            _updatingProgramDetailsInfoCoroutine = null;

			ShowIndicatorView (false);
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

        private void ShowIndicatorView(bool show, UIIndicatorView.Mode mode = UIIndicatorView.Mode.Networking, string logMessage = null)
		{
            uiIndicatorView.Show(show, mode, logMessage);
		}
	}
}