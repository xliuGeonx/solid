// RUN: "@ILASM@" /DLL @TEST_CASE@
.assembly TestCase {}

.class public auto ansi beforefieldinit TestCase
       extends [mscorlib]System.Object
{
	// public static void Main(int x, int y) {
	//   if (x > y)
	//     ConsoleWriteLine("x > y")
	// }
	.method public hidebysig static void Main(int32 x, int32 y) cil managed
	{
		IL_0000: ldarg.0
		IL_0001: ldarg.1
		IL_0002: ble.s IL_000e
		IL_0004: ldstr "x > y"
		IL_0009: call void [mscorlib]System.Console::WriteLine(string)
		IL_000e: ret
	}
}

