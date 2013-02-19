using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
//using NLog;
using Raven.Abstractions.Exceptions;
using Raven.Client;
using ShareLikeAnything.Helpers;

namespace ShareLikeAnything.Tasks.Infrastructure
{
	public abstract class BackgroundTask
	{
		protected IDocumentSession DocumentSession;
		protected IDocumentStore DocumentStore;
		protected DropboxHelper DropboxHelper;

		//private readonly Logger logger = LogManager.GetCurrentClassLogger();

		protected virtual void Initialize(IDocumentSession session, IDocumentStore documentStore, DropboxHelper dropboxHelper)
		{
			DocumentSession = session;
			DocumentStore = documentStore;
			DocumentSession.Advanced.UseOptimisticConcurrency = false;
			DropboxHelper = dropboxHelper;
		}

		protected virtual void OnError(Exception e)
		{
		}

		public bool? Run(IDocumentSession openSession, IDocumentStore documentStore, DropboxHelper dropboxHelper)
		{
			Initialize(openSession, documentStore, dropboxHelper);
			try
			{
				Execute();
				DocumentSession.SaveChanges();
				TaskExecutor.StartExecuting();
				return true;
			}
			catch (ConcurrencyException e)
			{
				//logger.ErrorException("Could not execute task " + GetType().Name, e);
				OnError(e);
				return null;
			}
			catch (Exception e)
			{
				//logger.ErrorException("Could not execute task " + GetType().Name, e);
				OnError(e);
				return false;
			}
			finally
			{
				TaskExecutor.Discard();
			}
		}

		public abstract void Execute();
	}
}