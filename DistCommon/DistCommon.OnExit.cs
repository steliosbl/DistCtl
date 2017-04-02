namespace DistCommon
{
    using System;
    using System.Reflection;

    public static class OnExit
    {
        public static void Register(Action onExit)
        {
            var assemblyLoadContextType = Type.GetType("System.Runtime.Loader.AssemblyLoadContext, System.Runtime.Loader");
            if (assemblyLoadContextType != null)
            {
                var currentLoadContext = assemblyLoadContextType.GetTypeInfo().GetProperty("Default").GetValue(null, null);
                var unloadingEvent = currentLoadContext.GetType().GetTypeInfo().GetEvent("Unloading");
                var delegateType = typeof(Action<>).MakeGenericType(assemblyLoadContextType);
                Action<object> lambda = (context) => onExit();
                unloadingEvent.AddEventHandler(currentLoadContext, lambda.GetMethodInfo().CreateDelegate(delegateType, lambda.Target));
                return;
            }

            var appDomainType = Type.GetType("System.AppDomain, mscorlib");
            if (appDomainType != null)
            {
                var currentAppDomain = appDomainType.GetTypeInfo().GetProperty("CurrentDomain").GetValue(null, null);
                var processExitEvent = currentAppDomain.GetType().GetTypeInfo().GetEvent("ProcessExit");
                EventHandler lambda = (sender, e) => onExit();
                processExitEvent.AddEventHandler(currentAppDomain, lambda);
                return;

                // Note that .NETCore has a private System.AppDomain which lacks the ProcessExit event.
                // That's why we test for AssemblyLoadContext first!
            }

            bool isNetCore = Type.GetType("System.Object, System.Runtime") != null;
            if (isNetCore)
            {
                throw new Exception("Before calling this function, declare a variable of type 'System.Runtime.Loader.AssemblyLoadContext' from NuGet package 'System.Runtime.Loader'");
            }
            else
            {
                throw new Exception("Neither mscorlib nor System.Runtime.Loader is referenced");
            }
        }
    }
}
