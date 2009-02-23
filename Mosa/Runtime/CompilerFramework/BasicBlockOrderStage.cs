/*
 * (c) 2008 MOSA - The Managed Operating System Alliance
 *
 * Licensed under the terms of the New BSD License.
 *
 * Authors:
 *  Phil Garcia (tgiphil) <phil@thinkedge.com>
 */

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

namespace Mosa.Runtime.CompilerFramework
{
	/// <summary>
	/// 
	/// </summary>
	public class BasicBlockOrderStage : IMethodCompilerStage
	{
		#region Data members

		/// <summary>
		/// 
		/// </summary>
		protected IArchitecture arch;

		#endregion // Data members

		#region Properties

		/// <summary>
		/// Retrieves the name of the compilation stage.
		/// </summary>
		/// <value>The name of the compilation stage.</value>
		public string Name
		{
			get { return @"Basic Block Order"; }
		}

		#endregion // Properties

		#region IMethodCompilerStage Members

		/// <summary>
		/// Runs the specified compiler.
		/// </summary>
		/// <param name="compiler">The compiler.</param>
		public void Run(IMethodCompiler compiler)
		{
			// Retrieve the basic block provider
			IBasicBlockProvider blockProvider = (IBasicBlockProvider)compiler.GetPreviousStage(typeof(IBasicBlockProvider));

			if (blockProvider == null)
				throw new InvalidOperationException(@"Basic Block Order stage requires basic blocks.");

			// Order the blocks based on loop depth
			IBasicBlockOrder orderProvider = (IBasicBlockOrder)compiler.GetPreviousStage(typeof(IBasicBlockOrder));

			if (orderProvider == null)
				throw new InvalidOperationException(@"Basic Block Order stage requires ordered blocks.");

			// Get list of blocks
			List<BasicBlock> blocks = blockProvider.Blocks;

			// Set the new indexes of each block
			for (int i = 0; i < blocks.Count; i++)
				blocks[orderProvider.OrderedBlocks[i]].Index = i;

			// Sort the blocks based on new index values
			blocks.Sort(CompareBlocksByIndex);
		}

		/// <summary>
		/// Compares two blocks by their index.
		/// </summary>
		/// <param name="a">A.</param>
		/// <param name="b">The b.</param>
		/// <returns></returns>
		private static int CompareBlocksByIndex(BasicBlock a, BasicBlock b)
		{
			return (a.Index - b.Index);
		}

		/// <summary>
		/// Adds to pipeline.
		/// </summary>
		/// <param name="pipeline">The pipeline.</param>
		public void AddToPipeline(CompilerPipeline<IMethodCompilerStage> pipeline)
		{
			pipeline.InsertBefore<IL.CilToIrTransformationStage>(this);
		}

		#endregion // Methods
	}
}