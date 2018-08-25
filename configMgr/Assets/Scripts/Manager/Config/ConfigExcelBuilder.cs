#if CODE_GEN && !DISABLE_EXCEL_BUILDER
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Kernel.Game;
using Kernel.Lang.Attribute;

using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using ICSharpCode.SharpZipLib.Core;
using ICSharpCode.SharpZipLib.Zip;
using Kernel.Lang;
using BottomBorder = DocumentFormat.OpenXml.Spreadsheet.BottomBorder;
using Color = DocumentFormat.OpenXml.Spreadsheet.Color;
using Fill = DocumentFormat.OpenXml.Spreadsheet.Fill;
using Font = DocumentFormat.OpenXml.Spreadsheet.Font;
using Fonts = DocumentFormat.OpenXml.Spreadsheet.Fonts;
using ForegroundColor = DocumentFormat.OpenXml.Spreadsheet.ForegroundColor;
using LeftBorder = DocumentFormat.OpenXml.Spreadsheet.LeftBorder;
using Path = System.IO.Path;
using PatternFill = DocumentFormat.OpenXml.Spreadsheet.PatternFill;
using RightBorder = DocumentFormat.OpenXml.Spreadsheet.RightBorder;
using TopBorder = DocumentFormat.OpenXml.Spreadsheet.TopBorder;

namespace Kernel.Config
{
	public class ConfigExcelBuilder
	{
		private static readonly Dictionary<Type, string> typeNames = new Dictionary<Type, string>
		{
			{ typeof(int), "INT" },
			{ typeof(uint), "UINT" },
			{ typeof(long), "LONG" },
			{ typeof(ulong), "ULONG" },
			{ typeof(short), "SHORT" },
			{ typeof(ushort), "USHORT" },
			{ typeof(byte), "BYTE" },
			{ typeof(float), "FLOAT" },
			{ typeof(double), "DOUBLE" },
			{ typeof(string), "STRING" },
			{ typeof(TimeSpan), "STRING" },
			{ typeof(SerializableTimeSpan), "STRING" },
			{ typeof(DateTime), "STRING" },
		};

		private static readonly Dictionary<Type, string> fieldNames = new Dictionary<Type, string>
		{
			{ typeof(SerializableTimeSpan), "timespan" },
		};

		private static readonly HashSet<Type> customAtomType = new HashSet<Type>
		{
			{ typeof(SerializableTimeSpan) },
			{ typeof(TimeSpan) },
			{ typeof(DateTime) },
		};

		private static readonly string[] groupColors =
		{
			"87CEFF",
			"EEE685",
			"90EE90",
			"CDBA96",
			"EE9A49",
		};

		private HashSet<Type> enums = new HashSet<Type>();
		private List<KeyValuePair<int, uint>> columnWidthAndStyle = new List<KeyValuePair<int, uint>>();
		private uint selectedColor;
		private byte[] buffer = new byte[4096];
		private HashSet<Type> upper = new HashSet<Type>();

		public void Clear()
		{
			string folder = Path.GetFullPath(PathManager.Instance.ExternalExcelExampleFolder);
			PlatformManager.Instance.ClearDirectory(folder);
		}

		public void ExportExamples(IEnumerable<KeyValuePair<Type, ConfigAttribute>> configTypes)
		{
			string folder = Path.GetFullPath(PathManager.Instance.ExternalExcelExampleFolder);
			foreach(var type in configTypes)
			{
				upper.Clear();
				string name = type.Value.Name ?? ConfigManager.Instance.GetDefaultConfigFileName(type.Key);
				string fileName = "example_" + name.ToLower() + ".xlsx";
				Export(type.Key, 
					type.Value is DictionaryConfigAttribute ? (type.Value as DictionaryConfigAttribute).Key : null, 
					Path.Combine(folder, fileName));
			}
		}

		public void ExportLuaExamples(IEnumerable<KeyValuePair<Type, ConfigAttribute>> configs)
		{
			string folder = Path.GetFullPath(PathManager.Instance.ExternalExcelExampleFolder);
			foreach(var config in configs)
			{
				upper.Clear();
				if(config.Key.IsEnum) continue;
				string fileName = string.IsNullOrEmpty(config.Value.Name)
					? ConfigManager.Instance.GetDefaultConfigFileName(config.Key)
					: config.Value.Name.ToLower();
				string key = config.Value is DictionaryConfigAttribute ? (config.Value as DictionaryConfigAttribute).Key : null;
				Export(config.Key, key, Path.Combine(folder, "example_" + fileName + ".xlsx"));
			}
		}

