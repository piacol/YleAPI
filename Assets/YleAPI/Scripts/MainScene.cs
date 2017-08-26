using UnityEngine;
using System.Collections;	
using System.Collections.Generic;	
using YleAPI.Net;
using YleAPI.UI;

namespace YleAPI
{
	public class ProgramInfo
	{
        public int number;
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
		public string duration;
		public string startTime;
		public string type;
		public string region;
	}

	public class SearchProgramParameter
	{		
        public int number;
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
        private int _number;
		private string _keyword;
		private int _offset;
		private int _metaCount;
		private List<ProgramInfo> _programInfos = new List<ProgramInfo>();
		private ProgramDetailsInfo _programDetailsInfo;
		private Coroutine _updatingProgramsCoroutine;
		private Coroutine _appendingProgramsCoroutine;
		private Coroutine _updatingProgramDetailsInfoCoroutine;
        private int _beginOffsetIndex;
        private int _endOffsetIndex;
        private List<ProgramInfo> _cachedProgramInfos = new List<ProgramInfo>();

        public bool MessageProc(eMessage type, object value)
        {
            switch(type)
            {
                case eMessage.SearchButtonClick:
                    {
                        OnSearchButtonClick((string)value);                                          
                        return true;
                    }
                case eMessage.SearchProgramViewAppend:
                    {
                        OnSearchProgramViewAppend();
                        return true;
                    }
                case eMessage.SearchProgramViewPrepend:
                    {
                        OnSearchProgramViewPrepend();
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
            MessageObjectManager.Instance.Add(eMessage.SearchProgramViewAppend, this);
            MessageObjectManager.Instance.Add(eMessage.SearchProgramViewPrepend, this);
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
			
            _number = 1;
            _keyword = keyword;
            _offset = 0;
            _metaCount = 0;
            _beginOffsetIndex = 0;
            _endOffsetIndex = 0;
            _programInfos.Clear ();

			SearchProgramParameter parameter = new SearchProgramParameter ();

			parameter.keyword = _keyword;
            parameter.number = 1;

			_updatingProgramsCoroutine = StartCoroutine (UpdatingPrograms (parameter));
        }

        private void OnSearchProgramViewAppend()
        {
            if(_programInfos.Count == 0 ||                
                _appendingProgramsCoroutine != null)
            {
                return;
            }

            if(_endOffsetIndex == _programInfos.Count)
            {
                if(_offset == _metaCount)
                {
                    return;
                }

                SearchProgramParameter parameter = new SearchProgramParameter ();

                parameter.number = _number;
                parameter.keyword = _keyword;
                parameter.offset = _offset;
                parameter.metaCount = _metaCount;

                _appendingProgramsCoroutine = StartCoroutine (AppendingPrograms (parameter));            
            }
            else
            {   
                int count;
                int startIndex = _endOffsetIndex;
                
                if(_endOffsetIndex + Constants.MaxSearchProgramCount < _programInfos.Count)
                {
                    _beginOffsetIndex += Constants.MaxSearchProgramCount;
                    _endOffsetIndex += Constants.MaxSearchProgramCount;
                    count = Constants.MaxSearchProgramCount;
                }
                else
                {
                    count = _programInfos.Count - _endOffsetIndex;
                    _endOffsetIndex = _programInfos.Count;
                    _beginOffsetIndex += count;                    
                }

                _cachedProgramInfos.Clear();

                for(int i = 0; i < count; ++i)
                {
                    _cachedProgramInfos.Add(_programInfos[startIndex + i]);
                }

                uiSearchProgramView.AppendView (_cachedProgramInfos);                

                //Debug.Log("(OnSearchProgramViewAppend() - (_beginOffset)" + _beginOffsetIndex + "(_endOffset)" + _endOffsetIndex);
            }
        }

        private void OnSearchProgramViewPrepend()
        {
            if(_programInfos.Count == 0 ||
                _beginOffsetIndex == 0)
            {
                return;
            }

            int count;

            if(_beginOffsetIndex >= Constants.MaxSearchProgramCount)
            {
                _beginOffsetIndex -= Constants.MaxSearchProgramCount;
                _endOffsetIndex -= Constants.MaxSearchProgramCount;
                count = Constants.MaxSearchProgramCount;
            }
            else
            {
                count = _beginOffsetIndex;
                _endOffsetIndex -= _beginOffsetIndex;
                _beginOffsetIndex = 0;

            }

            _cachedProgramInfos.Clear();

            for(int i = 0; i < count; ++i)
            {
                _cachedProgramInfos.Add(_programInfos[_beginOffsetIndex + i]);
            }

            uiSearchProgramView.PrependView (_cachedProgramInfos);

            //Debug.Log("(OnSearchProgramViewPrepend() - (_beginOffset)" + _beginOffsetIndex + "(_endOffset)" + _endOffsetIndex);
        }

        private void OnSearchProgramItemClick(string programID)
        {   
			if (_updatingProgramDetailsInfoCoroutine != null ||
                _appendingProgramsCoroutine != null)
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

            _number = parameter.number;
            _offset = parameter.offset;
            _metaCount = parameter.metaCount;
			_programInfos.AddRange(parameter.programInfos);
            _beginOffsetIndex = 0;
            _endOffsetIndex = _programInfos.Count;

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

            _number = parameter.number;
            _offset = parameter.offset;
            _metaCount = parameter.metaCount;
			_programInfos.AddRange(parameter.programInfos);
            _beginOffsetIndex = Mathf.Max(0, _programInfos.Count - Constants.MaxSearchProgramBufferCount);
            _endOffsetIndex = _programInfos.Count;

			uiSearchProgramView.AppendView (parameter.programInfos);

            // to prevent from clicking button after appending programs.
            yield return new WaitForSeconds(0.5f);

			_appendingProgramsCoroutine = null;

            ShowIndicatorView (false);

            //Debug.Log("(AppendingPrograms()) - (programCount)" + _programInfos.Count + "(_beginOffset)" + _beginOffsetIndex + "(_endOffset)" + _endOffsetIndex + "(Time.time)" + Time.time);
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