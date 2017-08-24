using UnityEngine;
using System.Collections;
using System.Text;
using System.Net;
using System.IO;
using System.Collections.Generic;
using System.Security.Cryptography;
using System;

namespace YleAPI.Net
{
	public class NetClient 
	{
		private const string _appID = "6b180ff5";
		private const string _appKey = "65de6b12ce702726ada98da78cea015f";
		private const string _secretKey = "383ae543a87b5a03";
		private string _authInfo;
		private StringBuilder _sb = new StringBuilder();

		public NetClient()
		{
			ServicePointManager.ServerCertificateValidationCallback = new System.Net.Security.RemoteCertificateValidationCallback(AcceptAllCertifications);

			_authInfo = string.Format("app_id={0}&app_key={1}", _appID, _appKey);
		}

		public List<ProgramInfo> GetPrograms(string keyword, ref int offset, ref int metaCount, int maxSearchCount = Constants.MaxSearchProgramCount)
		{
			List<ProgramInfo> result = new List<ProgramInfo> ();
			int searchCount = 0;

			do
			{
				var list = GetPrograms(keyword, ref offset, ref metaCount, ref searchCount, maxSearchCount);

				result.AddRange(list);

			} while(offset < metaCount && searchCount < maxSearchCount);

			return result;
		}

		public List<ProgramInfo> GetPrograms(string keyword, ref int offset, ref int metaCount, ref int searchCount, int maxSearchCount)
		{            
			List<ProgramInfo> result = new List<ProgramInfo> ();
			string strUri = "https://external.api.yle.fi/v1/programs/items.json?";
			_sb.Length = 0;
			_sb.Append (strUri);
            _sb.Append ("q=");
            _sb.Append (keyword);
            _sb.Append ('&');
			_sb.Append ("offset=");
			_sb.Append (offset);
			_sb.Append ('&');
			//_sb.Append ("category=5-135&");
			_sb.Append ("availability=ondemand&"); // available
			_sb.Append ("contentprotection=22-0,22-1&"); // exclude drm protected
			_sb.Append ("region=world&"); // exclude fi region
			_sb.Append ("language=fi&"); // exclude fi region
			_sb.Append (_authInfo);
			//Debug.Log (_sb.ToString ());

			string resultString = null;
			RequestAndResponse (ref resultString, _sb.ToString ());

			JSONObject jsonResult = new JSONObject (resultString);
			JSONObject jsonMeta = jsonResult ["meta"];
			metaCount = (int)jsonMeta ["count"].i;
			int limit = (int)jsonMeta ["limit"].i;
			JSONObject jsonData = jsonResult ["data"];
			List<JSONObject> jsonPrograms = jsonData.list;

			if (jsonPrograms.Count == 0) 
			{
				offset += limit;
			}

			foreach (var program in jsonPrograms) 
			{
				offset++;
				
				if (IsValidSubject (program ["subject"]) == false) 
				{
					continue;
				}

				JSONObject title = program ["title"];
				JSONObject description = program ["description"];
				JSONObject longDescription = program ["longDescription"];
				ProgramInfo newInfo = new ProgramInfo ();

                if(title != null &&
					title ["fi"] != null) 
                {
					newInfo.title = title ["fi"].str;
                }

                if(description != null &&
					description ["fi"] != null) 
                {
					newInfo.description = description ["fi"].str;
                }

                if(longDescription != null &&
					longDescription ["fi"] != null)
                {					
					newInfo.longDescription = longDescription ["fi"].str;
                }

				result.Add (newInfo);

				searchCount++;

				if (searchCount >= maxSearchCount) 
				{
					break;
				}
			}

			return result;
		}

		public void GetProgramDetailsByID(ref string result, string id)
		{
			string strUri = "https://external.api.yle.fi/v1/programs/items/";

			_sb.Length = 0;
			_sb.Append (strUri);
			_sb.Append (id);
			_sb.Append (".json?");
			_sb.Append (_authInfo);

			RequestAndResponse (ref result, _sb.ToString ());
		}

		/*
		public void GetService(ref string result, string serviceID)
		{
			string strUri = "https://external.api.yle.fi/v1/programs/services/";

			_sb.Length = 0;
			_sb.Append (strUri);
			_sb.Append (serviceID);
			_sb.Append (".json?");
			_sb.Append (_authInfo);

			RequestAndResponse (ref result, _sb.ToString ());

			JSONObject jsonResult = new JSONObject (result);

			//Debug.Log (jsonResult.ToString ());
		}
		*/

		public void GetServices(ref string result, string serviceType)
		{
			string strUri = "https://external.api.yle.fi/v1/programs/services.json?type=";

			_sb.Length = 0;
			_sb.Append (strUri);
			_sb.Append (serviceType);
			_sb.Append ("&");
			_sb.Append (_authInfo);

			//Debug.Log (_sb.ToString ());

			RequestAndResponse (ref result, _sb.ToString ());

			/*
			JSONObject jsonResult = new JSONObject (result);
			JSONObject jsonData = jsonResult ["data"];
			List<JSONObject> jsonServices = jsonData.list;
					
			foreach (var service in jsonServices) 
			{
			}
			*/
		}

		private bool AcceptAllCertifications(object sender, System.Security.Cryptography.X509Certificates.X509Certificate certification, System.Security.Cryptography.X509Certificates.X509Chain chain, System.Net.Security.SslPolicyErrors sslPolicyErrors)
		{
			return true;
		}

