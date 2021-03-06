﻿using System;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using System.IO;
using System.Text.RegularExpressions;
using System.Linq;
using System.Text;
using System.Collections.Generic;

namespace Bin2CMSBuildTask
{
	public class Bin2C : Task
	{
		private static readonly Regex _titleFormat = new Regex("\\W", RegexOptions.Compiled);

		[Required]
		public ITaskItem[] InputFiles { get; set; }

		[Required]
		public ITaskItem OutputFile { get; set; }

		public override bool Execute()
		{
			if(InputFiles.Any(e => !File.Exists(e.ToString())))
			{
				Log.LogError("File(s) not found: {0}", string.Join(", ", InputFiles.Where(e => !File.Exists(e.ToString())).Select(e => e.ToString())));
				return false;
			}
				
			var outputFileName = Path.GetFileName(OutputFile.ToString());
			var hFilePath = string.Format("{0}.h", OutputFile);
			var hFileName = string.Format("{0}.h", outputFileName);
			var cFilePath = string.Format("{0}.c", OutputFile);
			var outputFileTitle = _titleFormat.Replace(outputFileName.ToString(), "_");
			var outputFileTitleUpper = outputFileTitle.ToUpper();

			try
			{
				using(var hFile = new FileStream(hFilePath, FileMode.Create, FileAccess.Write))
				{
					using (var hsw = new StreamWriter(hFile))
					{
						hsw.WriteLine("#ifndef __{0}_H__", outputFileTitleUpper);
						hsw.WriteLine("#define __{0}_H__", outputFileTitleUpper);

						using (var cFile = new FileStream(cFilePath, FileMode.Create, FileAccess.Write))
						{
							using (var csw = new StreamWriter(cFile))
							{
								csw.WriteLine("#include \"{0}\"", hFileName);
								csw.WriteLine("const unsigned int {0}_size = {1};", outputFileTitle, InputFiles.Length);

								var filenames = new Stack<string>();
								var data = new Stack<string>();

								foreach(var item in InputFiles)
								{
									var itemName = Path.GetFileName(item.ToString());

									var title = _titleFormat.Replace(itemName, "_");

									Log.LogMessage("Writing file {0} to variable {1}", itemName, title);

									filenames.Push(string.Format("\"{0}\"", itemName));
									data.Push(title);

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

								csw.WriteLine("const char * {0}_filenames[] = {{{1}}};", outputFileTitle, string.Join(",", filenames));
								csw.WriteLine("const unsigned char * {0}_data[] = {{{1}}};", outputFileTitle, string.Join(",", data));
							}
						}

						hsw.WriteLine("extern const char * {0}_filenames[];", outputFileTitle);
						hsw.WriteLine("extern const unsigned char * {0}_data[];", outputFileTitle);
						hsw.WriteLine("extern const unsigned int {0}_size;", outputFileTitle);

						hsw.WriteLine("#endif");
					}
				}
			}
			catch(IOException ex)
			{
				Log.LogErrorFromException(ex);
				return false;
			}

			return true;
		}
	}
}

