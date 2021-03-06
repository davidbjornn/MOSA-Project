/*
 * (c) 2008 MOSA - The Managed Operating System Alliance
 *
 * Licensed under the terms of the New BSD License.
 *
 * Authors:
 *  Phil Garcia (tgiphil) <phil@thinkedge.com>
 */

using System;
using System.Collections.Generic;

using Mosa.Runtime;
using Mosa.Runtime.CompilerFramework;
using Mosa.Runtime.Vm;
using Mosa.Runtime.Metadata;
using Mosa.Runtime.Metadata.Signatures;
using Mosa.Runtime.CompilerFramework.Operands;
using Mosa.Tools.Compiler.LinkTimeCodeGeneration;
using Mosa.Runtime.Linker;

using Mosa.Platforms.x86;
using IR = Mosa.Runtime.CompilerFramework.IR;
using CPUx86 = Mosa.Platforms.x86.CPUx86;

namespace Mosa.Tools.Compiler.x86
{

	/// <summary>
	/// 
	/// </summary>
	public sealed class InterruptBuilderStage : IAssemblyCompilerStage, IPipelineStage
	{
		#region Data Members
		
		private AssemblyCompiler compiler;

		private IAssemblyLinker linker;

		#endregion // Data Members

		#region IPipelineStage

		/// <summary>
		/// Retrieves the name of the compilation stage.
		/// </summary>
		/// <value>The name of the compilation stage.</value>
		string IPipelineStage.Name { get { return @"Interrupt Stage"; } }

		#endregion // IPipelineStage Members

		#region IAssemblyCompilerStage Members
		
		public void Setup(AssemblyCompiler compiler)
		{
			this.compiler = compiler;
			this.linker = compiler.Pipeline.FindFirst<IAssemblyLinker>();
		}

		/// <summary>
		/// Performs stage specific processing on the compiler context.
		/// </summary>
		public void Run()
		{
			CreateISRMethods();
		}

		#endregion // IAssemblyCompilerStage Members

		#region Internal

		/// <summary>
		/// Finds the method.
		/// </summary>
		/// <param name="rt">The runtime type.</param>
		/// <param name="name">The name.</param>
		/// <returns></returns>
		private RuntimeMethod FindMethod(RuntimeType rt, string name)
		{
			foreach (RuntimeMethod method in rt.Methods)
				if (name == method.Name)
					return method;

			throw new MissingMethodException(rt.Name, name);
		}

		/// <summary>
		/// Creates the ISR methods.
		/// </summary>
		private void CreateISRMethods()
		{
			// Get RuntimeMethod for the Mosa.Kernel.X86.IDT.InterruptHandler
			RuntimeType rt = RuntimeBase.Instance.TypeLoader.GetType(@"Mosa.Kernel.X86.IDT");
            if (rt == null)
            {
                throw new CompilationException(@"Failed to locate Mosa.Kernel.X86.IDT while building ISRs for the kernel.");
            }

		    RuntimeMethod InterruptMethod = FindMethod(rt, "InterruptHandler");
			if (InterruptMethod == null)
				return;

            SymbolOperand interruptMethod = SymbolOperand.FromMethod(InterruptMethod);

            SigType I1 = new SigType(CilElementType.I1);
			SigType I4 = new SigType(CilElementType.I4);

			RegisterOperand esp = new RegisterOperand(I4, GeneralPurposeRegister.ESP);

			for (int i = 0; i <= 255; i++) {
				InstructionSet set = new InstructionSet(100);
				Context ctx = new Context(set, -1);

				ctx.AppendInstruction(CPUx86.Instruction.CliInstruction);
				if (i <= 7 || i >= 16 | i == 9) // For IRQ 8, 10, 11, 12, 13, 14 the cpu automatically pushed the error code
					ctx.AppendInstruction(CPUx86.Instruction.PushInstruction, null, new ConstantOperand(I1, 0x0));
				ctx.AppendInstruction(CPUx86.Instruction.PushInstruction, null, new ConstantOperand(I1, (byte)i));
				ctx.AppendInstruction(CPUx86.Instruction.PushadInstruction);
				ctx.AppendInstruction(CPUx86.Instruction.CallInstruction, null, interruptMethod);
				ctx.AppendInstruction(CPUx86.Instruction.PopadInstruction);
				ctx.AppendInstruction(CPUx86.Instruction.AddInstruction, esp, new ConstantOperand(I4, 0x08));
				ctx.AppendInstruction(CPUx86.Instruction.StiInstruction);
				ctx.AppendInstruction(CPUx86.Instruction.IRetdInstruction);

				CompilerGeneratedMethod method = LinkTimeCodeGenerator.Compile(this.compiler, @"InterruptISR" + i.ToString(), set);
			}
		}

		#endregion Internal
	}
}
