/*
 * (c) 2008 MOSA - The Managed Operating System Alliance
 *
 * Licensed under the terms of the New BSD License.
 *
 * Authors:
 *  Michael Ruck (grover) <sharpos@michaelruck.de>
 *  Scott Balmos <sbalmos@fastmail.fm>
 *  Phil Garcia (tgiphil) <phil@thinkedge.com>
 */

using System;
using System.Diagnostics;
using System.IO;

using Mosa.Runtime.CompilerFramework;
using Mosa.Runtime.CompilerFramework.Operands;
using Mosa.Runtime.Linker;
using Mosa.Runtime.Metadata;
using Mosa.Runtime.Metadata.Signatures;

using IR = Mosa.Runtime.CompilerFramework.IR;

namespace Mosa.Platforms.x86
{
	/// <summary>
	/// 
	/// </summary>
	public sealed class TweakTransformationStage : BaseTransformationStage, CPUx86.IX86Visitor, IMethodCompilerStage, IPlatformStage, IPipelineStage
	{

		#region IPipelineStage Members

		/// <summary>
		/// Retrieves the name of the compilation stage.
		/// </summary>
		/// <value>The name of the compilation stage.</value>
		string IPipelineStage.Name { get { return @"X86.TweakTransformationStage"; } }

		#endregion // IPipelineStage Members

		#region IX86Visitor

		/// <summary>
		/// Visitation function for <see cref="CPUx86.IX86Visitor.Mov"/> instructions.
		/// </summary>
		/// <param name="ctx">The context.</param>
		void CPUx86.IX86Visitor.Mov(Context ctx)
		{
			if (ctx.Result is ConstantOperand) {
				ctx.SetInstruction(CPUx86.Instruction.NopInstruction);
				return;
			}

			if (ctx.Operand1 is ConstantOperand && ctx.Operand1.StackType == StackTypeCode.F)
				ctx.Operand1 = EmitConstant(ctx.Operand1);

			// Check that we're not dealing with floating point values
			if (ctx.Result.StackType == StackTypeCode.F || ctx.Operand1.StackType == StackTypeCode.F)
				if (ctx.Result.Type.Type == CilElementType.R4)
					ctx.SetInstruction(CPUx86.Instruction.MovssInstruction, ctx.Result, ctx.Operand1);
				else if (ctx.Result.Type.Type == CilElementType.R8)
					ctx.SetInstruction(CPUx86.Instruction.MovsdInstruction, ctx.Result, ctx.Operand1);
		}

		/// <summary>
		/// Visitation function for <see cref="CPUx86.IX86Visitor.Mul"/> instructions.
		/// </summary>
		/// <param name="ctx">The context.</param>
		void CPUx86.IX86Visitor.Mul(Context ctx)
		{
			if (ctx.Operand1 is ConstantOperand) {
				RegisterOperand ecx = new RegisterOperand(ctx.Operand1.Type, GeneralPurposeRegister.ECX);
                Context before = ctx.InsertBefore();
                before.SetInstruction(CPUx86.Instruction.PushInstruction, null, ecx);
				before.AppendInstruction(CPUx86.Instruction.MovInstruction, ecx, ctx.Operand1);

				ctx.Operand1 = ecx;
                ctx.AppendInstruction(CPUx86.Instruction.PopInstruction, ecx);
			}

            if (ctx.Operand1 == null || ctx.Result == null)
                return;
		}

		/// <summary>
		/// Visitation function for <see cref="CPUx86.IX86Visitor.Cvtss2sd"/> instructions.
		/// </summary>
		/// <param name="ctx">The context.</param>
		void CPUx86.IX86Visitor.Cvtss2sd(Context ctx)
		{
			if (ctx.Operand2 is ConstantOperand)
				ctx.SetInstruction(CPUx86.Instruction.Cvtss2sdInstruction, ctx.Operand1, EmitConstant(ctx.Operand2));
		}

