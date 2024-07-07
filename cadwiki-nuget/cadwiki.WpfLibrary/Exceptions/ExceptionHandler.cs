
using System;
using System.Collections.ObjectModel;
using System.Threading;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Threading;
using MessageBox = System.Windows.Forms.MessageBox;
using Screen = Caliburn.Micro.Screen;

namespace cadwiki.WpfLibrary.Exceptions
{
    public class ExceptionHandler : Screen
    {
        public delegate void ExceptionHandlerDelegate(Exception ex);
        private ExceptionHandlerDelegate customDelegate;
        private bool IsCustomExceptionHandlerOn = true;

        /// <summary>
        /// Set method that will be called whenever any of the exception event handlers catch an exception
        /// </summary>
        /// <param name="del"></param>
        public void SetCustomDelegate(ExceptionHandlerDelegate del)
        {
            customDelegate = del;
        }

        public ExceptionHandler()
        {
            LoggedExceptionsStack = new ObservableCollection<Exception>();
        }

        private static DateTime _timeOfLastException;

        private ObservableCollection<Exception> _loggedExceptionsStack = new ObservableCollection<Exception>();
        public ObservableCollection<Exception> LoggedExceptionsStack
        {
            get { return _loggedExceptionsStack; }
            set
            {
                _loggedExceptionsStack = value;
                NotifyOfPropertyChange(nameof(LoggedExceptionsStack));
            }
        }

        private ObservableCollection<Exception> _trackedExceptionsStack = new ObservableCollection<Exception>();
        public ObservableCollection<Exception> TrackedExceptionsStack
        {
            get { return _trackedExceptionsStack; }
            set
            {
                _trackedExceptionsStack = value;
                NotifyOfPropertyChange(nameof(TrackedExceptionsStack));
            }
        }

        public void ResetTrackedExceptions()
        {
            TrackedExceptionsStack.Clear();
        }

        public void CurrentDomainMonitorAllFirstChanceExceptions()
        {
            var currentDomain = AppDomain.CurrentDomain;
            currentDomain.FirstChanceException += AppDomain_FirstChanceException;
        }


        public void WindowsAppMonitorThreadExceptions()
        {
            System.Windows.Forms.Application.ThreadException += Application_ThreadException;
        }

        public void CurrentDomainMonitorAlUnhandledExceptions()
        {
            var currentDomain = AppDomain.CurrentDomain;
            currentDomain.UnhandledException += AppDomain_UnhandledException;
        }

        public void CurrentDispatcherMonitorAlUnhandledExceptions()
        {
            var cd = Dispatcher.CurrentDispatcher;
            cd.UnhandledException += Dispatcher_UnhandledException;
        }


        public void Log(Exception ex)
        {
            LogException(null, ex);
            var now = DateTime.Now;
            TimeSpan timeSinceLastException = now - _timeOfLastException;
            _timeOfLastException = now;
        }

        public void Show(Exception ex)
        {
            MessageBox.Show("Error", ex.Message, MessageBoxButtons.OKCancel, MessageBoxIcon.Error);
        }

        private void Log(string caughtBy, Exception ex)
        {
            LogException(caughtBy, ex);
            var now = DateTime.Now;
            TimeSpan timeSinceLastException = now - _timeOfLastException;
            _timeOfLastException = now;
        }

        private void LogException(string caughtBy, Exception ex)
        {
            if (!LoggedExceptionsStack.Contains(ex))
            {
                var temp = LoggedExceptionsStack;
                temp.Insert(0, ex);
                LoggedExceptionsStack = temp;

                temp = TrackedExceptionsStack;
                temp.Insert(0, ex);
                TrackedExceptionsStack = temp;
            }
            if (IsCustomExceptionHandlerOn && customDelegate != null)
            {
                customDelegate(ex);
            }
        }


        private void AppDomain_FirstChanceException(object sender, System.Runtime.ExceptionServices.FirstChanceExceptionEventArgs e)
        {
            Exception ex = (Exception)e.Exception;
            var caughtBy = "AppDomain_FirstChanceException";
            Log(caughtBy, ex);
        }


        private void Dispatcher_UnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            var ex = e.Exception;
            var caughtBy = "Dispatcher_UnhandledException";
            Log(caughtBy, ex);

            // Prevent the exception from crashing the application
            e.Handled = true;

            //cleanup
            Mouse.OverrideCursor = null;
        }

        private void AppDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Exception ex = (Exception)e.ExceptionObject;
            var caughtBy = "AppDomain_UnhandledException";
            Log(caughtBy, ex);
            //cleanup
            Mouse.OverrideCursor = null;
        }

        private void Application_ThreadException(object sender, ThreadExceptionEventArgs e)
        {
            Exception ex = (Exception)e.Exception;
            var caughtBy = "Application_ThreadException";
            Log(caughtBy, ex);

            //cleanup
            Mouse.OverrideCursor = null;
        }
    }
}



