using UnityEngine;
using System.Collections;
using System.Threading;
using System.Net;
using System.IO;
using System.Text;

namespace YleAPI.Net
{
	public class NetThread
	{
		private Thread _thread;
		private bool _threadExit = false;
		private string _requestData;
		private string _reponseData;

		public NetThread()
		{
			ServicePointManager.ServerCertificateValidationCallback = new System.Net.Security.RemoteCertificateValidationCallback(AcceptAllCertifications);

			_thread = new Thread (ThreadUpdate);
			_thread.Start ();
			Thread.Sleep (1000);
		}

		public void Release()
		{
			_threadExit = true;
		}

		public bool Request(string requestData)
		{
			if (_requestData != null) 
			{
				return false;
			}		

			lock (this) 
			{
				_requestData = requestData;
			}

			return true;
		}

		public bool PopResponseData(ref string result)
		{
			if (_reponseData == null) 
			{
				return false;
			}

			lock (this) 
			{			
				result = _reponseData;				
			}

			return true;
		}

        public void FinishResponse()
        {
            if(_requestData == null || _reponseData == null)
            {
                return;                
            }

            lock (this) 
            {           
                _requestData = null;
                _reponseData = null;            
            }
        }

		private void ThreadUpdate()
		{
			do
			{
				lock(this)
				{
					if(_requestData != null && 
						_reponseData == null)
					{				
						RequestAndResponse(ref _reponseData, _requestData);

						Thread.Sleep(500);
					}
				}

				Thread.Sleep(500);

			}while(_threadExit == false);			
		}

		private bool RequestAndResponse(ref string result, string requestUriString)
		{				
			HttpWebRequest request = (HttpWebRequest)WebRequest.Create (requestUriString);
			HttpWebResponse response = (HttpWebResponse)request.GetResponse();
			Stream dataStream = response.GetResponseStream();
			StreamReader reader = new StreamReader(dataStream, Encoding.UTF8);

			result = reader.ReadToEnd();
			//Debug.Log ("(RequestAndResponse() - )" + result);

			reader.Close ();
			response.Close ();

			return true;
		}

		private bool AcceptAllCertifications(object sender, System.Security.Cryptography.X509Certificates.X509Certificate certification, System.Security.Cryptography.X509Certificates.X509Chain chain, System.Net.Security.SslPolicyErrors sslPolicyErrors)
		{
			return true;
		}
	}
}
