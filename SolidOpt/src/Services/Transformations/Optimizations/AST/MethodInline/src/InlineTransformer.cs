﻿/*
 * $Id$
 * It is part of the SolidOpt Copyright Policy (see Copyright.txt)
 * For further details see the nearest License.txt
 */

using System;
using System.Collections.Generic;

using Mono.Cecil;
using Mono.Cecil.Cil;
using Cecil.Decompiler.Ast;

using SolidOpt.Services.Transformations.CodeModel.AbstractSyntacticTree;
using SolidOpt.Services.Transformations.Multimodel.ILtoAST;

using SolidOpt.Services.Transformations.Optimizations;

namespace SolidOpt.Services.Transformations.Optimizations.AST.MethodInline
{
  /// <summary>
  /// Replaces method invocation with the invoked method itself. For correct replacement we need the method,
  /// which is going to be inlined to be maked with the attribute "Inlineable". The method should be pure, which 
  /// means that it shouldn't contain any side-effects.
  /// Inlining method example:
  /// <code>
  /// [Inlineable]
  /// int Inlinee (a, b)
  /// {
  ///   if (a &lt;= b)
  ///     return a * b;
  ///   else
  ///       return a + b;
  /// }
  /// 
  /// void Inliner ()
  /// {
  ///   ...
  ///   int sum = Inlinee(5, 8);
  /// }
  /// </code>
  /// After the transformation Inliner becomes:
  /// <code>
  /// void Inliner ()
  /// {
  ///   ...
  ///   int result;
  ///   if (5 &lt;= 8)
  ///     result = 5 * 8;
  ///   else
  ///       result = 5 + 8;
  ///   int sum = result;
  /// }  
  /// </code>
  /// </summary>
  public class InlineTransformer : BaseCodeTransformer, IOptimize<AstMethodDefinition>
  {
    #region Fields
    
    /// <summary>
    /// Field, where the blocks, containing the method invocations of the inlined method, are stored.
    /// </summary>
    private List<BlockStatement> blocks = new List<BlockStatement>();
    
    /// <summary>
    /// Field, where expressions, containing invocations of the inline candidate are stored.
    /// </summary>
    private List<ExpressionStatement> expressions = new List<ExpressionStatement>();
    
    /// <summary>
    /// Structure, where new local variables are stored. The new local variables are going to replace
    /// the old ones. There shouldn't be local variable, which overlaps with variable of the inlined method.
    /// </summary>
    private Dictionary<VariableDefinition, VariableDefinition> localVarSubstitution =
      new Dictionary<VariableDefinition, VariableDefinition>();
    
    /// <summary>
    /// Structure, where new local variables are stored. The new local variables are going to replace
    /// the arguments of the inlined method. All arguments should be mapped to new local variables.
    /// </summary>
    private Dictionary<ParameterDefinition, Expression> paramVarSubstitution =
      new Dictionary<ParameterDefinition, Expression>();
    
    private Expression thisSubstitution;
    
    private AstFinalFixer fixer = new AstFinalFixer();
    
    private static VariableReference returnVariable;
    public static VariableReference ReturnVariable {
      get { return returnVariable; }
      set { returnVariable = value; }
    }
    
    private static ParameterReference returnParameter;
    public static ParameterReference ReturnParameter {
      get { return returnParameter; }
      set { returnParameter = value; }
    }
    
    private AstMethodDefinition source;
    
    private List<SideEffectInfo> sideEffects = new List<SideEffectInfo>();
    private SideEffectInfo currentSideEffect = new SideEffectInfo();
    
    #endregion
    
    #region Constructors
    
    public InlineTransformer()
    {
    }
    
    #endregion

    public AstMethodDefinition Transform(AstMethodDefinition source)
    {
      return Optimize(source);
    }
    
    public AstMethodDefinition Optimize(AstMethodDefinition source)
    {
//      if (source.Method.Name == "OutParamAssign")
//        return source;
      
      expressions.Clear();
      blocks.Clear();
      localVarSubstitution.Clear();
      this.source = source;
      source.CecilBlock = (BlockStatement) Visit(source.CecilBlock);
      source = fixer.FixUp(source, localVarSubstitution);
      
      return source;
    }
    
