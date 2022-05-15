using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;

namespace ArtifactOfEndingYourRun
{
    public class ReflectionHelper
    {
        public static T ReflectGetField<T>(Type classType, string fieldName, object o = null)
        {
            FieldInfo field = classType.GetField(fieldName, BindingFlags.Public | BindingFlags.NonPublic | ((o != null) ? BindingFlags.Instance : BindingFlags.Static));
            return (T)((object)field.GetValue(o));
        }
    }
}
