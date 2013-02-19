using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Raven.Client.Indexes;
using ShareLikeAnything.Models;
using Raven.Abstractions.Indexing;

namespace ShareLikeAnything.Helpers.Indexes
{
	public class DataCreatedDateIndex : AbstractIndexCreationTask<Data>
	{
		public DataCreatedDateIndex()
		{
			Map = datas => from data in datas
						   select new { CreatedDate = data.CreatedDate, IsText = (data.Url != null && data.Url != string.Empty ? false : true) };
		}
	}
}