using System;

namespace Server.LOS;

public delegate void ShadowMaker(uint[,] shadow);
public delegate bool ShadowTester(uint[,] shadow, int y);
//------------------------------------------------------------------------------
//  This is the code generator for the LOS system. It produces the 'shadows'
//  recorded in the arrays and used by LOS, for each positional obstruction.
//
//  You'll note that this program has a commented-out Main(). It has to be
//  enabled as a whole program to work properly.
//------------------------------------------------------------------------------
public class CodeGenMain
{
	internal int Nelements;
	internal int Center;

	internal int Nbytes;
	internal int Tail;

	internal ulong[] Bitposvalues = {
//            1LU << 63,
//            1LU << 62,
//            1LU << 61,
//            1LU << 60,
//            1LU << 59,
//            1LU << 58,
//            1LU << 57,
//            1LU << 56,
//            1LU << 55,
//            1LU << 54,
//            1LU << 53,
//            1LU << 52,
//            1LU << 51,
//            1LU << 50,
//            1LU << 49,
//            1LU << 48,
//            1LU << 47,
//            1LU << 46,
//            1LU << 45,
//            1LU << 44,
//            1LU << 43,
//            1LU << 42,
//            1LU << 41,
//            1LU << 40,
//            1LU << 39,
//            1LU << 38,
//            1LU << 37,
//            1LU << 36,
//            1LU << 35,
//            1LU << 34,
//            1LU << 33,
//            1LU << 32,
		1LU << 31,
		1LU << 30,
		1LU << 29,
		1LU << 28,
		1LU << 27,
		1LU << 26,
		1LU << 25,
		1LU << 24,
		1LU << 23,
		1LU << 22,
		1LU << 21,
		1LU << 20,
		1LU << 19,
		1LU << 18,
		1LU << 17,
		1LU << 16,
		1LU << 15,
		1LU << 14,
		1LU << 13,
		1LU << 12,
		1LU << 11,
		1LU << 10,
		1LU <<  9,
		1LU <<  8,
		1LU <<  7,
		1LU <<  6,
		1LU <<  5,
		1LU <<  4,
		1LU <<  3,
		1LU <<  2,
		1LU <<  1,
		1LU <<  0
	};

