using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;

namespace WallPaperClassificator.Util
{
	public class IOUtils
	{
		public static void ComputeFileHash(string path, out byte[] fileHash)
		{
			if (!File.Exists(path))
			{
				throw new FileNotFoundException($"File not found: {path}");
			}
			using FileStream fs = File.OpenRead(path);
			fileHash =  SHA256.Create().ComputeHash(fs);
		}

		public static ResultData<string> CreateDirectory(string path, bool checkFileContains = true)
		{
			if (File.Exists(path))
			{
				return Result.Error<string>("The path is already allocated to a file. please delete it.");
			}

			if (Directory.Exists(path))
			{
				if (Directory.GetFiles(path).Length > 0 && checkFileContains)
				{
					return Result.Error<string>("The directory includes some file(s), please delete them.");
				}
				else
				{
					return Result.Ok<string>(path);
				}
			}
			else
			{
				try
				{
					Directory.CreateDirectory(path);
					return Result.Ok<string>(path);
				}
				catch (Exception e)
				{
					return Result.Error<string>(e.Message);
				}
			}
		}
	}
}
