using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using DropNet;

namespace ShareLikeAnything.Helpers
{
	public class DropboxCredentials
	{
		public string ApiKey { get; private set; }
		public string ApiSecret { get; private set; }
		public string UserToken { get; private set; }
		public string UserSecret { get; private set; }

		public DropboxCredentials(string apiKey, string apiSecret, string userToken, string userSecret)
		{
			ApiKey = apiKey;
			ApiSecret = apiSecret;
			UserToken = userToken;
			UserSecret = userSecret;
		}
	}

	public class DropboxHelper
	{
		readonly DropNetClient _client;

		public DropboxHelper(DropboxCredentials credentials)
		{
			_client = new DropNetClient(credentials.ApiKey, credentials.ApiSecret, credentials.UserToken, credentials.UserSecret);
		}

		public string Upload(string fileName, byte[] bytes)
		{
			var metaData = _client.UploadFile("Apps/ShareLikeAnything", fileName, bytes);
			return metaData.Path;
		}

		public byte[] GetFile(string path)
		{
			return _client.GetFile(path);
		}
	}
}