using System.Xml;

namespace PhotonServer.Resource.XML
{
	public interface IGoogleInAppBillingSettings : IResource
	{
		string ClientId { get; }
		string ClientSecret { get; }
		string Scope { get; }
		string PackageName { get; }
	}

	public class GoogleInAppBillingSettings : XmlResource, IGoogleInAppBillingSettings
	{
		#region override
		protected override string LocalFilePath { get { return @"donat\googleInAppBilling.xml"; } }
		protected override void XmlParseActions(XmlDocument source)
		{
			Server.Log.Info("GoogleInAppBillingSettings: обновление параметров системы работы с Google In-App Billing...");
			XmlNode node = source.SelectSingleNode(@"/settings");
			ClientId = node.Attributes["clientId"].Value;
			ClientSecret = node.Attributes["clientSecret"].Value;
			Scope = node.Attributes["scope"].Value;
			PackageName = node.Attributes["packageName"].Value;

			Server.Log.InfoFormat(
				"GoogleInAppBillingSettings: id клиента: {0}\n секрет: {1}\n scope: {2}\n имя пака: {3}",
				ClientId, ClientSecret, Scope, PackageName
				);
			Server.Log.Info("GoogleInAppBillingSettings: обновление параметров системы работы с Google In-App Billing закончено \n");
		}
		#endregion

		#region Члены IGoogleInAppBillingSettings
		public string ClientId { get; private set; }
		public string ClientSecret { get; private set; }
		public string Scope { get; private set; }
		public string PackageName { get; private set; }
		#endregion

		
	}
}
