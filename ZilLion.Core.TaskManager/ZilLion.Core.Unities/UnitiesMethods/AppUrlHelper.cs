using System;

namespace ZilLion.Core.Unities.UnitiesMethods
{
    public static class AppUrlHelper
    {
        static AppUrlHelper()
        {
             var appname = System.IO.Path.GetFileName(System.Reflection.Assembly.GetEntryAssembly().GetName().Name);
            AppDataLocalPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) +
                               $@"\{appname}\";
        }
        public static string AppDataLocalPath { get; set; }

    }
}