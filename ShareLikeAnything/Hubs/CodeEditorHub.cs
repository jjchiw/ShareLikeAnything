using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.AspNet.SignalR.Hubs;
using Microsoft.AspNet.SignalR;

namespace ShareLikeAnything.Hubs
{
	[HubName("codeEditorHub")]
	public class CodeEditorHub : Hub
	{
		public void Send(string message, string sessionId)
		{
			Clients.All.addMessage(message, sessionId);
		}
	}
}