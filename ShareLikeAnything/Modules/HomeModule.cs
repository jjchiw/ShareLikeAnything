using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Nancy;
using ShareLikeAnything.Helpers;
using System.IO;
using Nancy.Extensions;
using Raven.Client;
using ShareLikeAnything.Models;

namespace ShareLikeAnything.Modules
{
	public class HomeModule : NancyModule
	{
		readonly DataUploadHelper _dataUploadHelper;
		readonly IDocumentSession _session;
		readonly DropboxHelper _dropboxHelper;

		public HomeModule(DataUploadHelper dataUploadHelper, IDocumentSession session, DropboxHelper dropboxHelper)
		{
			_dataUploadHelper = dataUploadHelper;
			_session = session;
			_dropboxHelper = dropboxHelper;

			Get["/"] = x =>
			{
				return View["index.html"];
			};

			Post["/fileupload"] = x =>
			{
				var file = this.Request.Files.FirstOrDefault();
				string dataId;
				if (file == null)
				{
					dataId = _dataUploadHelper.UploadData(Request.Form["data"].Value as string);
				}
				else
				{
					dataId = _dataUploadHelper.UploadData(file.ContentType, file.Value, Path.GetExtension(file.Name));
				}
				
				return dataId;
			};

			Get["/view/datas/{Id}"] = x =>
			{
				string id = x.Id.Value;

				Data data = _session.Load<Data>("datas/" + id);
				if(data.ContentType == "text/plain" && data.Url == string.Empty)
					return Response.AsText(data.Text);

				var fileBytes = _dropboxHelper.GetFile(data.Url);
				var memoryStream = new MemoryStream(fileBytes);

				return Response.FromStream(memoryStream, data.ContentType);
			};


			Get["/ergo"] = x =>
			{
				return "hola";
			};
		}
	}
}