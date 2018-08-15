using System;

namespace Kernel.Lang.Attribute
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = true)]
    public abstract class ValidateAttribute : System.Attribute
    {
        public virtual string LastErrorStr()
        {
            return "未知错误 ";
        }

        public virtual bool Validate(object value, string path)
        {
            return true;
        }
    }
}