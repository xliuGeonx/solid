﻿/*
 * Created by SharpDevelop.
 * User: Vassil Vassilev
 * Date: 24.6.2009 г.
 * Time: 15:52
 * 
 */
using System;
using System.Diagnostics;

namespace TransformLoader
{
	class Program
	{
		public static int Main(string[] args)
		{
			Console.WriteLine("Hello Loader!");
			
			Trace.Listeners.Add(new ConsoleTraceListener());
			
			int result = new TransformLoader().Run(args);
			
			Console.ReadKey(true);
			return result;
			
		}
	}
}