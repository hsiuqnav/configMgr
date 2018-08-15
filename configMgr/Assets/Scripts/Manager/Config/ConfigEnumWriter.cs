using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Kernel.Config
{
    public class ConfigEnumWriter
    {
        public void CheckAndWriteAllEnum(string file, Type type)
        {
            List<IGrouping<Type, string>> enumTypes = ListRelatedEnums(type);
            if (enumTypes.Count > 0)
                WriteEnumToFile(enumTypes, file);
        }

        private List<IGrouping<Type, string>> ListRelatedEnums(Type type)
        {
            return TypeUtil
                .TraversalGetRelatedFieldTypes(type)
                .Where(t => t.type.IsEnum)
                .OrderBy(t => t.type.Name)
                .GroupBy(o => o.type, o => o.field)
                .ToList();
        }

        private void WriteEnumToFile(List<IGrouping<Type, string>> enumTypes, string path)
        {
            using (var fileStream = File.Open(path, FileMode.Create))
            using (StreamWriter stream = new StreamWriter(fileStream, Encoding.UTF8))
            {
                stream.NewLine = "\r\n";
                foreach (IGrouping<Type, string> type in enumTypes)
                {
                    if (!type.Key.IsEnum)
                        continue;

                    WriteEnumType(stream, type.Key, type.Where(o => o != null).ToArray());
                }
            }
        }

        private void WriteEnumType(StreamWriter stream, Type type, string[] fields)
        {
            stream.WriteLine("[{0}] {1}", type.Name, string.Join(", ", fields));

            string[] names = Enum.GetNames(type);
            Array values = Enum.GetValues(type);
            for (int i = 0; i < names.Length; ++i)
            {
                string valueName = names[i];
                string comment = TypeUtil.GetEnumComment(type, (Enum)values.GetValue(i));

                var isInt32 = (Enum.GetUnderlyingType(type) == typeof(int));
                var valobj = values.GetValue(i);
                var valstr = isInt32 ? ((int)valobj).ToString() : ((long)valobj).ToString();
                stream.WriteLine("\t{0,-5}:  {1}  {2}",
                    valstr,
                    valueName,
                    comment != null ? string.Format("// {0}", comment) : "");
            }
        }
    }
}
