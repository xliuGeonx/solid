// RUN: "@ILASM@" /DLL @TEST_CASE@
.assembly TestCase {}

.class public auto ansi beforefieldinit TestCase
       extends [mscorlib]System.Object
{
  //  public static bool Main(int x) {
  //    bool flag;
  //    flag = val >= 42;
  //    return flag;
  // }
	.method public hidebysig static bool Main(int32 x) cil managed
	{
		.locals init (bool local0)
		IL_0000: ldarg.0
		IL_0001: ldc.i4.s 42
		IL_0003: clt
		IL_0005: ldc.i4.0
		IL_0006: ceq
		IL_0008: stloc.0
		IL_0009: br.s IL_000b
		IL_000b: ldloc.0
		IL_000c: ret
	}
}