    #region Model Transformers
    
    
    
//    public override ICodeNode VisitAssignExpression(AssignExpression node)
//    {
//      CodeNodeCollection<Expression> collection = new CodeNodeCollection<Expression>();
//      collection.Add(node.Expression);
//      collection = (CodeNodeCollection<Expression>) Visit (collection);
//      
//      if (collection.Count > 0 && collection[0].Equals(node.Expression)) {
//        return node;
//      }
//      
//      node.Expression = collection[collection.Count - 1];
//      collection[collection.Count - 1] = node;
//      return collection;
//    }
    
    public override ICodeNode VisitExpressionStatement(ExpressionStatement node)
    {
      sideEffects.Clear();
      currentSideEffect = new SideEffectInfo();
      
      ICodeNode result = (ICodeNode) Visit (node.Expression);
      CodeNodeCollection<Expression> original = result as CodeNodeCollection<Expression>;
      
      if (original != null) {
        var collection = new CodeNodeCollection<Statement>();
        
        for (int i = 0; i < original.Count; i++) {
          collection.Add(new ExpressionStatement(original[i]));
        }
        
        return (CodeNodeCollection<Statement>) Visit(collection);
      }
      else {
        node.Expression = (Expression) result;
        
        var assignExp = result as AssignExpression;
        if (assignExp != null) {
          var mInvoke = assignExp.Expression as MethodInvocationExpression;
          if (mInvoke != null && IsSimpleInlineCase(mInvoke)) {
            return InlineExpansion(mInvoke, assignExp.Target, source);
          }
          else {
            return node;
          }
        }
        else {
          SideEffectInfo row;
          MethodInvocationExpression mInvoke;
          MethodReferenceExpression mRef;
          var expansion = new CodeNodeCollection<Statement>();
          
          for (int i = 0; i < sideEffects.Count; i++) {
            row = sideEffects[i];
            VariableDefinition @var;
            
            for (int j = 0; j < row.SideEffectsInNode.Count; j++) {
              mInvoke = row.SideEffectsInNode[j];
              mRef = mInvoke.Method as MethodReferenceExpression;
              //Mono.Cecil 0.9.3 migration: if (mRef.Method.ReturnType.ReturnType.Name != "Void") {
              if (mRef.Method.ReturnType.Name != "Void") {
                expansion.Add(new ExpressionStatement(new AssignExpression(
                  new VariableReferenceExpression(row.SideEffectsInNodeVar[j]), mInvoke)));
              }
              else {
                expansion.Add(new ExpressionStatement(mInvoke));
              }
            }
            mRef = row.mInvokeNode.Method as MethodReferenceExpression;
            for (int j = 0; j < row.mInvokeNode.Arguments.Count; j++) {
              ParameterDefinition paramDef = mRef.Method.Parameters[j];
              
              //enable constant folding
              Expression arg = row.mInvokeNode.Arguments[j];
              if (!(arg is ArgumentReferenceExpression || arg is VariableReferenceExpression
                    || arg is LiteralExpression)) {
                @var = RegisterVariable(paramDef.ParameterType, source.Method);
              
                expansion.Add(new ExpressionStatement(new AssignExpression(
                  new VariableReferenceExpression(@var), arg)));
                row.mInvokeNode.Arguments[j] = new VariableReferenceExpression(@var);
              }
//              @var = RegisterVariable(paramDef.ParameterType, source.Method);
              
//              expansion.Add(new ExpressionStatement(new AssignExpression(
//                new VariableReferenceExpression(@var), row.mInvokeNode.Arguments[j])));
//              row.mInvokeNode.Arguments[j] = new VariableReferenceExpression(@var);
            }
            
            //TODO: Трябва да се оптимизира в случая когато има странични ефекти, а няма инлайн
            //или има странични ефекти останали след последния инлайн.
            for (int j = 0; j < currentSideEffect.SideEffectsInNode.Count; j++) {
              mInvoke = currentSideEffect.SideEffectsInNode[j];
              mRef = mInvoke.Method as MethodReferenceExpression;
              expansion.Add(new ExpressionStatement(new AssignExpression(
                new VariableReferenceExpression(currentSideEffect.SideEffectsInNodeVar[j]), mInvoke)));
            }
            //endtodo.
            
            mRef = row.mInvokeNode.Method as MethodReferenceExpression;
            
            //Mono.Cecil 0.9.3 migration: if (mRef.Method.ReturnType.ReturnType.Name != "Void") {
            if (mRef.Method.ReturnType.Name != "Void") {
              expansion.Add(new ExpressionStatement(new AssignExpression(
                  new VariableReferenceExpression(row.mInvokeNodeVar), row.mInvokeNode)));
            }
            else {
              expansion.Add(new ExpressionStatement(row.mInvokeNode));
            }
          }
          
          expansion.Add(node);
          
          if (expansion.Count > 1) {
            return (CodeNodeCollection<Statement>) Visit(expansion);
          }
          else {
            return node;
          }
        }
      }
    }
    /// <summary>
    /// This handles the case Expr '=' MethodInvocationExpr ';'
    /// For example:
    /// <code>sometype somevar = Inlinee(...)</code>
    /// </summary>
    /// <param name="node">
    /// A <see cref="AssignExpression"/>
    /// </param>
    /// <returns>
    /// A <see cref="ICodeNode"/>
    /// </returns>
    public override ICodeNode VisitAssignExpression(AssignExpression node)
    {
      ICodeNode result;
      if (node.Expression is MethodInvocationExpression) {
        var oldExpression = node.Expression;
        
        result = (ICodeNode) base.VisitAssignExpression(node);
        
        AssignExpression assign = (AssignExpression) result;
        var varRef = assign.Expression as VariableReferenceExpression;
        if (varRef != null) {
          assign.Expression = oldExpression;
          return assign;
        } 
//        else {
//          var varDecl = assign.Expression as VariableDeclarationExpression;
//          if (varDecl != null) {
//            assign.Expression = oldExpression;
//            return assign;
//          } 
//        }
      }
      else {
        result = (ICodeNode) base.VisitAssignExpression(node);
      }
      return result;
    }
    
