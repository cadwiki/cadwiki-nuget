﻿using System;
// Option Strict On
// Option Infer Off
// Option Explicit On

using System.IO;
using System.Windows.Input;
using Autodesk.Windows;
using Microsoft.VisualBasic;

namespace cadwiki.DllReloader.AutoCAD.UiRibbon.Buttons
{
    public class GenericClickCommandHandler : ICommand
    {

        public GenericClickCommandHandler()
        {
        }

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public event EventHandler CanExecuteChanged;


        public void Execute(object parameter)
        {
            UiRouter uiRouter = null;
            var netReloader = new AutoCADAppDomainDllReloader();
            netReloader.Log("GenericClickCommandHandler Executing Method.");
            try
            {
                if (parameter is null)
                {
                    netReloader.Log("Parameter was null.");
                    return;
                }

                if (parameter is RibbonButton)
                {

                    RibbonButton button = parameter as RibbonButton;
                    if (button is null)
                    {
                        netReloader.Log("Button was null.");
                        return;
                    }

                    uiRouter = (UiRouter)button.CommandParameter;
                    netReloader = uiRouter.NetReloader;

                    string assemblyName = uiRouter.AssemblyName;

                    if (uiRouter.FullClassName is null)
                    {
                        netReloader.Log("Full class name was null. Make sure you're Ribbon button full class name is correct.");
                        return;
                    }
                    netReloader.Log("Full class name: " + uiRouter.FullClassName);

                    if (uiRouter.MethodName is null)
                    {
                        netReloader.Log("MethodName was null. Make sure you're Ribbon button full class name is correct.");
                        return;
                    }
                    netReloader.Log("Method name: " + uiRouter.MethodName);

                    string dllRepo = Path.GetDirectoryName(netReloader.GetDllPath());
                    var asm = AcadAssemblyUtils.GetNewestAssembly(AppDomain.CurrentDomain.GetAssemblies(), assemblyName, dllRepo + @"\" + assemblyName + ".dll");
                    // Dim asm As System.Reflection.Assembly = If(App.ReloadedAssembly, Assembly.GetExecutingAssembly)
                    Type[] types = NetUtils.AssemblyUtils.GetTypesSafely(asm);
                    var @type = asm.GetType(uiRouter.FullClassName);
                    var methodInfo = type.GetMethod(uiRouter.MethodName);
                    if (methodInfo == null)
                    {
                        netReloader.Log("Method not found: " + uiRouter.MethodName);
                    }
                    else
                    {
                        var o = Activator.CreateInstance(type);
                        if (uiRouter.Parameters is not null)
                        {
                            methodInfo.Invoke(o, uiRouter.Parameters);
                        }
                        else
                        {
                            methodInfo.Invoke(o, null);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                var window = new WpfUi.Templates.WindowAutoCADException(ex);
                window.Show();

                if (ex.Message is not null)
                {
                    netReloader.Log("Exception: " + ex.Message);
                    if (ex.Message.Equals("The path is not of a legal form."))
                    {
                        netReloader.Log("Mostly likely caused by incorrect method name in UiRouter object.");
                        netReloader.Log("Double check that the Method name and Full class name above are correct.");
                    }
                    else
                    {
                        netReloader.Log("Mostly likely caused by incorrect solution name in UiRouter object: " + netReloader.GetIExtensionApplicationClassName());
                    }
                }
                else
                {

                }


                if (uiRouter is not null)
                {
                    netReloader.Log("UiRouter object: " + uiRouter.FullClassName);
                }
            }

        }



        public void CallMethod(Action f)
        {
            var doc = Autodesk.AutoCAD.ApplicationServices.Core.Application.DocumentManager.MdiActiveDocument;
            doc.Editor.WriteMessage(Environment.NewLine + "Calling...");
            f();
        }


    }
}