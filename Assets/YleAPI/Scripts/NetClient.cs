﻿using UnityEngine;
using System.Collections;
using System.Text;
using System.Collections.Generic;
using System.Security.Cryptography;
using System;
using YleAPI.Util;

namespace YleAPI.Net
{	
	public class NetClient 
	{
		private const string _appID = "6b180ff5";
		private const string _appKey = "65de6b12ce702726ada98da78cea015f";
		private const string _secretKey = "383ae543a87b5a03";
		private string _authInfo;
		private StringBuilder _sb = new StringBuilder();
		private NetThread _netThread = new NetThread();

		public NetClient()
		{			
			_authInfo = string.Format("app_id={0}&app_key={1}", _appID, _appKey);
		}

		public void Release()
		{
			_netThread.Release ();
		}

		public IEnumerator GetPrograms(SearchProgramParameter parameter, int maxSearchCount)
		{            
			List<ProgramInfo> result = new List<ProgramInfo> ();

			string strUri = "https://external.api.yle.fi/v1/programs/items.json?";
			_sb.Length = 0;
			_sb.Append (strUri);
            _sb.Append ("q=");
            _sb.Append (parameter.keyword);
            _sb.Append ('&');
			_sb.Append ("offset=");
			_sb.Append (parameter.offset);
			_sb.Append ('&');
			//_sb.Append ("category=5-135&");
			_sb.Append ("availability=ondemand&"); // available
			_sb.Append ("contentprotection=22-0,22-1&"); // exclude drm protected
			//_sb.Append ("region=world&"); // exclude fi region
			_sb.Append ("language=fi&"); // exclude fi region
			_sb.Append (_authInfo);

			string requestData = _sb.ToString ();

			while(_netThread.Request(requestData) == false)
			{
				yield return null;
			}

			string resultString = null;

			while(_netThread.PopResponseData(ref resultString) == false)
			{
				yield return null;
			}

            _netThread.FinishResponse();

			JSONObject jsonResult = new JSONObject (resultString);
			JSONObject meta = jsonResult ["meta"];
			parameter.metaCount = (int)meta ["count"].i;
			int limit = (int)meta ["limit"].i;
			JSONObject data = jsonResult ["data"];
			List<JSONObject> programs = data.list;

            if (programs.Count == 0) 
			{
				parameter.offset += limit;
			}

            foreach (var program in programs) 
			{
				parameter.offset++;
				
				if (IsValidSubject (program ["subject"]) == false) 
				{
					continue;
				}

				JSONObject title = program ["title"];
				JSONObject description = program ["description"];
				JSONObject type = program ["type"];
				List<JSONObject> publicationEvents = program ["publicationEvent"].list;
				JSONObject publicationEvent = null;
				JSONObject startTime = null;

				if(publicationEvents.Count > 0)
				{
					publicationEvent = publicationEvents [0];
					startTime = publicationEvent ["startTime"];
				}

				ProgramInfo newInfo = new ProgramInfo ();

				newInfo.id = program ["id"].str;

                if(title != null &&
					title ["fi"] != null) 
                {
					newInfo.title = title ["fi"].str;
                }

                if(title != null &&
                    title ["fi"] != null) 
                {                
					string[] titles = Util_String.SplitTitle(Util_String.ReplaceString(title ["fi"].str));

                    if(titles != null)
                    {
                        if(titles.Length > 1)
                        {
                            newInfo.title = string.Format("{0} | {1}", titles[1], titles[0]);
                        }
                        else
                        {
                            newInfo.title = titles[0];
                        }
                    }
                }

                if(description != null &&
					description ["fi"] != null) 
                {
					newInfo.description = Util_String.ReplaceString(description ["fi"].str);
                }

				if(type != null)
				{
					newInfo.type = type.str;
				}

				if(startTime != null)
				{
					newInfo.startTime = Util_String.ToDisplayStartTime(startTime.str);
				}

				result.Add (newInfo);

				parameter.searchCount++;

				if (parameter.searchCount >= maxSearchCount) 
				{
					break;
				}
			}

			parameter.programInfos.AddRange (result);
		}

        public IEnumerator GetProgramDetailsByID(ProgramDetailsInfo result, string id)
		{		
			string strUri = "https://external.api.yle.fi/v1/programs/items/";

			_sb.Length = 0;
			_sb.Append (strUri);
			_sb.Append (id);
			_sb.Append (".json?");
			_sb.Append (_authInfo);

			string requestData = _sb.ToString ();

			while(_netThread.Request(requestData) == false)
			{
				yield return null;
			}

			string resultString = null;

			while(_netThread.PopResponseData(ref resultString) == false)
			{
				yield return null;
			}			

			JSONObject jsonResult = new JSONObject (resultString);
            JSONObject data = jsonResult ["data"];
            JSONObject title = data ["title"];
            JSONObject description = data ["description"];            
            JSONObject image = data ["image"];
			//JSONObject typeMedia = data ["typeMedia"];
			JSONObject type = data ["type"];
			JSONObject duration = data ["duration"];
			List<JSONObject> publicationEvents = data ["publicationEvent"].list;
			JSONObject publicationEvent = null;
			JSONObject startTime = null;
			JSONObject region = null;

			if(publicationEvents.Count > 0)
			{
				publicationEvent = publicationEvents [0];
				startTime = publicationEvent ["startTime"];
				region = publicationEvent ["region"];
			}

			if(title != null &&
				title ["fi"] != null) 
			{                
				string[] titles = Util_String.SplitTitle(Util_String.ReplaceString(title ["fi"].str));

                if(titles.Length > 1)
                {
                    result.title1 = titles[0];
                    result.title2 = titles[1];
                }
                else
                {   
                    result.title1 = null;
                    result.title2 = titles[0];
                }
			}

			if(description != null &&
				description ["fi"] != null) 
			{                
				result.description = Util_String.ReplaceString(description ["fi"].str);
			}

            if(image != null &&
                image.Count > 0 &&
                image["id"] != null)
            {                
                int size = Screen.width;                
                string imageFormat = "jpg";
                string imageUrl = string.Format("http://images.cdn.yle.fi/image/upload/w_{0},h_{1},c_fit/{2}.{3}", size, size, image["id"].str, imageFormat);

                WWW www = new WWW(imageUrl);

                yield return www;

                result.image = Sprite.Create(www.texture, new Rect(0, 0, www.texture.width, www.texture.height), new Vector2(0, 0));
				//result.imageWidth = www.texture.width;
				//result.imageHeight = www.texture.height;

                Resources.UnloadUnusedAssets();
            }

			if(type != null)
			{
				result.type = type.str;
			}

			if(duration != null)
			{
				result.duration = Util_String.ToDisplayDuration(duration.str);
			}

			if(startTime != null)
			{
				result.startTime = Util_String.ToDisplayStartTime(startTime.str);
			}

			if(region != null)
			{
				result.region = region.str;
			}

            _netThread.FinishResponse();
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
        
        public void GetServices(ref string result, string serviceType)
        {
            string strUri = "https://external.api.yle.fi/v1/programs/services.json?type=";

            _sb.Length = 0;
            _sb.Append (strUri);
            _sb.Append (serviceType);
            _sb.Append ("&");
            _sb.Append (_authInfo);         

            RequestAndResponse (ref result, _sb.ToString ());           
        }
        */

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