    /// <summary>
    /// This handles the cases MethodInvocationExpr BinaryOp MethodInvocationExpr.
    /// For example:
    /// <code>MethodInv1(...) == MethodInv2(...)</code>
    /// We make the substitution
    /// <code>
    /// sometype1 somevar1 = MethodInv1(...);
    /// sometype2 somevar2 = MethodInv2(...);
    /// somevar1 == somevar2
    /// </code>
    /// </summary>
    /// <param name="node">
    /// A <see cref="BinaryExpression"/>
    /// </param>
    /// <returns>
    /// A <see cref="ICodeNode"/>
    /// </returns>
    public override ICodeNode VisitBinaryExpression(BinaryExpression node)
    {
      ICodeNode currentLeft = (ICodeNode) Visit(node.Left);
      ICodeNode currentRight = (ICodeNode) Visit(node.Right);
      
      var argExpand = new CodeNodeCollection<Expression>();
      
      var argAsCollection = currentLeft as CodeNodeCollection<Expression>;
      if (argAsCollection != null) {
        for (int j = 0; j < argAsCollection.Count - 1; j++) {
          argExpand.Add(argAsCollection[j]);
        }
        node.Left = argAsCollection[argAsCollection.Count - 1];
      }
      else {
        node.Left = (Expression) currentLeft;
      }
      
      argAsCollection = currentRight as CodeNodeCollection<Expression>;
      if (argAsCollection != null) {
        for (int j = 0; j < argAsCollection.Count - 1; j++) {
          argExpand.Add(argAsCollection[j]);
        }
        node.Right = argAsCollection[argAsCollection.Count - 1];
      }
      else {
        node.Right = (Expression) currentRight;
      }
      
      if (argExpand.Count > 0) {
        argExpand.Add(node);
        return argExpand;
      }
      else {
        return node;
      }
    }
    
    /// <summary>
    /// This handles the cases UnaryOperator MethodInvocationExpr.
    /// For example:
    /// <code>!MethodInv(...)</code>
    /// We make the substitution:
    /// <code>
    /// sometype somevar = MethodInv(...);
    /// !somevar
    /// </code>
    /// </summary>
    /// <param name="node">
    /// A <see cref="UnaryExpression"/>
    /// </param>
    /// <returns>
    /// A <see cref="ICodeNode"/>
    /// </returns>
    public override ICodeNode VisitUnaryExpression(UnaryExpression node)
    {
      ICodeNode currentOperand = (ICodeNode) Visit(node.Operand);
      
      var argAsCollection = currentOperand as CodeNodeCollection<Expression>;
      if (argAsCollection != null) {
        node.Operand = argAsCollection[argAsCollection.Count - 1];
        argAsCollection[argAsCollection.Count - 1] = node;
        return argAsCollection;
      }
      else {
        node.Operand = (Expression) currentOperand;
        return node;
      }
    }
    
