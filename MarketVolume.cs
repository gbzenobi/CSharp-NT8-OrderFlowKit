#region Using declarations
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Xml.Serialization;
using NinjaTrader.Cbi;
using NinjaTrader.Gui;
using NinjaTrader.Gui.Chart;
using NinjaTrader.Gui.SuperDom;
using NinjaTrader.Gui.Tools;
using NinjaTrader.Data;
using NinjaTrader.NinjaScript;
using NinjaTrader.Core.FloatingPoint;
using NinjaTrader.NinjaScript.DrawingTools;
#endregion

using NinjaTrader.NinjaScript.AddOns.SightEngine;

public static class _MarketVolumeEnums
{
	public enum Calculation
	{
		BidAsk,
		Delta,
		Total,
		TotalBidAsk,
		TotalDelta,
		//TotalDeltaBidAsk
	}
}

namespace NinjaTrader.NinjaScript.Indicators.WyckoffZen
{
	public class MarketVolume : Indicator
	{
		#region MAIN
		
		private class WyckoffMarketVolume
		{
			private VolumeAnalysis.WyckoffBars wyckoffBars;
			private VolumeAnalysis.MarketOrder sigmaVolume;
			private Brush brushBidVolume;
			private Brush brushAskVolume;
			private Brush brushTotalVolume;
			private BrushSeries bidPlotBrushes;
			private BrushSeries askPlotBrushes;
			private BrushSeries totalPlotBrushes;
			private Series<double> currentBidValue;
			private Series<double> currentAskValue;
			private Series<double> currentTotalValue;
			private Action<int, VolumeAnalysis.MarketOrder> RenderCalculation;
			// !- Optimizacion de funcion de periodos
			private Action computePeriods;
			private int Period;
			
			private int NT8barsCount;
			
			public WyckoffMarketVolume(){
				this.sigmaVolume = new VolumeAnalysis.MarketOrder();
			}
			
			public void setBidAskColor(Brush brushBidVolume, Brush brushAskVolume)
			{
				this.brushBidVolume = brushBidVolume;
				this.brushAskVolume = brushAskVolume;
			}
			public void setTotalColor(Brush brushTotalVolume)
			{
				this.brushTotalVolume = brushTotalVolume;
			}
			public void setBidAskPlotBrushes(BrushSeries bidPlotBrushes, BrushSeries askPlotBrushes)
			{
				this.bidPlotBrushes = bidPlotBrushes;
				this.askPlotBrushes = askPlotBrushes;
			}
			public void setTotalPlotBrushes(BrushSeries totalPlotBrushes)
			{
				this.totalPlotBrushes = totalPlotBrushes;
			}
			public void setBidAskPlotValues(Series<double> bidPlotValues, Series<double> askPlotValues)
			{
				this.currentBidValue = bidPlotValues;
				this.currentAskValue = askPlotValues;
			}
			public void setTotalPlotValues(Series<double> totalPlotValues)
			{
				this.currentTotalValue = totalPlotValues;
			}
			
			public void setWyckoffBars(VolumeAnalysis.WyckoffBars wyckoffBars)
			{
				this.wyckoffBars = wyckoffBars;
				this.NT8barsCount = wyckoffBars.NT8Bars.Count - 1;
			}
			public void setPeriod(int Period)
			{
				this.Period = Period;
				// *- Optimizacion de funcion de periodos
				if( Period > 1 ){
					this.computePeriods = this._computeXPeriods;
				}
				else {
					this.computePeriods = this._compute1Period;
				}
			}
			
			#region RENDER_CALCULATION
			
			private void __renderBidAsk(int currBar, VolumeAnalysis.MarketOrder volInfo)
			{
				this.bidPlotBrushes[currBar] = this.brushBidVolume;
				this.askPlotBrushes[currBar] = this.brushAskVolume;
				this.currentBidValue[currBar] = volInfo.Bid;
				this.currentAskValue[currBar] = volInfo.Ask;
			}
			private void __renderDelta(int currBar, VolumeAnalysis.MarketOrder volInfo)
			{
				// !- Usamos el bid pero es irrelevante en que Brush y Value lo pongamos...
				// no podemos usar @totalPlotBrushes ni @currentTotalValue porque puede ser
				// combinado con __renderTotal y producir sobre-escritura de datos
				long D = volInfo.Delta;
				if( D >= 0 ){
					this.bidPlotBrushes[currBar] = this.brushAskVolume;
				}
				else{
					this.bidPlotBrushes[currBar] = this.brushBidVolume;
				}
				this.currentBidValue[currBar] = D;
			}
			private void __renderTotal(int currBar, VolumeAnalysis.MarketOrder volInfo)
			{
				this.totalPlotBrushes[currBar] = this.brushTotalVolume;
				this.currentTotalValue[currBar] = volInfo.Total;
			}
			private void __renderTotalBidAsk(int currBar, VolumeAnalysis.MarketOrder volInfo)
			{
				__renderBidAsk(currBar, volInfo);
				__renderTotal(currBar, volInfo);
			}
			private void __renderTotalDelta(int currBar, VolumeAnalysis.MarketOrder volInfo)
			{
				__renderDelta(currBar, volInfo);
				__renderTotal(currBar, volInfo);
			}
			