		private GradientStop CreateGradientStop(int position, string color = null)
		{
			return new GradientStop(color == null 
				? new Color { Rgb = new HexBinaryValue { Value = "FFFFFFFF" } } 
				: new Color { Rgb = new HexBinaryValue { Value = color } }
			)
			{
				Position = position
			};
		}

		private Stylesheet GenerateStylesheet()
		{
			Fonts fonts = new Fonts(
				new Font( // Index 0 - The default font.
					new FontSize { Val = 10 },
					new Color { Rgb = new HexBinaryValue { Value = "000000" } },
					new FontName { Val = "微软雅黑" }),
				new Font( // Index 1 - The bold font.
					new Bold(),
					new FontSize { Val = 10 },
					new Color { Rgb = new HexBinaryValue { Value = "000000" } },
					new FontName { Val = "微软雅黑" })
			);

			Fills fills = new Fills(
				new Fill( // Index 0 - The default fill.
					new PatternFill { PatternType = PatternValues.None }),
				new Fill( // Index 1 - The default fill of gray 125 (required)
					new PatternFill { PatternType = PatternValues.Gray125 }),
				new Fill( // Index 2 - The header fill.
					new PatternFill(new ForegroundColor { Rgb = new HexBinaryValue { Value = "FFD9E1F2" } })
					{
						PatternType = PatternValues.Solid
					}));
			
			for(int i = 0; i < groupColors.Length; i++)
			{
				fills.AppendChild(new Fill(
					new GradientFill(CreateGradientStop(0), CreateGradientStop(1, groupColors[i]))
					{
						Degree = 180
					}));
				fills.AppendChild(new Fill(
					new GradientFill(CreateGradientStop(0), CreateGradientStop(1, groupColors[i]))
					{
						Degree = 0
					}));
			}

			Borders borders = new Borders(
				new Border( // Index 0 - The default border.
					new LeftBorder(),
					new RightBorder(),
					new TopBorder(),
					new BottomBorder(),
					new DiagonalBorder()),
				new Border( // Index 1 - Applies a Left, Right, Top, Bottom border to a cell
					new LeftBorder(new Color() { Auto = true }) { Style = BorderStyleValues.Thin },
					new RightBorder( new Color() { Auto = true } ) { Style = BorderStyleValues.Thin },
					new TopBorder(new Color() { Auto = true }) { Style = BorderStyleValues.Thin },
					new BottomBorder(new Color() { Auto = true }) { Style = BorderStyleValues.Thin },
					new DiagonalBorder()),
				new Border( // Index 2 - Top Border.
					new LeftBorder(),
					new RightBorder(),
					new TopBorder(new Color(){ Auto = true }) { Style = BorderStyleValues.Thin },
					new BottomBorder(),
					new DiagonalBorder())
			);

			CellFormats cellFormats = new CellFormats(
				new CellFormat()
				{
					FontId = 0,
					FillId = 0,
					BorderId = 0
				}, // Index 0 - The default cell style.  If a cell does not have a style index applied it will use this style combination instead
				new CellFormat(new Alignment(){Horizontal = HorizontalAlignmentValues.Center})
				{
					FontId = 0,
					FillId = 0,
					BorderId = 1,
					ApplyBorder = true,
					ApplyAlignment = true,
				}, // Index 1 - All 
				new CellFormat(new Alignment() { Horizontal = HorizontalAlignmentValues.Center })
				{
					FontId = 1,
					FillId = 2,
					BorderId = 1,
					ApplyFont = true,
					ApplyFill = true,
					ApplyBorder = true,
					ApplyAlignment = true,
				}, // Index 2 - Header 
				new CellFormat(new Alignment() { Horizontal = HorizontalAlignmentValues.Center })
				{
					FontId = 0,
					FillId = 2,
					BorderId = 1,
					ApplyFill = true,
					ApplyBorder = true,
					ApplyAlignment = true,
				}, // Index 3 - Sub Header 
				new CellFormat(new Alignment() { Horizontal = HorizontalAlignmentValues.Fill })
				{
					FontId = 1,
					FillId = 0,
					BorderId = 1,
					ApplyFont = true,
					ApplyBorder = true,
					ApplyAlignment = true,
				}, // Index 4
				new CellFormat()
				{
					FontId = 0,
					FillId = 0,
					BorderId = 2,
					ApplyBorder = true,
				} // Index 5 - Enum
			);

			for(uint i = 0; i < groupColors.Length; i++)
			{
				cellFormats.AppendChild(new CellFormat(new Alignment() { Horizontal = HorizontalAlignmentValues.Center })
				{
					FontId = 0,
					FillId = i*2 + 3,
					BorderId = 1,
					ApplyBorder = true,
					ApplyAlignment = true,
					ApplyFill = true,
				});
				cellFormats.AppendChild(new CellFormat(new Alignment() { Horizontal = HorizontalAlignmentValues.Center })
				{
					FontId = 0,
					FillId = i*2 + 4,
					BorderId = 1,
					ApplyBorder = true,
					ApplyAlignment = true,
					ApplyFill = true,
				});
			}

			return new Stylesheet(fonts, fills, borders, cellFormats);
		}