    /// <summary>
    /// This handles the case MethodInvocationExpr(...).
    /// If the MethodInvocationExpr is appropriate for inlining then we inline it or if it is not appropriate
    /// we check if there is in the arguments something to be inlined.
    /// For example:
    /// <code> MethodInv1(somearg1, Inlinee(...), someargs...) </code>
    /// In that case we make the substitution:
    /// <code>
    /// sometype somevar = Inlinee(...);//here we inline the actual code of the method
    /// MethodInv1(somearg1, somevar, someargs...)
    /// </code>
    /// </summary>
    /// <param name="node">
    /// A <see cref="MethodInvocationExpression"/>
    /// </param>
    /// <returns>
    /// A <see cref="ICodeNode"/>
    /// </returns>
    public override ICodeNode VisitMethodInvocationExpression(MethodInvocationExpression node)
    {
      MethodReferenceExpression methodRef = (MethodReferenceExpression) node.Method;
        
      if (IsInlineable(methodRef.Method)) {
        currentSideEffect.mInvokeNode = node;
        VariableReferenceExpression varRefExp = null;
        //Mono.Cecil 0.9.3 migration: if (methodRef.Method.ReturnType.ReturnType.FullName != "System.Void") {
        //Mono.Cecil 0.9.3 migration:   currentSideEffect.mInvokeNodeVar = RegisterVariable(methodRef.Method.ReturnType.ReturnType, source.Method);
        if (methodRef.Method.ReturnType.FullName != "System.Void") {
          currentSideEffect.mInvokeNodeVar = RegisterVariable(methodRef.Method.ReturnType, source.Method);
          varRefExp = new VariableReferenceExpression(currentSideEffect.mInvokeNodeVar);
        }
        sideEffects.Add(currentSideEffect);
        currentSideEffect = new SideEffectInfo();
        return varRefExp;
        
      }
//      else if (HasSideEffects(methodRef.Method)) {
//        currentSideEffect.SideEffectsInNode.Add(node);
//        VariableDefinition @var = RegisterVariable(methodRef.Method.ReturnType.ReturnType, source.Method);
//        currentSideEffect.SideEffectsInNodeVar.Add(@var);
//        return new VariableReferenceExpression(@var);
//        
//      }
      
      //node.Method = (Expression) Visit(node.Method);
      
      var argExpand = new CodeNodeCollection<Expression>();
      
      for (int i = 0; i < node.Arguments.Count; i++) {
        ICodeNode currentArgument = (ICodeNode) Visit(node.Arguments[i]); 
        var argAsCollection = currentArgument as CodeNodeCollection<Expression>;
        
        if (argAsCollection != null) {
          for (int j = 0; j < argAsCollection.Count - 1; j++) {
            argExpand.Add(argAsCollection[j]);
          }
          node.Arguments[i] = argAsCollection[argAsCollection.Count - 1];
        }
      }
      if (argExpand.Count > 0) {
        argExpand.Add(node);
        return argExpand;
      }
      else {
        return node;
      }
    }
    
    
    public override ICodeNode VisitIfStatement(IfStatement node)
    {
      return base.VisitIfStatement(node);
    }
    
    
    #endregion
    
    /// <summary>
    /// Checks if the method is appropriate for inlining. 
    /// </summary>
    /// <param name="method">
    /// A <see cref="MethodReference"/>
    /// </param>
    /// <returns>
    /// A <see cref="System.Boolean"/>
    /// </returns>
    private bool IsInlineable(MethodReference method)
    {
      foreach (CustomAttribute ca in method.Resolve().CustomAttributes) {
        if (ca.Constructor.DeclaringType.FullName == (typeof(InlineableAttribute).FullName)) {
          return true;
        }
      }
      return false;
    }
    
    /// <summary>
    /// Check if the method is pure. I.e has no side-effects. 
    /// </summary>
    /// <param name="method">
    /// A <see cref="MethodReference"/>
    /// </param>
    /// <returns>
    /// A <see cref="System.Boolean"/>
    /// </returns>
    private bool HasSideEffects(MethodReference method)
    {
      foreach (CustomAttribute ca in method.Resolve().CustomAttributes) {
        if (ca.Constructor.DeclaringType.FullName == (typeof(SideEffectsAttribute).FullName)) {
          //Mono.Cecil 0.9.3 migration: bool a = (bool) ca.ConstructorParameters[0];
          bool a = (bool) ca.ConstructorArguments[0].Value;
          return a;
        }
      }
      return true;
    }
    
