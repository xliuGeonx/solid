/*
 * $Id$
 * It is part of the SolidOpt Copyright Policy (see Copyright.txt)
 * For further details see the nearest License.txt
 */

using System;
using System.IO;

using Mono.Cecil;

using NUnit.Framework;

using SolidOpt.Services.Transformations.Multimodel.ILtoCG;
using SolidOpt.Services.Transformations.CodeModel.CallGraph;

using SolidOpt.Services.Multimodel.Test;

namespace SolidOpt.Services.Transformations.Multimodel.ILtoCG.Test
{
 [TestFixture]
 public class CGTestFixture : BaseTestFixture {

   public override void RunTestCase(string testCaseName)
   {
      string testCaseFile = GetTestCaseFullPath(testCaseName);
      // Check whether the file exists first.
      Assert.IsTrue(File.Exists(testCaseFile),
                    String.Format("{0} does not exist.", testCaseName));

      string testCaseResultFile = GetTestCaseResultFullPath(testCaseName);
      // Check whether the result file exists first.
      Assert.IsTrue(File.Exists(testCaseResultFile),
                    String.Format("{0} does not exist.", testCaseResultFile));

      MethodDefinition mainMethodDef = LoadTestCaseMethod(testCaseName);
      var transformIL = new CilToCallGraph();
      CallGraph cg = transformIL.Process(mainMethodDef.Body);
      string errMsg = String.Empty;
      string seen = cg.ToString();
      string expected = File.ReadAllText(GetTestCaseResultFullPath(testCaseName));
      Assert.IsTrue(Validate(seen, expected, ref errMsg), errMsg);
   }

    protected override string GetTestCaseFileExtension()
    {
      return "il";
    }

    protected override string GetTestCaseResultFileExtension()
    {
      return "il.cg";
    }

    [Test]
    public void TwoCalls()
    {
      RunTestCase("TwoCalls");
    }

    [Test]
    [ExpectedException(typeof(XFailException))]
    public void XFailTwoSystemCalls()
    {
      RunTestCase("TwoSystemCalls");
    }
  }
}