		private void AppendStylePart(SpreadsheetDocument spreadsheetDocument)
		{
			WorkbookStylesPart stylesPart = spreadsheetDocument.WorkbookPart.AddNewPart<WorkbookStylesPart>("rIdstyle0");
			stylesPart.Stylesheet = GenerateStylesheet();
			stylesPart.Stylesheet.Save();
		}

		private void AppendSheetViews(WorksheetPart worksheetPart, bool hasKey)
		{
			SheetViews sheetViews = worksheetPart.Worksheet.AppendChild(new SheetViews());
			SheetView sheetView = sheetViews.AppendChild(new SheetView() { WorkbookViewId = 0U });

			Pane pane = new Pane()
			{
				VerticalSplit = 4D,
				HorizontalSplit = hasKey ? 2D : 1D,
				TopLeftCell = hasKey ? "C5" : "B5",
				ActivePane = PaneValues.BottomRight,
				State = PaneStateValues.Frozen
			};

			sheetView.AppendChild(pane);
		}

		private Row[] AppendDefaultRows(WorksheetPart worksheetPart, WorkbookPart workbookpart, string title)
		{
			SheetData sheetData = AppendSheet(worksheetPart, workbookpart);

			int maxlen = 0;
			sheetData.AppendChild(new Row() { CustomFormat = true, StyleIndex = 1 }).AppendChild(CreateCell(title, 4, ref maxlen));
			Row[] rows =
			{
				sheetData.AppendChild(new Row() { CustomFormat = true, StyleIndex = 2 }),
				sheetData.AppendChild(new Row() { CustomFormat = true, StyleIndex = 3 }),
				sheetData.AppendChild(new Row() { CustomFormat = true, StyleIndex = 3 }),
			};

			rows[0].AppendChild(new Cell()
			{
				StyleIndex = 1
			});
			rows[1].AppendChild(new Cell()
			{
				StyleIndex = 1
			});
			rows[2].AppendChild(new Cell()
			{
				StyleIndex = 1
			});

			return rows;
		}

		private void AppendColumns(Columns columns, bool hasKey)
		{
			columns.AppendChild(new Column()
			{
				Min = 1,
				Max = 1,
				Width = 9,
				Style = 1,
			});
			for(uint i = 0; i < columnWidthAndStyle.Count; i++)
			{
				uint style = hasKey ? 3 : columnWidthAndStyle[(int)i].Value;
				columns.AppendChild(new Column()
				{
					Min = i+2,
					Max = i+2,
					Width = Math.Max(7, columnWidthAndStyle[(int)i].Key + 2),
					Style = style,
				});
				hasKey = false;
			}
			columns.AppendChild(new Column()
			{
				Min = (uint)columnWidthAndStyle.Count + 2,
				Max = 16384,
				Width = 9,
				Style = 1,
			});
		}