    /// <summary>
    /// Checks if the case is the trivial one, where there is no MethodInvocationExpr in the arguments. 
    /// </summary>
    /// <param name="mInvoke">
    /// A <see cref="MethodInvocationExpression"/>
    /// </param>
    /// <returns>
    /// A <see cref="System.Boolean"/>
    /// </returns>
    private bool IsSimpleInlineCase(MethodInvocationExpression mInvoke)
    {
      foreach (Expression arg in mInvoke.Arguments) {
        if (!(arg is ArgumentReferenceExpression || arg is VariableReferenceExpression
            || arg is LiteralExpression)) {
          return false;
        }
      }
      
      return true;
    }
    
    /// <summary>
    ///  Performs the actual inline.
    /// </summary>
    /// <param name="mInvoke">
    /// A <see cref="MethodInvocationExpression"/>
    /// </param>
    /// <param name="target">
    /// A <see cref="Expression"/>
    /// </param>
    /// <param name="source">
    /// A <see cref="AstMethodDefinition"/>
    /// </param>
    /// <returns>
    /// A <see cref="CodeNodeCollection{Statement}"/>
    /// </returns>
    private CodeNodeCollection<Statement> 
      InlineExpansion(MethodInvocationExpression mInvoke, Expression target, AstMethodDefinition source)
    {
      ILtoASTTransformer il2astTransformer = new ILtoASTTransformer();
      AstMethodDefinition ast;
      
      AstPreInsertFixer preFixer = new AstPreInsertFixer();
      
      MethodReferenceExpression mRef = mInvoke.Method as MethodReferenceExpression;

      MethodDefinition mDef = mRef.Method.Resolve();
      var result = new CodeNodeCollection<Statement> ();
      ParameterDefinition paramDef;
      Expression arg;
      
      ReturnVariable = null;
      ReturnParameter = null;
      if (target == null) {
        //Mono.Cecil 0.9.3 migration: if (mDef.ReturnType.ReturnType.FullName != "System.Void") {
        //Mono.Cecil 0.9.3 migration:   ReturnVariable = RegisterVariable(mDef.ReturnType.ReturnType, source.Method);
        if (mDef.ReturnType.FullName != "System.Void") {
          ReturnVariable = RegisterVariable(mDef.ReturnType, source.Method);
        }
      } else if (target is VariableReferenceExpression) {
        ReturnVariable = (target as VariableReferenceExpression).Variable;
      } else if (target is VariableDeclarationExpression) {
        //Mono.Cecil 0.9.3 migration: ReturnVariable = RegisterVariable(mDef.ReturnType.ReturnType, source.Method);
        ReturnVariable = RegisterVariable(mDef.ReturnType, source.Method);
      } else if (target is ArgumentReferenceExpression) {
        ReturnParameter = (target as ArgumentReferenceExpression).Parameter;
      }
      
      //заместване на this
      
      if (mRef.Target != null) {
        thisSubstitution = new VariableReferenceExpression(
          //Mono.Cecil 0.9.3 migration: RegisterVariable(mDef.This.ParameterType, source.Method));
          //new ParameterDefinition ("this", (ParameterAttributes) 0, null);
          RegisterVariable(null, source.Method));
      }
      else {
        thisSubstitution = null;
      }
      
      for (int current = mInvoke.Arguments.Count - 1; current >= 0 ; current--) {
        arg = mInvoke.Arguments[current];
        paramDef = mRef.Method.Parameters[current];
        paramVarSubstitution[paramDef] = arg;
      }
    
      //Ако вече е inline-вано
      if (mDef.Body.Variables.Count != 0 && (!localVarSubstitution.ContainsKey(mDef.Body.Variables[0]))) {
        foreach (VariableDefinition variable in mDef.Body.Variables) {
          localVarSubstitution[variable] = RegisterVariable(variable.VariableType, source.Method);
        }
      }
      
//      foreach (KeyValuePair<VariableDefinition, VariableDefinition> pair in localVarSubstitution) {
//        Console.WriteLine("!!! {0} -> {1}", pair.Key.Name, pair.Value.Name);
//      }
      
      ast = preFixer.FixUp(il2astTransformer.Decompile(mDef), paramVarSubstitution, thisSubstitution);
      
      
      //this
      if (thisSubstitution != null) {
        result.Add(new ExpressionStatement(new AssignExpression(thisSubstitution, mRef.Target)));
      }
      
      for (int i = 0; i < ast.Block.Statements.Count; i++ ) {
        result.Add(ast.CecilBlock.Statements[i]);
      }
      
      return result;
    }
    
