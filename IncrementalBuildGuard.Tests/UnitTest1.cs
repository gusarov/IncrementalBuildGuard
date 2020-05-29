using System;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace IncrementalBuildGuard.Tests
{
	[TestClass]
	public class UnitTest1
	{
		private static object ref1 = typeof(IncrementalBuildGuardLogger);

		int Run(string cmd = null)
		{
			if (cmd == null)
			{
				var binDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
				cmd = Path.Combine(binDir, "ibg.cmd");
			}
			// Console.WriteLine(Environment.GetEnvironmentVariable("PATH"));
			var psi = new ProcessStartInfo(cmd);
			psi.UseShellExecute = false;
			psi.RedirectStandardOutput = true;
			psi.RedirectStandardError = true;
			var p = Process.Start(psi);
			p.BeginOutputReadLine();
			p.BeginErrorReadLine();
			p.OutputDataReceived += (s, e) =>
			{
				Console.WriteLine(e.Data);
			};
			p.ErrorDataReceived += (s, e) =>
			{
				Console.WriteLine(e.Data);
			};
			p.WaitForExit();
			return p.ExitCode;
		}

		// public TestContext TestContext { get; set; }

		[TestInitialize]
		public void TestInit()
		{
			var tmpdir = Path.Combine(Path.GetTempPath(), "IbgTest");
			Directory.CreateDirectory(tmpdir);
			// Console.WriteLine(TestContext.DeploymentDirectory);
			Directory.SetCurrentDirectory(tmpdir);
			foreach (var file in Directory.GetFiles(tmpdir))
			{
				File.Delete(file);
			}

			Console.WriteLine(Assembly.GetExecutingAssembly().Location);
			var binDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
			var path = Environment.GetEnvironmentVariable("PATH");
			if (!path.Contains(binDir))
			{
				path += ";" + binDir;
				Environment.SetEnvironmentVariable("PATH", path);
			}
		}

		[TestMethod]
		public void Should_pass_normal_project()
		{
			File.WriteAllText("normal.csproj", @"<Project Sdk=""Microsoft.NET.Sdk"">
  <PropertyGroup>
    <TargetFramework>netstandard2.0</TargetFramework>
  </PropertyGroup>

</Project>
");
			File.WriteAllText("normal.cs", @"class Test {}");
			Assert.AreEqual(0, Run());
		}

		[TestMethod]
		public void Should_fail_bad_project()
		{
			File.WriteAllText("bad.csproj", @"<Project Sdk=""Microsoft.NET.Sdk"">
  <PropertyGroup>
    <TargetFramework>netstandard2.0</TargetFramework>
  </PropertyGroup>
  <Target Name=""Date"" BeforeTargets=""CoreCompile"">
    <WriteLinesToFile File=""$(IntermediateOutputPath)gen.cs"" Lines=""static partial class Builtin { public static long CompileTime = $([System.DateTime]::UtcNow.Ticks) %3B }"" Overwrite=""true"" />
    <ItemGroup>
        <Compile Include=""$(IntermediateOutputPath)gen.cs"" />
    </ItemGroup>
  </Target>
</Project>
");
			File.WriteAllText("Test.cs", @"class Test {}");
			Assert.AreEqual(1, Run());
		}

		[TestMethod]
		public void Should_fail_non_buildable_project()
		{
			File.WriteAllText("nonbuild.csproj", @"<Project Sdk=""Microsoft.NET.Sdk"">
  <PropertyGroup>
    <TargetFramework>netstandard2.0</TargetFramework>
  </PropertyGroup>
</Project>
");
			File.WriteAllText("Test.cs", @"class Test { asd }");
			Assert.AreEqual(1, Run());
		}
	}
}
