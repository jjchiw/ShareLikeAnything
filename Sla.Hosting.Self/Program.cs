using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Owin;
using Microsoft.Owin.Hosting;
using Microsoft.AspNet.SignalR;
using Nancy.Owin;

namespace Sla.Hosting.Self
{
	class Program
	{
		static void Main(string[] args)
		{
			string url = "http://127.0.0.1";

			using (WebApplication.Start<Startup>(url))
			{
				Console.WriteLine("Server running on {0}", url);

				//Under mono if you deamonize a process a Console.ReadLine with cause an EOF 
				//so we need to block another way
				if (args.Any(s => s.Equals("-d", StringComparison.CurrentCultureIgnoreCase)))
				{
					while (true) Thread.Sleep(10000000);
				}
				else
				{
					Console.ReadKey();
				}
			}

			
		}

		class Startup
		{
			public void Configuration(IAppBuilder app)
			{
				// Turn cross domain on 
				var config = new HubConfiguration { EnableCrossDomain = true };
				
				// This will map out to http://localhost:8001/signalr by default
				app.MapHubs(config);
				app.UseNancy();
				
			}
		}
	}
}
