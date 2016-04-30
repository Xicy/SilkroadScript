using System;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Windows;

namespace SilkroadScript
{
    public partial class App
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            AppDomain.CurrentDomain.AssemblyResolve += CurrentDomainOnAssemblyResolve;
            AppDomain.CurrentDomain.UnhandledException += CurrentDomainOnUnhandledException;
            Current.DispatcherUnhandledException += CurrentOnDispatcherUnhandledException;

            base.OnStartup(e);
            MainWindow = new MainWindow();
        }

        private static void CurrentOnDispatcherUnhandledException(object sender, dynamic handle)
        {
            Debug.WriteLine($"UI Error -> {handle.Exception.Message}");
        }

        private static void CurrentDomainOnUnhandledException(object sender, dynamic handle)
        {
            Debug.WriteLine($"UH Error -> {handle.ExceptionObject.Message}");
        }

        private static Assembly CurrentDomainOnAssemblyResolve(object sender, ResolveEventArgs args)
        {
            var assembly = Assembly.GetEntryAssembly().GetManifestResourceNames().First(x => x.Contains(args.Name.Split(',')[0]));
            if (string.IsNullOrEmpty(assembly)) return null;
            var stream = Assembly.GetEntryAssembly().GetManifestResourceStream(assembly);
            var buffer = new byte[stream.Length];
            stream.Read(buffer, 0, buffer.Length);
            stream.Dispose();
            return Assembly.Load(buffer);
        }
    }
}
