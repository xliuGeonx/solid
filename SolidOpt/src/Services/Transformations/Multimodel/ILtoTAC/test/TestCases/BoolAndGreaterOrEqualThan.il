// RUN: "@ILASM@" /DLL @TEST_CASE@
.assembly TestCase {}

.class public auto ansi beforefieldinit TestCase
       extends [mscorlib]System.Object
{
  // public static bool Main(string s) {
  //   bool flag;
  //   if (s.StartsWith("A"))
  //     flag = s.Length >= 3;
  //   else
  //     flag = false;
  //   return flag;
  // }
	.method public hidebysig static bool Main(string s) cil managed
	{
    .maxstack  2
		.locals init (bool local0)
		IL_0000:  ldarg.0
		IL_0001:  ldstr "A"
		IL_0006:  callvirt bool string::StartsWith(string)
		IL_000b:  brfalse.s IL_001c
		IL_000d:  ldarg.0
		IL_000e:  callvirt int32 string::get_Length()
		IL_0013:  ldc.i4.3
		IL_0014:  clt
		IL_0016:  ldc.i4.0
		IL_0017:  ceq
    IL_0019:  stloc.0
    IL_001a:  br.s       IL_001e
    IL_001c:  ldc.i4.0
    IL_001d:  stloc.0
    IL_001e:  ldloc.0
    IL_001f:  ret
	}
}
