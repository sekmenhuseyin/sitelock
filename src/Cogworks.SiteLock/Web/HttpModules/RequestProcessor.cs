﻿using Cogworks.SiteLock.Web.Authentication;
using Cogworks.SiteLock.Web.Configuration;
using Cogworks.SiteLock.Web.Helpers;
using System.Web;

namespace Cogworks.SiteLock.Web.HttpModules
{
    public class RequestProcessor
    {
        private readonly ISiteLockConfiguration _config;
        private readonly IAuthenticationChecker _authChecker;

        public RequestProcessor(ISiteLockConfiguration config, IAuthenticationChecker authenticationChecker)
        {
            _config = config;
            _authChecker = authenticationChecker;
        }

        public void ProcessRequest(HttpContextBase httpContext)
        {
            System.Uri requestUri = httpContext.Request.Url;
            if (requestUri == null) return;

            string absolutePath = requestUri.AbsolutePath;
            System.Uri urlReferrer = httpContext.Request.UrlReferrer;

            if (!RequestHelper.IsLockedDomain(_config, requestUri.Host)) return;

            if (RequestHelper.IsAllowedIP(_config, httpContext.Request.UserHostAddress)) { return; }

            if (RequestHelper.IsAllowedReferrerPath(_config, absolutePath, urlReferrer)) { return; }

            if (RequestHelper.IsAllowedPath(_config, absolutePath)) { return; }

            if (RequestHelper.IsUmbracoAllowedPath(_config, absolutePath, urlReferrer)) { return; }

            // get here if path is not allowed
            if (_authChecker.IsAuthenticated(httpContext)) return;

            httpContext.Response.StatusCode = 403;

            throw new HttpException(403, "Locked by Cogworks.SiteLock Module");
        }
    }
}