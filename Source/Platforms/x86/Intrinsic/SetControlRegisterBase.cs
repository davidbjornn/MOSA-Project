﻿/*
 * (c) 2008 MOSA - The Managed Operating System Alliance
 *
 * Licensed under the terms of the New BSD License.
 *
 * Authors:
 *  Phil Garcia (tgiphil) <phil@thinkedge.com>
 *  Simon Wollwage (rootnode) <kintaro@think-in-co.de>
 */

using System;

using Mosa.Runtime.CompilerFramework;
using Mosa.Runtime.CompilerFramework.Operands;
using Mosa.Runtime.Metadata;
using Mosa.Runtime.Metadata.Signatures;
using IR = Mosa.Runtime.CompilerFramework.IR;

namespace Mosa.Platforms.x86.Intrinsic
{
	/// <summary>
	/// 
	/// </summary>
	public class SetControlRegisterBase : IIntrinsicMethod
	{

		private ControlRegister _control;

		protected SetControlRegisterBase(ControlRegister control) 
		{
			_control = control;
		}

		#region Methods

		/// <summary>
		/// Replaces the instrinsic call site
		/// </summary>
		/// <param name="context">The context.</param>
		public void ReplaceIntrinsicCall(Context context)
		{
			Operand operand1 = context.Operand1;

			RegisterOperand imm = new RegisterOperand(new SigType(CilElementType.U4), GeneralPurposeRegister.EAX);

			context.SetInstruction(IR.Instruction.MoveInstruction, imm, operand1);
			context.AppendInstruction(IR.Instruction.MoveInstruction, new RegisterOperand(new SigType(CilElementType.U4), _control), imm);
		}

		#endregion // Methods

	}
}
