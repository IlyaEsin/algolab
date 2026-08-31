using System.Text;
using AlgoLab.Runner;

// The console codepage the OS attaches at spawn time is not under our control (it differs
// between a real console and a redirected pipe), but the parent always deserializes the
// stream as UTF-8. Write through an explicitly-encoded, BOM-less writer so the bytes on
// stdout are deterministic regardless of what environment launches this process.
using var output = new StreamWriter(Console.OpenStandardOutput(), new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));

return RunnerProgram.Run(args, output);
