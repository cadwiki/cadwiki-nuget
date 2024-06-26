
using System;
using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;

namespace cadwiki.NUnitTestRunner
{

    public class Utils
    {
        public static Type GetTypeByFullName(string fullName, Type[] types)
        {
            foreach (Type @type in types)
            {
                if (type.FullName.Equals(fullName))
                {
                    return type;
                }
            }
            return null;
        }

        public static Tuple<Type, MethodInfo> GetSetupMethod(Type[] types)
        {

            var typeToMethodInfo = new List<Tuple<Type, MethodInfo>>();
            foreach (Type @type in types)
            {
                MethodInfo[] methodInfos = type.GetMethods();

                foreach (MethodInfo methodInfo in methodInfos)
                {
                    var setupAttribute = DoesMethodInfoHaveSetupAttribute(methodInfo);
                    if (setupAttribute is not null)
                    {
                        var tuple = new Tuple<Type, MethodInfo>(type, methodInfo);
                        return tuple;
                    }

                }
            }
            return null;
        }

        public static Tuple<Type, MethodInfo> GetTearDownMethod(Type[] types)
        {

            var typeToMethodInfo = new List<Tuple<Type, MethodInfo>>();
            foreach (Type @type in types)
            {
                MethodInfo[] methodInfos = type.GetMethods();

                foreach (MethodInfo methodInfo in methodInfos)
                {
                    var tearDownAttribute = DoesMethodInfoHaveTearDownAttribute(methodInfo);
                    if (tearDownAttribute is not null)
                    {
                        var tuple = new Tuple<Type, MethodInfo>(type, methodInfo);
                        return tuple;
                    }

                }
            }
            return null;
        }

        public static List<Tuple<Type, MethodInfo>> GetTestMethodDictionarySafely(Type[] types)
        {

            var typeToMethodInfo = new List<Tuple<Type, MethodInfo>>();
            foreach (Type @type in types)
            {
                MethodInfo[] methodInfos = type.GetMethods();

                foreach (MethodInfo methodInfo in methodInfos)
                {
                    var testAttribute = DoesMethodInfoHaveTestAttribute(methodInfo);
                    if (testAttribute is not null)
                    {
                        var tuple = new Tuple<Type, MethodInfo>(type, methodInfo);
                        typeToMethodInfo.Add(tuple);
                    }

                }
            }
            return typeToMethodInfo;
        }

        private static object DoesMethodInfoHaveSetupAttribute(MethodInfo methodInfo)
        {
            object[] objectAttributes = methodInfo.GetCustomAttributes(true);
            foreach (object objectAttribute in objectAttributes)
            {
                var attributeType = objectAttribute.GetType();
                var setupAttribute = typeof(SetUpAttribute);
                if (attributeType.FullName.Equals(setupAttribute.FullName))
                {
                    return objectAttribute;
                }
            }
            return null;
        }

        private static object DoesMethodInfoHaveTearDownAttribute(MethodInfo methodInfo)
        {
            object[] objectAttributes = methodInfo.GetCustomAttributes(true);
            foreach (object objectAttribute in objectAttributes)
            {
                var attributeType = objectAttribute.GetType();
                var tearDownAttribute = typeof(TearDownAttribute);
                if (attributeType.FullName.Equals(tearDownAttribute.FullName))
                {
                    return objectAttribute;
                }
            }
            return null;
        }

        private static object DoesMethodInfoHaveTestAttribute(MethodInfo methodInfo)
        {
            object[] objectAttributes = methodInfo.GetCustomAttributes(true);
            foreach (object objectAttribute in objectAttributes)
            {
                var attributeType = objectAttribute.GetType();
                var testAttribute = typeof(TestAttribute);
                if (attributeType.FullName.Equals(testAttribute.FullName))
                {
                    return objectAttribute;
                }
            }
            return null;
        }
    }
}