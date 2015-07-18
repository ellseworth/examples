using System;
using System.IO;
using System.Timers;

namespace PhotonServer.Resource
{
	public class ResourceBase : IResource
	{
		#region static
		private static readonly Timer _refreshTimer =
			new Timer() { AutoReset = true, Interval = new TimeSpan(0, 0, 10).TotalSeconds };
		private static bool _disposed;
		private static volatile bool _isOnRefresh;

		static ResourceBase()
		{
			Server.OnTearDown += () =>
			{
				if (_disposed) return;
				_refreshTimer.Stop();
				_refreshTimer.Dispose();
			};

			_refreshTimer.Start();
		}
		#endregion

		protected string FullFilePath { get { return Server.i.ApplicationPath + @"\resources\" + LocalFilePath; } }
		protected virtual string LocalFilePath{ get { throw new NotImplementedException("переопределить путь у файлу ресурсов"); } }
		
		private DateTime _lastFileWriteTime = DateTime.Now;

		#region Члены IResource
		public event Action<IResource> Refreshed;
		public bool IsInited { get; private set; }
		#endregion

		protected ResourceBase()
		{
			IsInited = false;
			_refreshTimer.Elapsed += (sender, args) =>
				{
					if (_isOnRefresh) return;
					lock (sender)
					{
						try
						{
							FileInfo _fileInfo = new FileInfo(FullFilePath);
							if(_fileInfo.LastWriteTime != _lastFileWriteTime)
							{
								_isOnRefresh = true;
								RefreshActions();
								if (Refreshed != null) Refreshed(this);
								_lastFileWriteTime = _fileInfo.LastWriteTime;
								IsInited = true;
							}
						}
						catch(Exception ex)
						{
							Server.Log.ErrorFormat(
								"ResourceBase._refreshTimer.Elapsed: не удалось провести обновление из источника данных, файл: {0}",
								LocalFilePath
								);
							Server.Log.Error(ex.ToString());
						}
						finally { _isOnRefresh = false; }
					}
				};
		}

		protected virtual void RefreshActions() { throw new NotImplementedException(); }
	}
}
