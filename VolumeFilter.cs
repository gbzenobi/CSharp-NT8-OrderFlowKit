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
using NinjaTrader.NinjaScript.AddOns.WyckoffRenderUtils;

public static class __VolumeFilterProperties{
	public enum Formula
	{
		Delta,
		Total
	}
	
//	//** Implementar mas adelante para detectar volumen en barras ademas de clusters de barra
//	public enum Type
//	{
//		Cluster,
//		Bar
//	}
	public enum Geometry
	{
		Circle,
		Rectangle
	}
}

namespace NinjaTrader.NinjaScript.Indicators.WyckoffZen
{
	public class VolumeFilter : Indicator
	{
		#region MAIN
		
		private VolumeAnalysis.VolumeType getVolumeType(__VolumeFilterProperties.Formula volumeType)
		{
			switch( volumeType )
			{
				case __VolumeFilterProperties.Formula.Total:
				{
					return VolumeAnalysis.VolumeType.Total;
				}
				case __VolumeFilterProperties.Formula.Delta:
				{
					return VolumeAnalysis.VolumeType.Delta;
				}
			}
			return VolumeAnalysis.VolumeType.BidAsk;
		}
		
		private class WyckoffVolumeFilter : WyckoffRenderControl
		{
			private SharpDX.RectangleF Rect;
			private SharpDX.Direct2D1.Ellipse Ellipse;
			private Action renderCluster;
			private Action<float, SharpDX.Color> renderGeometry;
			private SharpDX.DirectWrite.TextFormat volumeTextFormat;
			private bool hasGeometryFill;
			private int geometryAggresiveLevel;
			private bool showText;
			
			private SharpDX.Color colorBidClusterColor;
			private SharpDX.Color colorAskClusterColor;
			private SharpDX.Color colorTotalClusterColor;
			private SharpDX.Color colorBidFontColor;
			private SharpDX.Color colorAskFontColor;
			private SharpDX.Color colorTotalFontColor;
			
			// !- Para copiado de datos internos;
			private float barX;
			private float barY;
			// !- setup calculation
			private float minFontWidth;
			private float minFontHeight;
			// !- setup opacity
			private float brushBidFontOpacity;
			private float brushAskFontOpacity;
			private float brushTotalFontOpacity;
			private float outlineOpacity;
			
			// !- informacion
			private VolumeAnalysis.WyckoffBars wyckoffBars;
			private VolumeAnalysis.WyckoffBars.Bar currentBar;
			private VolumeAnalysis.MarketOrder marketOrder;
			private bool isRealtime;
			private VolumeAnalysis.VolumeType volumeType;
			private long minVolumeFilter;
			
			public WyckoffVolumeFilter()
			{
				this.marketOrder = new VolumeAnalysis.MarketOrder();
				this.isRealtime = false;
			}
			
			#region SETS
			
