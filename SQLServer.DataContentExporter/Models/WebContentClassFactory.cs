using System;

namespace SQLServer.DataContentExporter.Models
{
    public static class WebContentClassFactory
    {
        private static string _ClassLibrary = "SQLServer.DataContentExporter.Models";
        public static Object GetInstance(string ClassName,string customerName,string ConnectionString)
        {
            String fullObjectName = _ClassLibrary + "." + ClassName;
            Type type = Type.GetType(fullObjectName);

            Object?[]? args = new Object[2];

            args[0] = customerName;
            args[1] = ConnectionString;

            if (type != null)
                return Activator.CreateInstance(type,args);

            foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
            {
                type = asm.GetType(fullObjectName);
                if (type != null)
                    return Activator.CreateInstance(type,args);
            }

            return null;
        }
    }
}
