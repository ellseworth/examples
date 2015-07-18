using System;

namespace PhotonServer.Resource
{
	/// <summary>
	/// предоставляет базовый функционал для всех ресурсов сервера
	/// важно помнить, что ресурс, если он есть должен быть всегда готов отдать запрашиваемые данные
	/// </summary>
	public interface IResource
	{
		bool IsInited { get; }
		event Action<IResource> Refreshed;
	}
}
