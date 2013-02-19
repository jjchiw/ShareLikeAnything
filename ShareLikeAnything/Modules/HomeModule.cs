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
using OpenPop.Pop3;
using OpenPop.Mime;

namespace ShareLikeAnything.Modules
{
	public class HomeModule : NancyModule
	{
		readonly DataUploadHelper _dataUploadHelper;
		readonly IDocumentSession _session;
		readonly MailHelper _mailHelper;
		readonly DropboxHelper _dropboxHelper;
		readonly Pop3Helper _pop3Helper;

		public HomeModule(DataUploadHelper dataUploadHelper, IDocumentSession session, MailHelper mailHelper, Pop3Helper pop3Helper, DropboxHelper dropboxHelper)
		{
			_dataUploadHelper = dataUploadHelper;
			_session = session;
			_mailHelper = mailHelper;
			_pop3Helper = pop3Helper;
			_dropboxHelper = dropboxHelper;

			Get["/"] = x =>
			{
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

			Post["/mail-share"] = _ =>
			{
				var allMessage = _pop3Helper.FetchAllMessages();
				foreach (var m in allMessage)
				{
					var dataId = "";
					var attachments = m.FindAllAttachments();
					if(attachments.Count > 0)
					{
						var msgAttachment = attachments.FirstOrDefault();

						using(var ms = new MemoryStream(msgAttachment.Body))
						{
							if(Path.GetExtension(msgAttachment.FileName) != "")
								dataId = _dataUploadHelper.UploadData(msgAttachment.FileName, msgAttachment.ContentType.MediaType, ms);
							else
								dataId = _dataUploadHelper.UploadData(msgAttachment.ContentType.MediaType, ms);
						}
					}
					else
					{
						var txt = m.FindFirstPlainTextVersion().GetBodyAsText();
						dataId = _dataUploadHelper.UploadData(txt);
					}
					var body = string.Format("http://{0}/view/datas/{1}", Request.Url.BasePath, dataId);
					var ccs = m.Headers.Cc.Select(x => x.Address).ToList();
					_mailHelper.SendMessage(ccs, body, m.Headers.From.Address);
				}

				return HttpStatusCode.OK;
			};

		}

		
	}
}