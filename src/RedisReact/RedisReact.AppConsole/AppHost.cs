﻿using System;
using System.Linq;
using System.Net;
using Funq;
using ServiceStack;
using RedisReact.Resources;
using RedisReact.ServiceInterface;

namespace RedisReact.AppConsole
{
    public class AppHost : AppSelfHostBase
    {
        /// <summary>
        /// Default constructor.
        /// Base constructor requires a name and assembly to locate web service classes. 
        /// </summary>
        public AppHost()
            : base("RedisReact.AppConsole", typeof(RedisServices).Assembly)
        {
            AppSettings = SharedUtils.GetAppSettings();
        }

        /// <summary>
        /// Application specific configuration
        /// This method should initialize any IoC resources utilized by your web service classes.
        /// </summary>
        /// <param name="container"></param>
        public override void Configure(Container container)
        {
            SharedUtils.Configure(this);

            SetConfig(new HostConfig {
                DebugMode = AppSettings.Get("DebugMode", false),
                EmbeddedResourceBaseTypes = { typeof(AppHost), typeof(CefResources) },
            });

            // This route is added using Routes.Add and ServiceController.RegisterService due to
            // using ILMerge limiting our AppHost : base() call to one assembly.
            // If two assemblies are used, the base() call searchs the same assembly twice due to the ILMerged result.
            Routes.Add<NativeHostAction>("/nativehost/{Action}");
            ServiceController.RegisterService(typeof(NativeHostService));
        }
    }

    public class NativeHostService : Service
    {
        public object Get(NativeHostAction request)
        {
            if (string.IsNullOrEmpty(request.Action))
                throw HttpError.NotFound("Function Not Found");

            var nativeHost = typeof(NativeHost).CreateInstance<NativeHost>();
            var methodName = request.Action.First().ToString().ToUpper() + string.Join("", request.Action.Skip(1));
            var methodInfo = typeof(NativeHost).GetMethod(methodName);
            if (methodInfo == null)
                throw new HttpError(HttpStatusCode.NotFound, "Function Not Found");

            methodInfo.Invoke(nativeHost, null);
            return null;
        }
    }

    public class NativeHostAction : IReturnVoid
    {
        public string Action { get; set; }
    }

    public class NativeHost
    {
        public void Quit()
        {
            Environment.Exit(0);
        }
    }
}
