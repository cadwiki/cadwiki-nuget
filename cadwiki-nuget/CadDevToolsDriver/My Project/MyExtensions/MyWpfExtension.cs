using System;
using System.Collections;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Windows;
using Microsoft.VisualBasic;
using Microsoft.VisualBasic.ApplicationServices;
using Microsoft.VisualBasic.Devices;
using Microsoft.VisualBasic.Logging;

namespace CadDevToolsDriver
{

    /* TODO ERROR: Skipped IfDirectiveTrivia
    #If _MyType <> "Empty" Then
    */
    namespace My
    {
        /// <summary>
    ///     Module used to define the properties that are available in the My Namespace for WPF
    /// </summary>
    /// <remarks></remarks>
        [HideModuleName]
        static class MyWpfExtension
        {
            private readonly static MyProject.ThreadSafeObjectProvider<Computer> s_Computer = new MyProject.ThreadSafeObjectProvider<Computer>();
            private readonly static MyProject.ThreadSafeObjectProvider<User> s_User = new MyProject.ThreadSafeObjectProvider<User>();
            private readonly static MyProject.ThreadSafeObjectProvider<MyWindows> s_Windows = new MyProject.ThreadSafeObjectProvider<MyWindows>();
            private readonly static MyProject.ThreadSafeObjectProvider<Log> s_Log = new MyProject.ThreadSafeObjectProvider<Log>();

            /// <summary>
        ///     Returns the application object for the running application
        /// </summary>
            [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
            internal static Application Application
            {
                get
                {
                    return (Application)System.Windows.Application.Current;
                }
            }

            /// <summary>
        ///     Returns information about the host computer.
        /// </summary>
            [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
            internal static Computer Computer
            {
                get
                {
                    return s_Computer.GetInstance;
                }
            }

            /// <summary>
        ///     Returns information for the current user.  If you wish to run the application with the current
        ///     Windows user credentials, call My.User.InitializeWithWindowsUser().
        /// </summary>
            [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
            internal static User User
            {
                get
                {
                    return s_User.GetInstance;
                }
            }

            /// <summary>
        ///     Returns the application log. The listeners can be configured by the application's configuration file.
        /// </summary>
            [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
            internal static Log Log
            {
                get
                {
                    return s_Log.GetInstance;
                }
            }

            /// <summary>
        ///     Returns the collection of Windows defined in the project.
        /// </summary>
            [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
            internal static MyWindows Windows
            {
                [DebuggerHidden]
                get
                {
                    return s_Windows.GetInstance;
                }
            }

            [EditorBrowsable(EditorBrowsableState.Never)]
            [MyGroupCollection("System.Windows.Window", "Create__Instance__", "Dispose__Instance__", "My.MyWpfExtenstionModule.Windows")]
            internal sealed class MyWindows
            {
                [DebuggerHidden]
                private static T Create__Instance__<T>(T Instance) where T : Window, new()
                {
                    if (Instance is null)
                    {
                        if (s_WindowBeingCreated is not null)
                        {
                            if (s_WindowBeingCreated.ContainsKey(typeof(T)) == true)
                            {
                                throw new InvalidOperationException("The window cannot be accessed via My.Windows from the Window constructor.");
                            }
                        }
                        else
                        {
                            s_WindowBeingCreated = new Hashtable();
                        }
                        s_WindowBeingCreated.Add(typeof(T), null);
                        return new T();
                        s_WindowBeingCreated.Remove(typeof(T));
                    }
                    else
                    {
                        return Instance;
                    }
                }

                [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")]
                [DebuggerHidden]
                private void Dispose__Instance__<T>(ref T instance) where T : Window
                {
                    instance = null;
                }

                [DebuggerHidden]
                [EditorBrowsable(EditorBrowsableState.Never)]
                public MyWindows() : base()
                {
                }

                [ThreadStatic]
                private static Hashtable s_WindowBeingCreated;

                [EditorBrowsable(EditorBrowsableState.Never)]
                public override bool Equals(object o)
                {
                    return base.Equals(o);
                }

                [EditorBrowsable(EditorBrowsableState.Never)]
                public override int GetHashCode()
                {
                    return base.GetHashCode();
                }

                [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")]
                [EditorBrowsable(EditorBrowsableState.Never)]
                internal new Type GetType()
                {
                    return typeof(MyWindows);
                }

                [EditorBrowsable(EditorBrowsableState.Never)]
                public override string ToString()
                {
                    return base.ToString();
                }
            }
        }
    }

    public partial class Application : System.Windows.Application
    {

        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")]
        internal AssemblyInfo Info
        {
            [DebuggerHidden]
            get
            {
                return new AssemblyInfo(Assembly.GetExecutingAssembly());
            }
        }
    }
}

/* TODO ERROR: Skipped EndIfDirectiveTrivia
#End If*/