			public void setWyckoffBars(VolumeAnalysis.WyckoffBars wyckoffBars)
			{
				this.wyckoffBars = wyckoffBars;
			}
			public void setFontStyle(SimpleFont font)
			{
				base.setFontStyle(font, out volumeTextFormat);
			}
			public void setBidAskClusterColor(Brush brushBidClusterColor, Brush brushAskClusterColor)
			{
				this.colorBidClusterColor = WyckoffRenderControl.BrushToColor(brushBidClusterColor);
				this.colorAskClusterColor = WyckoffRenderControl.BrushToColor(brushAskClusterColor);
			}
			public void setTotalClusterColor(Brush brushTotalClusterColor)
			{
				this.colorTotalClusterColor = WyckoffRenderControl.BrushToColor(brushTotalClusterColor);
			}
			public void setMinSizeFont(float minFontWidth, float minFontHeight)
			{
				this.minFontWidth = minFontWidth;
				this.minFontHeight = minFontHeight;
			}
			public void setBidAskFontColor(
				Brush brushBidFontColor, float bidOpacity,
				Brush brushAskFontColor, float askOpacity)
			{
				this.colorBidFontColor = WyckoffRenderControl.BrushToColor(brushBidFontColor);
				this.brushBidFontOpacity = bidOpacity / 100f;
				this.colorAskFontColor = WyckoffRenderControl.BrushToColor(brushAskFontColor);
				this.brushAskFontOpacity = askOpacity / 100f;
			}
			public void setTotalFontColor(Brush brushTotalFontColor, float totalOpacity)
			{
				this.colorTotalFontColor = WyckoffRenderControl.BrushToColor(brushTotalFontColor);
				this.brushTotalFontOpacity = totalOpacity / 100f;
			}
			public void setGeometry(__VolumeFilterProperties.Geometry volumeFilterGeometry, bool hasGeometryFill, int geometryAggresiveLevel)
			{
				this.geometryAggresiveLevel = geometryAggresiveLevel;
				this.hasGeometryFill = hasGeometryFill;
				switch(volumeFilterGeometry)
				{
					// !- rectangulo relleno
					case __VolumeFilterProperties.Geometry.Rectangle:
					{
						this.Rect = new SharpDX.RectangleF();
						this.renderGeometry = this._renderRectangle;
						break;
					}
					// !- circulo relleno
					case __VolumeFilterProperties.Geometry.Circle:
					{
						this.Ellipse = new SharpDX.Direct2D1.Ellipse();
						this.renderGeometry = this._renderCircle;
						break;
					}
				}
			}
			
			public void setOutlineOpacity(float outlineOpacity)
			{
				this.outlineOpacity = outlineOpacity / 100f;
			}
			public void setShowText(bool showText)
			{
				this.showText = showText;
			}
			public void setRealtime(bool isRealtime)
			{
				this.isRealtime = isRealtime;
			}
			public bool IsRealtime
			{
				get{ return this.isRealtime; }
			}
			public void setMinVolumeFilter(long minVolumeFilter, VolumeAnalysis.VolumeType volumeType)
			{
				this.minVolumeFilter = minVolumeFilter;
				this.volumeType = volumeType;
				
				switch( this.volumeType )
				{
					case VolumeAnalysis.VolumeType.Total:
					{
						this.renderCluster = this.__renderClusterTotal;
						break;
					}
					case VolumeAnalysis.VolumeType.Delta:
					{
						this.renderCluster = this.__renderClusterDelta;
						break;
					}
				}
			}
			
			#endregion
			
