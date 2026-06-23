namespace Utils
{
    public static class Functions
    {
        public static T GetPropertyValue<T>(object obj, string propName)
        {
            return (T)obj.GetType().GetProperty(propName)?.GetValue(obj, null);
        }
        
        public static T GetFieldValue<T>(object obj, string fieldName)
        {
            return (T)obj.GetType().GetField(fieldName)?.GetValue(obj);
        }

    }
}