using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;

namespace MoneyWeb.RequestServer.Base
{
    public abstract class HTTPRequestBase : IHttpModule
    {
        public EventHandler OnBeginRequest;

        public EventHandler OnEndRequest;

        public EventHandler OnErrorRequest;

        public HttpApplication CurrentContext;

        #region constructor Members

        public HTTPRequestBase()
        {
            OnBeginRequest = new EventHandler(ContextBeginRequest);

            OnEndRequest = new EventHandler(ContextEndRequest);

            OnErrorRequest = new EventHandler(ContextError);

        }
        #endregion

        #region IHttpModule Members

        public void Dispose()
        {
            CurrentContext.BeginRequest -= OnBeginRequest;

            CurrentContext.EndRequest -= OnEndRequest;

            CurrentContext.Error -= OnErrorRequest;
        }

        public void Init(HttpApplication context)
        {
            CurrentContext = context;

            CurrentContext.BeginRequest += OnBeginRequest;

            CurrentContext.EndRequest += OnEndRequest;

            CurrentContext.Error += OnErrorRequest;
        }

        #endregion

        /// <summary>
        /// handle the error context
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public abstract void ContextError(Object sender, EventArgs e);
        /// <summary>
        /// end the page request event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public abstract void ContextEndRequest(Object sender, EventArgs e);
        /// <summary>
        /// begin the page request event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public abstract void ContextBeginRequest(Object sender, EventArgs e);
    }
}