    /// <summary>
    /// Adds new variable to the given MethodBody. 
    /// </summary>
    /// <param name="type">
    /// A <see cref="TypeReference"/>
    /// </param>
    /// <param name="method">
    /// A <see cref="MethodDefinition"/>
    /// </param>
    /// <returns>
    /// A <see cref="VariableDefinition"/>
    /// </returns>
    internal VariableDefinition RegisterVariable(TypeReference type, MethodDefinition method)
    {
      VariableDefinition variable = new VariableDefinition(type);
      //Mono.Cecil 0.9.3 migration: variable.Method = method;
      //variable.Method = method;
      //variable.Index = method.Body.Variables.Count;
      variable.Name = variable.ToString();
      method.Body.Variables.Add(variable);
      
      return variable;
    }
    
    
    /// <summary>
    /// Performs preliminary transformations upon the inlined method to prepare it for the actual inlining. 
    /// </summary>
    internal class AstPreInsertFixer : BaseCodeTransformer 
    {
      #region Fields
      
      private Dictionary<ParameterDefinition, Expression> paramVarSubstitution;
      private AstMethodDefinition source;
      private LabeledStatement exitLabel;
      private static int exitNumber = 0;
      //private BlockStatement currentBlock;
      private Expression thisSubstitution;
      
      #endregion
      
      #region Constructors
      
      public AstPreInsertFixer ()
      {
        
      }
      
      #endregion
      
      /// <summary>
      /// Adds unique lables to the method in which the inline is performed. This is necessary when we have
      /// return statements. It replaces the return with goto specific unique label
      /// </summary>
      /// <param name="source">
      /// A <see cref="AstMethodDefinition"/>
      /// </param>
      /// <param name="paramVarSubstitution">
      /// A <see cref="System.Collections.Generic.Dictionary{ParameterDefinition, Expression}"/>
      /// </param>
      /// <param name="thisSubstitution">
      /// A <see cref="Expression"/>
      /// </param>
      /// <returns>
      /// A <see cref="AstMethodDefinition"/>
      /// </returns>
      public AstMethodDefinition 
        FixUp(AstMethodDefinition source, Dictionary<ParameterDefinition, 
              Expression> paramVarSubstitution, Expression thisSubstitution)
      {
        this.source = source;
        this.paramVarSubstitution = paramVarSubstitution;
        this.thisSubstitution = thisSubstitution;
        
        exitNumber++;
        this.exitLabel = this.source.CecilBlock.Statements[this.source.Block.Statements.Count-1] as LabeledStatement;
        
        if (exitLabel == null) {
          this.exitLabel = new LabeledStatement("@_exit" + exitNumber);
          this.source.CecilBlock.Statements.Add(exitLabel);
          this.source.CecilBlock = (BlockStatement) Visit(this.source.CecilBlock);
        }
        else {
          this.source.CecilBlock = (BlockStatement) Visit(this.source.CecilBlock);  
        }
        
        return this.source;
      }
      
      #region Model Transformers
      
      /// <summary>
      /// Stores the current block in field. 
      /// </summary>
      /// <param name="node">
      /// A <see cref="BlockStatement"/>
      /// </param>
      /// <returns>
      /// A <see cref="ICodeNode"/>
      /// </returns>
      public override ICodeNode VisitBlockStatement(BlockStatement node)
      {
        //currentBlock = node;
        return base.VisitBlockStatement(node);
      }
      
      
      /// <summary>
      /// Replaces the return statements with goto in the end of the inlined method. 
      /// </summary>
      /// <param name="node">
      /// A <see cref="ReturnStatement"/>
      /// </param>
      /// <returns>
      /// A <see cref="ICodeNode"/>
      /// </returns>
      public override ICodeNode VisitReturnStatement(ReturnStatement node)
      {
        GotoStatement @goto = new GotoStatement(exitLabel.Label);
          
        if (node.Expression != null) {
          CodeNodeCollection<Statement> collection = new CodeNodeCollection<Statement>();
          if (ReturnVariable != null) {
            collection.Add(new ExpressionStatement(
              new AssignExpression(new VariableReferenceExpression(ReturnVariable), node.Expression)));
          }
          else if (ReturnParameter != null) {
            collection.Add(new ExpressionStatement(
              new AssignExpression(new ArgumentReferenceExpression(ReturnParameter), node.Expression)));
          }
          else {
            collection.Add(new ExpressionStatement(node.Expression));
          }
          collection.Add(@goto);
          return Visit<CodeNodeCollection<Statement>, Statement>(collection);
        }
        
        return @goto;
      }
      
