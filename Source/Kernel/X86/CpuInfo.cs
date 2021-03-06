/*
 * (c) 2008 MOSA - The Managed Operating System Alliance
 *
 * Licensed under the terms of the New BSD License.
 *
 */

using Mosa.Platforms.x86;
using Mosa.Kernel.X86;

namespace Mosa.HelloWorld
{
	public class CpuInfo
	{
		public CpuInfo()
		{
			Setup();
		}
		
		public void Setup() 
		{
			
		}
		
		public ulong NumberOfCores
		{
			get
			{
				return (ulong)((Platforms.x86.Native.CpuIdEax(4) >> 26) + 1);
			}
		}
		
		public ulong Type
		{
			get
			{
				return (ulong)((Platforms.x86.Native.CpuIdEax(1) & 0x3000) >> 12);
			}
		}
		
		public ulong Stepping
		{
			get
			{
				return (ulong)(Platforms.x86.Native.CpuIdEax(1) & 0xF);
			}
		}
		
		public ulong Model
		{
			get
			{
				return (ulong)((Platforms.x86.Native.CpuIdEax(1) & 0xF0) >> 4);
			}
		}
		
		public ulong Family
		{
			get
			{
				return (ulong)((Platforms.x86.Native.CpuIdEax(1) & 0xF00) >> 8);
			}
		}
		
		public void PrintVendorString()
		{
			int identifier = Platforms.x86.Native.CpuIdEbx(0);
			for (int i = 0; i < 4; ++i)
				Screen.Write((char)((identifier >> (i * 8)) & 0xFF));

			identifier = Platforms.x86.Native.CpuIdEdx(0);
			for (int i = 0; i < 4; ++i)
				Screen.Write((char)((identifier >> (i * 8)) & 0xFF));

			identifier = Platforms.x86.Native.CpuIdEcx(0);
			for (int i = 0; i < 4; ++i)
				Screen.Write((char)((identifier >> (i * 8)) & 0xFF));
		}
		
		public void PrintBrandString()
		{
			PrintBrand((uint)2147483650);
			PrintBrand((uint)2147483651);
			PrintBrand((uint)2147483652);
		}
		
		private void PrintBrand(uint param)
		{
			int identifier = Platforms.x86.Native.CpuIdEax(param);
			bool whitespace = true;
			if (identifier != 0x20202020)
				for (int i = 0; i < 4; ++i)
					PrintBrandPart(identifier, i, ref whitespace);
					

			identifier = Platforms.x86.Native.CpuIdEbx(param);
			if (identifier != 0x20202020)
				for (int i = 0; i < 4; ++i)
					PrintBrandPart(identifier, i, ref whitespace);

			identifier = Platforms.x86.Native.CpuIdEcx(param);
			if (identifier != 0x20202020)
				for (int i = 0; i < 4; ++i)
					PrintBrandPart(identifier, i, ref whitespace);

			identifier = Platforms.x86.Native.CpuIdEdx(param);
			if (identifier != 0x20202020)
				for (int i = 0; i < 4; ++i)
					PrintBrandPart(identifier, i, ref whitespace);
		}
		
		private void PrintBrandPart(int identifier, int i, ref bool whitespace)
		{
			char character = (char)((identifier >> (i * 8)) & 0xFF);
			
			if (whitespace && character == ' ')
				return;
			if (whitespace && character != ' ')
			{
				Screen.Write(character);
				whitespace = false;
				return;
			}
			if (character == ' ')
				whitespace = true;
			Screen.Write(character);
		}
	}
}