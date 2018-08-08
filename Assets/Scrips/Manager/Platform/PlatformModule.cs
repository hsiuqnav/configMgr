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

		public virtual bool IsBatchMode
		{
			get
			{
				if(isBatchMode == null)
				{
					var args = Environment.GetCommandLineArgs();
					foreach(var arg in args)
					{
						if(arg.ToLowerInvariant().Contains("-batchmode"))
						{
							isBatchMode = true;
							return true;
						}
					}
					isBatchMode = false;
				}
				return isBatchMode.Value;
			}
		}

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

		public virtual bool ClearDirectory(string folder, string extension = null)
		{
			try
			{
				foreach (var file in Directory.GetFiles(folder))
				{
					if(extension == null || file.EndsWith(extension))
					{
						RemoveFileIfExists(file);
					}
				}
				return true;
			}
			catch(Exception ex)
			{
				Logger.Warn("ClearDirectory {0} failed: {1}", folder, ex);
				return false;
			}
		}

		public virtual bool RemoveFolder(string folder)
		{
			try
			{
				Directory.Delete(folder, true);
				return true;
			}
			catch(Exception ex)
			{
				Logger.Warn("Remove {0} failed: {1}", folder, ex);
				return false;
			}
		}

		public virtual bool CreateDirectoryIfNeed(string folder)
		{
			try
			{
				if(!Directory.Exists(folder))
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

		public virtual string[] GetDirectories(string path, string searchPattern, bool recursive)
		{
			if(recursive)
			{
				var ret = new List<string>();
				GetDirectoriesRecursive(path, searchPattern, ret);
				return ret.ToArray();
			}
			return GetDirectories(path, searchPattern);
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

		public virtual Stream OpenAppend(string path)
		{
			try
			{
				return File.Open(path, FileMode.Append, FileAccess.Write);
			}
			catch(Exception e)
			{
				Logger.Warn("OpenAppend failed, path {0}, e {1}", path ?? string.Empty, e);
				return null;
			}
		}

		public virtual StreamReader OpenText(string path)
		{
			if(File.Exists(path))
			{
				return File.OpenText(path);
			}
			return null;
		}

		public virtual Stream OpenWrite(string path)
		{
			return new FileStream(path, FileMode.Create);
		}

		public virtual void QuitGame()
		{
			
		}

		public virtual string[] ReadAllLines(string path, Encoding encoding = null)
		{
			try
			{
				return File.ReadAllLines(path);
			}
			catch
			{
				Logger.Warn("ReadAllLines failed:{0}", path);
			}
			return new string[0];
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

		public virtual bool RemoveFileIfExists(string file)
		{
			try
			{
				if(File.Exists(file))
				{
					RemoveReadonlyAttribute(file);
					File.Delete(file);
					return true;
				}
				return false;
			}
			catch
			{
				return false;
			}
		}

		public virtual void RemoveReadonlyAttribute(string file)
		{
			RemoveAttribute(file, FileAttributes.ReadOnly);
		}

		public virtual string ReplaceExtension(string file, string ext)
		{
			var prefix = string.Empty;
			var directoryName = Path.GetDirectoryName(file);
			return prefix + Path.Combine(directoryName ?? "", Path.GetFileNameWithoutExtension(file) + ext);
		}


		public virtual bool UseWWW()
		{
			return false;
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

		public virtual bool FileReplace(string source, string dest, string backup)
		{
			try
			{
				if(!FileExists(dest))
				{
					return FileMove(source, dest);
				}
				File.Replace(source, dest, backup);
				return true;
			}
			catch(Exception e)
			{
				Logger.Error("FileReplace failed {0}", e);
				return false;
			}
		}

		public string GetFullPath(string path)
		{
			return Path.GetFullPath(path);
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
		
		private void GetDirectoriesRecursive(string path, string searchPattern, List<string> ret)
		{
			var paths = new Queue<string>();
			paths.Enqueue(path);
			while(paths.Count != 0)
			{
				var p = paths.Dequeue();
				var dirs = GetDirectories(p, searchPattern);
				if(dirs != null && dirs.Length > 0)
				{
					ret.AddRange(dirs);
					for(var i = 0; i < dirs.Length; ++i)
					{
						paths.Enqueue(dirs[i]);
					}
				}
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