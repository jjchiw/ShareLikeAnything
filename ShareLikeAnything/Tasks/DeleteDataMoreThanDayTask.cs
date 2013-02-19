using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using Raven.Abstractions.Data;
using ShareLikeAnything.Tasks.Infrastructure;
using ShareLikeAnything.Models;

namespace ShareLikeAnything.Tasks
{
	public class DeleteDataMoreThanDayTask : BackgroundTask
	{
		public override void Execute()
		{
			var beforeDate = DateTime.UtcNow.AddDays(-1).Date.ToString("yyyy-MM-dd");

			var datas = DocumentSession.Advanced.LuceneQuery<Data>("DataCreatedDateIndex")
												.Where(string.Format("CreatedDate:[* TO {0}]", beforeDate))
												.AndAlso()
												.Where("IsText:false");

			foreach (var item in datas)
			{
				DropboxHelper.DeleteFile(item.Url);
			}


			DocumentStore.DatabaseCommands.DeleteByIndex("DataCreatedDateIndex", new IndexQuery
																					{
																						Query = string.Format("CreatedDate:[* TO {0}]", beforeDate)
																					}, false);
		}
	}
}