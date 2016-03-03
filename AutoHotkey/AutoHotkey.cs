﻿using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using Microsoft.CodeAnalysis.Scripting;
using Microsoft.CodeAnalysis.Scripting.CSharp;

namespace ScriptCs.AutoHotkey
{
    public interface IService : IDisposable
    {
    }

    public sealed class AutoHotkey : IDisposable
    {
        private bool disposed;

        public IKeyboard Keyboard { get; } = new Keyboard();

        public IProcesses Processes { get; } = new Processes();

        public IRegistry Registry { get; } = new Registry();

        public AutoHotkey()
        {
            AppDomain.CurrentDomain.UnhandledException += (_, args) => OnException(args.ExceptionObject);
            Application.ThreadException += (_, args) => OnException(args.Exception);
            var threadId = Helpers.GetCurrentThreadId();
            AppDomain.CurrentDomain.DomainUnload += (_, __)=> Helpers.TraceResult(Helpers.PostThreadMessage((uint)threadId, 0, 0, 0), "PostThreadMessage");
        }

        private static void OnException(object ex)
        {
            var message = ex.ToString();
            Trace.WriteLine(message);
            MessageBox.Show(message, "AutoHotkey error");
        }

        [STAThread]
        static void Main(string[] args)
        {
            var path = args.FirstOrDefault() ?? Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "AutoHotkey.csx");
            var options = ScriptOptions.Default
                                    .WithReferences("System", "System.Windows.Forms")
                                    .WithNamespaces("System", "System.Windows.Forms", "System.Diagnostics", "System.Threading", "Microsoft.Win32", "ScriptCs.AutoHotkey");

            var autoHotkey = new AutoHotkey();

            CSharpScript.RunAsync(File.ReadAllText(path), options, autoHotkey).Wait();

            RunGC();

            autoHotkey.Run();
        }

        private static void RunGC()
        {
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
        }

        void IDisposable.Dispose()
        {
            Trace.WriteLine("DISPOSE");
            if(disposed)
            {
                return;
            }
            disposed = true;
            Keyboard.Dispose();
        }

        private void Run()
        {
            using(this)
            {
                Application.Run();
            }
        }
    }
}