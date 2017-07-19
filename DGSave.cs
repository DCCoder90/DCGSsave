using UnityEngine;
using System.Collections;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System;
using System.Text;

/*
 * Settings:
 * 		Name - Type //Description (Required for)
 * 
 * 		Data - String 	//Data to serialize					(Only when saving)
 * 		Encrypt - Bool 	//Whether or not data is encrypted	(Only when using encryption)
 * 		Key - String  	//Key for saved data				(Always)
 * 		Type - String	//Type of data saved				(Always)
 * 		Method - LSMethod//Method data is saved				(When using any method other than prefs)
 * 		Pass - String	//Password for encryption			(Only when using encryption)
 * 		WUser - String	//Username for web saving/loading 	(Web loading/saving)
 * 		WPass - String	//Password for web saving/loading 	(Web loading/saving)
 * 		File - String //File name for disk saving/loading 	(File loading/saving)
 * 		WURL - String //URL location for web saving/loading (Web loading/saving)
 * 		WIdent - String //User unique identifier			(Web loading/saving)
 *
 */

namespace DGSave{
	/*! 
	 *  \brief     DGSave
	 *  \details   Provides the required functionality for the DGSave system
	 *  \author    Ernest Mallett
	 *  \date      February 2014
	 *  \copyright DarkCloud Games 2014
	 */

	public class DGSave{
		private static string password = "";
		private const int Iterations = 234;
		private static string WebURL = "";

		#region Serialization
		/**
		* Serialize an Object
		* @param data The data to serialize
		* @return string The serialized data
		* @static
		* @private
		*/
		private static string Serialize<T>(T data){
			MemoryStream ms = new MemoryStream();
			BinaryFormatter bf = new BinaryFormatter();
			bf.Serialize(ms, data);
			byte[] bt = ms.ToArray();
			return Convert.ToBase64String(bt);
		}
		
		/**
		* Deserialize an Object
		* @param data The data to deserialize
		* @return T The deserialized data
		* @static
		* @private
		*/
		private static T Deserialize<T>(string data){
			byte[] bt = null;

			try{
				bt = Convert.FromBase64String(data.Trim());
			}catch(FormatException e){
				Debug.LogError("Exception converting from Base64. Data: "+e.Message);
				bt=Encoding.UTF8.GetBytes(data);
			}

			MemoryStream ms = new System.IO.MemoryStream();
			BinaryFormatter bf = new BinaryFormatter();
			ms.Write(bt,0,bt.Length);
			ms.Seek(0,SeekOrigin.Begin);
			T rt = (T)bf.Deserialize(ms);
			return rt;
		}
		#endregion

		#region Saving
		/**
		* Saves data according to the specified settings
		* @param settings The hashtable of settings to use
		* @return void
		* @static
		*/
		public static void Save(Hashtable settings){
			string data = Serialize(settings["data"]);
			
			if(settings.Contains("encrypt")){
				if((bool)settings["encrypt"]){
					password=(string)settings["pass"];
					byte[] pass = Encoding.UTF8.GetBytes(password).PadToMultipleOf(8);
					Encryption.SimpleAES aes = new Encryption.SimpleAES();
					data=aes.EncryptToString(data);
					password=null;
				}
			}

			switch((LSMethod)settings["method"]){
				
			case LSMethod.File:
				SaveToFile((string)settings["key"],(LSTypes)settings["type"],data,(string)settings["file"]);
				break;
				
			case LSMethod.Prefs:
				SaveToPrefs((string)settings["key"],(LSTypes)settings["type"],data);
				break;
				
			case LSMethod.Web:
				WebURL=(string)settings["wurl"];
				SaveToWeb(data,(LSTypes)settings["type"],(string)settings["wuser"],(string)settings["wpass"],(string)settings["key"],(string)settings["wident"]);
				break;
				
			default:
				SaveToPrefs((string)settings["key"],(LSTypes)settings["type"],data);
				break;
			}
		}
		
		/**
		* Saves data according to the specified settings
		* @param key The key to use when saving the data
		* @param data The data to save
		* @param settings The hashtable of settings to use
		* @return void
		* @static
		*/
		public static void Save<T>(string key, T data, Hashtable settings){
			settings.Add("key",key);
			settings.Add("data",data);
			Save(settings);
		}
		
		/**
		* Saves data according to the specified settings
		* @param key The key to use when saving the data
		* @param data The data to save
		* @param method The method in which to save the data
		* @param settings The hashtable of settings to use
		* @return void
		* @static
		*/
		public static void Save<T>(string key, T data, LSMethod method, Hashtable settings){
			settings.Add("key",key);
			settings.Add("data",data);
			settings.Add("method",method);
			Save(settings);
		}

		/**
		* Saves data to a file
		* @param key The key to use when saving the data
		* @param type The type of data to be save
		* @param data The data to save
		* @param file The file to save to
		* @return void
		* @private
		* @static
		*/
		private static void SaveToFile(string key, LSTypes type, string data, string file){
			using(FileStream fs = new FileStream(file,FileMode.OpenOrCreate, FileAccess.Write)){
				byte[] write =System.Text.Encoding.UTF8.GetBytes(type.ToString()+"/"+key+"/"+data);
				byte[] length = System.Text.Encoding.UTF8.GetBytes(write.Length.ToString().PadRight(5));
				fs.Seek(0,SeekOrigin.End);
				fs.Write(length,0,length.Length);
				fs.Seek(0,SeekOrigin.End); 
				fs.Write(write,0,write.Length);
			}
		}
		
