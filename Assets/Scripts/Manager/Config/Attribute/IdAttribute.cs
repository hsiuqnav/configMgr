namespace Kernel.Lang.Attribute
{
    public class IdAttribute : ValidateAttribute
    {
        public override string LastErrorStr()
        {
            return "id必须唯一，该id出现重复";
        }

        public override bool Validate(object value, string path)
        {
            return true;
        }
    }
}