      /// <summary>
      /// Makes the branches pointing to the new labels 
      /// </summary>
      /// <param name="node">
      /// A <see cref="GotoStatement"/>
      /// </param>
      /// <returns>
      /// A <see cref="ICodeNode"/>
      /// </returns>
      public override ICodeNode VisitGotoStatement(GotoStatement node)
      {
//        LabeledStatement lbl = currentBlock.Statements[currentBlock.Statements.IndexOf(node)+1] as LabeledStatement;
//        if (lbl != null && lbl.Label == node.Label) {
//          return null;
//        }
//        
//        lastGoTo = node;
        node.Label = "@_" + source.Method.Name + "@" + exitNumber + node.Label;
        return base.VisitGotoStatement(node);
      }
      
      /// <summary>
      /// Makes other goto constructions unique, because they may overlap the existing goto in the method, 
      /// where we are inlining
      /// </summary>
      /// <param name="node">
      /// A <see cref="LabeledStatement"/>
      /// </param>
      /// <returns>
      /// A <see cref="ICodeNode"/>
      /// </returns>
      public override ICodeNode VisitLabeledStatement(LabeledStatement node)
      {
        node.Label = "@_" + source.Method.Name + "@" + exitNumber + node.Label;
        
//        if (lastGoTo.Label == node.Label) {
//          currentBlock.Statements.Remove(lastGoTo);
//          return null;
//        }
          
        return base.VisitLabeledStatement(node);
      }
      
      public override ICodeNode VisitArgumentReferenceExpression(ArgumentReferenceExpression node)
      {
        Expression exp;
        if (paramVarSubstitution.TryGetValue(node.Parameter.Resolve(), out exp)) {
          return exp;
        }
        return base.VisitArgumentReferenceExpression(node);
      }
      
      /// <summary>
      /// Replaces this pointer if there is one in the inlined method. 
      /// </summary>
      /// <param name="node">
      /// A <see cref="FieldReferenceExpression"/>
      /// </param>
      /// <returns>
      /// A <see cref="ICodeNode"/>
      /// </returns>
      public override ICodeNode VisitFieldReferenceExpression(FieldReferenceExpression node)
      {
        if (thisSubstitution != null && node.Target == null
            && node.Field.DeclaringType == source.Method.DeclaringType) {
          
          node.Target = thisSubstitution;          
        }
        
        return base.VisitFieldReferenceExpression(node);
      }
      
      /// <summary>
      /// Replaces this pointer if there is one in the inlined method. 
      /// </summary>
      /// <param name="node">
      /// A <see cref="MethodReferenceExpression"/>
      /// </param>
      /// <returns>
      /// A <see cref="ICodeNode"/>
      /// </returns>
      public override ICodeNode VisitMethodReferenceExpression(MethodReferenceExpression node)
      {
        if (thisSubstitution != null && node.Target == null
            && node.Method.DeclaringType == source.Method.DeclaringType) {
          
          node.Target = thisSubstitution;          
        }
        
        return base.VisitMethodReferenceExpression(node);
      }
      
      /// <summary>
      /// The actual substitution of the this pointer. 
      /// </summary>
      /// <param name="node">
      /// A <see cref="ThisReferenceExpression"/>
      /// </param>
      /// <returns>
      /// A <see cref="ICodeNode"/>
      /// </returns>
      public override ICodeNode VisitThisReferenceExpression(ThisReferenceExpression node)
      {
        return thisSubstitution;
      }
      
      #endregion
    }
    
    /// <summary>
    /// Performs transformations at the end of the process.
    /// </summary>
    internal class AstFinalFixer : BaseCodeTransformer
    {
      #region Fields
      
