using System;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using System.IO;
using System.Text.RegularExpressions;
using System.Linq;

namespace Bin2CMSBuildTask
{
	public class Bin2C : Task
	{
		private static readonly Regex _titleFormat = new Regex("\\W", RegexOptions.Compiled);

		[Required]
		public ITaskItem[] InputAssemblies { get; set; }

		[Required]
		public ITaskItem OutputFile { get; set; }

		public override bool Execute()
		{
			if(InputAssemblies.Any(e => !File.Exists(e.ToString())))
			{
				return false;
			}

			var hFileName = string.Format("{0}.h", OutputFile);
			var cFileName = string.Format("{0}.c", OutputFile);

			try
			{
				using(var hFile = new FileStream(hFileName, FileMode.Create, FileAccess.Write))
				{
					using (var hsw = new StreamWriter(hFile))
					{
						var outputFileTitle = _titleFormat.Replace(OutputFile.ToString(), "_").ToUpper();

						hsw.WriteLine("#ifndef __{0}_H__", outputFileTitle);
						hsw.WriteLine("#define __{0}_H__", outputFileTitle);

						using (var cFile = new FileStream(cFileName, FileMode.Create, FileAccess.Write))
						{
							using (var csw = new StreamWriter(cFile))
							{
								foreach(var item in InputAssemblies)
								{
									var title = _titleFormat.Replace(item.ToString(), "_");

									hsw.WriteLine("extern const unsigned char * {0};", title);

									csw.Write("const unsigned char * {0} = \"", title);

									using(var file = new FileStream(item.ToString(), FileMode.Open, FileAccess.Read))
									{
										int b;

										while((b = file.ReadByte()) >= 0)
										{
											csw.Write("\\x{0:X2}", b);
										}
									}

									csw.WriteLine("\";");
								}
							}
						}

						hsw.WriteLine("#endif");
					}
				}
			}
			catch(IOException)
			{
				return false;
			}

			return true;
		}
	}
}

