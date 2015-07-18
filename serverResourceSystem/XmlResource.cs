using System;
using System.IO;
using System.Threading;
using System.Xml;

namespace PhotonServer.Resource.XML
{
	public abstract class XmlResource : ResourceBase
	{
		XmlDocument _doc = new XmlDocument();

		#region override
		protected override sealed void RefreshActions()
		{
			Exception loadEx = null;
			try
			{
				//иногда, при обновлении после сохранения файда может возникнуть ошибка,
				//так как обновляемый документ на момент загрузки обновляемого файла может быть занят другим процессом,
				//чтобы избежать этой ошибки, паузим поток
				Thread.Sleep(10);
				_doc.Load(FullFilePath);
				XmlParseActions(_doc);
			}
			catch(ArgumentNullException nullWrongVal)
			{
				Server.Log.ErrorFormat("XmlResource[{0}].RefreshActions: не указан путь к файлу", ToString());
				loadEx = nullWrongVal;
			}
			catch(ArgumentException wrongVal)
			{
				Server.Log.ErrorFormat("XmlResource[{0}].RefreshActions: пустой путь к файлу", ToString());
				loadEx = wrongVal;
			}
			catch(DirectoryNotFoundException dirNotFound)
			{
				Server.Log.ErrorFormat("XmlResource[{0}].RefreshActions: указанный путь недопустим, путь: {1}", ToString(), LocalFilePath);
				loadEx = dirNotFound;
			}
			catch(FileNotFoundException fileNotFound)
			{
				Server.Log.ErrorFormat("XmlResource[{0}].RefreshActions: файл не найден, путь: {1}", ToString(), LocalFilePath);
				loadEx = fileNotFound;
			}
			catch(Exception ex)
			{
				Server.Log.ErrorFormat("XmlResource[{0}].RefreshActions: при загрузке файла возникла ошибка, путь: {1}", ToString(), LocalFilePath);
				loadEx = ex;
			}
			if(loadEx != null) Server.Log.Error(loadEx.ToString());
		}
		#endregion

		protected XmlResource() { }

		protected abstract void XmlParseActions(XmlDocument source);
	}
}