		/// <summary>
		/// Visitation function for <see cref="CPUx86.IX86Visitor.Cvttsd2si"/> instructions.
		/// </summary>
		/// <param name="ctx">The context.</param>
		void CPUx86.IX86Visitor.Cvttsd2si(Context ctx)
		{
			Operand result = ctx.Result;
			RegisterOperand register = new RegisterOperand(result.Type, GeneralPurposeRegister.EAX);

			if (!(result is RegisterOperand)) {
				ctx.Result = register;
				ctx.AppendInstruction(CPUx86.Instruction.MovInstruction, result, register);
			}
		}

		/// <summary>
		/// Visitation function for <see cref="CPUx86.IX86Visitor.Cvttss2si"/> instructions.
		/// </summary>
		/// <param name="ctx">The context.</param>
		void CPUx86.IX86Visitor.Cvttss2si(Context ctx)
		{
			Operand result = ctx.Result;
			RegisterOperand register = new RegisterOperand(result.Type, GeneralPurposeRegister.EAX);

			if (!(result is RegisterOperand)) {
				ctx.Result = register;
				ctx.AppendInstruction(CPUx86.Instruction.MovInstruction, result, register);
			}
		}

		/// <summary>
		/// Visitation function for <see cref="CPUx86.IX86Visitor.In"/> instructions.
		/// </summary>
		/// <param name="context">The context.</param>
		void CPUx86.IX86Visitor.In(Context context)
		{
		}

		/// <summary>
		/// Visitation function for <see cref="CPUx86.IX86Visitor.Movsx"/> instructions.
		/// </summary>
		/// <param name="context">The context.</param>
		void CPUx86.IX86Visitor.Movsx(Context context)
		{
			if (Is32Bit(context.Operand1))
				context.ReplaceInstructionOnly(CPUx86.Instruction.MovInstruction);
			else {
				Operand result = context.Result;
				if (!(result is RegisterOperand)) {
					RegisterOperand ecx = new RegisterOperand(context.Result.Type, GeneralPurposeRegister.ECX);
                    context.Result = ecx;
					//context.SetInstruction(CPUx86.Instruction.MovsxInstruction, ecx, context.Operand1);
					context.AppendInstruction(CPUx86.Instruction.MovInstruction, result, ecx);
				}
			}
		}