			public void setCalculation(_MarketVolumeEnums.Calculation marketVolumeCalculation)
			{
				switch( marketVolumeCalculation )
				{
					case _MarketVolumeEnums.Calculation.BidAsk:
					{
						this.RenderCalculation = this.__renderBidAsk;
						break;
					}
					case _MarketVolumeEnums.Calculation.Delta:
					{
						this.RenderCalculation = this.__renderDelta;
						break;
					}
					case _MarketVolumeEnums.Calculation.Total:
					{
						this.RenderCalculation = this.__renderTotal;
						break;
					}
					case _MarketVolumeEnums.Calculation.TotalBidAsk:
					{
						this.RenderCalculation = this.__renderTotalBidAsk;
						break;
					}
					case _MarketVolumeEnums.Calculation.TotalDelta:
					{
						this.RenderCalculation = this.__renderTotalDelta;
						break;
					}
					/*case _MarketVolumeEnums.Calculation.TotalDeltaBidAsk:
					{
						this.RenderCalculation = ;
						break;
					}*/
				}
			}
			
			#endregion
			
			public Brush BrushBidVolume
			{
				get{ return this.brushBidVolume; }
			}
			public Brush BrushAskVolume
			{
				get{ return this.brushAskVolume; }
			}
			public Brush BrushTotalVolume
			{
				get{ return this.brushTotalVolume; }
			}
			
			private bool calculateBarsSigmaVolume(int barIndex)
			{
				int currBar = barIndex;//this.wyckoffBars.CurrentBarIndex;
				if( currBar < this.Period ){
					return false;
				}
				
				this.sigmaVolume.Clear();
				int n_bars = 0;
				for(int idx = currBar - 1; idx >= 0; idx--)
				{
					if( n_bars == this.Period )
						break;
					this.sigmaVolume.CalculateSigmaVolume(this.wyckoffBars[idx]);
					n_bars++;
				}
				return true;
			}
			
			// !- Optimizacion de funcion de periodos
			private void _compute1Period()
			{
				this.RenderCalculation(0, this.wyckoffBars.CurrentBar);
			}
			private void _computeXPeriods()
			{
				int currBarIndex = this.wyckoffBars.CurrentBarIndex;
				// !- una vez que una nueva barra es creada el puntero de barra suma: currentBarIndex + 1
				// por este motivo debemos calcular: currentBarIndex - 1
				// de otro modo estaremos en la barra recien acaba de crear, la informacion que subyace
				// ahi es la de los ultimos datos a mercado...
				if( this.wyckoffBars.IsNewBar )
				{
					if( this.calculateBarsSigmaVolume(currBarIndex) ){
						// !- Ponemos la informacion en la barra previa
						this.RenderCalculation(1, this.sigmaVolume);
					}
				}
				// !- Optimizacion para tiempo real
				if( currBarIndex >= this.NT8barsCount && this.calculateBarsSigmaVolume(currBarIndex+1) ){
					this.RenderCalculation(0, this.sigmaVolume);
				}
			}
			// !- renderizamos los calculos en OnMarketData
			public void onMarketData()
			{
				this.computePeriods();
			}
		}
		
		#endregion
		#region GLOBAL_VARIABLES
		
		private VolumeAnalysis.WyckoffBars wyckoffBars;
		private WyckoffMarketVolume wyckoffMarketVolume;
		
		#endregion
		
		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				wyckoffMarketVolume = new WyckoffMarketVolume ();
				
				Description									= @"";
				Name										= "Market Volume";
				Calculate									= Calculate.OnBarClose;
				IsOverlay									= false;
				DisplayInDataBox							= true;
				DrawOnPricePanel							= true;
				DrawHorizontalGridLines						= true;
				DrawVerticalGridLines						= true;
				PaintPriceMarkers							= true;
				ScaleJustification							= NinjaTrader.Gui.Chart.ScaleJustification.Right;
				//Disable this property if your indicator requires custom values that cumulate with each new market data event. 
				//See Help Guide for additional information.
				IsSuspendedWhileInactive					= false;
				
				wyckoffMarketVolume.setBidAskColor(Brushes.Red, Brushes.SeaGreen);
				wyckoffMarketVolume.setTotalColor(Brushes.PowderBlue);
				
				AddPlot(new Stroke(wyckoffMarketVolume.BrushBidVolume, DashStyleHelper.Solid, 3), PlotStyle.Bar, "Bid color");
				AddPlot(new Stroke(wyckoffMarketVolume.BrushAskVolume, DashStyleHelper.Solid, 3), PlotStyle.Bar, "Ask color");
				AddPlot(new Stroke(wyckoffMarketVolume.BrushTotalVolume, DashStyleHelper.DashDot, 1), PlotStyle.Line, "Total color");
				
