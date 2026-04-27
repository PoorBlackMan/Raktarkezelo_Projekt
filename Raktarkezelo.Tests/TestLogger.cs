using System;
using Xunit.Abstractions;

namespace Raktarkezelo.Tests
{
    public static class TestLogger
    {
        public static void Log(ITestOutputHelper output, string testId, string description, bool success)
        {
            string status = success ? "SIKERES" : "HIBÁS";
            string message = $"[{testId}] {description} => {status}";

            Console.WriteLine(message);
            output.WriteLine(message);
        }

        public static void LogStep(ITestOutputHelper output, string message)
        {
            Console.WriteLine(message);
            output.WriteLine(message);
        }
    }
}