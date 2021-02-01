﻿namespace ServiceControlInstaller.Engine.UrlAcl.Api
{
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    struct HttpServiceConfigSslQuery
    {
        public HttpServiceConfigQueryType QueryDesc;

        public HttpServiceConfigSslKey KeyDesc;

        public uint Token;
    }
}