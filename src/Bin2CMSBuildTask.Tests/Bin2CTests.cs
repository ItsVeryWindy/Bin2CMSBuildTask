using System.IO;
using FakeItEasy;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using NUnit.Framework;

namespace Bin2CMSBuildTask.Tests
{
	[TestFixture]
	public class Bin2CTests
	{
		[SetUp]
		public void SetUp()
		{
			if(File.Exists("output.h")) File.Delete("output.h");
			if(File.Exists("output.c")) File.Delete("output.c");
			if(File.Exists("output.c")) File.Delete("nonexistent.txt");
		}

		[Test]
		public void ShouldGenerateOutputFile()
		{
			var buildEngine = A.Fake<IBuildEngine>();

			var unitUnderTest = new Bin2C
			{
				BuildEngine = buildEngine,
				InputAssemblies = new ITaskItem[] {
					new TaskItem("test.txt")	
				},
				OutputFile = new TaskItem("output")
			};

			Assert.That(unitUnderTest.Execute(), Is.True);
			Assert.That(File.Exists("output.h"), Is.True);
			Assert.That(File.ReadAllText("output.h"), Is.EqualTo(File.ReadAllText("output_expected.h")));
			Assert.That(File.Exists("output.c"), Is.True);
			Assert.That(File.ReadAllText("output.c"), Is.EqualTo(File.ReadAllText("output_expected.c")));
		}

		[Test]
		public void ShouldFailIfAFileCouldNotBeFound()
		{
			var buildEngine = A.Fake<IBuildEngine>();

			var unitUnderTest = new Bin2C
			{
				BuildEngine = buildEngine,
				InputAssemblies = new ITaskItem[] {
					new TaskItem("nonexistent.txt")	
				},
				OutputFile = new TaskItem("output")
			};

			Assert.That(unitUnderTest.Execute(), Is.False);
			Assert.That(File.Exists("output.h"), Is.False);
			Assert.That(File.Exists("output.c"), Is.False);
		}

		[Test]
		public void ShouldFailIfAFileCouldNotBeAccessed()
		{
			using (var fs = new FileStream("output.h", FileMode.Create, FileAccess.ReadWrite))
			{
				var buildEngine = A.Fake<IBuildEngine>();

				var unitUnderTest = new Bin2C {
					BuildEngine = buildEngine,
					InputAssemblies = new ITaskItem[] {
						new TaskItem("test.txt")	
					},
					OutputFile = new TaskItem("output")
				};

				Assert.That(unitUnderTest.Execute(), Is.False);
			}
		}
	}
}

