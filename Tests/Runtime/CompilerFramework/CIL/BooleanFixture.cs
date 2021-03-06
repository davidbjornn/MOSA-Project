﻿/*
 * (c) 2008 MOSA - The Managed Operating System Alliance
 *
 * Licensed under the terms of the New BSD License.
 *
 * Authors:
 *  Michael Fröhlich (aka grover, <mailto:sharpos@michaelruck.de>)
 *  
 */

using MbUnit.Framework;

using Test.Mosa.Runtime.CompilerFramework.CLI;

namespace Test.Mosa.Runtime.CompilerFramework.CIL
{
    [TestFixture]
    [Importance(Importance.Critical)]
    [Category(@"Basic types")]
    [Description(@"Tests support for the basic type System.Boolean")]
    public class BooleanFixture : RuntimeFixture
    {
        private readonly BinaryLogicInstructionTestRunner<bool, bool> logicTests = new BinaryLogicInstructionTestRunner<bool, bool>
        {
            ExpectedTypeName = @"bool",
            TypeName = @"bool",
            IncludeShl = false,
            IncludeShr = false
        };

        private readonly ComparisonInstructionTestRunner<bool> comparisonTests = new ComparisonInstructionTestRunner<bool>
        {
            TypeName = @"bool",
            IncludeCge = false,
            IncludeCgt = false,
            IncludeCle = false,
            IncludeClt = false
        };

        private readonly SZArrayInstructionTestRunner<bool> arrayTests = new SZArrayInstructionTestRunner<bool>
        {
            TypeName = @"bool"
        };

        #region And

        [Row(true, true)]
        [Row(true, false)]
        [Row(false, true)]
        [Row(false, false)]
        [Test, Author(@"Michael Fröhlich, sharpos@michaelruck.de")]
        public void And(bool first, bool second)
        {
            this.logicTests.And((first & second), first, second);
        }

        #endregion // And

        #region Or

        [Row(true, true)]
        [Row(true, false)]
        [Row(false, true)]
        [Row(false, false)]
        [Test, Author(@"Michael Fröhlich, sharpos@michaelruck.de")]
        public void Or(bool first, bool second)
        {
            this.logicTests.Or((first | second), first, second);
        }

        #endregion // Or

        #region Xor

        [Row(true, true)]
        [Row(true, false)]
        [Row(false, true)]
        [Row(false, false)]
        [Test, Author(@"Michael Fröhlich, sharpos@michaelruck.de")]
        public void Xor(bool first, bool second)
        {
            this.logicTests.Xor((first ^ second), first, second);
        }

        #endregion // Xor

        #region Not

        [Row(true)]
        [Row(false)]
        [Test, Author(@"Michael Fröhlich, sharpos@michaelruck.de")]
        public void Not(bool value)
        {
            this.logicTests.Not(!value, value);
        }

        #endregion // Not

        #region Ceq

        [Row(true, true, true)]
        [Row(false, true, false)]
        [Row(false, false, true)]
        [Row(true, false, false)]
        [Test, Author(@"Michael Fröhlich, sharpos@michaelruck.de")]
        public void Ceq(bool expectedValue, bool first, bool second)
        {
            this.comparisonTests.Ceq(expectedValue, first, second);
        }

        #endregion // Ceq

        #region Newarr

        [Test, Author(@"Michael Fröhlich, sharpos@michaelruck.de")]
        public void Newarr()
        {
            this.arrayTests.Newarr();
        }

        #endregion // Newarr

        #region Ldlen

        [Row(0)]
        [Row(1)]
        [Row(10)]
        [Test, Author(@"Michael Fröhlich, sharpos@michaelruck.de")]
        public void Ldlen(int length)
        {
            this.arrayTests.Ldlen(length);
        }

        #endregion // Ldlen

        #region Stelem

        [Row(0, true)]
        [Row(0, false)]
        [Row(3, true)]
        [Row(7, false)]
        [Row(9, true)]
        [Row(6, false)]
        [Test, Author(@"Michael Fröhlich, sharpos@michaelruck.de")]
        public void Stelem(int index, bool value)
        {
            this.arrayTests.Stelem(index, value);
        }

        #endregion // Stelem

        #region Ldelem

        [Row(0, true)]
        [Row(0, false)]
        [Row(3, true)]
        [Row(7, false)]
        [Row(9, true)]
        [Row(6, false)]
        [Test, Author(@"Michael Fröhlich, sharpos@michaelruck.de")]
        public void Ldelem(int index, bool value)
        {
            this.arrayTests.Ldelem(index, value);
        }

        #endregion // Ldelem

        #region Ldelema

        [Row(0, true)]
        [Row(0, false)]
        [Row(3, true)]
        [Row(7, false)]
        [Row(9, true)]
        [Row(6, false)]
        [Test, Author(@"Michael Fröhlich, sharpos@michaelruck.de")]
        public void Ldelema(int index, bool value)
        {
            this.arrayTests.Ldelema(index, value);
        }

        #endregion // Ldelema
    }
}
