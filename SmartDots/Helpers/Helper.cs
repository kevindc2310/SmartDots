﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Dynamic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization.Formatters.Binary;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using DevExpress.Mvvm;
using DevExpress.Xpf.WindowsUI;

namespace SmartDots.Helpers
{
    public static class Helper
    {

        public static float Version { get; } = 4.0f;

        public static void ShowWinUIMessageBox(string message, string caption, MessageBoxButton msgBoxButton, MessageBoxImage img, Exception e = null)
        {
            //Log("errors.txt", message + e?.StackTrace, e);

            try
            {
                Application.Current.Dispatcher.Invoke((Action)delegate
                {

                    new WinUIMessageBoxService().Show(message, caption, msgBoxButton, img);

                });
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
                throw;
            }



        }

        public static MessageBoxResult ShowWinUIDialog(string message, string caption, MessageBoxImage img, Exception e = null)
        {
            var messageBoxResult = System.Windows.MessageBoxResult.No;

            Application.Current.Dispatcher.Invoke((Action)delegate
            {

                messageBoxResult = new WinUIMessageBoxService().Show(message, caption, MessageBoxButton.YesNo, img);

            });
            return messageBoxResult;
        }



        public static void Log(string file, string message, Exception e = null, bool logInRelease = false)
        {
#if DEBUG
            try
            {
                using (StreamWriter writer = new StreamWriter(file, true))
                {
                    writer.WriteLine(DateTime.Now + Environment.NewLine + "Computer: " + Environment.MachineName + Environment.NewLine +
                                     "User: " + System.Security.Principal.WindowsIdentity.GetCurrent().Name.ToString() + Environment.NewLine +
                                     message + Environment.NewLine + e?.InnerException?.ToString() + Environment.NewLine);
                }
            }
            catch (Exception)
            {

            }
#else

            if (logInRelease)
            {
                try
                {
                    using (StreamWriter writer = new StreamWriter(file, true))
                    {
                        writer.WriteLine(DateTime.Now + Environment.NewLine + "Computer: " + Environment.MachineName + Environment.NewLine +
                                         "User: " + System.Security.Principal.WindowsIdentity.GetCurrent().Name.ToString() + Environment.NewLine +
                                         message + Environment.NewLine + e?.InnerException?.ToString() + Environment.NewLine + e?.StackTrace?.ToString() + Environment.NewLine);
                    }
                }
                catch (Exception)
                {

                }
            }
#endif
        }

        public static void LogWebAPIResult(string result)
        {
#if DEBUG
            try
            {
                using (StreamWriter writer = new StreamWriter("webapi.txt", true))
                {
                    writer.WriteLine(result + Environment.NewLine + Environment.NewLine);
                }
            }
            catch (Exception)
            {

            }
#endif
        }

        public static object ConvertType(object sourceObject, Type targetType)
        {
            var obj = Activator.CreateInstance(targetType);

            var sourceProps = sourceObject.GetType().GetProperties().Where(x => x.CanRead).ToList();
            var targetProps = targetType.GetProperties().Where(x => x.CanWrite).ToList();
            foreach (var prop in targetProps)
            {
                var propMatch = sourceProps.FirstOrDefault(x => x.Name.ToLower() == prop.Name.ToLower());
                if (propMatch == null) continue;
                try
                {
                    var value = propMatch.GetValue(sourceObject);
                    prop.SetValue(obj, value);
                }
                catch { }
            }

            var sourceFields = sourceObject.GetType().GetFields().Where(x => x.IsPublic).ToList();
            var targetFields = targetType.GetFields().Where(x => x.IsPublic).ToList();
            foreach (var prop in targetFields)
            {
                var propMatch = sourceFields.FirstOrDefault(x => x.Name.ToLower() == prop.Name.ToLower());
                if (propMatch == null) continue;
                try
                {
                    var value = propMatch.GetValue(sourceObject);
                    prop.SetValue(obj, value);
                }
                catch { }
            }

            return obj;
        }

        public static dynamic ToDynamic(this object value)
        {
            IDictionary<string, object> expando = new ExpandoObject();

            foreach (PropertyDescriptor property in TypeDescriptor.GetProperties(value.GetType()))
                expando.Add(property.Name, property.GetValue(value));

            return expando as ExpandoObject;
        }

        public static T DeepCopy<T>(T item)
        {
            BinaryFormatter formatter = new BinaryFormatter();
            MemoryStream stream = new MemoryStream();
            formatter.Serialize(stream, item);
            stream.Seek(0, SeekOrigin.Begin);
            T result = (T)formatter.Deserialize(stream);
            stream.Close();
            return result;
        }

        public static void DoAsync(Action action)
        {
            new System.Threading.Tasks.Task(action).Start();
        }

        public static Size MeasureString(TextBlock tb)
        {
            var formattedText = new FormattedText(
                tb.Text,
                CultureInfo.CurrentCulture,
                FlowDirection.LeftToRight,
                new Typeface(tb.FontFamily, tb.FontStyle, tb.FontWeight, tb.FontStretch),
                tb.FontSize,
                Brushes.Black,
                new NumberSubstitution());

            return new Size(formattedText.Width, formattedText.Height);
        }

        public static List<string> MultiUserColors { get; } = new List<string>() {
            "#00FFFF",
            "#FFFF00",
            "#FF0000",
            "#FF8000",
            "#8000FF",
            "#00FF00",
            "#80FF00",
            "#FF0080",
            "#0080FF",
            "#00FF80",
            "#8080FF",
            "#FF8080",
            "#80FF80",
            "#44DDDD",
            "#DDDD44",
            "#DD4444",
            "#DD8044",
            "#8440DD",
            "#44DD44",
            "#80DD44",
            "#DD4480",
            "#4480DD",
            "#44DD80",
            "#8080DD",
            "#DD8080",
            "#80DD80",
        };

        public static Dictionary<Guid, string> MultiUserColorsDict { get; } = new Dictionary<Guid, string>();

        public static bool FolderExists(string path)
        {
            if (path.StartsWith("http"))
            {
                //todo have to implement
                return true;
            }
            else
            {
                return Directory.Exists(path);
            }
        }
    }
}
