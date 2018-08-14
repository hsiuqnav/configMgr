using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Kernel.Runtime;

namespace Kernel
{
	public abstract class PlatformModule : Module
	{
		private bool? isBatchMode;

		public abstract string ClientVersion
		{
			get;
		}

		public abstract string DeviceCode
		{
			get;
		}

		public abstract string DeviceModel
		{
			get;
		}

		public abstract bool CheckStorage(long byteSize);

		public abstract bool ApplyPatch(string oldFile, string newFile, string patchFile);

		public abstract string OperatingSystem
		{
			get;
		}

		public abstract Platform Platform
		{
			get;
		}

		public abstract int ProcessorCount
		{
			get;
		}

		public virtual bool CreateParentDirectoryIfNeed(string file)
		{
			try
			{
				var folder = Path.GetDirectoryName(file);
				if(folder != null && !Directory.Exists(folder))
				{
					Directory.CreateDirectory(folder);
				}
				return true;
			}
			catch
			{
				return false;
			}
		}

		public virtual bool DirectoryExists(string path)
		{
			if(path == null)
			{
				return false;
			}
			return Directory.Exists(path);
		}


		public virtual bool FileExists(string path)
		{
			return path != null && File.Exists(path);
		}

		public virtual string[] GetDirectories(string path, string searchPattern = "*", SearchOption searchOption = SearchOption.TopDirectoryOnly)
		{
			if(!Directory.Exists(path))
			{
				return new string[0];
			}

			try
			{
				return Directory.GetDirectories(path, searchPattern, searchOption);
			}
			catch(IOException ex)
			{
				Logger.Warn("Directory.GetDirectories failed twice! path:{0}, searchPattern:{1}, err:{2}, inner err:{3}", path,
					searchPattern, ex.Message, ex.InnerException);
			}
			catch(UnauthorizedAccessException)
			{
				return new string[0];
			}
			return new string[0];
		}

		public virtual string[] GetFiles(string path, string searchPattern,
			SearchOption searchOption)
		{
			if(!Directory.Exists(path))
			{
				return new string[0];
			}

			return Directory.GetFiles(path, searchPattern, searchOption);
		}

		public virtual string[] GetFiles(string path, string searchPattern, bool recursive)
		{
			if(recursive)
			{
				var ret = new List<string>();
				GetFilesRecursive(path, searchPattern, ret);
				return ret.ToArray();
			}
			return GetFiles(path, searchPattern, SearchOption.TopDirectoryOnly);
		}

		public virtual Stream OpenRead(string path)
		{
			if(File.Exists(path))
			{
				return new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
			}
			Logger.Warn("OpenRead failed, file {0} not exists!", path);
			return null;
		}

		public virtual StreamReader OpenText(string path)
		{
			if(File.Exists(path))
			{
				return File.OpenText(path);
			}
			return null;
		}

		public virtual void QuitGame()
		{
			
		}

		public virtual string ReadAllText(string path, Encoding encoding = null)
		{
			try
			{
				return File.ReadAllText(path, encoding ?? Encoding.UTF8);
			}
			catch
			{
				Logger.Warn("ReadAllText failed:{0}", path);
			}
			return string.Empty;
		}

		public virtual void WriteAllText(string content, string path, Encoding encoding = null)
		{
			try
			{
				File.WriteAllText(path, content, encoding ?? Encoding.UTF8);
			}
			catch
			{
				Logger.Warn("WriteAllText failed:{0}", path);
			}
		}

		public virtual void RemoveReadonlyAttribute(string file)
		{
			RemoveAttribute(file, FileAttributes.ReadOnly);
		}

		public virtual void InitPlatformSettings()
		{
		}

		public virtual bool FileMove(string source, string dest)
		{
			try
			{
				File.Move(source, dest);
				return true;
			}
			catch(Exception e)
			{
				Logger.Error("FileMove failed {0}", e);
				return false;
			}
		}

		public virtual void InitPlatformInfoAndDevicesInfo()
		{

		}

		private static void RemoveAttribute(string file, FileAttributes attr)
		{
			try
			{
				if(!File.Exists(file))
				{
					return;
				}
				var attributes = File.GetAttributes(file);
				if((attributes & attr) != 0)
				{
					File.SetAttributes(file, attributes & ~attr);
				}
			}
			catch
			{
				// Empty
			}
		}
		
		private void GetFilesRecursive(string path, string searchPattern, List<string> ret)
		{
			var paths = new Queue<string>();
			paths.Enqueue(path);
			while(paths.Count != 0)
			{
				var p = paths.Dequeue();
				ret.AddRange(Directory.GetFiles(p, searchPattern));
				var dirs = Directory.Exists(p) ? Directory.GetDirectories(p) : null;
				if(dirs != null && dirs.Length > 0)
				{
					for(var i = 0; i < dirs.Length; ++i)
					{
						paths.Enqueue(dirs[i]);
					}
				}
			}
		}
	}
}