		private SheetData AppendSheet(WorksheetPart worksheetPart, WorkbookPart workbookpart)
		{
			SheetData sheetData = new SheetData();
			worksheetPart.Worksheet.AppendChild(sheetData);

			// Add Sheets to the Workbook.
			Sheets sheets = workbookpart.Workbook.
				AppendChild<Sheets>(new Sheets());

			// Append a new worksheet and associate it with the workbook.
			Sheet sheet = new Sheet()
			{
				Id = workbookpart.GetIdOfPart(worksheetPart),
				SheetId = 1,
				Name = "data"
			};
			sheets.Append(sheet);

			return sheetData;
		}

		private void DoExport(string saveFileName, string rootName, string elemName, Action<Row[]> buildContent, bool hasKey)
		{
			if(File.Exists(saveFileName)) saveFileName = saveFileName.Replace(".xlsx", "_2.xlsx");

			Console.WriteLine("Export Excel {0}", saveFileName);

			selectedColor = 0;
			columnWidthAndStyle.Clear();
			SpreadsheetDocument spreadsheetDocument = SpreadsheetDocument.Create(saveFileName, SpreadsheetDocumentType.Workbook);

			// Add a WorkbookPart to the document.
			WorkbookPart workbookpart = spreadsheetDocument.AddWorkbookPart();
			spreadsheetDocument.ChangeIdOfPart(workbookpart, "rIdbk0");
			workbookpart.Workbook = new Workbook();

			AppendStylePart(spreadsheetDocument);

			// Add a WorksheetPart to the WorkbookPart.
			WorksheetPart worksheetPart = workbookpart.AddNewPart<WorksheetPart>("rIdsht0");
			worksheetPart.Worksheet = new Worksheet();

			AppendSheetViews(worksheetPart, hasKey);

			Columns columns = new Columns();
			worksheetPart.Worksheet.AppendChild(columns);

			Row[] rows = AppendDefaultRows(worksheetPart, workbookpart, string.IsNullOrEmpty(elemName) ? rootName : rootName + "/" + elemName);
			buildContent(rows);

			AppendColumns(columns, hasKey);

			workbookpart.Workbook.Save();
			
			// Close the document.
			spreadsheetDocument.Close();

			RepackFile(saveFileName);
		}

		private void RepackFile(string saveFileName)
		{
			string temp = saveFileName + ".zip";
			if(File.Exists(temp)) File.Delete(temp);

			ZipOutputStream output = new ZipOutputStream(new FileStream(temp, FileMode.CreateNew));
			
			ZipInputStream input = new ZipInputStream(new FileStream(saveFileName, FileMode.Open));

			ZipEntry entry = null;
			while((entry = input.GetNextEntry()) != null)
			{
				var clone = new ZipEntry(entry.Name);
				clone.CompressionMethod = entry.CompressionMethod;
				clone.DateTime = DateTime.MinValue;
				output.PutNextEntry(clone);
				StreamUtils.Copy(input, output, buffer);
				output.CloseEntry();
			}

			input.Close();
			output.Close();

			File.Delete(saveFileName);
			File.Move(temp, saveFileName);
			File.Delete(temp);
		}

		private void ExportContent(Type confType, string key, Row[] rows)
		{
			if(TypeUtil.IsDictionary(confType))
				BuildHead(rows, TypeUtil.GetDictionaryValueType(confType), key);
			else if(TypeUtil.IsArray(confType))
				BuildHead(rows, TypeUtil.GetArrayValueType(confType));
			else
				BuildHead(rows, confType);
		}

		private void Export(Type confType, string key, string saveFileName)
		{
			string rootName, elemName;
			ConfigManager.Instance.GetConfigRootName(confType, out rootName, out elemName);

			DoExport(saveFileName, rootName, elemName, rows => ExportContent(confType, key, rows),
				TypeUtil.IsDictionary(confType));
		}
		
