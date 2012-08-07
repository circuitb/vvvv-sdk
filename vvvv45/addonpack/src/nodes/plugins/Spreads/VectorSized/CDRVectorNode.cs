#region usings
using System;
using System.ComponentModel.Composition;

using VVVV.PluginInterfaces.V2;
using VVVV.Utils.Streams;

using VVVV.Utils.VColor;
#endregion usings

namespace VVVV.Nodes
{
	#region PluginInfo
	[PluginInfo(Name = "CDR", Category = "Spreads", Version = "Vector", Help = "CDR (Spreads) with bin and vector size", Author = "woei")]
	#endregion PluginInfo
	public class CDRVectorNode : IPluginEvaluate
	{
		#region fields & pins
		#pragma warning disable 649
		[Input("Input")]
		IInStream<double> FInput;

		[Input("Vector Size", MinValue = 1, DefaultValue = 1, IsSingle = true)]
		IInStream<int> FVec;
		
		[Input("Bin Size", DefaultValue = -1)]
		IInStream<int> FBin;
		
		[Output("Remainder")]
		IOutStream<double> FRemainder;
		
		[Output("Last Slice")]
		IOutStream<double> FLast;
		#pragma warning restore
		#endregion fields & pins
		
		//called when data for any output pin is requested
		public void Evaluate(int SpreadMax)
		{
			if (FVec.Length>0)
			{
				int vecSize = Math.Max(1,FVec.GetReader().Read());
				VecBinSpread<double> spread = new VecBinSpread<double>(FInput,vecSize,FBin);
				
				FLast.Length = spread.Count * vecSize;
				if (FLast.Length == spread.ItemCount)
				{
					FRemainder.Length = 0;
					FLast.AssignFrom(FInput);
				}
				else
				{
					FRemainder.Length = spread.ItemCount-FLast.Length;
					using (var rWriter = FRemainder.GetWriter())
					using (var lWriter = FLast.GetWriter())
					{
						for (int b = 0; b < spread.Count; b++)
						{
							int rLength = spread[b].Length-vecSize;
							rWriter.Write(spread[b], 0, rLength);
							
							lWriter.Write(spread.GetBinRow(b,rLength/vecSize).ToArray(), 0, vecSize);
						}
					}
				}
			}
			else
				FRemainder.Length = FLast.Length = 0;
		}
	}
	
	public class CDRNode<T> : IPluginEvaluate
	{
		#region fields & pins
		#pragma warning disable 649
		[Input("Input")]
		IInStream<T> FInput;
		
		[Input("Bin Size", DefaultValue = -1)]
		IInStream<int> FBin;
		
		[Output("Remainder")]
		IOutStream<T> FRemainder;
		
		[Output("Last Slice")]
		IOutStream<T> FLast;
		#pragma warning restore
		#endregion fields & pins
		
		//called when data for any output pin is requested
		public void Evaluate(int SpreadMax)
		{
			VecBinSpread<T> spread = new VecBinSpread<T>(FInput,1,FBin);
			
			FLast.Length = spread.Count;
			if (FLast.Length == spread.ItemCount)
			{
				FRemainder.Length = 0;
				FLast.AssignFrom(FInput);
			}
			else
			{
				FRemainder.Length = spread.ItemCount-FLast.Length;
				using (var rWriter = FRemainder.GetWriter())
				using (var lWriter = FLast.GetWriter())
				{
					for (int b = 0; b < spread.Count; b++)
					{
						int rLength = spread[b].Length-1;
						rWriter.Write(spread[b], 0, rLength);
						
						lWriter.Write(spread[b][rLength]);
					}
				}
			}
		}
	}
	
	#region PluginInfo
	[PluginInfo(Name = "CDR", Category = "String", Version = "Bin", Help = "CDR (String) with bin size", Author = "woei")]
	#endregion PluginInfo
	public class CDRStringBin : CDRNode<string> {}
	
	#region PluginInfo
	[PluginInfo(Name = "CDR", Category = "Color", Version = "Bin", Help = "CDR (Color) with bin size", Author = "woei")]
	#endregion PluginInfo
	public class CDRColorBin : CDRNode<RGBAColor> {}
	
}
