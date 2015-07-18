using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

namespace SerializeData
{
	/// <summary>
	/// содержит методы для сериализации
	/// </summary>
	public static class Serializer
	{
		/// <summary>
		/// сериализует екземпляр объекта указанного типа 
		/// </summary>
		/// <typeparam name="T">тип объекта, который необходимо сериализовать</typeparam>
		/// <param name="source">селиализуемый объект</param>
		/// <returns></returns>
		public static byte[] Serialize<T>(T source)
		{
			MemoryStream stream = new MemoryStream();
			BinaryFormatter formatter = new BinaryFormatter();
			formatter.Serialize(stream, source);
			byte[] result = stream.ToArray();
			stream.Close();
			return result;
		}
		/// <summary>
		/// сериализует массив екземпляров объектов указанного типа 
		/// </summary>
		/// <typeparam name="T">тип объекта, который необходимо сериализовать</typeparam>
		/// <param name="source">селиализуемый массив объектов</param>
		/// <returns></returns>
		public static byte[] SerializeArray<T>(T[] source)
		{
			MemoryStream stream = new MemoryStream();
			BinaryFormatter formatter = new BinaryFormatter();
			formatter.Serialize(stream, source);
			byte[] result = stream.ToArray();
			stream.Close();
			return result;
		}
		/// <summary>
		/// десериализует массив байт в новый екземпляр объекта указанного типа
		/// </summary>
		/// <typeparam name="T">тип объекта, в который необходимо сериализовать</typeparam>
		/// <param name="source">деселиализуемый массив байт</param>
		/// <returns>новый экземпляр объекта типа Т</returns>
		public static T Deserialize<T>(byte[] source)
		{
			MemoryStream stream = new MemoryStream(source);
			BinaryFormatter formatter = new BinaryFormatter();
			T result = (T)formatter.Deserialize(stream);
			stream.Close();
			return result;
		}
		/// <summary>
		/// десериализует массив байт в новый массив екземпляров объектов указанного типа
		/// </summary>
		/// <typeparam name="T">тип объекта, в который необходимо сериализовать</typeparam>
		/// <param name="source">деселиализуемый массив байт</param>
		/// <returns>новый массив экземпляров объектов типа Т</returns>
		public static T[] DeserializeArray<T>(byte[] source)
		{
			MemoryStream stream = new MemoryStream(source);
			BinaryFormatter formatter = new BinaryFormatter();
			T[] result = (T[])formatter.Deserialize(stream);
			stream.Close();
			return result;
		}
	}
}