		public void ExportEnums()
		{
			string folder = Path.GetFullPath(PathManager.Instance.ExternalExcelExampleFolder);
			string saveFileName = Path.Combine(folder, "enum.xlsx");

			if(File.Exists(saveFileName)) File.Delete(saveFileName);

			SpreadsheetDocument spreadsheetDocument = SpreadsheetDocument.Create(saveFileName, SpreadsheetDocumentType.Workbook);

			// Add a WorkbookPart to the document.
			WorkbookPart workbookpart = spreadsheetDocument.AddWorkbookPart();
			spreadsheetDocument.ChangeIdOfPart(workbookpart, "rIdbk0");
			workbookpart.Workbook = new Workbook();

			AppendStylePart(spreadsheetDocument);

			// Add a WorksheetPart to the WorkbookPart.
			WorksheetPart worksheetPart = workbookpart.AddNewPart<WorksheetPart>("rIdsht0");
			worksheetPart.Worksheet = new Worksheet();

			Columns columns = new Columns();
			columns.AppendChild(new Column()
			{
				Min = 1,
				Max = 1,
				Width = 20,
			});
			columns.AppendChild(new Column()
			{
				Min = 2,
				Max = 2,
				Width = 10,
			});
			columns.AppendChild(new Column()
			{
				Min = 3,
				Max = 3,
				Width = 20,
			});
			columns.AppendChild(new Column()
			{
				Min = 4,
				Max = 4,
				Width = 20,
			});
			worksheetPart.Worksheet.AppendChild(columns);

			SheetData sheetData = AppendSheet(worksheetPart, workbookpart);

			int maxlen = 0;
			foreach(var type in enums)
			{
				var nocommentAttribute = TypeUtil.GetCustomAttribute<NoCommentAttribute>(type, false);
				if(nocommentAttribute != null) continue;

				UInt32Value style = 5;
				Row row = sheetData.AppendChild(new Row() { CustomFormat = true, StyleIndex = style });
				row.AppendChild(CreateCell(type.Name, style, ref maxlen));

				string[] names = Enum.GetNames(type);
				foreach(var name in names)
				{
					if(row == null)
					{
						row = sheetData.AppendChild(new Row());
						row.AppendChild(CreateCell("", null, ref maxlen));
					}
					var value = Enum.Parse(type, name);
					row.AppendChild(CreateCell(TypeUtil.GetEnumComment(type, value as Enum), style, ref maxlen));
					row.AppendChild(CreateCell(name, style, ref maxlen));
					row.AppendChild(CreateCell((int)value, style, ref maxlen));
					row = null;
					style = null;
				}
			}
			
			workbookpart.Workbook.Save();

			// Close the document.
			spreadsheetDocument.Close();

			RepackFile(saveFileName);
		}

		private Cell CreateCell(string content, UInt32Value styleIndex, ref int maxlen)
		{
			if(content != null) maxlen = Math.Max(maxlen, content.Length);
			CellValue value = new CellValue(content);
			return new Cell { CellValue = value, DataType = new EnumValue<CellValues>(CellValues.String), StyleIndex = styleIndex };
		}

		private Cell CreateCell(int content, UInt32Value styleIndex, ref int maxlen)
		{
			CellValue value = new CellValue(content.ToString());
			return new Cell { CellValue = value, DataType = new EnumValue<CellValues>(CellValues.Number), StyleIndex = styleIndex };
		}

		private void BuildHead(Row[] rows, Type confType, string key = null)
		{
			var fields = TypeUtil.GetPublicInstanceFieldsExcept(confType, new[]{typeof(NonSerializedAttribute)});
			fields.RemoveAll(o => TypeUtil.IsDelegation(o.FieldType));

			if(!string.IsNullOrEmpty(key))
			{
				var keyField = fields.FirstOrDefault(f => f.Name.ToLower() == key.ToLower());
				fields.Remove(keyField);
				fields.Insert(0, keyField);
			}

			if(upper.Contains(confType)) return;

			upper.Add(confType);
			foreach(var field in fields)
			{
				BuildField(rows, field);
			}
			upper.Remove(confType);
		}
		