		/**
		* Saves data to a website
		* @param data The data to save
		* @param type The type of data to be save
		* @param username The username specified in the PHP file
		* @param password The password specified in the PHP file
		* @param key The key to use when saving the data
		* @param ident The unique identifier for this user
		* @return void
		* @private
		* @static
		*/
		private static void SaveToWeb(string data,LSTypes type, string username, string password, string key,string ident){
			WWWForm form = new WWWForm();
			form.AddField("ident",ident);
			form.AddField("user",username);
			form.AddField("pass",password);
			form.AddField("data",data);
			form.AddField("type",type.ToString());
			form.AddField("key",key);
			form.AddField("act","put");

			WWW w = new WWW(WebURL,form);
			if(!String.IsNullOrEmpty(w.error))
				Debug.LogError("Error Saving to web: "+w.error);
		}
		
		/**
		* Saves data to the playerprefs
		* @param key The key to use when saving the data
		* @param type The type of data to be save
		* @param data The data to save
		* @return void
		* @private
		* @static
		*/
		private static void SaveToPrefs(string key,LSTypes type, string data){
			PlayerPrefs.SetString(type.ToString()+"/"+key,data);
		}
		#endregion
		
		#region Loading
		/**
		* Loads data according to the specified settings
		* @param settings A hashtable of settings
		* @return T The loaded data
		* @static
		*/
		public static T Load<T>(Hashtable settings){
			string data = "";

			switch((LSMethod)settings["method"]){
			case LSMethod.Resource:
				break;

			case LSMethod.File:
				data=LoadFromFile((string)settings["key"],(LSTypes)settings["type"],(string)settings["file"]);
				break;
				
			case LSMethod.Prefs:
				data=LoadFromPrefs((string)settings["key"],(LSTypes)settings["type"]);
				break;
				
			case LSMethod.Web:
				WebURL=(string)settings["wurl"];
				data=LoadFromWeb((string)settings["key"],(LSTypes)settings["type"],(string)settings["wuser"],(string)settings["wpass"],(string)settings["wident"]);
				break;
				
			default:
				data=LoadFromPrefs((string)settings["key"],(LSTypes)settings["type"]);
				break;
			}

			//If data was encrypted
			if(settings.Contains("encrypt")){
				if((bool)settings["encrypt"]){
					password=(string)settings["pass"];
					//data=Decrypt(data);

					byte[] pass = Encoding.UTF8.GetBytes(password).PadToMultipleOf(8);
					Encryption.SimpleAES aes = new Encryption.SimpleAES();
					data=aes.DecryptString(data);
					password=null;
				}
			}

			T obj = Deserialize<T>(data);

			return obj;
		}
		
		/**
		* Loads data according to the specified settings
		* @param key The key the data was saved with
		* @param type The type of data that is being loaded
		* @return T The loaded data
		* @static
		*/
		public static T Load<T>(string key, LSTypes type){
			Hashtable s = new Hashtable();
			s.Add("key",key);
			s.Add("type",type);
			return Load<T>(s);
		}
		
		/**
		* Loads data according to the specified settings
		* @param key The key the data was saved with
		* @param type The type of data that is being loaded
		* @param settings A hashtable of settings
		* @return T The loaded data
		* @static
		*/
		public static T Load<T>(string key, LSTypes type, Hashtable settings){
			settings.Add("key",key);
			settings.Add("type",type);
			return Load<T>(settings);
		}

		/**
		* Loads data from a file
		* @param key The key to use when saving the data
		* @param type The type of data to be save
		* @param file The file to save to
		* @return string The serialized data
		* @private
		* @static
		*/
		private static string LoadFromFile(string key,LSTypes type,string file){
			bool isDone = false;
	
			FileStream fs = new FileStream(file, FileMode.Open, FileAccess.Read);
			fs.Seek(0, SeekOrigin.Begin);

			int run = 0;

			while(!isDone){
				byte[] length = new byte[5];
				fs.Read(length,0,5);
				int datalength = int.Parse(System.Text.Encoding.UTF8.GetString(length));
				byte[] data = new byte[datalength];

				fs.Read(data,0,datalength);
	
				string dcdata = System.Text.Encoding.UTF8.GetString(data);

				string[] contents = dcdata.Split('/');
				if(contents[0]==type.ToString()){
					if(contents[1]==key){
						return dcdata.Substring(contents[0].Length+contents[1].Length+2);
					}
				}

				if(fs.Position == fs.Length){
					isDone=true;
				}
				run++;
			}
			Debug.LogError("Error loading data from file. Does it exist?");
			return null;
		}

		/**
		* Loads data from PlayerPrefs
		* @param key The key to use when saving the data
		* @param type The type of data to be save
		* @return string The serialized data
		* @private
		* @static
		*/
		private static string LoadFromPrefs(string key,LSTypes type){
			return PlayerPrefs.GetString(type.ToString()+"/"+key);
		}

		/**
		* Loads data from a website
		* @param key The key to use when saving the data
		* @param type The type of data to be save
		* @param user The username specified in the PHP file
		* @param pass The password specified in the PHP file
		* @param ident The unique identifier for this user
		* @return string The serialized data
		* @private
		* @static
		*/
		private static string LoadFromWeb(string key,LSTypes type, string user, string pass, string ident){
			WWWForm form = new WWWForm();
			form.AddField("ident",ident);
			form.AddField("key",key);
			form.AddField("type",type.ToString());
			form.AddField("user",user);
			form.AddField("pass",pass);
			form.AddField("act","get");

			WWW download = new WWW(WebURL,form);

			while(!download.isDone){}

			if(download.error!=null&&download.error!="")
				Debug.LogError("Error downloading data: "+download);
			return download.text;
		}
		#endregion
	}
}