
namespace Atma
{
    using System.Diagnostics;

    [DebuggerStepThrough]
    public static partial class Contract
    {
        public static void EqualTo(bool actual, bool expected) { if (!(actual == expected)) throw ContractException.GenerateException(actual, expected); }
        public static void NotEqualTo(bool actual, bool expected) { if (!(actual != expected)) throw ContractException.GenerateException(actual, expected); }
        public static void EqualTo(byte actual, byte expected) { if (!(actual == expected)) throw ContractException.GenerateException(actual, expected); }
        public static void NotEqualTo(byte actual, byte expected) { if (!(actual != expected)) throw ContractException.GenerateException(actual, expected); }
        public static void GreatherThan(byte actual, byte expected) { if (!(actual > expected)) throw ContractException.GenerateException(actual, expected); }
        public static void GreatherThanEqualTo(byte actual, byte expected) { if (!(actual >= expected)) throw ContractException.GenerateException(actual, expected); }
        public static void LessThan(byte actual, byte expected) { if (!(actual < expected)) throw ContractException.GenerateException(actual, expected); }
        public static void LessThanEqualTo(byte actual, byte expected) { if (!(actual <= expected)) throw ContractException.GenerateException(actual, expected); }
        public static void EqualTo(sbyte actual, sbyte expected) { if (!(actual == expected)) throw ContractException.GenerateException(actual, expected); }
        public static void NotEqualTo(sbyte actual, sbyte expected) { if (!(actual != expected)) throw ContractException.GenerateException(actual, expected); }
        public static void GreatherThan(sbyte actual, sbyte expected) { if (!(actual > expected)) throw ContractException.GenerateException(actual, expected); }
        public static void GreatherThanEqualTo(sbyte actual, sbyte expected) { if (!(actual >= expected)) throw ContractException.GenerateException(actual, expected); }
        public static void LessThan(sbyte actual, sbyte expected) { if (!(actual < expected)) throw ContractException.GenerateException(actual, expected); }
        public static void LessThanEqualTo(sbyte actual, sbyte expected) { if (!(actual <= expected)) throw ContractException.GenerateException(actual, expected); }
        public static void EqualTo(ushort actual, ushort expected) { if (!(actual == expected)) throw ContractException.GenerateException(actual, expected); }
        public static void NotEqualTo(ushort actual, ushort expected) { if (!(actual != expected)) throw ContractException.GenerateException(actual, expected); }
        public static void GreatherThan(ushort actual, ushort expected) { if (!(actual > expected)) throw ContractException.GenerateException(actual, expected); }
        public static void GreatherThanEqualTo(ushort actual, ushort expected) { if (!(actual >= expected)) throw ContractException.GenerateException(actual, expected); }
        public static void LessThan(ushort actual, ushort expected) { if (!(actual < expected)) throw ContractException.GenerateException(actual, expected); }
        public static void LessThanEqualTo(ushort actual, ushort expected) { if (!(actual <= expected)) throw ContractException.GenerateException(actual, expected); }
        public static void EqualTo(short actual, short expected) { if (!(actual == expected)) throw ContractException.GenerateException(actual, expected); }
        public static void NotEqualTo(short actual, short expected) { if (!(actual != expected)) throw ContractException.GenerateException(actual, expected); }
        public static void GreatherThan(short actual, short expected) { if (!(actual > expected)) throw ContractException.GenerateException(actual, expected); }
        public static void GreatherThanEqualTo(short actual, short expected) { if (!(actual >= expected)) throw ContractException.GenerateException(actual, expected); }
        public static void LessThan(short actual, short expected) { if (!(actual < expected)) throw ContractException.GenerateException(actual, expected); }
        public static void LessThanEqualTo(short actual, short expected) { if (!(actual <= expected)) throw ContractException.GenerateException(actual, expected); }
        public static void EqualTo(uint actual, uint expected) { if (!(actual == expected)) throw ContractException.GenerateException(actual, expected); }
        public static void NotEqualTo(uint actual, uint expected) { if (!(actual != expected)) throw ContractException.GenerateException(actual, expected); }
        public static void GreatherThan(uint actual, uint expected) { if (!(actual > expected)) throw ContractException.GenerateException(actual, expected); }
        public static void GreatherThanEqualTo(uint actual, uint expected) { if (!(actual >= expected)) throw ContractException.GenerateException(actual, expected); }
        public static void LessThan(uint actual, uint expected) { if (!(actual < expected)) throw ContractException.GenerateException(actual, expected); }
        public static void LessThanEqualTo(uint actual, uint expected) { if (!(actual <= expected)) throw ContractException.GenerateException(actual, expected); }
        public static void EqualTo(int actual, int expected) { if (!(actual == expected)) throw ContractException.GenerateException(actual, expected); }
        public static void NotEqualTo(int actual, int expected) { if (!(actual != expected)) throw ContractException.GenerateException(actual, expected); }
        public static void GreatherThan(int actual, int expected) { if (!(actual > expected)) throw ContractException.GenerateException(actual, expected); }
        public static void GreatherThanEqualTo(int actual, int expected) { if (!(actual >= expected)) throw ContractException.GenerateException(actual, expected); }
        public static void LessThan(int actual, int expected) { if (!(actual < expected)) throw ContractException.GenerateException(actual, expected); }
        public static void LessThanEqualTo(int actual, int expected) { if (!(actual <= expected)) throw ContractException.GenerateException(actual, expected); }
        public static void EqualTo(float actual, float expected) { if (!(actual == expected)) throw ContractException.GenerateException(actual, expected); }
        public static void NotEqualTo(float actual, float expected) { if (!(actual != expected)) throw ContractException.GenerateException(actual, expected); }
        public static void GreatherThan(float actual, float expected) { if (!(actual > expected)) throw ContractException.GenerateException(actual, expected); }
        public static void GreatherThanEqualTo(float actual, float expected) { if (!(actual >= expected)) throw ContractException.GenerateException(actual, expected); }
        public static void LessThan(float actual, float expected) { if (!(actual < expected)) throw ContractException.GenerateException(actual, expected); }
        public static void LessThanEqualTo(float actual, float expected) { if (!(actual <= expected)) throw ContractException.GenerateException(actual, expected); }
        public static void EqualTo(double actual, double expected) { if (!(actual == expected)) throw ContractException.GenerateException(actual, expected); }
        public static void NotEqualTo(double actual, double expected) { if (!(actual != expected)) throw ContractException.GenerateException(actual, expected); }
        public static void GreatherThan(double actual, double expected) { if (!(actual > expected)) throw ContractException.GenerateException(actual, expected); }
        public static void GreatherThanEqualTo(double actual, double expected) { if (!(actual >= expected)) throw ContractException.GenerateException(actual, expected); }
        public static void LessThan(double actual, double expected) { if (!(actual < expected)) throw ContractException.GenerateException(actual, expected); }
        public static void LessThanEqualTo(double actual, double expected) { if (!(actual <= expected)) throw ContractException.GenerateException(actual, expected); }

    }

