﻿/*
 * (c) 2008 MOSA - The Managed Operating System Alliance
 *
 * Licensed under the terms of the New BSD License.
 *
 * Authors:
 *  Michael Ruck (<mailto:sharpos@michaelruck.de>)
 */

using System;
using System.Collections.Generic;
using System.Text;

namespace Mosa.Runtime.CompilerFramework.IR
{
    /// <summary>
    /// Intermediate representation of the and instruction.
    /// </summary>
    public class LogicalAndInstruction : ThreeOperandInstruction
    {
        #region Construction

        /// <summary>
        /// Initializes a new instance of the <see cref="LogicalAndInstruction"/> class.
        /// </summary>
        public LogicalAndInstruction()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LogicalAndInstruction"/> class.
        /// </summary>
        /// <param name="result">The result.</param>
        /// <param name="op1">The op1.</param>
        /// <param name="op2">The op2.</param>
        public LogicalAndInstruction(Operand result, Operand op1, Operand op2) :
            base(result, op1, op2)
        {
        }

        #endregion // Construction

        #region Instruction Overrides

        /// <summary>
        /// Returns a string representation of the <see cref="LogicalAndInstruction"/>.
        /// </summary>
        /// <returns>A string representation of the and instruction.</returns>
        public override string ToString()
        {
            return String.Format(@"IR and {0} <- {1} & {2}", this.Operand0, this.Operand1, this.Operand2);
        }

        /// <summary>
        /// Allows visitor based dispatch for this instruction object.
        /// </summary>
        /// <param name="visitor">The visitor object.</param>
        /// <param name="arg">A visitor specific context argument.</param>
        /// <typeparam name="ArgType">An additional visitor context argument.</typeparam>
        protected override void Visit<ArgType>(IIRVisitor<ArgType> visitor, ArgType arg)
        {
            visitor.Visit(this, arg);
        }

        #endregion // Instruction Overrides
    }
}