	//------------------------------------------------------------------------------
	public CodeGenMain(int nelements)
	{
		Nelements = nelements;
	}
	//------------------------------------------------------------------------------
	public void Execute()
	{
		Nbytes = Nelements / 32;

		if (Nelements % 32 > 0) Nbytes++;

		Tail = Nbytes * 32 - Nelements;

		Center = Nelements / 2;

		CalcAllVis();
	}
	//------------------------------------------------------------------------------
	public void PrintText(char[,] shadowmask)
	{
		for (int i = 0; i < Nelements; i++)
		{
			for (int j = 0; j < Nelements; j++)
			{
				Console.WriteLine(shadowmask[j, i]);
			}
		}
	}
	//------------------------------------------------------------------------------
	public void PrintShadowMakers(char[][,] shadowmask)
	{
		Console.WriteLine("//------------------------------------------------------------------------------");
		Console.WriteLine("//  LOS shadow mask for visibility fields " + Nelements + " cells square.");
		Console.WriteLine("//  Note: the individual visibility cells are bits in each byte.");
		Console.WriteLine("//------------------------------------------------------------------------------");
		Console.WriteLine("internal static ShadowMaker[,] /*[{0},{0}]*/ m_ShadowMakers = ", Nelements);
		Console.WriteLine("{");

		for (int n = 0; n < Nelements * Nelements; n++)
		{
			//int[,] bytemask = new int[m_nbytes,m_nelements];
			ulong[,] bytemask = new ulong[Nelements, Nbytes];
			for (int i = 0; i < Nelements; i++)
			{
				for (int j = 0; j < Nelements; j++)
				{
					int bitpos = i % 32;
					//Console.WriteLine( "bitpos of " + i + " = " + bitpos + " = " + bitposvalues[bitpos] );
					if ((shadowmask[n][i, j] == '0') || (shadowmask[n][i, j] == '#'))
						bytemask[j, i / 32] += Bitposvalues[bitpos];
				}
			}

			if (n % Nelements == 0)
				Console.WriteLine("  {");

			//Console.WriteLine("//  for a visibility block at ["+n%m_nelements+"]["+(int)(n/m_nelements)+"]:");
			Console.WriteLine("//  for an occlusion at [" + (int)(n / Nelements) + "," + n % Nelements + "]:");

			int masklines = 0;
			for (int i = 0; i < Nelements; i++)
			for (int j = 0; j < Nbytes; j++)
				if (bytemask[i, j] != 0) { masklines++; break; }

			if (masklines == 0)
			{
				Console.WriteLine("    null,");
			}
			else
				for (int i = 0; i < Nelements; i++)
				{
					if (i == 0)
					{
						Console.WriteLine("    delegate( uint[,] shadow )");
						Console.WriteLine("    {");
					}
					for (int j = 0; j < Nbytes; j++)
					{
						//if( bytemask[j,i]!=0 ) Console.WriteLine("        shadow[{0}][{1}] |= {2};",i,j,bytemask[j,i]);
						//if( bytemask[j,i]!=0 ) nonZeroCount++;
						if (bytemask[i, j] != 0) Console.WriteLine("        shadow[{0},{1}] |= {2}u;", i, j, bytemask[i, j]);
						if (bytemask[i, j] != 0)
						{
						}
					}
					if (i == Nelements - 1) Console.WriteLine("    },");
				}

			if (n % Nelements == Nelements - 1)
				Console.WriteLine("  },");
		}
		Console.WriteLine("};");
		//Console.WriteLine( "Counted: "+nonZeroCount );
	}
	//------------------------------------------------------------------------------
	public void PrintShadowTesters(char[][,] shadowmask)
	{
		Console.WriteLine("//------------------------------------------------------------------------------");
		Console.WriteLine("//  LOS shadow testers for visibility fields " + Nelements + " cells square.");
		Console.WriteLine("//------------------------------------------------------------------------------");
		Console.WriteLine("internal static ShadowTester[/* {0} */] m_ShadowTesters = ", Nelements);
		Console.WriteLine("{");

		for (int j = 0; j < Nelements; j++)
		{
			int column = j / 32;
			//ulong    mask = 1LU << ( m_nelements - j );
			ulong mask = 1LU << (32 - j - 1);

			Console.WriteLine("//  for an object in position at [XXX," + j + "]:");
			Console.WriteLine("//  " + BinaryRepr(mask));

			Console.WriteLine("    delegate ( uint[,] shadow, int y )");
			Console.WriteLine("    {");
			Console.WriteLine("        if( ( shadow[y,{1}] & {2}u ) == {2}u ) return true; else return false;",
				//j,
				column,
				mask
			);
			Console.WriteLine("    },");
		}
		Console.WriteLine("};");
	}
	//------------------------------------------------------------------------------
	public void PrintByteArray(char[][,] shadowmask)
	{
		int nonZeroCount = 0;

		Console.WriteLine("//------------------------------------------------------------------------------");
		Console.WriteLine("//  LOS shadow mask for visibility fields " + Nelements + " cells square.");
		Console.WriteLine("//  Note: the individual visibility cells are bits in each byte.");
		Console.WriteLine("//------------------------------------------------------------------------------");
		Console.WriteLine("shadows [{0},{0}] shadows = ", Nelements);
		Console.WriteLine("{");

		for (int n = 0; n < Nelements * Nelements; n++)
		{
			ulong[,] bytemask = new ulong[Nelements, Nbytes];
			for (int i = 0; i < Nelements; i++)
			{
				for (int j = 0; j < Nelements; j++)
				{
					int bitpos = i % 32;
					//Console.WriteLine( "bitpos of " + i + " = " + bitpos + " = " + bitposvalues[bitpos] );
					if ((shadowmask[n][i, j] == '0') || (shadowmask[n][i, j] == '#'))
						bytemask[j, i / 32] += Bitposvalues[bitpos];
				}
			}

			Console.WriteLine("//  Obstruction at elem[" + (int)(n / Nelements) + "][" + n % Nelements + "]:");
			if (n == 0) Console.WriteLine("{");
			for (int i = 0; i < Nelements; i++)
			{
				if (i == 0) Console.Write(" {{");
				else Console.Write("  {");
				for (int j = 0; j < Nbytes; j++)
				{
					Console.Write(" " + bytemask[i, j]);
					//Console.Write(" "+BinaryRepr(bytemask[i,j]));
					if (bytemask[i, j] != 0) nonZeroCount++;
					if (j != Nbytes - 1) Console.Write(",");
				}
				if (i != Nelements - 1) Console.WriteLine(" },");
				else Console.WriteLine(" }},");
			}
		}
		Console.WriteLine("};");
		Console.WriteLine("Counted: " + nonZeroCount);
	}
	//------------------------------------------------------------------------------
	public void PrintBoolArray(char[][,] shadowmask, bool pretty)
	{
		Console.WriteLine("//------------------------------------------------------------------------------");
		Console.WriteLine("// LOS shadow mask for visibility fields " + Nelements + " cells square.");
		Console.WriteLine("//------------------------------------------------------------------------------");
		for (int n = 0; n < Nelements * Nelements; n++)
		{
			Console.WriteLine("//  Obstruction at elem[" + (int)(n / Nelements) + "][" + n % Nelements + "]:");
			if (n == 0) Console.WriteLine("{");
			for (int i = 0; i < Nelements; i++)
			{
				if (i == 0) Console.Write(" {{");
				else Console.Write("  {");
				for (int j = 0; j < Nelements; j++)
				{
					if ((shadowmask[n][j, i] == '-') ||
					    (shadowmask[n][j, i] == 'O') ||
					    (shadowmask[n][j, i] == 'X'))
					{
						if (pretty) Console.Write(shadowmask[n][j, i]);
						else Console.Write(0);
					}
					else
					{
						if (pretty) Console.Write(shadowmask[n][j, i]);
						else Console.Write(1);
					}
					if (j != Nelements - 1) Console.Write(",");
				}
				if (i != Nelements - 1) Console.WriteLine("},");
				else Console.WriteLine("}},");
			}
		}
		Console.WriteLine("};");
	}
	//------------------------------------------------------------------------------
	public void InitMask(char[,] shadowmask, char marker)
	{
		for (int i = 0; i < Nelements; i++)
		for (int j = 0; j < Nelements; j++)
			shadowmask[i, j] = marker;
		shadowmask[Center, Center] = 'O';
	}
	//------------------------------------------------------------------------------
	public void TestSumming()
	{
		// this probably isn't working; I wrote the "AddShadow" method
		// to sanity check the method of summing shadows to create aggregated
		// shadow regions. Seems to work right.

		char[,] summary = new char[Nelements, Nelements];
		char[,] mask1 = new char[Nelements, Nelements];
		char[,] mask2 = new char[Nelements, Nelements];
		char[,] mask3 = new char[Nelements, Nelements];

		InitMask(summary, '-');
		InitMask(mask1, '-');
		InitMask(mask2, '-');
		InitMask(mask3, '-');

		CalcVis(mask1, 11, 4);
		CalcVis(mask2, 12, 4);
		CalcVis(mask3, 13, 4);

		AddShadow(summary, mask1);
		AddShadow(summary, mask2);
		AddShadow(summary, mask3);

		PrintText(mask1);
		PrintText(mask2);
		PrintText(mask3);

		PrintText(summary);
	}
	//------------------------------------------------------------------------------
	public void CalcAllVis()
	{
		char[][,] shadowmask = new char[Nelements * Nelements][,];

		for (int i = 0; i < shadowmask.Length; i++)
		{
			shadowmask[i] = new char[Nelements, Nelements];
		}

		int count = 0;
		for (int i = 0; i < Nelements; i++)
		for (int j = 0; j < Nelements; j++)
		{
			InitMask(shadowmask[count], '-');
			CalcVis(shadowmask[count], i, j);
			count++;
		}
		PrintLos(shadowmask);
		//PrintByteArray(shadowmask);
		//PrintBoolArray(shadowmask, true);
	}
	//------------------------------------------------------------------------------
	public void PrintLos(char[][,] shadowmask)
	{
		Console.WriteLine("//------------------------------------------------------------------------------");
		Console.WriteLine("//  WARNING--WARNING--THIS IS GENERATED CODE; DO NOT EDIT!!--WARNING--WARNING");
		Console.WriteLine("//------------------------------------------------------------------------------");
		Console.WriteLine("using Server;");
		Console.WriteLine("using System;");
		Console.WriteLine("//------------------------------------------------------------------------------");
		Console.WriteLine("namespace Custom.LOS {");
		Console.WriteLine("//------------------------------------------------------------------------------");
		Console.WriteLine("public partial class LineOfSight");
		Console.WriteLine("{");
		PrintShadowMakers(shadowmask);
		PrintShadowTesters(shadowmask);
		Console.WriteLine("}");
		Console.WriteLine("} // namespace");
	}
	//------------------------------------------------------------------------------
	public void AddShadow(char[,] summary, char[,] shadow)
	{
		for (int i = 0; i < Nelements; i++)
		for (int j = 0; j < Nelements; j++)
		{
			if (shadow[i, j] == '#') summary[i, j] = '#';
			if (shadow[i, j] == 'X') summary[i, j] = 'X';
		}
	}
	//------------------------------------------------------------------------------
	public void CalcVis(char[,] shadowmask, int xObstruct, int yObstruct)
	{
		shadowmask[xObstruct, yObstruct] = 'X';

		for (int i = 0; i < Nelements; i++)
		for (int j = 0; j < Nelements; j++)
			DrawVisLine(shadowmask, Center, Center, i, j);
	}
	//------------------------------------------------------------------------------
	public void DrawVisLine(char[,] shadowmask, int x1, int y1, int x2, int y2)
	{
		int dX = Math.Abs(x2 - x1);    // store the change in X and Y of the line endpoints
		int dY = Math.Abs(y2 - y1);

		int xcursor = x1;
		int ycursor = y1;

		int xincr; if (x2 < x1) { xincr = -1; } else { xincr = 1; }    // which direction in X?
		int yincr; if (y2 < y1) { yincr = -1; } else { yincr = 1; }    // which direction in Y?

		bool blocked = false;

		if (dX >= dY)   // if X is the independent variable
		{
			int dPr = dY << 1;                      // amount to increment decision if right is chosen (always)
			int dPru = dPr - (dX << 1);              // amount to increment decision if up is chosen
			int p = dPr - dX;                   // decision variable start value

			for (; dX >= 0; dX--)                           // process each point in the line one 
				// at a time (just use dX)
			{
				if (Center == xcursor && Center == ycursor) {
				}
				else if (shadowmask[xcursor, ycursor] == 'X') blocked = true;
				else if (!blocked) {
				}                   // shadowmask[xcursor][ycursor]='-';
				else shadowmask[xcursor, ycursor] = '#';
				if (p > 0)                                // is the pixel going right AND up?
				{
					xcursor += xincr;                       // increment independent variable
					ycursor += yincr;                       // increment dependent variable
					p += dPru;                              // increment decision (for up)
				}
				else                                      // is the pixel just going right?
				{
					xcursor += xincr;                       // increment independent variable
					p += dPr;                               // increment decision (for right)
				}
			}
		}
		else                                              // if Y is the independent variable
		{
			int dPr = dX << 1;                      // amount to increment decision if right is chosen (always)
			int dPru = dPr - (dY << 1);              // amount to increment decision if up is chosen
			int p = dPr - dY;                   // decision variable start value

			for (; dY >= 0; dY--)                           // process each point in the line one at a time (just use dY)
			{
				if (Center == xcursor && Center == ycursor) {
				}
				else if (shadowmask[xcursor, ycursor] == 'X') blocked = true;
				else if (!blocked) {
				}                   // shadowmask[xcursor][ycursor]='-';
				else shadowmask[xcursor, ycursor] = '#';
				if (p > 0)                                // is the pixel going up AND right?
				{
					xcursor += xincr;                       // increment dependent variable
					ycursor += yincr;                       // increment independent variable
					p += dPru;                              // increment decision (for up)
				}
				else                                      // is the pixel just going up?
				{
					ycursor += yincr;                       // increment independent variable
					p += dPr;                               // increment decision (for right)
				}
			}
		}
	}
	//------------------------------------------------------------------------------
	public static string BinaryRepr(ulong c)
	{
		char[] ret = new char[32];

		//        if( c >= ( 1LU << 63 ) ) { c = c - ( 1LU << 63 ); ret[ 0]='X'; } else ret[ 0]='-';
		//        if( c >= ( 1LU << 62 ) ) { c = c - ( 1LU << 62 ); ret[ 1]='X'; } else ret[ 1]='-';
		//        if( c >= ( 1LU << 61 ) ) { c = c - ( 1LU << 61 ); ret[ 2]='X'; } else ret[ 2]='-';
		//        if( c >= ( 1LU << 60 ) ) { c = c - ( 1LU << 60 ); ret[ 3]='X'; } else ret[ 3]='-';
		//        if( c >= ( 1LU << 59 ) ) { c = c - ( 1LU << 59 ); ret[ 4]='X'; } else ret[ 4]='-';
		//        if( c >= ( 1LU << 58 ) ) { c = c - ( 1LU << 58 ); ret[ 5]='X'; } else ret[ 5]='-';
		//        if( c >= ( 1LU << 57 ) ) { c = c - ( 1LU << 57 ); ret[ 6]='X'; } else ret[ 6]='-';
		//        if( c >= ( 1LU << 56 ) ) { c = c - ( 1LU << 56 ); ret[ 7]='X'; } else ret[ 7]='-';
		//        if( c >= ( 1LU << 55 ) ) { c = c - ( 1LU << 55 ); ret[ 8]='X'; } else ret[ 8]='-';
		//        if( c >= ( 1LU << 54 ) ) { c = c - ( 1LU << 54 ); ret[ 9]='X'; } else ret[ 9]='-';
		//        if( c >= ( 1LU << 53 ) ) { c = c - ( 1LU << 53 ); ret[10]='X'; } else ret[10]='-';
		//        if( c >= ( 1LU << 52 ) ) { c = c - ( 1LU << 52 ); ret[11]='X'; } else ret[11]='-';
		//        if( c >= ( 1LU << 51 ) ) { c = c - ( 1LU << 51 ); ret[12]='X'; } else ret[12]='-';
		//        if( c >= ( 1LU << 50 ) ) { c = c - ( 1LU << 50 ); ret[13]='X'; } else ret[13]='-';
		//        if( c >= ( 1LU << 49 ) ) { c = c - ( 1LU << 49 ); ret[14]='X'; } else ret[14]='-';
		//        if( c >= ( 1LU << 48 ) ) { c = c - ( 1LU << 48 ); ret[15]='X'; } else ret[15]='-';
		//        if( c >= ( 1LU << 47 ) ) { c = c - ( 1LU << 47 ); ret[16]='X'; } else ret[16]='-';
		//        if( c >= ( 1LU << 46 ) ) { c = c - ( 1LU << 46 ); ret[17]='X'; } else ret[17]='-';
		//        if( c >= ( 1LU << 45 ) ) { c = c - ( 1LU << 45 ); ret[18]='X'; } else ret[18]='-';
		//        if( c >= ( 1LU << 44 ) ) { c = c - ( 1LU << 44 ); ret[19]='X'; } else ret[19]='-';
		//        if( c >= ( 1LU << 43 ) ) { c = c - ( 1LU << 43 ); ret[20]='X'; } else ret[20]='-';
		//        if( c >= ( 1LU << 42 ) ) { c = c - ( 1LU << 42 ); ret[21]='X'; } else ret[21]='-';
		//        if( c >= ( 1LU << 41 ) ) { c = c - ( 1LU << 41 ); ret[22]='X'; } else ret[22]='-';
		//        if( c >= ( 1LU << 40 ) ) { c = c - ( 1LU << 40 ); ret[23]='X'; } else ret[23]='-';
		//        if( c >= ( 1LU << 39 ) ) { c = c - ( 1LU << 39 ); ret[24]='X'; } else ret[24]='-';
		//        if( c >= ( 1LU << 38 ) ) { c = c - ( 1LU << 38 ); ret[25]='X'; } else ret[25]='-';
		//        if( c >= ( 1LU << 37 ) ) { c = c - ( 1LU << 37 ); ret[26]='X'; } else ret[26]='-';
		//        if( c >= ( 1LU << 36 ) ) { c = c - ( 1LU << 36 ); ret[27]='X'; } else ret[27]='-';
		//        if( c >= ( 1LU << 35 ) ) { c = c - ( 1LU << 35 ); ret[28]='X'; } else ret[28]='-';
		//        if( c >= ( 1LU << 34 ) ) { c = c - ( 1LU << 34 ); ret[29]='X'; } else ret[29]='-';
		//        if( c >= ( 1LU << 33 ) ) { c = c - ( 1LU << 33 ); ret[30]='X'; } else ret[30]='-';
		//        if( c >= ( 1LU << 32 ) ) { c = c - ( 1LU << 32 ); ret[31]='X'; } else ret[31]='-';
		//        if( c >= ( 1LU << 31 ) ) { c = c - ( 1LU << 31 ); ret[32]='X'; } else ret[32]='-';
		//        if( c >= ( 1LU << 30 ) ) { c = c - ( 1LU << 30 ); ret[33]='X'; } else ret[33]='-';
		//        if( c >= ( 1LU << 29 ) ) { c = c - ( 1LU << 29 ); ret[34]='X'; } else ret[34]='-';
		//        if( c >= ( 1LU << 28 ) ) { c = c - ( 1LU << 28 ); ret[35]='X'; } else ret[35]='-';
		//        if( c >= ( 1LU << 27 ) ) { c = c - ( 1LU << 27 ); ret[36]='X'; } else ret[36]='-';
		//        if( c >= ( 1LU << 26 ) ) { c = c - ( 1LU << 26 ); ret[37]='X'; } else ret[37]='-';
		//        if( c >= ( 1LU << 25 ) ) { c = c - ( 1LU << 25 ); ret[38]='X'; } else ret[38]='-';
		//        if( c >= ( 1LU << 24 ) ) { c = c - ( 1LU << 24 ); ret[39]='X'; } else ret[39]='-';
		//        if( c >= ( 1LU << 23 ) ) { c = c - ( 1LU << 23 ); ret[40]='X'; } else ret[40]='-';
		//        if( c >= ( 1LU << 22 ) ) { c = c - ( 1LU << 22 ); ret[41]='X'; } else ret[41]='-';
		//        if( c >= ( 1LU << 21 ) ) { c = c - ( 1LU << 21 ); ret[42]='X'; } else ret[42]='-';
		//        if( c >= ( 1LU << 20 ) ) { c = c - ( 1LU << 20 ); ret[43]='X'; } else ret[43]='-';
		//        if( c >= ( 1LU << 19 ) ) { c = c - ( 1LU << 19 ); ret[44]='X'; } else ret[44]='-';
		//        if( c >= ( 1LU << 18 ) ) { c = c - ( 1LU << 18 ); ret[45]='X'; } else ret[45]='-';
		//        if( c >= ( 1LU << 17 ) ) { c = c - ( 1LU << 17 ); ret[46]='X'; } else ret[46]='-';
		//        if( c >= ( 1LU << 16 ) ) { c = c - ( 1LU << 16 ); ret[47]='X'; } else ret[47]='-';
		//        if( c >= ( 1LU << 15 ) ) { c = c - ( 1LU << 15 ); ret[48]='X'; } else ret[48]='-';
		//        if( c >= ( 1LU << 14 ) ) { c = c - ( 1LU << 14 ); ret[49]='X'; } else ret[49]='-';
		//        if( c >= ( 1LU << 13 ) ) { c = c - ( 1LU << 13 ); ret[50]='X'; } else ret[50]='-';
		//        if( c >= ( 1LU << 12 ) ) { c = c - ( 1LU << 12 ); ret[51]='X'; } else ret[51]='-';
		//        if( c >= ( 1LU << 11 ) ) { c = c - ( 1LU << 11 ); ret[52]='X'; } else ret[52]='-';
		//        if( c >= ( 1LU << 10 ) ) { c = c - ( 1LU << 10 ); ret[53]='X'; } else ret[53]='-';
		//        if( c >= ( 1LU <<  9 ) ) { c = c - ( 1LU <<  9 ); ret[54]='X'; } else ret[54]='-';
		//        if( c >= ( 1LU <<  8 ) ) { c = c - ( 1LU <<  8 ); ret[55]='X'; } else ret[55]='-';
		//        if( c >= ( 1LU <<  7 ) ) { c = c - ( 1LU <<  7 ); ret[56]='X'; } else ret[56]='-';
		//        if( c >= ( 1LU <<  6 ) ) { c = c - ( 1LU <<  6 ); ret[57]='X'; } else ret[57]='-';
		//        if( c >= ( 1LU <<  5 ) ) { c = c - ( 1LU <<  5 ); ret[58]='X'; } else ret[58]='-';
		//        if( c >= ( 1LU <<  4 ) ) { c = c - ( 1LU <<  4 ); ret[59]='X'; } else ret[59]='-';
		//        if( c >= ( 1LU <<  3 ) ) { c = c - ( 1LU <<  3 ); ret[60]='X'; } else ret[60]='-'; 
		//        if( c >= ( 1LU <<  2 ) ) { c = c - ( 1LU <<  2 ); ret[61]='X'; } else ret[61]='-';
		//        if( c >= ( 1LU <<  1 ) ) { c = c - ( 1LU <<  1 ); ret[62]='X'; } else ret[62]='-';
		//        if( c >= ( 1LU <<  0 ) ) { c = c - ( 1LU <<  0 ); ret[63]='X'; } else ret[63]='-';

		int i = -1;

		if (c >= (1LU << 31)) { c -= 1LU << 31; ret[i += 1] = 'X'; } else ret[i += 1] = '-';
		if (c >= (1LU << 30)) { c -= 1LU << 30; ret[i += 1] = 'X'; } else ret[i += 1] = '-';
		if (c >= (1LU << 29)) { c -= 1LU << 29; ret[i += 1] = 'X'; } else ret[i += 1] = '-';
		if (c >= (1LU << 28)) { c -= 1LU << 28; ret[i += 1] = 'X'; } else ret[i += 1] = '-';
		if (c >= (1LU << 27)) { c -= 1LU << 27; ret[i += 1] = 'X'; } else ret[i += 1] = '-';
		if (c >= (1LU << 26)) { c -= 1LU << 26; ret[i += 1] = 'X'; } else ret[i += 1] = '-';
		if (c >= (1LU << 25)) { c -= 1LU << 25; ret[i += 1] = 'X'; } else ret[i += 1] = '-';
		if (c >= (1LU << 24)) { c -= 1LU << 24; ret[i += 1] = 'X'; } else ret[i += 1] = '-';
		if (c >= (1LU << 23)) { c -= 1LU << 23; ret[i += 1] = 'X'; } else ret[i += 1] = '-';
		if (c >= (1LU << 22)) { c -= 1LU << 22; ret[i += 1] = 'X'; } else ret[i += 1] = '-';
		if (c >= (1LU << 21)) { c -= 1LU << 21; ret[i += 1] = 'X'; } else ret[i += 1] = '-';
		if (c >= (1LU << 20)) { c -= 1LU << 20; ret[i += 1] = 'X'; } else ret[i += 1] = '-';
		if (c >= (1LU << 19)) { c -= 1LU << 19; ret[i += 1] = 'X'; } else ret[i += 1] = '-';
		if (c >= (1LU << 18)) { c -= 1LU << 18; ret[i += 1] = 'X'; } else ret[i += 1] = '-';
		if (c >= (1LU << 17)) { c -= 1LU << 17; ret[i += 1] = 'X'; } else ret[i += 1] = '-';
		if (c >= (1LU << 16)) { c -= 1LU << 16; ret[i += 1] = 'X'; } else ret[i += 1] = '-';
		if (c >= (1LU << 15)) { c -= 1LU << 15; ret[i += 1] = 'X'; } else ret[i += 1] = '-';
		if (c >= (1LU << 14)) { c -= 1LU << 14; ret[i += 1] = 'X'; } else ret[i += 1] = '-';
		if (c >= (1LU << 13)) { c -= 1LU << 13; ret[i += 1] = 'X'; } else ret[i += 1] = '-';
		if (c >= (1LU << 12)) { c -= 1LU << 12; ret[i += 1] = 'X'; } else ret[i += 1] = '-';
		if (c >= (1LU << 11)) { c -= 1LU << 11; ret[i += 1] = 'X'; } else ret[i += 1] = '-';
		if (c >= (1LU << 10)) { c -= 1LU << 10; ret[i += 1] = 'X'; } else ret[i += 1] = '-';
		if (c >= (1LU << 9)) { c -= 1LU << 9; ret[i += 1] = 'X'; } else ret[i += 1] = '-';
		if (c >= (1LU << 8)) { c -= 1LU << 8; ret[i += 1] = 'X'; } else ret[i += 1] = '-';
		if (c >= (1LU << 7)) { c -= 1LU << 7; ret[i += 1] = 'X'; } else ret[i += 1] = '-';
		if (c >= (1LU << 6)) { c -= 1LU << 6; ret[i += 1] = 'X'; } else ret[i += 1] = '-';
		if (c >= (1LU << 5)) { c -= 1LU << 5; ret[i += 1] = 'X'; } else ret[i += 1] = '-';
		if (c >= (1LU << 4)) { c -= 1LU << 4; ret[i += 1] = 'X'; } else ret[i += 1] = '-';
		if (c >= (1LU << 3)) { c -= 1LU << 3; ret[i += 1] = 'X'; } else ret[i += 1] = '-';
		if (c >= (1LU << 2)) { c -= 1LU << 2; ret[i += 1] = 'X'; } else ret[i += 1] = '-';
		if (c >= (1LU << 1)) { c -= 1LU << 1; ret[i += 1] = 'X'; } else ret[i += 1] = '-';
		if (c >= (1LU << 0)) { c = c - (1LU << 0); ret[i += 1] = 'X'; } else ret[i += 1] = '-';
		return new string(ret);
	}
	//------------------------------------------------------------------------------
	public delegate void ShadowMaker(ulong[,] shadow);

	public delegate bool ShadowTester(ulong[,] shadow);

	internal ShadowTester[] ShadowTesters =
	{
		shadow => (shadow[0, 0] & 137438953472ul) == 137438953472ul,
		shadow => (shadow[0, 0] & 68719476736ul) == 68719476736ul
	};

	//    public static void Main( string[] args )
	//    {
	//        try
	//        { 
	//            CodeGenMain ms = new CodeGenMain ( 31 );
	//            ms.Execute();
	//        //ms.TestSumming();
	//
	//        }
	//        catch( Exception e )
	//        {
	//            Console.WriteLine( e );
	//        }
	//    }
}
