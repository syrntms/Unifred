using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace Unifred.Utility
{

	//original is from http://stackoverflow.com/questions/10392268/serialize-deserialze-to-a-string-c-sharp
	public static class SerializeUtility
	{

		public static string SerializeObject<T>(T objectToSerialize)
		{
			BinaryFormatter bf = new BinaryFormatter();
			MemoryStream memStr = new MemoryStream();
			
			try
			{
				bf.Serialize(memStr, objectToSerialize);
				memStr.Position = 0;
				
				return Convert.ToBase64String(memStr.ToArray());
			}
			finally
			{
				memStr.Close();
			}
		}
		
		public static T DeserializeObject<T>(string str)
		{
			BinaryFormatter bf = new BinaryFormatter();
			byte[] b = Convert.FromBase64String(str);
			MemoryStream ms = new MemoryStream(b);
			
			try
			{
				return (T)bf.Deserialize(ms);
			}
			finally
			{
				ms.Close();
			}
		}
	}
}
