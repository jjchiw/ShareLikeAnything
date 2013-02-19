using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using OpenPop.Mime;
using OpenPop.Pop3;

namespace ShareLikeAnything.Helpers
{
	public class Pop3Helper
	{
		string _hostname;
		int _port;
		bool _useSsl;
		string _username;
		string _password;

		public Pop3Helper(string hostname, int port, bool useSsl, string username, string password)
		{
			_hostname = hostname;
			_port = port;
			_useSsl = useSsl;
			_username = username;
			_password = password;
		}

		public List<Message> FetchAllMessages()
		{
			// The client disconnects from the server when being disposed
			using (Pop3Client client = new Pop3Client())
			{
				// Connect to the server
				client.Connect(_hostname, _port, _useSsl);

				// Authenticate ourselves towards the server
				client.Authenticate(_username, _password);

				// Get the number of messages in the inbox
				int messageCount = client.GetMessageCount();

				// We want to download all messages
				List<Message> allMessages = new List<Message>(messageCount);

				// Messages are numbered in the interval: [1, messageCount]
				// Ergo: message numbers are 1-based.
				// Most servers give the latest message the highest number
				for (int i = messageCount; i > 0; i--)
				{
					allMessages.Add(client.GetMessage(i));
					client.DeleteMessage(i);
				}

				// Now return the fetched messages
				return allMessages;
			}
		}
	}
}