			private void _renderRectangle(float maxClusterPer, SharpDX.Color color)
			{
				float rad;
				float half_rad;
				float _maxClusterPer = maxClusterPer / 100f;
				if( maxClusterPer > 100f )
					rad = this.W * 2.0f;
				else
					rad = this.W * _maxClusterPer;
				
				half_rad = rad / 2f;
				this.Rect.X = this.barX - half_rad;
				this.Rect.Y = this.barY - half_rad;
				this.Rect.Width = rad;
				this.Rect.Height= rad;//this.H;
				
				if( this.outlineOpacity > 0.0f )
					//Target.DrawRectangle(this.Rect, brushColor.ToDxBrush(Target, this.outlineOpacity));
					myDrawRectangle(ref Rect, color, outlineOpacity, 1.0f);
				if( this.hasGeometryFill )
					//Target.FillRectangle(this.Rect, brushColor.ToDxBrush(Target, _maxClusterPer / 2f));
					myFillRectangle(ref Rect, color, _maxClusterPer / 2f);
			}
			private void _renderCircle(float maxClusterPer, SharpDX.Color color)
			{
				float rad;
				this.Ellipse.Point.X = this.barX;
				this.Ellipse.Point.Y = this.barY;
				
				float _maxClusterPer = maxClusterPer / 100f;
				if( maxClusterPer > 100f )
					rad = this.W * 2.0f;
				else
					rad = this.W * _maxClusterPer;
				this.Ellipse.RadiusX = rad / 2f;
				this.Ellipse.RadiusY = this.Ellipse.RadiusX;
				
				if( this.outlineOpacity > 0.0f )
					//Target.DrawEllipse(this.Ellipse, brushColor.ToDxBrush(Target, this.outlineOpacity));
					myDrawEllipse(ref Ellipse, color, outlineOpacity, 1.0f);
				if( this.hasGeometryFill )
					//Target.FillEllipse(this.Ellipse, brushColor.ToDxBrush(Target, _maxClusterPer / 2f));
					myFillEllipse(ref Ellipse, color, _maxClusterPer / 2f);
			}
			private void renderDrawText(long volume, SharpDX.Color color, float opacity)
			{
				this.Rect.X = this.barX - (this.W / 2f);
				this.Rect.Y = this.barY - (this.H / 2f);
				this.Rect.Width = this.W;
				this.Rect.Height= this.H;
				myDrawText(volume.ToString(), ref Rect, color, -1, -1, volumeTextFormat, opacity);
			}
			private void __renderClusterTotal()
			{
				long T = this.marketOrder.Total;
				if( T < this.minVolumeFilter ){
					return;
				}
				
				//float maxClusterPer = (float)Math2.Percent(this.currentBar.MaxClusterVolume.Total, T) / 100f;
				float maxClusterPer = (float)Math2.Percent(this.minVolumeFilter * this.geometryAggresiveLevel, T);
				this.renderGeometry(maxClusterPer, colorTotalClusterColor);
				if( this.showText && this.W >= this.minFontWidth && this.H >= this.minFontHeight ){
					renderDrawText(T, colorTotalFontColor, brushTotalFontOpacity);
				}
			}
			private void __renderClusterDelta()
			{
				long D = this.marketOrder.Delta;
				long d = Math.Abs(D);
				if( d < this.minVolumeFilter ){
					return;
				}
				// !- de esta forma graficamos la geometria dependiendo del volumen maximo del cluster
				// de la barra
				//long maxClusterVolume = Math.Abs(this.currentBar.MaxClusterVolume.Delta);
				//float maxClusterPer = (float)Math2.Percent(maxClusterVolume, d) / 100f;
				float maxClusterPer = (float)Math2.Percent(this.minVolumeFilter * this.geometryAggresiveLevel, d);
				// !- Renderizamos el cluster de precio
				if( D >= 0 ){
					this.renderGeometry(maxClusterPer, this.colorAskClusterColor);
				}
				else{
					this.renderGeometry(maxClusterPer, this.colorBidClusterColor);
				}
				if( this.showText && this.W >= this.minFontWidth && this.H >= this.minFontHeight )
				{
					this.Rect.X = this.barX - (this.W / 2f);
					this.Rect.Y = this.barY - (this.H / 2f);
					this.Rect.Width = this.W;
					
					if( D >= 0 ){
						renderDrawText(d, colorAskFontColor, brushAskFontOpacity);
					}
					else{
						renderDrawText(d, colorBidFontColor, brushBidFontOpacity);
					}
				}
			}
			public void renderBarClusters(int barIndex, bool realtimeCalculation)
			{
				if( realtimeCalculation ){
					//if( !this.wyckoffBars.BarExists(barIndex) )
						//return;
					this.currentBar = this.wyckoffBars.CurrentBar;//this.currentBar = this.wyckoffBars[barIndex];
					this.currentBar.CalculateMinAndMaxCluster();
				}
				else{
					this.currentBar = this.wyckoffBars[barIndex];
				}
				this.barX = CHART_CONTROL.GetXByBarIndex(CHART_BARS, barIndex);
				
				foreach(var wb in currentBar)
				{
					this.barY = CHART_SCALE.GetYByValue(wb.Key);
					// !- informacion de volumen
					this.marketOrder = wb.Value;
					// !- renderizamos el cluster precio a precio
					this.renderCluster();
				}
			}
		}
		
		#endregion
		#region GLOBAL_VARIABLES
		
		private VolumeAnalysis.WyckoffBars wyckoffBars;
		private WyckoffVolumeFilter wyckoffVF;
		
		#endregion
		
		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description									= @"";
				Name										= "Volume Filter";
				Calculate									= Calculate.OnEachTick;
				IsOverlay									= true;
				DisplayInDataBox							= true;
				DrawOnPricePanel							= true;
				DrawHorizontalGridLines						= true;
				DrawVerticalGridLines						= true;
				PaintPriceMarkers							= true;
				ScaleJustification							= NinjaTrader.Gui.Chart.ScaleJustification.Right;
				//Disable this property if your indicator requires custom values that cumulate with each new market data event. 
				//See Help Guide for additional information.
				IsSuspendedWhileInactive					= false;
				