        /// <summary>
        /// 
        /// </summary>
        /// <param name="context"></param>
        void CPUx86.IX86Visitor.Movss(Context context)
        {
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="context"></param>
        void CPUx86.IX86Visitor.Movsd(Context context)
        {
        }

		/// <summary>
		/// Visitation function for <see cref="CPUx86.IX86Visitor.Movzx"/> instructions.
		/// </summary>
		/// <param name="context">The context.</param>
		void CPUx86.IX86Visitor.Movzx(Context context)
		{
			if (Is32Bit(context.Operand1))
				context.ReplaceInstructionOnly(CPUx86.Instruction.MovInstruction);
			else {
				Operand result = context.Result;
				if (!(result is RegisterOperand)) {
					RegisterOperand ecx = new RegisterOperand(context.Result.Type, GeneralPurposeRegister.ECX);
					context.SetInstruction(CPUx86.Instruction.MovzxInstruction, ecx, context.Operand1);
					context.AppendInstruction(CPUx86.Instruction.MovInstruction, result, ecx);
				}
			}
		}

		/// <summary>
		/// Visitation function for <see cref="CPUx86.IX86Visitor.DirectMultiplication"/> instructions.
		/// </summary>
		/// <param name="ctx">The context.</param>
		void CPUx86.IX86Visitor.DirectMultiplication(Context ctx)
		{
			Operand op = ctx.Operand1;

			if (op is ConstantOperand) {
				RegisterOperand ebx = new RegisterOperand(new SigType(CilElementType.I4), GeneralPurposeRegister.EBX);
				ctx.SetInstruction(CPUx86.Instruction.PushInstruction, null, ebx);
				ctx.AppendInstruction(CPUx86.Instruction.MovInstruction, ebx, op);
				ctx.AppendInstruction(CPUx86.Instruction.DivInstruction, ebx);
				ctx.AppendInstruction(CPUx86.Instruction.PopInstruction, ebx);
			}
			else
				ctx.SetInstruction(CPUx86.Instruction.DivInstruction, null, op);
		}

		/// <summary>
		/// Visitation function for <see cref="CPUx86.IX86Visitor.Cmp"/> instructions.
		/// </summary>
		/// <param name="ctx">The context.</param>
		void CPUx86.IX86Visitor.Cmp(Context ctx)
		{
            Operand left = ctx.Result;
            Operand right = ctx.Operand1;

            if (left is ConstantOperand)
            {
                RegisterOperand ecx = new RegisterOperand(left.Type, GeneralPurposeRegister.ECX);
                Context before = ctx.InsertBefore();
                before.SetInstruction(CPUx86.Instruction.PushInstruction, null, ecx);
                before.AppendInstruction(CPUx86.Instruction.MovInstruction, ecx, left);
                ctx.Result = ecx;
                ctx.AppendInstruction(CPUx86.Instruction.PopInstruction, ecx);
            }
            if (right is ConstantOperand && !Is32Bit(left))
            {
                RegisterOperand edx = new RegisterOperand(new SigType(CilElementType.I4), GeneralPurposeRegister.EBX);
                Context before = ctx.InsertBefore();
                before.SetInstruction(CPUx86.Instruction.PushInstruction, null, edx);
                if (IsSigned(left))
                    before.AppendInstruction(CPUx86.Instruction.MovsxInstruction, edx, left);
                else
                    before.AppendInstruction(CPUx86.Instruction.MovzxInstruction, edx, left);
                ctx.Result = edx;
                ctx.AppendInstruction(CPUx86.Instruction.PopInstruction, edx);
            }
		}

		/// <summary>
		/// Visitation function for <see cref="CPUx86.IX86Visitor.Div"/> instructions.
		/// </summary>
		/// <param name="context">The context.</param>
		void CPUx86.IX86Visitor.Div(Context context)
		{
			Context before = context.InsertBefore();
			before.SetInstruction(CPUx86.Instruction.CdqInstruction);

			if (context.Operand1 is ConstantOperand) {
				RegisterOperand ecx = new RegisterOperand(context.Operand1.Type, GeneralPurposeRegister.ECX);
				before.AppendInstruction(CPUx86.Instruction.MovInstruction, ecx, context.Operand1);
				context.Operand1 = ecx;
			}
		}

		/// <summary>
		/// Visitation function for <see cref="CPUx86.IX86Visitor.UDiv"/> instructions.
		/// </summary>
		/// <param name="context">The context.</param>
		void CPUx86.IX86Visitor.UDiv(Context context)
		{
			RegisterOperand edx = new RegisterOperand(new SigType(CilElementType.I), GeneralPurposeRegister.EDX);
			Context before = context.InsertBefore();
			before.SetInstruction(CPUx86.Instruction.XorInstruction, edx, edx);

			if (context.Operand1 is ConstantOperand) {
				RegisterOperand ecx = new RegisterOperand(context.Operand1.Type, GeneralPurposeRegister.ECX);
				before.AppendInstruction(CPUx86.Instruction.MovInstruction, ecx, context.Operand1);
				context.Operand1 = ecx;
			}
		}

		/// <summary>
		/// Visitation function for <see cref="CPUx86.IX86Visitor.Sar"/> instructions.
		/// </summary>
		/// <param name="context">The context.</param>
		void CPUx86.IX86Visitor.Sar(Context context)
		{
			if (context.Operand1 is ConstantOperand)
				return;
			RegisterOperand ecx = new RegisterOperand(new SigType(CilElementType.I), GeneralPurposeRegister.ECX);
			Context before = context.InsertBefore();
			before.SetInstruction(CPUx86.Instruction.MovInstruction, ecx, context.Operand1);
			context.Operand1 = context.Result;
		}

		/// <summary>
		/// Visitation function for <see cref="CPUx86.IX86Visitor.Shl"/> instructions.
		/// </summary>
		/// <param name="context">The context.</param>
		void CPUx86.IX86Visitor.Shl(Context context)
		{
			if (context.Operand1 is ConstantOperand)
				return;
			RegisterOperand ecx = new RegisterOperand(new SigType(CilElementType.I), GeneralPurposeRegister.ECX);
			Context before = context.InsertBefore();
			before.SetInstruction(CPUx86.Instruction.MovInstruction, ecx, context.Operand1);
			context.Operand1 = context.Result;
		}

		/// <summary>
		/// Visitation function for <see cref="CPUx86.IX86Visitor.Shr"/> instructions.
		/// </summary>
		/// <param name="context">The context.</param>
		void CPUx86.IX86Visitor.Shr(Context context)
		{
			if (context.Operand1 is ConstantOperand)
				return;
			RegisterOperand ecx = new RegisterOperand(new SigType(CilElementType.I), GeneralPurposeRegister.ECX);
			Context before = context.InsertBefore();
			before.SetInstruction(CPUx86.Instruction.MovInstruction, ecx, context.Operand1);
			context.Operand1 = context.Result;
		}

		/// <summary>
		/// Visitation function for <see cref="CPUx86.IX86Visitor.Out"/> instructions.
		/// </summary>
		/// <param name="context">The context.</param>
		void CPUx86.IX86Visitor.Out(Context context)
		{
		}

		#endregion // IX86Visitor

		#region IX86Visitor - Unused

		/// <summary>
		/// Visitation function for <see cref="CPUx86.IX86Visitor.Lea"/> instructions.
		/// </summary>
		/// <param name="context"></param>
		void CPUx86.IX86Visitor.Lea(Context context) { }
		/// <summary>
		/// Visitation function for <see cref="CPUx86.IX86Visitor.SseSub"/> instructions.
		/// </summary>
		/// <param name="context">The context.</param>
		void CPUx86.IX86Visitor.SseSub(Context context) { }
		/// <summary>
		/// Visitation function for <see cref="CPUx86.IX86Visitor.DirectCompare"/> instructions.
		/// </summary>
		/// <param name="context"></param>
		void CPUx86.IX86Visitor.DirectCompare(Context context) { }
		/// <summary>
		/// Visitation function for <see cref="CPUx86.IX86Visitor.Add"/> instructions.
		/// </summary>
		/// <param name="context">The context.</param>
		void CPUx86.IX86Visitor.Add(Context context) { }
		/// <summary>
		/// Visitation function for <see cref="CPUx86.IX86Visitor.Adc"/> instructions.
		/// </summary>
		/// <param name="context">The context.</param>
		void CPUx86.IX86Visitor.Adc(Context context) { }
		/// <summary>
		/// Visitation function for <see cref="CPUx86.IX86Visitor.And"/> instructions.
		/// </summary>
		/// <param name="context">The context.</param>
		void CPUx86.IX86Visitor.And(Context context) { }
		/// <summary>
		/// Visitation function for <see cref="CPUx86.IX86Visitor.Or"/> instructions.
		/// </summary>
		/// <param name="context">The context.</param>
		void CPUx86.IX86Visitor.Or(Context context) { }
		/// <summary>
		/// Visitation function for <see cref="CPUx86.IX86Visitor.Xor"/> instructions.
		/// </summary>
		/// <param name="context">The context.</param>
		void CPUx86.IX86Visitor.Xor(Context context) { }
		/// <summary>
		/// Visitation function for <see cref="CPUx86.IX86Visitor.Sub"/> instructions.
		/// </summary>
		/// <param name="context">The context.</param>
		void CPUx86.IX86Visitor.Sub(Context context) { }
		/// <summary>
		/// Visitation function for <see cref="CPUx86.IX86Visitor.Sbb"/> instructions.
		/// </summary>
		/// <param name="context">The context.</param>
		void CPUx86.IX86Visitor.Sbb(Context context) { }
		/// <summary>
		/// Visitation function for <see cref="CPUx86.IX86Visitor.DirectDivision"/> instructions.
		/// </summary>
		/// <param name="context">The context.</param>
		void CPUx86.IX86Visitor.DirectDivision(Context context) { }
		/// <summary>
		/// Visitation function for <see cref="CPUx86.IX86Visitor.SseAdd"/> instructions.
		/// </summary>
		/// <param name="context">The context.</param>
		void CPUx86.IX86Visitor.SseAdd(Context context) { }
		/// <summary>
		/// Visitation function for <see cref="CPUx86.IX86Visitor.SseMul"/> instructions.
		/// </summary>
		/// <param name="context">The context.</param>
		void CPUx86.IX86Visitor.SseMul(Context context) { }
		/// <summary>
		/// Visitation function for <see cref="CPUx86.IX86Visitor.SseMul"/> instructions.
		/// </summary>
		/// <param name="context">The context.</param>
		void CPUx86.IX86Visitor.SseDiv(Context context) { }
		/// <summary>
		/// Visitation function for <see cref="CPUx86.IX86Visitor.Sal"/> instructions.
		/// </summary>
		/// <param name="context">The context.</param>
		void CPUx86.IX86Visitor.Sal(Context context) { }
		/// <summary>
		/// Visitation function for <see cref="CPUx86.IX86Visitor.Rcr"/> instructions.
		/// </summary>
		/// <param name="context">The context.</param>
		void CPUx86.IX86Visitor.Rcr(Context context) { }
		/// <summary>
		/// Visitation function for <see cref="CPUx86.IX86Visitor.Cvtsi2ss"/> instructions.
		/// </summary>
		/// <param name="context">The context.</param>
		void CPUx86.IX86Visitor.Cvtsi2ss(Context context) { }
		/// <summary>
		/// Visitation function for <see cref="CPUx86.IX86Visitor.Cvtsi2sd"/> instructions.
		/// </summary>
		/// <param name="context">The context.</param>
		void CPUx86.IX86Visitor.Cvtsi2sd(Context context) { }
		/// <summary>
		/// Visitation function for <see cref="CPUx86.IX86Visitor.Cvtsd2ss"/> instructions.
		/// </summary>
		/// <param name="context">The context.</param>
		void CPUx86.IX86Visitor.Cvtsd2ss(Context context) { }
		/// <summary>
		/// Visitation function for <see cref="CPUx86.IX86Visitor.Setcc"/> instructions.
		/// </summary>
		/// <param name="context">The context.</param>
		void CPUx86.IX86Visitor.Setcc(Context context) { }
		/// <summary>
		/// Visitation function for <see cref="CPUx86.IX86Visitor.Cdq"/> instructions.
		/// </summary>
		/// <param name="context">The context.</param>
		void CPUx86.IX86Visitor.Cdq(Context context) { }
		/// <summary>
		/// Visitation function for <see cref="CPUx86.IX86Visitor.Shld"/> instructions.
		/// </summary>
		/// <param name="context">The context.</param>
		void CPUx86.IX86Visitor.Shld(Context context) { }
		/// <summary>
		/// Visitation function for <see cref="CPUx86.IX86Visitor.Shrd"/> instructions.
		/// </summary>
		/// <param name="context">The context.</param>
		void CPUx86.IX86Visitor.Shrd(Context context) { }
		/// <summary>
		/// Visitation function for <see cref="CPUx86.IX86Visitor.Comisd"/> instructions.
		/// </summary>
		/// <param name="context">The context.</param>
		void CPUx86.IX86Visitor.Comisd(Context context) { }
		/// <summary>
		/// Visitation function for <see cref="CPUx86.IX86Visitor.Comiss"/> instructions.
		/// </summary>
		/// <param name="context">The context.</param>
		void CPUx86.IX86Visitor.Comiss(Context context) { }
		/// <summary>
		/// Visitation function for <see cref="CPUx86.IX86Visitor.Ucomisd"/> instructions.
		/// </summary>
		/// <param name="context">The context.</param>
		void CPUx86.IX86Visitor.Ucomisd(Context context) { }
		/// <summary>
		/// Visitation function for <see cref="CPUx86.IX86Visitor.Ucomiss"/> instructions.
		/// </summary>
		/// <param name="context">The context.</param>
		void CPUx86.IX86Visitor.Ucomiss(Context context) { }
		/// <summary>
		/// Visitation function for <see cref="CPUx86.IX86Visitor.Jns"/> instructions.
		/// </summary>
		/// <param name="context">The context.</param>
		void CPUx86.IX86Visitor.Jns(Context context) { }
		/// <summary>
		/// Visitation function for <see cref="CPUx86.IX86Visitor.BochsDebug"/> instructions.
		/// </summary>
		/// <param name="context">The context.</param>
		void CPUx86.IX86Visitor.BochsDebug(Context context) { }
		/// <summary>
		/// Visitation function for <see cref="CPUx86.IX86Visitor.Cli"/> instructions.
		/// </summary>
		/// <param name="context">The context.</param>
		void CPUx86.IX86Visitor.Cli(Context context) { }
		/// <summary>
		/// Visitation function for <see cref="CPUx86.IX86Visitor.Cld"/> instructions.
		/// </summary>
		/// <param name="context">The context.</param>
		void CPUx86.IX86Visitor.Cld(Context context) { }
		/// <summary>
		/// Visitation function for <see cref="CPUx86.IX86Visitor.CmpXchg"/> instructions.
		/// </summary>
		/// <param name="context">The context.</param>
		void CPUx86.IX86Visitor.CmpXchg(Context context) { }
		/// <summary>
		/// Visitation function for <see cref="CPUx86.IX86Visitor.CpuId"/> instructions.
		/// </summary>
		/// <param name="context">The context.</param>
		void CPUx86.IX86Visitor.CpuId(Context context) { }
		/// <summary>
		/// Visitation function for <see cref="CPUx86.IX86Visitor.CpuIdEax"/> instructions.
		/// </summary>
		/// <param name="context">The context.</param>
		void CPUx86.IX86Visitor.CpuIdEax(Context context) { }
		/// <summary>
		/// Visitation function for <see cref="CPUx86.IX86Visitor.CpuIdEbx"/> instructions.
		/// </summary>
		/// <param name="context">The context.</param>
		void CPUx86.IX86Visitor.CpuIdEbx(Context context) { }
		/// <summary>
		/// Visitation function for <see cref="CPUx86.IX86Visitor.CpuIdEcx"/> instructions.
		/// </summary>
		/// <param name="context">The context.</param>
		void CPUx86.IX86Visitor.CpuIdEcx(Context context) { }
		/// <summary>
		/// Visitation function for <see cref="CPUx86.IX86Visitor.CpuIdEdx"/> instructions.
		/// </summary>
		/// <param name="context">The context.</param>
		void CPUx86.IX86Visitor.CpuIdEdx(Context context) { }
		/// <summary>
		/// Visitation function for <see cref="CPUx86.IX86Visitor.Hlt"/> instructions.
		/// </summary>
		/// <param name="context">The context.</param>
		void CPUx86.IX86Visitor.Hlt(Context context) { }
		/// <summary>
		/// Visitation function for <see cref="CPUx86.IX86Visitor.Invlpg"/> instructions.
		/// </summary>
		/// <param name="context">The context.</param>
		void CPUx86.IX86Visitor.Invlpg(Context context) { }
		/// <summary>
		/// Visitation function for <see cref="CPUx86.IX86Visitor.Inc"/> instructions.
		/// </summary>
		/// <param name="context">The context.</param>
		void CPUx86.IX86Visitor.Inc(Context context) { }
		/// <summary>
		/// Visitation function for <see cref="CPUx86.IX86Visitor.Dec"/> instructions.
		/// </summary>
		/// <param name="context">The context.</param>
		void CPUx86.IX86Visitor.Dec(Context context) { }
		/// <summary>
		/// Visitation function for <see cref="CPUx86.IX86Visitor.Int"/> instructions.
		/// </summary>
		/// <param name="context">The context.</param>
		void CPUx86.IX86Visitor.Int(Context context) { }
		/// <summary>
		/// Visitation function for <see cref="CPUx86.IX86Visitor.Iretd"/> instructions.
		/// </summary>
		/// <param name="context">The context.</param>
		void CPUx86.IX86Visitor.Iretd(Context context) { }
		/// <summary>
		/// Visitation function for <see cref="CPUx86.IX86Visitor.Lgdt"/> instructions.
		/// </summary>
		/// <param name="context">The context.</param>
		void CPUx86.IX86Visitor.Lgdt(Context context) { }
		/// <summary>
		/// Visitation function for <see cref="CPUx86.IX86Visitor.Lidt"/> instructions.
		/// </summary>
		/// <param name="context">The context.</param>
		void CPUx86.IX86Visitor.Lidt(Context context) { }
		/// <summary>
		/// Visitation function for <see cref="CPUx86.IX86Visitor.Lock"/> instructions.
		/// </summary>
		/// <param name="context">The context.</param>
		void CPUx86.IX86Visitor.Lock(Context context) { }
		/// <summary>
		/// Visitation function for <see cref="CPUx86.IX86Visitor.Neg"/> instructions.
		/// </summary>
		/// <param name="context">The context.</param>
		void CPUx86.IX86Visitor.Neg(Context context) { }
		/// <summary>
		/// Visitation function for <see cref="CPUx86.IX86Visitor.Nop"/> instructions.
		/// </summary>
		/// <param name="context">The context.</param>
		void CPUx86.IX86Visitor.Nop(Context context) { }
		/// <summary>
		/// Visitation function for <see cref="CPUx86.IX86Visitor.Pause"/> instructions.
		/// </summary>
		/// <param name="context">The context.</param>
		void CPUx86.IX86Visitor.Pause(Context context) { }
		/// <summary>
		/// Visitation function for <see cref="CPUx86.IX86Visitor.Pop"/> instructions.
		/// </summary>
		/// <param name="context">The context.</param>
		void CPUx86.IX86Visitor.Pop(Context context) { }
		/// <summary>
		/// Visitation function for <see cref="CPUx86.IX86Visitor.Popad"/> instructions.
		/// </summary>
		/// <param name="context">The context.</param>
		void CPUx86.IX86Visitor.Popad(Context context) { }
		/// <summary>
		/// Visitation function for <see cref="CPUx86.IX86Visitor.Popfd"/> instructions.
		/// </summary>
		/// <param name="context">The context.</param>
		void CPUx86.IX86Visitor.Popfd(Context context) { }
		/// <summary>
		/// Visitation function for <see cref="CPUx86.IX86Visitor.Push"/> instructions.
		/// </summary>
		/// <param name="context">The context.</param>
		void CPUx86.IX86Visitor.Push(Context context) { }
		/// <summary>
		/// Visitation function for <see cref="CPUx86.IX86Visitor.Pushad"/> instructions.
		/// </summary>
		/// <param name="context">The context.</param>
		void CPUx86.IX86Visitor.Pushad(Context context) { }
		/// <summary>
		/// Visitation function for <see cref="CPUx86.IX86Visitor.Pushfd"/> instructions.
		/// </summary>
		/// <param name="context">The context.</param>
		void CPUx86.IX86Visitor.Pushfd(Context context) { }
		/// <summary>
		/// Visitation function for <see cref="CPUx86.IX86Visitor.Rdmsr"/> instructions.
		/// </summary>
		/// <param name="context">The context.</param>
		void CPUx86.IX86Visitor.Rdmsr(Context context) { }
		/// <summary>
		/// Visitation function for <see cref="CPUx86.IX86Visitor.Rdpmc"/> instructions.
		/// </summary>
		/// <param name="context">The context.</param>
		void CPUx86.IX86Visitor.Rdpmc(Context context) { }
		/// <summary>
		/// Visitation function for <see cref="CPUx86.IX86Visitor.Rdtsc"/> instructions.
		/// </summary>
		/// <param name="context">The context.</param>
		void CPUx86.IX86Visitor.Rdtsc(Context context) { }
		/// <summary>
		/// Visitation function for <see cref="CPUx86.IX86Visitor.Rep"/> instructions.
		/// </summary>
		/// <param name="context">The context.</param>
		void CPUx86.IX86Visitor.Rep(Context context) { }
		/// <summary>
		/// Visitation function for <see cref="CPUx86.IX86Visitor.Sti"/> instructions.
		/// </summary>
		/// <param name="context">The context.</param>
		void CPUx86.IX86Visitor.Sti(Context context) { }
		/// <summary>
		/// Visitation function for <see cref="CPUx86.IX86Visitor.Stosb"/> instructions.
		/// </summary>
		/// <param name="context">The context.</param>
		void CPUx86.IX86Visitor.Stosb(Context context) { }
		/// <summary>
		/// Visitation function for <see cref="CPUx86.IX86Visitor.Stosd"/> instructions.
		/// </summary>
		/// <param name="context">The context.</param>
		void CPUx86.IX86Visitor.Stosd(Context context) { }
		/// <summary>
		/// Visitation function for <see cref="CPUx86.IX86Visitor.Xchg"/> instructions.
		/// </summary>
		/// <param name="context">The context.</param>
		void CPUx86.IX86Visitor.Xchg(Context context) { }
		/// <summary>
		/// Visitation function for <see cref="CPUx86.IX86Visitor.Jump"/> instructions.
		/// </summary>
		/// <param name="context">The context.</param>
		void CPUx86.IX86Visitor.Jump(Context context) { }
		
        /// <summary>
		/// Visitation function for <see cref="CPUx86.IX86Visitor.Call"/> instructions.
		/// </summary>
		/// <param name="context">The context.</param>
		void CPUx86.IX86Visitor.Call(Context context)
        {
            Operand destinationOperand = context.Operand1;

			if (destinationOperand == null)
			{
				return;
			}

            if (destinationOperand is SymbolOperand)
            {
                return;
            }

            if (!(destinationOperand is RegisterOperand))
            {
                Context before = context.InsertBefore();
                RegisterOperand eax = new RegisterOperand(BuiltInSigType.IntPtr, GeneralPurposeRegister.EAX);

                before.SetInstruction(CPUx86.Instruction.MovInstruction, eax, destinationOperand);
                context.Operand1 = eax;
            }
            
        }

		/// <summary>
		/// Visitation function for <see cref="CPUx86.IX86Visitor.Branch"/> instructions.
		/// </summary>
		/// <param name="context">The context.</param>
		void CPUx86.IX86Visitor.Branch(Context context) { }
		/// <summary>
		/// Visitation function for <see cref="CPUx86.IX86Visitor.Not"/> instructions.
		/// </summary>
		/// <param name="context">The context.</param>
		void CPUx86.IX86Visitor.Not(Context context) { }

        void CPUx86.IX86Visitor.SseRound(Context context) { }

		#endregion // IX86Visitor - Unused

	}
}