		private bool IsValidSubject(JSONObject subject)
		{
			List<JSONObject> subjects = subject.list;

			foreach (var item in subjects) 
			{				
				if (IsValidCategoryID (item ["id"].str) == false) 
				{
					return false;
				}

				JSONObject broader = item ["broader"];

				if (broader != null) 
				{
					if (IsValidCategoryID (broader["id"].str) == false) 
					{
						return false;
					}					 
				}
			}

			return true;
		}

		private bool IsValidCategoryID(string id)
		{
			switch (id) 
			{
			case "5-162":
			case "5-164":
			case "5-226":
			case "5-228":
				{
					return false;
				}	
			}

			return true;
		}

		private bool RequestAndResponse(ref string result, string requestUriString)
		{				
			HttpWebRequest request = (HttpWebRequest)WebRequest.Create (requestUriString);
			HttpWebResponse response = (HttpWebResponse)request.GetResponse();
			Stream dataStream = response.GetResponseStream();
			StreamReader reader = new StreamReader(dataStream, Encoding.UTF8);

			result = reader.ReadToEnd();
			Debug.Log ("(RequestAndResponse() - )" + result);

			reader.Close ();
			response.Close ();

			return true;
		}

		/*
		string GetMediaURL(string id, string mediaID)
		{
			string strUri = "https://external.api.yle.fi/v1/media/playouts.json?program_id=";

			_sb.Length = 0;
			_sb.Append (strUri);
			_sb.Append (id);
			_sb.Append ("&media_id=");
			_sb.Append (mediaID);
			_sb.Append ("&protocol=HLS&");
			_sb.Append (_authInfo);

			string tempResult = null;

			RequestAndResponse (ref tempResult, _sb.ToString ());

			JSONObject media = new JSONObject(tempResult);
			JSONObject data = media["data"];
			List<JSONObject> dataList = data.list;
			string encryptedURL = null;

			if(dataList.Count > 0)
			{
				JSONObject element = dataList[0];
				encryptedURL = element["url"].str;

				Debug.Log("(encryptedURL)" + encryptedURL);

				return GetDecryptedMediaURL(encryptedURL);                
			}

			return null;
		}

		public static string Decrypt(string textToDecrypt, string key)
		{
			RijndaelManaged rijndaelCipher = new RijndaelManaged();
			rijndaelCipher.Mode = CipherMode.CBC;
			rijndaelCipher.Padding = PaddingMode.PKCS7;

			rijndaelCipher.KeySize = 128;
			rijndaelCipher.BlockSize = 128;
			byte[] encryptedData = Convert.FromBase64String(textToDecrypt);
			byte[] pwdBytes = Encoding.UTF8.GetBytes(key);
			byte[] keyBytes = new byte[16];
			int len = pwdBytes.Length;
			if (len > keyBytes.Length)
			{
				len = keyBytes.Length;
			}
			Array.Copy(pwdBytes, keyBytes, len);
			rijndaelCipher.Key = keyBytes;
			rijndaelCipher.IV = keyBytes;
			byte[] plainText = rijndaelCipher.CreateDecryptor().TransformFinalBlock(encryptedData, 0, encryptedData.Length);
			return Encoding.UTF8.GetString(plainText);
		}

		public string GetDecryptedMediaURL(string encryptedURL)
		{
			byte[] encryptBytes = Convert.FromBase64String(encryptedURL);
			RijndaelManaged rm = new RijndaelManaged();

			rm.Mode = CipherMode.CBC;
			rm.Padding = PaddingMode.PKCS7;
			rm.KeySize = 128;

			MemoryStream memoryStream = new MemoryStream(encryptBytes);
			ICryptoTransform decryptor = rm.CreateDecryptor(Encoding.UTF8.GetBytes(_secretKey), Encoding.UTF8.GetBytes(_secretKey));
			CryptoStream cryptosStream = new CryptoStream(memoryStream, decryptor, CryptoStreamMode.Read);
			byte[] plainBytes = new byte[encryptBytes.Length];
			int plainCount = cryptosStream.Read(plainBytes, 0, plainBytes.Length);
			string plainString = Encoding.UTF8.GetString(plainBytes, 0, plainCount);

			cryptosStream.Close();
			memoryStream.Close();

			return plainString;
		}
		*/
		/*            
            String secret = "this_is_your_secret";
            String data = "this_is_the_url";
            Cipher cipher = Cipher.getInstance("AES/CBC/PKCS5Padding");

            byte[] baseDecoded = Base64.getDecoder().decode(data);
            byte[] iv = Arrays.copyOfRange(baseDecoded, 0, 16);
            byte[] msg = Arrays.copyOfRange(baseDecoded, 16, baseDecoded.length);

            SecretKeySpec secretKeySpec = new SecretKeySpec(secret.getBytes("UTF-8"), "AES");
            IvParameterSpec ivSpec = new IvParameterSpec(iv);

            cipher.init(Cipher.DECRYPT_MODE, secretKeySpec, ivSpec);
            byte[] resultBytes = cipher.doFinal(msg);
            System.out.println(new String(resultBytes));         
        */

		// media
		/*
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
                string resultMediaURL = GetMediaURL (programID, mediaID.str);	

                Debug.Log ("(resultMediaURL)" + resultMediaURL);
			}
			*/
	}
}