      private AstMethodDefinition source;
      private Dictionary<VariableDefinition, VariableDefinition> localVarSubstitution;
      //TODO: HashSet<> is .net 4.0 class. May be we need use some 2.0 class (Dictionary<,>)?
      private HashSet<VariableDefinition> isVariableDefined = new HashSet<VariableDefinition>();
      private AssignExpression lastAssignment;
      
      #endregion
      
      #region Constructors
      
      public AstFinalFixer()
      {
      }
      
      #endregion
      
      public AstMethodDefinition 
        FixUp(AstMethodDefinition source, Dictionary<VariableDefinition, 
              VariableDefinition> localVarSubstitution)
      {
        isVariableDefined.Clear();
        
        this.source = source;
        this.localVarSubstitution = localVarSubstitution;
        if (localVarSubstitution.Count > 0) {
          this.source.CecilBlock = (BlockStatement) Visit(this.source.CecilBlock);
        }
        return this.source;
      }
      
      #region Model Transformations
      
      /// <summary>
      /// Just checking for empty expressions. 
      /// </summary>
      /// <param name="node">
      /// A <see cref="ExpressionStatement"/>
      /// </param>
      /// <returns>
      /// A <see cref="ICodeNode"/>
      /// </returns>
      public override ICodeNode VisitExpressionStatement(ExpressionStatement node)
      {
        var result = base.VisitExpressionStatement(node);
        if (node.Expression == null) {
          return null;
        }
        return result;
      }
      
      /// <summary>
      /// Stores the last visited AssignmentExpr. 
      /// </summary>
      /// <param name="node">
      /// A <see cref="AssignExpression"/>
      /// </param>
      /// <returns>
      /// A <see cref="ICodeNode"/>
      /// </returns>
      public override ICodeNode VisitAssignExpression(AssignExpression node)
      {
        lastAssignment = node;
        return base.VisitAssignExpression(node);
      }
      
      /// <summary>
      /// If it encouters variable reference, belonging to the inlined method it is substituted with the 
      /// new variable definition.
      /// </summary>
      /// <param name="node">
      /// A <see cref="VariableReferenceExpression"/>
      /// </param>
      /// <returns>
      /// A <see cref="ICodeNode"/>
      /// </returns>
      public override ICodeNode VisitVariableReferenceExpression(VariableReferenceExpression node)
      {
        VariableDefinition varDef;
        if (localVarSubstitution.TryGetValue(node.Variable.Resolve(), out varDef)) {
          node.Variable = varDef;
        }
        return base.VisitVariableReferenceExpression(node);
      }

      /// <summary>
      /// Checks if the variable is already defined and replaces the VariableDefinition with 
      /// VariableReference
      /// </summary>
      /// <param name="node">
      /// A <see cref="VariableDeclarationExpression"/>
      /// </param>
      /// <returns>
      /// A <see cref="ICodeNode"/>
      /// </returns>
      public override ICodeNode VisitVariableDeclarationExpression(VariableDeclarationExpression node)
      {
        VariableDefinition varDef;
        if (localVarSubstitution.TryGetValue(node.Variable, out varDef)) {
          if (!isVariableDefined.Add(node.Variable)) {
            var varRef = lastAssignment.Target as VariableDeclarationExpression;
            if (varRef != null) {
              if (varRef.Variable == varDef) {
                throw new NotImplementedException();
              }
              else {
                return new VariableReferenceExpression(varDef);
              }
            }
            else {
              return null;
            }
          }
          else {
            node.Variable = varDef;
          }
        }
        return base.VisitVariableDeclarationExpression(node);
      }
      
      #endregion
    }
  }
  
  internal class SideEffectInfo
  {
    public MethodInvocationExpression mInvokeNode = null;
    public VariableDefinition mInvokeNodeVar = null;
    public List<MethodInvocationExpression> SideEffectsInNode = new List<MethodInvocationExpression>();
    public List<VariableDefinition> SideEffectsInNodeVar = new List<VariableDefinition>();
    
    
    public SideEffectInfo()
    {
    }

    public SideEffectInfo(MethodInvocationExpression mInvokeNode, VariableDefinition mInvokeNodeVar,
              List<MethodInvocationExpression> SideEffectsInNode,
              List<VariableDefinition> SideEffectsInNodeVar)
    {
      this.mInvokeNode = mInvokeNode;
      this.mInvokeNodeVar = mInvokeNodeVar;
      this.SideEffectsInNode = SideEffectsInNode;
      this.SideEffectsInNodeVar = SideEffectsInNodeVar;
    }
  }
}
