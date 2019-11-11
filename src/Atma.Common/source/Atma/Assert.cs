namespace Atma
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Text.RegularExpressions;

    public class ContractException : Exception
    {
        private static readonly string[] namespacesToOmit = new string[] { "Atma.Contract", "Atma.Assert", "Xunit.", "System." };
#nullable enable
        private string? _stackTrace;

        public override string? StackTrace
        {
            get => _stackTrace;
        }
#nullable disable

        public ContractException(string message, StackTrace trace) : base(message)
        {
            var sb = new StringBuilder();
            var traceMessage = trace.ToString();
            var lines = traceMessage.Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var line in lines)
            {
                var skip = false;
                foreach (var it in namespacesToOmit)
                {
                    if (line.Contains($"at {it}"))
                    {
                        skip = true;
                        break;
                    }
                }

                if (!skip)
                    sb.AppendLine(line);

            }
            _stackTrace = sb.ToString();
        }

        public static ContractException GenerateException<T>(T actual, T expected)
        {
            var thisClassName = typeof(ContractException).Name;

            var stackTrace = new StackTrace(true);
            var i = 0;
            var frame = stackTrace.GetFrame(i);
            var shouldMethod = "";
            while (namespacesToOmit.Any(x => frame.GetMethod().DeclaringType.FullName.StartsWith(x)))
            {
                shouldMethod = frame.GetMethod().Name;
                frame = stackTrace.GetFrame(++i);
            }
            var lineNumber = frame.GetFileLineNumber() - 1;
            var fileName = frame.GetFileName();

            var lineOfCode = string.Empty;
            var fi = new FileInfo(fileName);
            if (fi.Exists)
            {
                var lines = File.ReadAllLines(fileName);
                if (lines.Length > lineNumber)
                    lineOfCode = lines[lineNumber].Trim().TrimEnd(';');
            }

            var sb = new StringBuilder();
            if (!string.IsNullOrEmpty(lineOfCode))
            {
                sb.AppendLine(lineOfCode);
            }

            var firstPar = lineOfCode.IndexOf('(');
            var lastComma = lineOfCode.LastIndexOf(',');


            if (firstPar > -1 && lastComma > -1)
            {
                sb.Append(lineOfCode.Substring(firstPar + 1, lastComma - firstPar - 1).Trim());
                sb.Append(' ');
            }

            sb.AppendLine(FromPascal(shouldMethod));
            sb.AppendLine(VariableToString(expected));

            sb.AppendLine("  but was");
            sb.AppendLine(VariableToString(actual));

            stackTrace = new StackTrace(i, true);
            return new ContractException(sb.ToString(), stackTrace);
        }

        public static ContractException GenerateException<T>(T actual, T expected0, T expected1)
        {
            var thisClassName = typeof(ContractException).Name;

            var stackTrace = new StackTrace(true);
            var i = 0;
            var frame = stackTrace.GetFrame(i);
            var shouldMethod = "";
            while (namespacesToOmit.Any(x => frame.GetMethod().DeclaringType.FullName.StartsWith(x)))
            {
                shouldMethod = frame.GetMethod().Name;
                frame = stackTrace.GetFrame(++i);
            }
            var lineNumber = frame.GetFileLineNumber() - 1;
            var fileName = frame.GetFileName();

            var lineOfCode = string.Empty;
            var fi = new FileInfo(fileName);
            if (fi.Exists)
            {
                var lines = File.ReadAllLines(fileName);
                if (lines.Length > lineNumber)
                    lineOfCode = lines[lineNumber].Trim().TrimEnd(';');
            }

            var sb = new StringBuilder();
            if (!string.IsNullOrEmpty(lineOfCode))
            {
                sb.AppendLine(lineOfCode);
            }

            var firstPar = lineOfCode.IndexOf('(');
            var lastComma = lineOfCode.LastIndexOf(',');


            if (firstPar > -1 && lastComma > -1)
            {
                sb.Append(lineOfCode.Substring(firstPar + 1, lastComma - firstPar - 1).Trim());
                sb.Append(' ');
            }

            sb.AppendLine(FromPascal(shouldMethod));
            sb.Append(VariableToString(expected0));
            sb.Append(" TO ");
            sb.Append(VariableToString(expected1));

            sb.AppendLine("  but was");
            sb.AppendLine(VariableToString(actual));

            stackTrace = new StackTrace(i, true);
            return new ContractException(sb.ToString(), stackTrace);
        }

        private static string VariableToString<T>(T value)
        {
            if (value is string)
                return "\"" + value + "\"";

            if (value is IEnumerable)
            {
                var objects = ((IEnumerable)value).Cast<object>();
                return "[" + string.Join(", ", objects.Select(o => VariableToString(o)).ToArray()) + "]";
            }

            if (value == null)
                return "null";

            return string.Format(@"{0}", value);
        }
        private static string FromPascal(string pascal)
        {
            return Regex.Replace(pascal, @"([A-Z])", match => " " + match.Value.ToLower()).Trim();
        }
    }

    public static partial class Assert
    {

        [Conditional("DEBUG"), Conditional("ASSERT")]
        public static void EqualTo<T>(T actual, T expected)
        {
            var equality = EqualityComparer<T>.Default;
            if (!equality.Equals(actual, expected))
                throw ContractException.GenerateException(actual, expected);
        }

        [Conditional("DEBUG"), Conditional("ASSERT")]
        public static void NotEqualTo<T>(T actual, T expected)
        {
            var equality = EqualityComparer<T>.Default;
            if (equality.Equals(actual, expected))
                throw ContractException.GenerateException(actual, expected);
        }

        public static void Range(int actual, int inclusiveMin, int exclusiveMax)
        {
            if (!(actual >= inclusiveMin) || !(actual < exclusiveMax))
                throw ContractException.GenerateException(actual, inclusiveMin, exclusiveMax);

        }
    }
}