				wyckoffVF = new WyckoffVolumeFilter();
				
				// !- Setup de estilo
				_TextFont = new SimpleFont();
				_TextFont.Family = new FontFamily("Arial");
				_TextFont.Size = 10f;
				_TextFont.Bold = false;
				_TextFont.Italic= false;
				
				_BidClusterVolumeColor = Brushes.Firebrick;
				_AskClusterVolumeColor = Brushes.SeaGreen;
				_TotalClusterVolumeColor = Brushes.CornflowerBlue;
				
				_MinFontWidth = 14f; _MinFontHeight = 14f;
				_BidTextVolumeColor = Brushes.Salmon;
				_BidTextOpacity = 100f;
				_AskTextVolumeColor = Brushes.LightGreen;
				_AskTextOpacity = 100f;
				_TotalTextVolumeColor = Brushes.Bisque;
				_TotalTextOpacity = 100f;
				_OutlineOpacity = 70f;
				
				// !- setup de filtro
				_MinVolumeFilter = 100;
				_Geometry = __VolumeFilterProperties.Geometry.Circle;
				_FillGeometry = true;
				_GeometryAggresiveLevel = 5;
				_ShowText = true;
			}
			else if (State == State.Configure)
			{
				wyckoffVF.setFontStyle(_TextFont);
				wyckoffVF.setBidAskClusterColor(_BidClusterVolumeColor, _AskClusterVolumeColor);
				wyckoffVF.setTotalClusterColor(_TotalClusterVolumeColor);
				wyckoffVF.setBidAskFontColor(_BidTextVolumeColor, _BidTextOpacity,
					_AskTextVolumeColor, _AskTextOpacity);
				wyckoffVF.setTotalFontColor(_TotalTextVolumeColor, _TotalTextOpacity);
				wyckoffVF.setMinSizeFont(_MinFontWidth, _MinFontHeight);
				wyckoffVF.setMinVolumeFilter(_MinVolumeFilter, getVolumeType(_Formula));
				wyckoffVF.setGeometry(_Geometry, _FillGeometry, _GeometryAggresiveLevel);
				wyckoffVF.setOutlineOpacity(_OutlineOpacity);
				wyckoffVF.setShowText(_ShowText);
				// !- Siempre calcular las barras tick por tick
				Calculate = Calculate.OnEachTick;
			}
			else if(State == State.DataLoaded)
			{
				wyckoffBars = new VolumeAnalysis.WyckoffBars(Bars);
				wyckoffBars.enableMinClusterVolumeFilter(_MinVolumeFilter, getVolumeType(_Formula));
				wyckoffVF.setWyckoffBars(wyckoffBars);
			}
			else if(State == State.Realtime)
			{
				wyckoffVF.setRealtime(true);
			}
		}
		protected override void OnRender(ChartControl chartControl, ChartScale chartScale)
		{
			base.OnRender(chartControl, chartScale);
			if( !wyckoffVF.IsRealtime ||
				IsInHitTest == null ||
				chartControl == null ||
				ChartBars.Bars == null ){
				return;
			}
			
			#region RENDER_VOLUME_FILTER
			
			// 1- Altura minima de un tick
			// 2- Ancho de barra en barra
			wyckoffVF.setHW(chartScale.GetPixelsForDistance(TickSize), chartControl.Properties.BarDistance);
			// !- Apuntamos al target de renderizado
			wyckoffVF.setRenderTarget(chartControl, chartScale, ChartBars, RenderTarget);
			
			int fromIndex = ChartBars.FromIndex;
			int toIndex = ChartBars.ToIndex;
			
			// !- para calculos en tiempo real
			if( wyckoffBars.CurrentBarIndex == toIndex ){
				wyckoffVF.renderBarClusters(toIndex, true);
				toIndex--;
			}
			
			try{
				for(int barIndex = fromIndex; barIndex <= toIndex; barIndex++)
					wyckoffVF.renderBarClusters(barIndex, false);
			}
			catch{}
			
			#endregion
		}
		protected override void OnMarketData(MarketDataEventArgs MarketArgs)
		{
			wyckoffBars.onMarketData(MarketArgs);
		}
		
		#region Properties
		
		[NinjaScriptProperty]
		[Display(Name = "Formula", Order = 0, GroupName = "Volume Filter Calculation")]
		public __VolumeFilterProperties.Formula _Formula
		{ get; set; }
		
		[NinjaScriptProperty]
		[Range(1, long.MaxValue)]
		[Display(Name="Min volume filter", Order = 1, GroupName="Volume Filter Calculation")]
		public long _MinVolumeFilter
		{ get; set; }
		
		// --
		
		[NinjaScriptProperty]
		[Display(Name = "Drawn", Order = 0, GroupName = "Volume Filter Geometry")]
		public __VolumeFilterProperties.Geometry _Geometry
		{ get; set; }
		
		[NinjaScriptProperty]
		[Display(Name="Fill", Order=1, GroupName="Volume Filter Geometry")]
		public bool _FillGeometry
		{ get; set; }
		
		[NinjaScriptProperty]
		[Range(2, 10)]
		[Display(Name="Aggresive level", Order=2, GroupName="Volume Filter Geometry")]
		public int _GeometryAggresiveLevel
		{ get; set; }
		
		// --
		
		[XmlIgnore]
		[Display(Name="Bid clusters", Order=1, GroupName="Volume Filter Style")]
		public Brush _BidClusterVolumeColor
		{ get; set; }
		[Browsable(false)]
		public string _BidClusterVolumeColorSerializable
		{
			get { return Serialize.BrushToString(_BidClusterVolumeColor); }
			set { _BidClusterVolumeColor = Serialize.StringToBrush(value); }
		}
		[XmlIgnore]
		[Display(Name="Ask clusters", Order=2, GroupName="Volume Filter Style")]
		public Brush _AskClusterVolumeColor
		{ get; set; }
		[Browsable(false)]
		public string _AskClusterVolumeColorSerializable
		{
			get { return Serialize.BrushToString(_AskClusterVolumeColor); }
			set { _AskClusterVolumeColor = Serialize.StringToBrush(value); }
		}
		[XmlIgnore]
		[Display(Name="Total clusters", Order=3, GroupName="Volume Filter Style")]
		public Brush _TotalClusterVolumeColor
		{ get; set; }
		[Browsable(false)]
		public string _TotalClusterVolumeColorSerializable
		{
			get { return Serialize.BrushToString(_TotalClusterVolumeColor); }
			set { _TotalClusterVolumeColor = Serialize.StringToBrush(value); }
		}
		
		[NinjaScriptProperty]
		[Range(0.0f, 100f)]
		[Display(Name="Outline opacity", Order=4, GroupName="Volume Filter Style")]
		public float _OutlineOpacity
		{ get; set; }
		
		[NinjaScriptProperty]
		[Display(Name="Font", Order=5, GroupName="Volume Filter Style")]
		public SimpleFont _TextFont
		{ get; set; }
		
		[NinjaScriptProperty]
		[Display(Name="Show text", Order=6, GroupName="Volume Filter Style")]
		public bool _ShowText
		{ get; set; }
		
		[NinjaScriptProperty]
		[Range(1.0f, float.MaxValue)]
		[Display(Name="Min font width", Order=7, GroupName="Volume Filter Style")]
		public float _MinFontWidth
		{ get; set; }
		[NinjaScriptProperty]
		[Range(1.0f, float.MaxValue)]
		[Display(Name="Min font height", Order=8, GroupName="Volume Filter Style")]
		public float _MinFontHeight
		{ get; set; }
		
		[XmlIgnore]
		[Display(Name="Ask text", Order=9, GroupName="Volume Filter Style")]
		public Brush _AskTextVolumeColor
		{ get; set; }
		[Browsable(false)]
		public string _AskTextVolumeColorSerializable
		{
			get { return Serialize.BrushToString(_AskTextVolumeColor); }
			set { _AskTextVolumeColor = Serialize.StringToBrush(value); }
		}
		[NinjaScriptProperty]
		[Range(0.0f, 100f)]
		[Display(Name="Opacity", Order=10, GroupName="Volume Filter Style")]
		public float _AskTextOpacity
		{ get; set; }
		
		[XmlIgnore]
		[Display(Name="Bid text", Order=11, GroupName="Volume Filter Style")]
		public Brush _BidTextVolumeColor
		{ get; set; }
		[Browsable(false)]
		public string _BidTextVolumeColorSerializable
		{
			get { return Serialize.BrushToString(_BidTextVolumeColor); }
			set { _BidTextVolumeColor = Serialize.StringToBrush(value); }
		}
		[NinjaScriptProperty]
		[Range(0.0f, 100f)]
		[Display(Name="Opacity", Order=12, GroupName="Volume Filter Style")]
		public float _BidTextOpacity
		{ get; set; }
		
		[XmlIgnore]
		[Display(Name="Total text", Order=13, GroupName="Volume Filter Style")]
		public Brush _TotalTextVolumeColor
		{ get; set; }
		[Browsable(false)]
		public string _TotalTextVolumeColorColorSerializable
		{
			get { return Serialize.BrushToString(_TotalTextVolumeColor); }
			set { _TotalTextVolumeColor = Serialize.StringToBrush(value); }
		}
		[NinjaScriptProperty]
		[Range(0.0f, 100f)]
		[Display(Name="Total text opacity", Order=14, GroupName="Volume Filter Style")]
		public float _TotalTextOpacity
		{ get; set; }
		
		#endregion
	}
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private WyckoffZen.VolumeFilter[] cacheVolumeFilter;
		public WyckoffZen.VolumeFilter VolumeFilter(__VolumeFilterProperties.Formula _formula, long _minVolumeFilter, __VolumeFilterProperties.Geometry _geometry, bool _fillGeometry, int _geometryAggresiveLevel, float _outlineOpacity, SimpleFont _textFont, bool _showText, float _minFontWidth, float _minFontHeight, float _askTextOpacity, float _bidTextOpacity, float _totalTextOpacity)
		{
			return VolumeFilter(Input, _formula, _minVolumeFilter, _geometry, _fillGeometry, _geometryAggresiveLevel, _outlineOpacity, _textFont, _showText, _minFontWidth, _minFontHeight, _askTextOpacity, _bidTextOpacity, _totalTextOpacity);
		}

		public WyckoffZen.VolumeFilter VolumeFilter(ISeries<double> input, __VolumeFilterProperties.Formula _formula, long _minVolumeFilter, __VolumeFilterProperties.Geometry _geometry, bool _fillGeometry, int _geometryAggresiveLevel, float _outlineOpacity, SimpleFont _textFont, bool _showText, float _minFontWidth, float _minFontHeight, float _askTextOpacity, float _bidTextOpacity, float _totalTextOpacity)
		{
			if (cacheVolumeFilter != null)
				for (int idx = 0; idx < cacheVolumeFilter.Length; idx++)
					if (cacheVolumeFilter[idx] != null && cacheVolumeFilter[idx]._Formula == _formula && cacheVolumeFilter[idx]._MinVolumeFilter == _minVolumeFilter && cacheVolumeFilter[idx]._Geometry == _geometry && cacheVolumeFilter[idx]._FillGeometry == _fillGeometry && cacheVolumeFilter[idx]._GeometryAggresiveLevel == _geometryAggresiveLevel && cacheVolumeFilter[idx]._OutlineOpacity == _outlineOpacity && cacheVolumeFilter[idx]._TextFont == _textFont && cacheVolumeFilter[idx]._ShowText == _showText && cacheVolumeFilter[idx]._MinFontWidth == _minFontWidth && cacheVolumeFilter[idx]._MinFontHeight == _minFontHeight && cacheVolumeFilter[idx]._AskTextOpacity == _askTextOpacity && cacheVolumeFilter[idx]._BidTextOpacity == _bidTextOpacity && cacheVolumeFilter[idx]._TotalTextOpacity == _totalTextOpacity && cacheVolumeFilter[idx].EqualsInput(input))
						return cacheVolumeFilter[idx];
			return CacheIndicator<WyckoffZen.VolumeFilter>(new WyckoffZen.VolumeFilter(){ _Formula = _formula, _MinVolumeFilter = _minVolumeFilter, _Geometry = _geometry, _FillGeometry = _fillGeometry, _GeometryAggresiveLevel = _geometryAggresiveLevel, _OutlineOpacity = _outlineOpacity, _TextFont = _textFont, _ShowText = _showText, _MinFontWidth = _minFontWidth, _MinFontHeight = _minFontHeight, _AskTextOpacity = _askTextOpacity, _BidTextOpacity = _bidTextOpacity, _TotalTextOpacity = _totalTextOpacity }, input, ref cacheVolumeFilter);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.WyckoffZen.VolumeFilter VolumeFilter(__VolumeFilterProperties.Formula _formula, long _minVolumeFilter, __VolumeFilterProperties.Geometry _geometry, bool _fillGeometry, int _geometryAggresiveLevel, float _outlineOpacity, SimpleFont _textFont, bool _showText, float _minFontWidth, float _minFontHeight, float _askTextOpacity, float _bidTextOpacity, float _totalTextOpacity)
		{
			return indicator.VolumeFilter(Input, _formula, _minVolumeFilter, _geometry, _fillGeometry, _geometryAggresiveLevel, _outlineOpacity, _textFont, _showText, _minFontWidth, _minFontHeight, _askTextOpacity, _bidTextOpacity, _totalTextOpacity);
		}

		public Indicators.WyckoffZen.VolumeFilter VolumeFilter(ISeries<double> input , __VolumeFilterProperties.Formula _formula, long _minVolumeFilter, __VolumeFilterProperties.Geometry _geometry, bool _fillGeometry, int _geometryAggresiveLevel, float _outlineOpacity, SimpleFont _textFont, bool _showText, float _minFontWidth, float _minFontHeight, float _askTextOpacity, float _bidTextOpacity, float _totalTextOpacity)
		{
			return indicator.VolumeFilter(input, _formula, _minVolumeFilter, _geometry, _fillGeometry, _geometryAggresiveLevel, _outlineOpacity, _textFont, _showText, _minFontWidth, _minFontHeight, _askTextOpacity, _bidTextOpacity, _totalTextOpacity);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.WyckoffZen.VolumeFilter VolumeFilter(__VolumeFilterProperties.Formula _formula, long _minVolumeFilter, __VolumeFilterProperties.Geometry _geometry, bool _fillGeometry, int _geometryAggresiveLevel, float _outlineOpacity, SimpleFont _textFont, bool _showText, float _minFontWidth, float _minFontHeight, float _askTextOpacity, float _bidTextOpacity, float _totalTextOpacity)
		{
			return indicator.VolumeFilter(Input, _formula, _minVolumeFilter, _geometry, _fillGeometry, _geometryAggresiveLevel, _outlineOpacity, _textFont, _showText, _minFontWidth, _minFontHeight, _askTextOpacity, _bidTextOpacity, _totalTextOpacity);
		}

		public Indicators.WyckoffZen.VolumeFilter VolumeFilter(ISeries<double> input , __VolumeFilterProperties.Formula _formula, long _minVolumeFilter, __VolumeFilterProperties.Geometry _geometry, bool _fillGeometry, int _geometryAggresiveLevel, float _outlineOpacity, SimpleFont _textFont, bool _showText, float _minFontWidth, float _minFontHeight, float _askTextOpacity, float _bidTextOpacity, float _totalTextOpacity)
		{
			return indicator.VolumeFilter(input, _formula, _minVolumeFilter, _geometry, _fillGeometry, _geometryAggresiveLevel, _outlineOpacity, _textFont, _showText, _minFontWidth, _minFontHeight, _askTextOpacity, _bidTextOpacity, _totalTextOpacity);
		}
	}
}

#endregion
