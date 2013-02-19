using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.IO;
using ShareLikeAnything.Models;
using Raven.Client;
using System.Text;

namespace ShareLikeAnything.Helpers
{
	public class DataUploadHelper
	{
		readonly IDocumentSession _session;
		readonly DropboxHelper _dropboxHelper;

		public DataUploadHelper(IDocumentSession session, DropboxHelper dropboxHelper)
		{
			_session = session;
			_dropboxHelper = dropboxHelper;
		}

		internal string UploadData(string value)
		{
			if (Encoding.UTF8.GetByteCount(value) > 1000000)
			{
				throw new FileLoadException();
			}

			var data = new Data
			{
				Text = value,
				ContentType = "text/plain",
				CreatedDate = DateTime.UtcNow
			};

			_session.Store(data);

			return data.Id;

		}

		internal string UploadData(string contentType, Stream stream, string fileExtension = "")
		{
			if (stream.Length > 5000000)
			{
				throw new FileLoadException();
			}

			var extension = (fileExtension != "" ? fileExtension : string.Format(".{0}", MimeHelper.GetExtension(contentType)));
			if(extension == ".")
				extension = "";
			var fileName = string.Format("{0}{1}", Guid.NewGuid().ToString(), extension);

			return UploadData(fileName, contentType, stream);
		}

		internal string UploadData(string fileName, string contentType, Stream stream)
		{
			if (stream.Length > 5000000)
			{
				throw new FileLoadException();
			}

			var path = _dropboxHelper.Upload(fileName, ReadFully(stream));
			var data = new Data
			{
				Url = path,
				ContentType = contentType,
				CreatedDate = DateTime.UtcNow
			};

			_session.Store(data);

			return data.Id;
		}

		internal static byte[] ReadFully(Stream input)
		{
			using (MemoryStream ms = new MemoryStream())
			{
				input.CopyTo(ms);
				return ms.ToArray();
			}
		}
	}
}