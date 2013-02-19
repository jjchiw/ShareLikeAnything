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
using System.Text;
using ShareLikeAnything.Tasks.Infrastructure;
using ShareLikeAnything.Tasks;

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
				TaskExecutor.DropboxHelper = _dropboxHelper;
				TaskExecutor.ExecuteTask(new DeleteDataMoreThanDayTask());

				return View["index.html"];
			};

			Post["/fileupload"] = x =>
			{
				var file = this.Request.Files.FirstOrDefault();
				string dataId;
				try
				{
					if (file == null)
					{
						var str = Request.Form["data"].Value as string;
						dataId = _dataUploadHelper.UploadData(Request.Form["data"].Value as string);
					}
					else
					{

						dataId = _dataUploadHelper.UploadData(file.ContentType, file.Value, Path.GetExtension(file.Name));
					}
				}
				catch (FileLoadException)
				{
					return HttpStatusCode.InsufficientStorage;
				}
				
				return dataId;
			};

			Get["/view/datas/{Id}"] = x =>
			{
				string id = x.Id.Value;

				Data data = _session.Load<Data>("datas/" + id);
				if (data == null)
				{
					return View["expire.html"];
				}
				data.TimesView--;

				if (data.TimesView == 0)
				{
					if (data.Url != null && data.Url != string.Empty)
					{
						_dropboxHelper.DeleteFile(data.Url);
					}
						
					session.Delete<Data>(data);
					return View["expire.html"];
				}

				if (data.ContentType == "text/plain" && data.Url == null || data.Url == string.Empty)
					return View["view", new { Data = data.Text, SessionId = Guid.NewGuid().ToString() }];

				var fileBytes = _dropboxHelper.GetFile(data.Url);
				var memoryStream = new MemoryStream(fileBytes);

				return Response.FromStream(memoryStream, data.ContentType);
			};
		}

		
	}
}