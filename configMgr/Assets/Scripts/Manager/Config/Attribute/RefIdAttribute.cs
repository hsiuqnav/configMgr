using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Kernel.Lang.Attribute
{
    public class RefIdAttribute : ValidateAttribute
    {
        public override bool Validate(FieldInfo fieldInfo, object fieldValue, Type configType, object configValue, string name)
        {
            return true;
        }
    }
}
