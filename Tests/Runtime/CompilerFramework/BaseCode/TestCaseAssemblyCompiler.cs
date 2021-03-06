/*
 * (c) 2008 MOSA - The Managed Operating System Alliance
 *
 * Licensed under the terms of the New BSD License.
 *
 * Authors:
 *  Michael Ruck (<mailto:sharpos@michaelruck.de>)
 */

using System.Collections.Generic;

using Mosa.Runtime.CompilerFramework;
using Mosa.Runtime.Linker;
using Mosa.Runtime.Loader;
using Mosa.Runtime.Vm;

using x86 = Mosa.Platforms.x86;

namespace Test.Mosa.Runtime.CompilerFramework.BaseCode
{
    public delegate void CCtor();

	class TestCaseAssemblyCompiler : AssemblyCompiler
	{
        private readonly Queue<CCtor> cctorQueue = new Queue<CCtor>();

		private TestCaseAssemblyCompiler(IArchitecture architecture, IMetadataModule module) :
			base(architecture, module)
		{
			// Build the assembly compiler pipeline
			CompilerPipeline pipeline = this.Pipeline;
			pipeline.AddRange(new IAssemblyCompilerStage[] {
                new TypeLayoutStage(),
                new AssemblyMemberCompilationSchedulerStage(),
                new MethodCompilerSchedulerStage(),
                new TestAssemblyLinker(),
            });
			architecture.ExtendAssemblyCompilerPipeline(pipeline);
		}

		public static void Compile(IMetadataModule module)
		{
			IArchitecture architecture = x86.Architecture.CreateArchitecture(x86.ArchitectureFeatureFlags.AutoDetect);
			new TestCaseAssemblyCompiler(architecture, module).Compile();
		}

        public override MethodCompilerBase CreateMethodCompiler(ICompilationSchedulerStage schedulerStage, RuntimeType type, RuntimeMethod method)
        {
            MethodCompilerBase mc = new TestCaseMethodCompiler(this, this.Pipeline.FindFirst<IAssemblyLinker>(), this.Architecture, schedulerStage, this.Assembly, type, method);
            this.Architecture.ExtendMethodCompilerPipeline(mc.Pipeline);
            return mc;
        }

        protected override void EndCompile()
        {
            base.EndCompile();

            while (this.cctorQueue.Count > 0)
            {
                CCtor cctor = this.cctorQueue.Dequeue();
                cctor();
            }
        }

        public void QueueCCtorForInvocationAfterCompilation(CCtor cctor)
        {
            this.cctorQueue.Enqueue(cctor);
        }
    }
}