    [DebuggerStepThrough]
    public static partial class Assert
    {
        [Conditional("DEBUG"), Conditional("ASSERT")] public static void EqualTo(bool actual, bool expected) { if (!(actual == expected)) throw ContractException.GenerateException(actual, expected); }
        [Conditional("DEBUG"), Conditional("ASSERT")] public static void NotEqualTo(bool actual, bool expected) { if (!(actual != expected)) throw ContractException.GenerateException(actual, expected); }
        [Conditional("DEBUG"), Conditional("ASSERT")] public static void EqualTo(byte actual, byte expected) { if (!(actual == expected)) throw ContractException.GenerateException(actual, expected); }
        [Conditional("DEBUG"), Conditional("ASSERT")] public static void NotEqualTo(byte actual, byte expected) { if (!(actual != expected)) throw ContractException.GenerateException(actual, expected); }
        [Conditional("DEBUG"), Conditional("ASSERT")] public static void GreatherThan(byte actual, byte expected) { if (!(actual > expected)) throw ContractException.GenerateException(actual, expected); }
        [Conditional("DEBUG"), Conditional("ASSERT")] public static void GreatherThanEqualTo(byte actual, byte expected) { if (!(actual >= expected)) throw ContractException.GenerateException(actual, expected); }
        [Conditional("DEBUG"), Conditional("ASSERT")] public static void LessThan(byte actual, byte expected) { if (!(actual < expected)) throw ContractException.GenerateException(actual, expected); }
        [Conditional("DEBUG"), Conditional("ASSERT")] public static void LessThanEqualTo(byte actual, byte expected) { if (!(actual <= expected)) throw ContractException.GenerateException(actual, expected); }
        [Conditional("DEBUG"), Conditional("ASSERT")] public static void EqualTo(sbyte actual, sbyte expected) { if (!(actual == expected)) throw ContractException.GenerateException(actual, expected); }
        [Conditional("DEBUG"), Conditional("ASSERT")] public static void NotEqualTo(sbyte actual, sbyte expected) { if (!(actual != expected)) throw ContractException.GenerateException(actual, expected); }
        [Conditional("DEBUG"), Conditional("ASSERT")] public static void GreatherThan(sbyte actual, sbyte expected) { if (!(actual > expected)) throw ContractException.GenerateException(actual, expected); }
        [Conditional("DEBUG"), Conditional("ASSERT")] public static void GreatherThanEqualTo(sbyte actual, sbyte expected) { if (!(actual >= expected)) throw ContractException.GenerateException(actual, expected); }
        [Conditional("DEBUG"), Conditional("ASSERT")] public static void LessThan(sbyte actual, sbyte expected) { if (!(actual < expected)) throw ContractException.GenerateException(actual, expected); }
        [Conditional("DEBUG"), Conditional("ASSERT")] public static void LessThanEqualTo(sbyte actual, sbyte expected) { if (!(actual <= expected)) throw ContractException.GenerateException(actual, expected); }
        [Conditional("DEBUG"), Conditional("ASSERT")] public static void EqualTo(ushort actual, ushort expected) { if (!(actual == expected)) throw ContractException.GenerateException(actual, expected); }
        [Conditional("DEBUG"), Conditional("ASSERT")] public static void NotEqualTo(ushort actual, ushort expected) { if (!(actual != expected)) throw ContractException.GenerateException(actual, expected); }
        [Conditional("DEBUG"), Conditional("ASSERT")] public static void GreatherThan(ushort actual, ushort expected) { if (!(actual > expected)) throw ContractException.GenerateException(actual, expected); }
        [Conditional("DEBUG"), Conditional("ASSERT")] public static void GreatherThanEqualTo(ushort actual, ushort expected) { if (!(actual >= expected)) throw ContractException.GenerateException(actual, expected); }
        [Conditional("DEBUG"), Conditional("ASSERT")] public static void LessThan(ushort actual, ushort expected) { if (!(actual < expected)) throw ContractException.GenerateException(actual, expected); }
        [Conditional("DEBUG"), Conditional("ASSERT")] public static void LessThanEqualTo(ushort actual, ushort expected) { if (!(actual <= expected)) throw ContractException.GenerateException(actual, expected); }
        [Conditional("DEBUG"), Conditional("ASSERT")] public static void EqualTo(short actual, short expected) { if (!(actual == expected)) throw ContractException.GenerateException(actual, expected); }
        [Conditional("DEBUG"), Conditional("ASSERT")] public static void NotEqualTo(short actual, short expected) { if (!(actual != expected)) throw ContractException.GenerateException(actual, expected); }
        [Conditional("DEBUG"), Conditional("ASSERT")] public static void GreatherThan(short actual, short expected) { if (!(actual > expected)) throw ContractException.GenerateException(actual, expected); }
        [Conditional("DEBUG"), Conditional("ASSERT")] public static void GreatherThanEqualTo(short actual, short expected) { if (!(actual >= expected)) throw ContractException.GenerateException(actual, expected); }
        [Conditional("DEBUG"), Conditional("ASSERT")] public static void LessThan(short actual, short expected) { if (!(actual < expected)) throw ContractException.GenerateException(actual, expected); }
        [Conditional("DEBUG"), Conditional("ASSERT")] public static void LessThanEqualTo(short actual, short expected) { if (!(actual <= expected)) throw ContractException.GenerateException(actual, expected); }
        [Conditional("DEBUG"), Conditional("ASSERT")] public static void EqualTo(uint actual, uint expected) { if (!(actual == expected)) throw ContractException.GenerateException(actual, expected); }
        [Conditional("DEBUG"), Conditional("ASSERT")] public static void NotEqualTo(uint actual, uint expected) { if (!(actual != expected)) throw ContractException.GenerateException(actual, expected); }
        [Conditional("DEBUG"), Conditional("ASSERT")] public static void GreatherThan(uint actual, uint expected) { if (!(actual > expected)) throw ContractException.GenerateException(actual, expected); }
        [Conditional("DEBUG"), Conditional("ASSERT")] public static void GreatherThanEqualTo(uint actual, uint expected) { if (!(actual >= expected)) throw ContractException.GenerateException(actual, expected); }
        [Conditional("DEBUG"), Conditional("ASSERT")] public static void LessThan(uint actual, uint expected) { if (!(actual < expected)) throw ContractException.GenerateException(actual, expected); }
        [Conditional("DEBUG"), Conditional("ASSERT")] public static void LessThanEqualTo(uint actual, uint expected) { if (!(actual <= expected)) throw ContractException.GenerateException(actual, expected); }
        [Conditional("DEBUG"), Conditional("ASSERT")] public static void EqualTo(int actual, int expected) { if (!(actual == expected)) throw ContractException.GenerateException(actual, expected); }
        [Conditional("DEBUG"), Conditional("ASSERT")] public static void NotEqualTo(int actual, int expected) { if (!(actual != expected)) throw ContractException.GenerateException(actual, expected); }
        [Conditional("DEBUG"), Conditional("ASSERT")] public static void GreatherThan(int actual, int expected) { if (!(actual > expected)) throw ContractException.GenerateException(actual, expected); }
        [Conditional("DEBUG"), Conditional("ASSERT")] public static void GreatherThanEqualTo(int actual, int expected) { if (!(actual >= expected)) throw ContractException.GenerateException(actual, expected); }
        [Conditional("DEBUG"), Conditional("ASSERT")] public static void LessThan(int actual, int expected) { if (!(actual < expected)) throw ContractException.GenerateException(actual, expected); }
        [Conditional("DEBUG"), Conditional("ASSERT")] public static void LessThanEqualTo(int actual, int expected) { if (!(actual <= expected)) throw ContractException.GenerateException(actual, expected); }
        [Conditional("DEBUG"), Conditional("ASSERT")] public static void EqualTo(float actual, float expected) { if (!(actual == expected)) throw ContractException.GenerateException(actual, expected); }
        [Conditional("DEBUG"), Conditional("ASSERT")] public static void NotEqualTo(float actual, float expected) { if (!(actual != expected)) throw ContractException.GenerateException(actual, expected); }
        [Conditional("DEBUG"), Conditional("ASSERT")] public static void GreatherThan(float actual, float expected) { if (!(actual > expected)) throw ContractException.GenerateException(actual, expected); }
        [Conditional("DEBUG"), Conditional("ASSERT")] public static void GreatherThanEqualTo(float actual, float expected) { if (!(actual >= expected)) throw ContractException.GenerateException(actual, expected); }
        [Conditional("DEBUG"), Conditional("ASSERT")] public static void LessThan(float actual, float expected) { if (!(actual < expected)) throw ContractException.GenerateException(actual, expected); }
        [Conditional("DEBUG"), Conditional("ASSERT")] public static void LessThanEqualTo(float actual, float expected) { if (!(actual <= expected)) throw ContractException.GenerateException(actual, expected); }
        [Conditional("DEBUG"), Conditional("ASSERT")] public static void EqualTo(double actual, double expected) { if (!(actual == expected)) throw ContractException.GenerateException(actual, expected); }
        [Conditional("DEBUG"), Conditional("ASSERT")] public static void NotEqualTo(double actual, double expected) { if (!(actual != expected)) throw ContractException.GenerateException(actual, expected); }
        [Conditional("DEBUG"), Conditional("ASSERT")] public static void GreatherThan(double actual, double expected) { if (!(actual > expected)) throw ContractException.GenerateException(actual, expected); }
        [Conditional("DEBUG"), Conditional("ASSERT")] public static void GreatherThanEqualTo(double actual, double expected) { if (!(actual >= expected)) throw ContractException.GenerateException(actual, expected); }
        [Conditional("DEBUG"), Conditional("ASSERT")] public static void LessThan(double actual, double expected) { if (!(actual < expected)) throw ContractException.GenerateException(actual, expected); }
        [Conditional("DEBUG"), Conditional("ASSERT")] public static void LessThanEqualTo(double actual, double expected) { if (!(actual <= expected)) throw ContractException.GenerateException(actual, expected); }

    }
}