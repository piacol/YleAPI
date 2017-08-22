using UnityEngine;
using System.Collections;	
using System.Text;
using System.Net;
using System.IO;
using System.Collections.Generic;

namespace YleAPI
{
	public class MainScene : MonoBehaviour 
	{
		private const string _appID = "6b180ff5";
		private const string _appKey = "65de6b12ce702726ada98da78cea015f";
		private string _authInfo;
		private StringBuilder _sb = new StringBuilder();

		void Awake()
		{
		}
		
		void Start()
		{	
			InitNetwork ();
		
			string result = null;
			string programID = "1-3702366";
			GetProgramDetailsByID (ref result, programID);

			JSONObject resultJson = new JSONObject(result);
			JSONObject data = resultJson ["data"];
			JSONObject publicationEvent = data ["publicationEvent"];
			List<JSONObject> publicationEvents = publicationEvent.list;
			JSONObject mediaID = null;

			for (int i = 0; i < publicationEvents.Count; ++i) 
			{
				JSONObject pe = publicationEvents [i];

				if (pe ["temporalStatus"].str != "currently" ||
					pe ["type"].str != "OnDemandPublication") 
				{
					continue;
				}

				JSONObject media = pe ["media"];

				if (media != null)
				{		
					mediaID = media ["id"];

					Debug.Log ("(mediaID)" + mediaID);
				}
			}

			if (mediaID != null) 
			{
				string resultMediaURL = null;				

				GetMediaURL (ref resultMediaURL, programID, mediaID.str);

				Debug.Log ("(mediaID)" + mediaID.str + "(resultMediaURL)" + resultMediaURL);
			}
		}

		void InitNetwork()
		{
			ServicePointManager.ServerCertificateValidationCallback = new System.Net.Security.RemoteCertificateValidationCallback(AcceptAllCertifications);

			_authInfo = string.Format("?app_id={0}&app_key={1}", _appID, _appKey);
		}

		void GetProgramDetailsByID(ref string result, string id)
		{
			string strUri = "https://external.api.yle.fi/v1/programs/items/";

			_sb.Length = 0;
			_sb.Append (strUri);
			_sb.Append (id);
			_sb.Append (".json");
			_sb.Append (_authInfo);

			Debug.Log (_sb.ToString ());

			RequestAndResponse (ref result, _sb.ToString ());
		}

		void GetMediaURL(ref string result, string id, string mediaID)
		{
			string strUri = "https://external.api.yle.fi/v1/media/playouts.json?program_id=";

			_sb.Length = 0;
			_sb.Append (strUri);
			_sb.Append (id);
			_sb.Append ("&media_id=");
			_sb.Append (mediaID);
			_sb.Append ("&protocol=HLS");
			_sb.Append (_authInfo);

			Debug.Log (_sb.ToString ());

			RequestAndResponse (ref result, _sb.ToString ());
		}

		bool RequestAndResponse(ref string result, string requestUriString)
		{				
			HttpWebRequest request = (HttpWebRequest)WebRequest.Create (requestUriString);
			HttpWebResponse response = (HttpWebResponse)request.GetResponse();
			Stream dataStream = response.GetResponseStream();
			StreamReader reader = new StreamReader(dataStream, Encoding.UTF8);

			result = reader.ReadToEnd();
			Debug.Log (result);

			reader.Close ();
			response.Close ();

			return true;
		}

		public bool AcceptAllCertifications(object sender, System.Security.Cryptography.X509Certificates.X509Certificate certification, System.Security.Cryptography.X509Certificates.X509Chain chain, System.Net.Security.SslPolicyErrors sslPolicyErrors)
		{
			return true;
		}
	}
}