using TestAssist;
using Consola.Tests;
using System.Collections.Generic;

List<string> Args = new List<string>(args);

Consola.StdStream.Init(
    Consola.CreationFlags.CreateLog|
    Consola.CreationFlags.NoInputLog|
    Consola.CreationFlags.UseConsole
);

TestSuite test = new MotorTest(
     Args.Contains("--verbose")
  || Args.Contains("-v"),
     Args.Contains("--xml")
).Run();

return test.wasErrors() ? -1
     : test.hasPassed() ? 0
     : test.getFailures();