				// !- Por defecto calculamos el delta
				//_MarketVolumeCalculation = _MarketVolumeEnums.Calculation.Delta;
				_Period = 1;
				_MarketVolumeCalculation = _MarketVolumeEnums.Calculation.Delta;
				_ZeroLineColor = Brushes.Beige;
			}
			else if (State == State.Configure)
			{
				AddLine(_ZeroLineColor, 0, "0PLine");
				wyckoffMarketVolume.setBidAskColor(Plots[0].Brush, Plots[1].Brush);
				wyckoffMarketVolume.setTotalColor(Plots[2].Brush);
				
				wyckoffMarketVolume.setBidAskPlotBrushes(PlotBrushes[0], PlotBrushes[1]);
				wyckoffMarketVolume.setTotalPlotBrushes(PlotBrushes[2]);
				wyckoffMarketVolume.setBidAskPlotValues(Values[0], Values[1]);
				wyckoffMarketVolume.setTotalPlotValues(Values[2]);
				
				wyckoffMarketVolume.setCalculation(_MarketVolumeCalculation);
				wyckoffMarketVolume.setPeriod(_Period);
				// !- Siempre al cerrar la barra
				Calculate = Calculate.OnEachTick;
			}
			else if(State == State.DataLoaded)
			{
				wyckoffBars = new VolumeAnalysis.WyckoffBars(Bars);
				wyckoffMarketVolume.setWyckoffBars(wyckoffBars);
			}
			//else if(State == State.Realtime){}
		}

		protected override void OnMarketData(MarketDataEventArgs MarketArgs)
		{
			if( !wyckoffBars.onMarketData(MarketArgs) )
				return;
			
			wyckoffMarketVolume.onMarketData();
		}
		
		#region Properties
		
		[NinjaScriptProperty]
		[Display(Name = "Formula", Order = 0, GroupName = "Market calculation")]
		public _MarketVolumeEnums.Calculation _MarketVolumeCalculation
		{ get; set; }
		
		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name = "Period", Order = 1, GroupName = "Market calculation")]
		public int _Period
		{ get; set; }
		
		[XmlIgnore]
		[Display(Name="Zero line color", Order=0, GroupName="Market calculation style")]
		public Brush _ZeroLineColor
		{ get; set; }
		[Browsable(false)]
		public string _ZeroLineColorSerializable
		{
			get { return Serialize.BrushToString(_ZeroLineColor); }
			set { _ZeroLineColor = Serialize.StringToBrush(value); }
		}
		
		#endregion
	}
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private WyckoffZen.MarketVolume[] cacheMarketVolume;
		public WyckoffZen.MarketVolume MarketVolume(_MarketVolumeEnums.Calculation _marketVolumeCalculation, int _period)
		{
			return MarketVolume(Input, _marketVolumeCalculation, _period);
		}

		public WyckoffZen.MarketVolume MarketVolume(ISeries<double> input, _MarketVolumeEnums.Calculation _marketVolumeCalculation, int _period)
		{
			if (cacheMarketVolume != null)
				for (int idx = 0; idx < cacheMarketVolume.Length; idx++)
					if (cacheMarketVolume[idx] != null && cacheMarketVolume[idx]._MarketVolumeCalculation == _marketVolumeCalculation && cacheMarketVolume[idx]._Period == _period && cacheMarketVolume[idx].EqualsInput(input))
						return cacheMarketVolume[idx];
			return CacheIndicator<WyckoffZen.MarketVolume>(new WyckoffZen.MarketVolume(){ _MarketVolumeCalculation = _marketVolumeCalculation, _Period = _period }, input, ref cacheMarketVolume);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.WyckoffZen.MarketVolume MarketVolume(_MarketVolumeEnums.Calculation _marketVolumeCalculation, int _period)
		{
			return indicator.MarketVolume(Input, _marketVolumeCalculation, _period);
		}

		public Indicators.WyckoffZen.MarketVolume MarketVolume(ISeries<double> input , _MarketVolumeEnums.Calculation _marketVolumeCalculation, int _period)
		{
			return indicator.MarketVolume(input, _marketVolumeCalculation, _period);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.WyckoffZen.MarketVolume MarketVolume(_MarketVolumeEnums.Calculation _marketVolumeCalculation, int _period)
		{
			return indicator.MarketVolume(Input, _marketVolumeCalculation, _period);
		}

		public Indicators.WyckoffZen.MarketVolume MarketVolume(ISeries<double> input , _MarketVolumeEnums.Calculation _marketVolumeCalculation, int _period)
		{
			return indicator.MarketVolume(input, _marketVolumeCalculation, _period);
		}
	}
}

#endregion
