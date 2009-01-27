﻿/*
 * Created by SharpDevelop.
 * User: Vassil Vassilev
 * Date: 25.1.2009 г.
 * Time: 13:50
 * 
 */
using System;
using System.Collections.Generic;

namespace SolidOpt.Core.Configurator.TypeResolvers
{
	/// <summary>
	/// Description of TypeManager.
	/// </summary>
	public class TypeManager<TParamName>
	{
//		private	Dictionary<TParamName, object> CIR = new Dictionary<TParamName, object>();

		private ITypeResolver resolvers = new EntryPoint();
		
		public TypeManager()
		{	
		}
		
		public Dictionary<TParamName, object> ResolveTypes(Dictionary<TParamName, object> CIR)
		{
			Dictionary<TParamName, object> dict = new Dictionary<TParamName, object>();

			foreach(KeyValuePair<TParamName, object> item in CIR){
				if (item.Value is Dictionary<TParamName, object>) {
					dict[item.Key] = Build(item.Value as Dictionary<TParamName, object>);
				}
				else {
					dict[item.Key] = resolvers.TryResolve(item.Value);
				}
			}
			return dict;
		}
		
		private Dictionary<TParamName, object> Build(Dictionary<TParamName, object> ResolvedCIR)
		{
			Dictionary<TParamName, object> Result = new Dictionary<TParamName, object>();
			foreach(KeyValuePair<TParamName, object> item in ResolvedCIR){
				if (item.Value is Dictionary<TParamName, object>) {
					Result[item.Key] = Build(item.Value as Dictionary<TParamName, object>);;
				}
				else {
					Result[item.Key] = resolvers.TryResolve(item.Value);
				}
			}
			return Result;
		}
	}
}
