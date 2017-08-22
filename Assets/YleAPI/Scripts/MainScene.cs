using UnityEngine;
using System.Collections;	
using System.Text;
using System.Net;
using System.IO;
using System.Collections.Generic;
using System.Security.Cryptography;
using System;

namespace YleAPI
{
	public class MainScene : MonoBehaviour 
	{
		private const string _appID = "6b180ff5";
		private const string _appKey = "65de6b12ce702726ada98da78cea015f";
        private const string _secretKey = "383ae543a87b5a03";
		private string _authInfo;
		private StringBuilder _sb = new StringBuilder();

		void Awake()
		{
		}
		
		void Start()
		{	
			InitNetwork ();
		
            string result = null;
            string programID = "1-2883110";//"1-4210050";//"1-3771942";//"1-3702366";
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
                string resultMediaURL = GetMediaURL (programID, mediaID.str);	

                Debug.Log ("(resultMediaURL)" + resultMediaURL);
			}
		}

		void InitNetwork()
		{
			ServicePointManager.ServerCertificateValidationCallback = new System.Net.Security.RemoteCertificateValidationCallback(AcceptAllCertifications);

			_authInfo = string.Format("app_id={0}&app_key={1}", _appID, _appKey);
		}

        public bool AcceptAllCertifications(object sender, System.Security.Cryptography.X509Certificates.X509Certificate certification, System.Security.Cryptography.X509Certificates.X509Chain chain, System.Net.Security.SslPolicyErrors sslPolicyErrors)
        {
            return true;
        }

		void GetProgramDetailsByID(ref string result, string id)
		{
			string strUri = "https://external.api.yle.fi/v1/programs/items/";

			_sb.Length = 0;
			_sb.Append (strUri);
			_sb.Append (id);
			_sb.Append (".json?");
			_sb.Append (_authInfo);

			Debug.Log (_sb.ToString ());

			RequestAndResponse (ref result, _sb.ToString ());
		}

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

                //return GetDecryptedMediaURL(encryptedURL);
                return Decrypt(encryptedURL, _secretKey);
            }

            return null;
		}

		bool RequestAndResponse(ref string result, string requestUriString)
		{				
			HttpWebRequest request = (HttpWebRequest)WebRequest.Create (requestUriString);
			HttpWebResponse response = (HttpWebResponse)request.GetResponse();
			Stream dataStream = response.GetResponseStream();
			StreamReader reader = new StreamReader(dataStream, Encoding.UTF8);

			result = reader.ReadToEnd();
            Debug.Log ("(Response)" + result);

			reader.Close ();
			response.Close ();

			return true;
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

            //AE_S128 복호화


            /*
            RijndaelManaged aesEncryption = new RijndaelManaged();            
            aesEncryption.BlockSize = 128;
            aesEncryption.KeySize = 256;

            //aesEncryption.Mode = CipherMode.CBC;
            aesEncryption.Padding = PaddingMode.None;

            string keyStr = encryptedURL;//"xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx";
            string ivStr = encryptedURL;//"xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx";           

            byte[] ivArr = Convert.FromBase64String(ivStr);
            byte[] IVkey16BytesValue = new byte[16];
            Array.Copy(ivArr, IVkey16BytesValue, 16);

            byte[] keyArr = Convert.FromBase64String(keyStr);
            byte[] KeyArr32BytesValue = new byte[32];
            Array.Copy(keyArr, KeyArr32BytesValue, 32);

            aesEncryption.IV = IVkey16BytesValue;
            aesEncryption.Key = KeyArr32BytesValue;
            //aesEncryption.Key = ASCIIEncoding.ASCII.GetBytes(keyStr);
            //aesEncryption.IV = ASCIIEncoding.ASCII.GetBytes(ivStr);

            ICryptoTransform decrypto = aesEncryption.CreateDecryptor();

            byte[] encryptedBytes = Convert.FromBase64CharArray(encryptedURL.ToCharArray(), 0, encryptedURL.Length);
            byte[] decryptedData = decrypto.TransformFinalBlock(encryptedBytes, 0, encryptedBytes.Length); 
            return ASCIIEncoding.UTF8.GetString(decryptedData);
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
        }
	}
}