		private void BuildClass(Row[] rows, FieldInfo field, Type type)
		{
			CommentAttribute comment = (field != null ? TypeUtil.GetAttribute<CommentAttribute>(field) : null) 
				?? TypeUtil.GetAttribute<CommentAttribute>(type);

			if(type == typeof(string) || customAtomType.Contains(type) ||
			   type.IsGenericType && type.GetGenericTypeDefinition() == typeof(SerializableEnum<>) || TypeUtil.IsEnum(type) || TypeUtil.IsPrimitive(type))
			{
				if(type.IsEnum && !enums.Contains(type)) enums.Add(type);
				if(type.IsGenericType && type.GetGenericTypeDefinition() == typeof(SerializableEnum<>))
				{
					Type enumType = type.GetGenericArguments().FirstOrDefault();
					if(!enums.Contains(enumType)) enums.Add(enumType);
				}
				string head = comment != null ? comment.Comment : "";

				int max = 0;
				rows[0].AppendChild(CreateCell(head, 2, ref max));
				rows[1].AppendChild(CreateCell(field != null ? field.Name : GetFieldName(type), 3, ref max));
				rows[2].AppendChild(CreateCell(GetTypeName(type), 3, ref max));
				columnWidthAndStyle.Add(new KeyValuePair<int, uint>(max, 1));
			}
			else
			{
				uint color = selectedColor * 2 + 6;
				selectedColor = (uint)((selectedColor + 1) % groupColors.Length);
				BuildStartColumn(true, rows, comment != null ? comment.Comment : "", field != null ? field.Name : type.Name, color);
				BuildHead(rows, type);
				BuildEndColumn(true, rows, field != null ? field.Name : type.Name, color + 1);
			}
		}

		private void BuildStartColumn(bool clazz, Row[] rows, string comment, string name, uint color)
		{
			int max = 0;
			rows[0].AppendChild(CreateCell(comment, 2, ref max));
			rows[1].AppendChild(CreateCell(name, 3, ref max));
			rows[2].AppendChild(CreateCell(clazz ? "{" : "[", 3, ref max));
			columnWidthAndStyle.Add(new KeyValuePair<int, uint>(max, color));
		}

		private void BuildEndColumn(bool clazz, Row[] rows, string name, uint color)
		{
			int max = 0;
			rows[0].AppendChild(CreateCell("", 2, ref max));
			rows[1].AppendChild(CreateCell(name, 3, ref max));
			rows[2].AppendChild(CreateCell(clazz ? "}" : "]", 3, ref max));
			columnWidthAndStyle.Add(new KeyValuePair<int, uint>(max, color));
		}
		
		private void BuildCollection(Row[] rows, FieldInfo field, Type keyType, Type valueType)
		{
			uint color = selectedColor * 2 + 6;
			selectedColor = (uint)((selectedColor + 1) % groupColors.Length);

			CommentAttribute comment = TypeUtil.GetAttribute<CommentAttribute>(field);
			BuildStartColumn(false, rows, comment != null ? comment.Comment : "", field.Name, color);
			for(int i = 0; i < 1; i++)
			{
				if(keyType != null) BuildClass(rows, null, keyType);
				BuildClass(rows, null, valueType);
			}
			BuildEndColumn(false, rows, field.Name, color + 1);
		}
		
		private void BuildField(Row[] rows, FieldInfo field)
		{
			if(TypeUtil.IsSubclassOfDictionary(field.FieldType))
			{
				BuildCollection(rows, field, TypeUtil.GetDictionaryKeyType(field.FieldType), TypeUtil.GetDictionaryValueType(field.FieldType));
			}
			else if(TypeUtil.IsArray(field.FieldType))
			{
				BuildCollection(rows, field, null, TypeUtil.GetArrayValueType(field.FieldType));
			}
			else if(TypeUtil.IsList(field.FieldType))
			{
				BuildCollection(rows, field, null, TypeUtil.GetListValueType(field.FieldType));
			}
			else
			{
				BuildClass(rows, field, field.FieldType);
			}
		}

		private string GetFieldName(Type type)
		{
			if(type.IsGenericType && type.GetGenericTypeDefinition() == typeof(SerializableEnum<>))
			{
				type = type.GetGenericArguments().FirstOrDefault();
			}
			if(fieldNames.ContainsKey(type))
			{
				return fieldNames[type];
			}
			if(typeNames.ContainsKey(type))
			{
				return typeNames[type].ToLower();
			}
			return type.Name;
		}

		private string GetTypeName(Type type)
		{
			if(type.IsGenericType && type.GetGenericTypeDefinition() == typeof(SerializableEnum<>))
			{
				type = type.GetGenericArguments().FirstOrDefault();
				return GetTypeName(type);
			}
			if(typeNames.ContainsKey(type))
			{
				return typeNames[type];
			}
			if(type.IsEnum)
			{
				return "ENUM/" + type.Name;
			}
			return type.Name.ToUpper();
		}
	}
}
#endif