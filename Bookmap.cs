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

public static class _BookMapEnums
{
	public enum LadderRange
	{
		Levels10,
		Levels30,
		Levels50,
		Levels70
	}
	public enum MarketOrdersCalculation
	{
		Delta,
		Total,
	}
	public enum MarketBarsCalculation
	{
		EachTick,
		Cummulative,
	}
	public enum MarketCummulativeCalculation
	{
		BidAndAsk,
		DeltaAndTotal
	}
}

namespace NinjaTrader.NinjaScript.Indicators.WyckoffZen
{
	public class Bookmap : Indicator
	{
		#region MAIN_CLASS
		
		public int getLadderRange(_BookMapEnums.LadderRange ladderRange)
		{
			switch(ladderRange)
			{
				//case _BookMapEnums.LadderRange.Default10Levels:
					//return 10;
				case _BookMapEnums.LadderRange.Levels30:
					return 30;
				case _BookMapEnums.LadderRange.Levels50:
					return 50;
				case _BookMapEnums.LadderRange.Levels70:
					return 70;
			}
			return 10;
		}
		
		public class WyckoffBookMap : WyckoffRenderControl
		{
			#region DEFS
			
			private VolumeAnalysis.BookMap bookMap;
			private VolumeAnalysis.WyckoffBars wyckoffBars;
			private VolumeAnalysis.WyckoffBars.Bar currentBar;
			private VolumeAnalysis.PriceLadder cummulativeMarketOrderLadder;
			private VolumeAnalysis.OrderBookLadder cummulativeOrderBookLadder;
			private VolumeAnalysis.MarketOrder maxMarketOL;
			private VolumeAnalysis.MarketOrder minMarketOL;
			
			private SharpDX.RectangleF ordersBookRect;			
			private SharpDX.RectangleF marketOrdersRect;
			private SharpDX.RectangleF cummulativeBookRect;
			private SharpDX.RectangleF backgroundRect;
			private SharpDX.Vector2 background_StVerticalVec;
			private SharpDX.Vector2 background_EndVerticalVec;
			
			private SharpDX.Direct2D1.Ellipse marketOrdersCircle;
			private SharpDX.RectangleF genRect;
			
			private SharpDX.Color colorBidPendingOrdersColor;
			private SharpDX.Color colorAskPendingOrdersColor;
			private SharpDX.Color colorBidMarketOrdersColor;
			private SharpDX.Color colorAskMarketOrdersColor;
			private SharpDX.Color colorBidPendingOrdersTextColor;
			private SharpDX.Color colorBidMarketOrdersTextColor;
			private SharpDX.Color colorAskPendingOrdersTextColor;
			private SharpDX.Color colorAskMarketOrdersTextColor;
			private SharpDX.Color colorTotalMarketOrdersColor;
			private SharpDX.Color colorTotalMarketOrdersFontColor;
			private SharpDX.Color colorBigPendingOrdersColor;
			private SharpDX.Color colorBidMarketCummulativeColor;
			private SharpDX.Color colorAskMarketCummulativeColor;
			private SharpDX.Color colorTotalMarketCummulativeColor;
			private SharpDX.Color colorBidMarketCummulativeTextColor;
			private SharpDX.Color colorAskMarketCummulativeTextColor;
			private SharpDX.Color colorTotalMarketCummulativeTextColor;
			private SharpDX.Color colorBidOrderBookColor;
			private SharpDX.Color colorAskOrderBookColor;
			private SharpDX.Color colorBidOrderBookTextColor;
			private SharpDX.Color colorAskOrderBookTextColor;
			private SharpDX.Color colorBackgroundColor;
			private SharpDX.Color colorVerticalLinesColor;
			
			private SharpDX.DirectWrite.TextFormat bookmapTextFormat, cummulativeTextFormat, orderbookTextFormat;
			private Action<float, float, Brush, float, float, float> renderGeometry;
			private Action<int, double, long> renderMarketOrder;
			private Action<double, VolumeAnalysis.MarketOrder, long> renderCummulative;
			private float filterPendingOrdersPer, filterTextPendingOrdersPer;
			private long filterAggresiveMarketOrders;
			private long filterBigPendingOrders;
			
			private float bidPendingOrdersTextOpacity;
			private float askPendingOrdersTextOpacity;
			private float bidMarketOrdersTextOpacity;
			private float askMarketOrdersTextOpacity;
			private float totalMarketOrdersTextOpacity;
			//private float askMarketOrdersOpacity;
			//private float bidMarketOrdersOpacity;
			private float bigPendingOrdersOpacity;
			private float bidOrderBookOpacity;
			private float askOrderBookOpacity;
			private float bidOrderBookTextOpacity;
			private float askOrderBookTextOpacity;
			private float backgroundOpacity;
//			private float verticalLinesOpacity;
			
			private float bidMarketCummulativeTextOpacity;
			private float askMarketCummulativeTextOpacity;
			private float totalMarketCummulativeTextOpacity;
			
			private float bookmapMinFontWidth;
			private float bookmapMinFontHeight;
			private float cummulativeBookMinFontWidth;
			private float cummulativeBookMinFontHeight;
			private float orderBookMinFontWidth;
			private float orderBookMinFontHeight;
			
			private float barX;
			private float barY;
			private double TickSize;
			private int marginRight;
			
			private bool showMarketOrdersText;
			private bool isRealtime;
			bool saveSession;
			bool loadSession;
			string sessionFilePath;
			
			#endregion
			
			public WyckoffBookMap()
			{
				this.genRect = new SharpDX.RectangleF();
				this.ordersBookRect = new SharpDX.RectangleF();
				this.marketOrdersRect= new SharpDX.RectangleF();
				this.cummulativeBookRect = new SharpDX.RectangleF();
				this.marketOrdersCircle = new SharpDX.Direct2D1.Ellipse();
				this.background_StVerticalVec = new SharpDX.Vector2();
				this.background_EndVerticalVec = new SharpDX.Vector2();
				this.backgroundRect = new SharpDX.RectangleF();
				this.maxMarketOL = new VolumeAnalysis.MarketOrder();
				this.minMarketOL = new VolumeAnalysis.MarketOrder();
				
				this.isRealtime = false;
			}
			
			#region BOOK_MAP_SETS
			
			public void setBookMap(VolumeAnalysis.BookMap bookMap)
			{
				this.bookMap = bookMap;
			}
			public void setWyckoffBars(VolumeAnalysis.WyckoffBars wyckoffBars)
			{
				this.wyckoffBars = wyckoffBars;
				this.TickSize = wyckoffBars.TickSize;
			}
			public void setCummulativeMarketOrdersLadder(VolumeAnalysis.PriceLadder cummulativeMarketOrderLadder)
			{
				this.cummulativeMarketOrderLadder= cummulativeMarketOrderLadder;
			}
			public void setOrderBookLadder(VolumeAnalysis.OrderBookLadder cummulativeOrderBookLadder)
			{
				this.cummulativeOrderBookLadder = cummulativeOrderBookLadder;
			}
			public void setDatabaseSession(bool saveSession, bool loadSession, string sessionFilePath)
			{
				this.saveSession= saveSession;
				this.loadSession= loadSession;
				this.sessionFilePath = sessionFilePath;
			}
			
			
			public void setBookmapFontStyle(SimpleFont font)
			{
				setFontStyle(font, out this.bookmapTextFormat);
			}
			public void setCummulativeBookFontStyle(SimpleFont font)
			{
				setFontStyle(font, out this.cummulativeTextFormat);
			}
			public void setOrderBookFontStyle(SimpleFont font)
			{
				setFontStyle(font, out this.orderbookTextFormat);
			}
			public void setBackgroundColor(Brush brushBackgroundColor, float opacity)
			{
				this.colorBackgroundColor = WyckoffRenderControl.BrushToColor(brushBackgroundColor);
				this.backgroundOpacity = opacity / 100.0f;
			}
			public void setVerticalLinesColor(Brush brushVerticalLinesColor)//, float opacity)
			{
				this.colorVerticalLinesColor = WyckoffRenderControl.BrushToColor(brushVerticalLinesColor);
				//this.verticalLinesOpacity = opacity;
			}
			public void setBidAskPendingOrdersColor(Brush brushBidPendingOrdersColor, Brush brushAskPendingOrdersColor)
			{
				this.colorBidPendingOrdersColor = WyckoffRenderControl.BrushToColor(brushBidPendingOrdersColor);
				this.colorAskPendingOrdersColor = WyckoffRenderControl.BrushToColor(brushAskPendingOrdersColor);
			}
			public void setBidAskPendingOrdersFontColor(
				Brush brushBidPendingOrdersTextColor, float bidOpacity,
				Brush brushAskPendingOrdersTextColor, float askOpacity)
			{
				this.colorBidPendingOrdersTextColor = WyckoffRenderControl.BrushToColor(brushBidPendingOrdersTextColor);
				this.bidPendingOrdersTextOpacity = bidOpacity / 100f;
				this.colorAskPendingOrdersTextColor = WyckoffRenderControl.BrushToColor(brushAskPendingOrdersTextColor);
				this.askPendingOrdersTextOpacity = askOpacity / 100f;
			}
			public void setBidAskMarketOrdersColor(Brush brushBidMarketOrdersColor, Brush brushAskMarketOrdersColor)
			{
				this.colorBidMarketOrdersColor = WyckoffRenderControl.BrushToColor(brushBidMarketOrdersColor);
				this.colorAskMarketOrdersColor = WyckoffRenderControl.BrushToColor(brushAskMarketOrdersColor);
			}
			public void setBidAskMarketOrdersFontColor(
				Brush brushBidMarketOrdersTextColor, float bidOpacity,
				Brush brushAskMarketOrdersTextColor, float askOpacity)
			{
				this.colorBidMarketOrdersTextColor = WyckoffRenderControl.BrushToColor(brushBidMarketOrdersTextColor);
				this.bidMarketOrdersTextOpacity = bidOpacity / 100f;
				this.colorAskMarketOrdersTextColor = WyckoffRenderControl.BrushToColor(brushAskMarketOrdersTextColor);
				this.askMarketOrdersTextOpacity = askOpacity / 100f;
			}
			public void setTotalMarketOrdersColor(Brush brushTotalMarketOrdersColor)
			{
				this.colorTotalMarketOrdersColor= WyckoffRenderControl.BrushToColor(brushTotalMarketOrdersColor);
			}
			public void setTotalMarketOrdersFontColor(Brush brushTotalMarketOrdersFontColor, float opacity)
			{
				this.colorTotalMarketOrdersFontColor = WyckoffRenderControl.BrushToColor(brushTotalMarketOrdersFontColor);
				this.totalMarketOrdersTextOpacity = opacity / 100.0f;
			}
			
			public void setBigPendingOrdersColor(Brush brushBigPendingOrdersColor, float opacity)
			{
				this.colorBigPendingOrdersColor = WyckoffRenderControl.BrushToColor(brushBigPendingOrdersColor);
				this.bigPendingOrdersOpacity = opacity / 100f;
			}
			public void setFilterPendingOrders(float filterPendingOrdersPer, float filterTextPendingOrdersPer)
			{
				this.filterPendingOrdersPer= filterPendingOrdersPer / 100f;
				this.filterTextPendingOrdersPer= filterTextPendingOrdersPer / 100f;
			}
			public void setFilterBigPendingOrders(long filterBigPendingOrders)
			{
				this.filterBigPendingOrders = filterBigPendingOrders;
			}
			public void setFilterAggresiveMarketOrders(long filterAggresiveMarketOrders)
			{
				this.filterAggresiveMarketOrders= filterAggresiveMarketOrders;
			}
			public void setMarginRight(int marginRight)
			{
				this.marginRight = marginRight;
			}
			public void setBookmapMinSizeFont(float minFontWidth, float minFontHeight)
			{
				this.bookmapMinFontWidth = minFontWidth;
				this.bookmapMinFontHeight = minFontHeight;
			}
			public void setOrderBookMinSizeFont(float minFontWidth, float minFontHeight)
			{
				this.orderBookMinFontWidth = minFontWidth;
				this.orderBookMinFontHeight= minFontHeight;
			}
			public void setCummulativeBookMinSizeFont(float minFontWidth, float minFontHeight)
			{
				this.cummulativeBookMinFontWidth = minFontWidth;
				this.cummulativeBookMinFontHeight= minFontHeight;
			}
			public void setShowMarketOrdersText(bool showText)
			{
				this.showMarketOrdersText = showText;
			}
			public void setRealtime(bool isRealtime)
			{
				this.isRealtime = isRealtime;
			}
			public bool IsRealtime
			{
				get{ return this.isRealtime; }
			}
			
			public void setMarketOrdersCalculation(
				_BookMapEnums.MarketOrdersCalculation marketOrdersCalculation,
				_BookMapEnums.MarketBarsCalculation marketBarsCalculation
				)
			{
				switch(marketOrdersCalculation)
				{
					case _BookMapEnums.MarketOrdersCalculation.Delta:
					{
						switch( marketBarsCalculation ){
							case _BookMapEnums.MarketBarsCalculation.EachTick:
							{
								this.renderMarketOrder = this.__renderDeltaMarketOrders;
								break;
							}
							case _BookMapEnums.MarketBarsCalculation.Cummulative:
							{
								this.renderMarketOrder = this.__renderDeltaMarketOrder;
								break;
							}
						}
						break;
					}
					case _BookMapEnums.MarketOrdersCalculation.Total:
					{
						switch( marketBarsCalculation ){
							case _BookMapEnums.MarketBarsCalculation.EachTick:
							{
								this.renderMarketOrder = __renderTotalMarketOrders;
								break;
							}
							case _BookMapEnums.MarketBarsCalculation.Cummulative:
							{
								this.renderMarketOrder = __renderTotalMarketOrder;
								break;
							}
						}
						break;
					}
				}
			}
			
			#endregion
			#region BOOK_CUMMULATIVE_STYLE_SETS
			
			public void setBidAskMarketCummulativeColor(Brush brushBidMarketCummulativeColor, Brush brushAskMarketCummulativeColor)
			{
				this.colorBidMarketCummulativeColor = WyckoffRenderControl.BrushToColor(brushBidMarketCummulativeColor);
				this.colorAskMarketCummulativeColor = WyckoffRenderControl.BrushToColor(brushAskMarketCummulativeColor);
			}
			public void setTotalMarketCummulativeColor(Brush brushTotalMarketCummulativeColor)
			{
				this.colorTotalMarketCummulativeColor = WyckoffRenderControl.BrushToColor(brushTotalMarketCummulativeColor);
			}
			public void setBidAskMarketCummulativeFontColor(
				Brush brushBidMarketCummulativeTextColor, float bidOpacity,
				Brush brushAskMarketCummulativeTextColor, float askOpacity,
				Brush brushTotalMarketCummulativeTextColor, float totalOpacity)
			{
				this.colorBidMarketCummulativeTextColor = WyckoffRenderControl.BrushToColor(brushBidMarketCummulativeTextColor);
				this.bidMarketCummulativeTextOpacity = bidOpacity / 100f;
				this.colorAskMarketCummulativeTextColor = WyckoffRenderControl.BrushToColor(brushAskMarketCummulativeTextColor);
				this.askMarketCummulativeTextOpacity = askOpacity / 100f;
				this.colorTotalMarketCummulativeTextColor = WyckoffRenderControl.BrushToColor(brushTotalMarketCummulativeTextColor);
				this.totalMarketCummulativeTextOpacity = totalOpacity / 100f;
			}
			
			#endregion
			#region ORDER_BOOK_STYLE_SETS
			
			public void setBidAskOrderBookColor(
				Brush brushBidOrderBookColor, float bidOpacity,
				Brush brushAskOrderBookColor, float askOpacity)
			{
				this.colorBidOrderBookColor = WyckoffRenderControl.BrushToColor(brushBidOrderBookColor);
				this.bidOrderBookOpacity = bidOpacity / 100f;
				this.colorAskOrderBookColor = WyckoffRenderControl.BrushToColor(brushAskOrderBookColor);
				this.askOrderBookOpacity = askOpacity / 100f;
			}
			public void setBidAskOrderBookFontColor(
					Brush brushBidOrderBookTextColor, float bidOpacity,
					Brush brushAskOrderBookTextColor, float askOpacity)
			{
				this.colorBidOrderBookTextColor = WyckoffRenderControl.BrushToColor(brushBidOrderBookTextColor);
				this.bidOrderBookTextOpacity = bidOpacity / 100f;
				this.colorAskOrderBookTextColor = WyckoffRenderControl.BrushToColor(brushAskOrderBookTextColor);
				this.askOrderBookTextOpacity = askOpacity / 100f;
			}
			
			#endregion
			#region RENDER_MARKET_ORDERS
			
			private void renderMarketBar(float X, float Y, SharpDX.Color color, long volume, float opacity)
			{
				if( volume >= this.filterAggresiveMarketOrders ){
					marketOrdersCircle.Point.X = X;
					marketOrdersCircle.Point.Y = Y;
					marketOrdersCircle.RadiusX = this.W;
					marketOrdersCircle.RadiusY = this.H;
					// !- multiplicando*10 el filtro de volumen configurado por el usuario garantizamos
					// que el porcentaje de opacidad parta de 100%(0,1f) en adelante para que el color sea maximo
					// deberia alcanzar con esta formula el 1000%
					float aggresivePer = (float)(Math2.Percent(this.filterAggresiveMarketOrders * 10, volume) / 100.0f);
					//Target.DrawEllipse(marketOrdersCircle, brush.ToDxBrush(Target, aggresivePer), 2.0f);
					//Target.FillEllipse(marketOrdersCircle, brush.ToDxBrush(Target, aggresivePer));
					
					myDrawEllipse(ref marketOrdersCircle, color, aggresivePer, 2.0f);
					myFillEllipse(ref marketOrdersCircle, color, aggresivePer);
					//return;
					// !- no dejamos que sobrepase el maximo para el rect
					//volume = this.filterAggresiveMarketOrders;
					return;
				}
				
				float per= (float)Math2.Percent(this.filterAggresiveMarketOrders, volume) / 100f;//volume / this.filterAggresiveMarketOrders;
				// !- setup y renderizado de las ordenes agresivas
				marketOrdersRect.X = X - (this.W / 2f) * per;
				marketOrdersRect.Y = Y - (this.H / 2f);
				marketOrdersRect.Width = this.W * per;
				marketOrdersRect.Height = this.H;
				
				myFillRectangle(ref marketOrdersRect, color, opacity);
				myDrawRectangle(ref marketOrdersRect, color, 0.7f, 2.0f);
			}
//			private void __renderCummulativeText(string volumeText, Brush brushColor, float brushOpacity)
//			{
//				if( this.W >= this.cummulativeBookMinFontWidth && this.H >= this.cummulativeBookMinFontHeight ){
//					Target.DrawText(volumeText,
//						this.cummulativeTextFormat, this.genRect,
//						brushColor.ToDxBrush(Target, brushOpacity));
//				}
//			}
			private void renderDeltaVolume(double price, long D, long maxOrderVolume)
			{
				long vol = Math.Abs(D);
				float priceOpacity = (float)Math2.Percent(maxOrderVolume, vol) / 100.0f;
				
				float barY = CHART_SCALE.GetYByValue(price);
				float h = this.H / 2f;
				this.genRect.Y = barY - h;
				if( D >= 0 ){
					this.renderMarketBar(this.barX, barY, colorAskMarketOrdersColor, vol, priceOpacity);
					if( this.showMarketOrdersText ){
						if( vol != 0 )
							this.myDrawText(vol.ToString(), ref this.genRect, colorAskMarketOrdersTextColor, bookmapMinFontWidth, bookmapMinFontHeight, bookmapTextFormat, askMarketOrdersTextOpacity);
					}
				}
				else{
					this.renderMarketBar(this.barX, barY, colorBidMarketOrdersColor, vol, priceOpacity);
					if( this.showMarketOrdersText ){
						if( vol != 0 )
							this.myDrawText(vol.ToString(), ref this.genRect, colorBidMarketOrdersTextColor, bookmapMinFontWidth, bookmapMinFontHeight, bookmapTextFormat, bidMarketOrdersTextOpacity);
					}
				}
			}
			private void renderTotalVolume(double price, long T, long maxOrderVolume)
			{
				float priceOpacity = (float)Math2.Percent(maxOrderVolume, T) / 100.0f;
				
				float barY = CHART_SCALE.GetYByValue(price);
				this.renderMarketBar(this.barX, barY, colorTotalMarketOrdersColor, T, priceOpacity);
				if( this.showMarketOrdersText ){
					this.genRect.Y = barY - this.H / 2f;
					this.myDrawText(T.ToString(), ref this.genRect, colorTotalMarketOrdersFontColor, bookmapMinFontWidth, bookmapMinFontHeight, bookmapTextFormat, totalMarketOrdersTextOpacity);
				}
			}
			private void __renderDeltaMarketOrders(int barIndex, double marketPrice, long maxOrderVolume)
			{
				//long D = this.wyckoffBars[barIndex].Delta; //this.wyckoffBars[barIndex].AtPrice(marketPrice).Delta;
				VolumeAnalysis.WyckoffBars.Bar wbar = this.wyckoffBars[barIndex];
				foreach(var bar in wbar){
					renderDeltaVolume(bar.Key, bar.Value.Delta, maxOrderVolume);
				}
			}
			private void __renderDeltaMarketOrder(int barIndex, double marketPrice, long maxOrderVolume)
			{
				renderDeltaVolume(marketPrice, this.wyckoffBars[barIndex].Delta, maxOrderVolume);
			}
			private void __renderTotalMarketOrder(int barIndex, double marketPrice, long maxOrderVolume)
			{
				renderTotalVolume(marketPrice,this.wyckoffBars[barIndex].Total, maxOrderVolume);
			}
			private void __renderTotalMarketOrders(int barIndex, double marketPrice, long maxOrderVolume)
			{
				VolumeAnalysis.WyckoffBars.Bar wbar = this.wyckoffBars[barIndex];
				foreach(var bar in wbar){
					renderTotalVolume(bar.Key, bar.Value.Total, maxOrderVolume);
				}
			}
			
			#endregion
			#region RENDER_CUMMULATIVE
			
			private void __renderCummulativeBidAndAsk(double price, VolumeAnalysis.MarketOrder marketOrderFlow, long maxMarketTotal)
			{
				long currBid = marketOrderFlow.Bid;
				long currAsk = marketOrderFlow.Ask;
				
				float maxVolumeTotalPercent = (float)Math2.Percent(maxMarketTotal, marketOrderFlow.Total) / 100f;
				float maxVolumeBidPercent = (float)Math2.Percent(maxMarketTotal, currBid) / 100f;
				float maxVolumeAskPercent = (float)Math2.Percent(maxMarketTotal, currAsk) / 100f;
				float half_margin_right = this.marginRight / 2f;
				
				// !- valores generales
				this.cummulativeBookRect.X = this.PanelW - half_margin_right + (this.W / 2f);
				this.cummulativeBookRect.Y = CHART_SCALE.GetYByValue(price) - (this.H / 2f);
				this.cummulativeBookRect.Height = this.H;
				this.genRect.Y = cummulativeBookRect.Y;
				this.genRect.Height = this.H;
				
				// !- render total
//				this.cummulativeBookRect.Width = half_margin_right * maxVolumeTotalPercent;
//				Target.FillRectangle(cummulativeBookRect, brushTotalMarketCummulativeColor.ToDxBrush(Target, maxVolumeTotalPercent));
				// !- ordenes de capa de dibujo, evitamos que se tapen
//				if( maxVolumeBidPercent >= maxVolumeAskPercent ){
				// !- render Bid
				this.cummulativeBookRect.Width = half_margin_right * maxVolumeBidPercent;
				/*Target.FillRectangle(cummulativeBookRect,
					colorBidMarketCummulativeColor.ToDxBrush(Target, maxVolumeBidPercent)
				);*/
				myFillRectangle(ref cummulativeBookRect, colorBidMarketCummulativeColor, maxVolumeBidPercent);
				// !- render Ask
				this.cummulativeBookRect.Width = half_margin_right * maxVolumeAskPercent;
				myFillRectangle(ref cummulativeBookRect, colorAskMarketCummulativeColor, maxVolumeBidPercent);
				
				if( this.W >= (half_margin_right - 45f) )
					return;
				/// !- render Bid text
				this.genRect.X = this.PanelW - this.marginRight + (this.W / 2f) + 3f;
				this.genRect.Width = this.marginRight + (this.W / 2f);
				this.myDrawText(currBid.ToString(), ref this.genRect, colorBidMarketCummulativeTextColor,
					this.cummulativeBookMinFontWidth, this.cummulativeBookMinFontHeight,
					this.cummulativeTextFormat, bidMarketCummulativeTextOpacity
				);
				/// !- render Ask text
				this.genRect.X = this.PanelW - this.marginRight + 50f;
				this.genRect.Width = this.marginRight + 50f;
				this.myDrawText(currAsk.ToString(), ref this.genRect, colorAskMarketCummulativeTextColor,
					this.cummulativeBookMinFontWidth, this.cummulativeBookMinFontHeight,
					this.cummulativeTextFormat, this.askMarketCummulativeTextOpacity
				);
			}
			private void __renderCummulativeDeltaAndTotal(double price, VolumeAnalysis.MarketOrder marketOrderFlow, long maxMarketTotal)
			{
				long currDelta = marketOrderFlow.Delta;
				long currTotal = marketOrderFlow.Total;
				//maxVolumePercent = (float)Math2.Percent(Math.Abs(maxMarketOF.Delta), Math.Abs(delta)) / 100f;
				float maxVolumeDeltaPercent = (float)Math2.Percent(maxMarketTotal, Math.Abs(currDelta)) / 100f;
				float maxVolumeTotalPercent = (float)Math2.Percent(maxMarketTotal, currTotal) / 100f;
				float half_margin_right = this.marginRight / 2f;
				bool isMaxMargin = this.W < (half_margin_right - 45f);
				
				// !- valores generales
				this.cummulativeBookRect.X = this.PanelW - half_margin_right + (this.W / 2f);
				this.cummulativeBookRect.Y = CHART_SCALE.GetYByValue(price) - (this.H / 2f);
				this.cummulativeBookRect.Height = this.H;
				this.genRect.Y = cummulativeBookRect.Y;
				this.genRect.Height = this.H;
				
				// !- render total
				this.cummulativeBookRect.Width = half_margin_right * maxVolumeTotalPercent;
				myFillRectangle(ref cummulativeBookRect, colorTotalMarketCummulativeColor, 0.5f);
				
				// !- render delta
				this.cummulativeBookRect.Width = half_margin_right * maxVolumeDeltaPercent;
				this.genRect.X = this.PanelW - this.marginRight + (this.W / 2f) + 5f;
				this.genRect.Width = this.marginRight + (this.W / 2f);
				if( currDelta >= 0 ){
					myFillRectangle(ref cummulativeBookRect, colorAskMarketCummulativeColor, 1.0f);
					if( isMaxMargin ){
						this.myDrawText(currDelta.ToString(), ref this.genRect, colorAskMarketCummulativeTextColor,
							this.cummulativeBookMinFontWidth, this.cummulativeBookMinFontHeight,
							this.cummulativeTextFormat, this.askMarketCummulativeTextOpacity
						);
					}
				}
				else{
					myFillRectangle(ref cummulativeBookRect, colorBidMarketCummulativeColor, 1.0f);
					if( isMaxMargin ){
						this.myDrawText(currDelta.ToString(), ref this.genRect, colorBidMarketCummulativeTextColor,
							this.cummulativeBookMinFontWidth, this.cummulativeBookMinFontHeight,
							this.cummulativeTextFormat, bidMarketCummulativeTextOpacity
						);
					}
				}
				if( isMaxMargin ){
					// !- render Total text
					this.genRect.X = this.PanelW - this.marginRight + 50f;
					this.genRect.Width = this.marginRight + 50f;
					this.myDrawText(currTotal.ToString(), ref this.genRect, this.colorTotalMarketCummulativeTextColor,
						this.cummulativeBookMinFontWidth, this.cummulativeBookMinFontHeight,
						this.cummulativeTextFormat, this.totalMarketCummulativeTextOpacity
					);
				}
			}
			public void setMarketCummulativeCalculation(_BookMapEnums.MarketCummulativeCalculation marketCummulativeCalculation)
			{
				switch( marketCummulativeCalculation )
				{
					case _BookMapEnums.MarketCummulativeCalculation.BidAndAsk:
					{
						this.renderCummulative = this.__renderCummulativeBidAndAsk;
						break;
					}
					case _BookMapEnums.MarketCummulativeCalculation.DeltaAndTotal:
					{
						this.renderCummulative = this.__renderCummulativeDeltaAndTotal;
						break;
					}
				}
			}
			
			#endregion
			
			private void renderPendingOrders(double price, long volume, VolumeAnalysis.OrderType orderType, float orderOpacity)
			{
				if( orderType == VolumeAnalysis.OrderType.Ask ){
					if( this.filterBigPendingOrders != 0 && volume >= this.filterBigPendingOrders ){
						//Target.FillRectangle(genRect, brushBigPendingOrdersColor.ToDxBrush(Target, (float)(Math2.Percent(this.filterBigPendingOrders*10, volume) / 100.0f)));
						myFillRectangle(ref genRect, colorBigPendingOrdersColor, (float)((volume / this.filterBigPendingOrders)*bigPendingOrdersOpacity));
					}
					else{
						myFillRectangle(ref genRect, colorAskPendingOrdersColor, orderOpacity);
					}
					// !- debemos saber el % para poder filtrar correctamente
					if( orderOpacity > this.filterTextPendingOrdersPer ){
						myDrawText(volume.ToString(), ref this.genRect, colorAskPendingOrdersTextColor, bookmapMinFontWidth, bookmapMinFontHeight, bookmapTextFormat, askPendingOrdersTextOpacity);
					}
				}
				else if( orderType == VolumeAnalysis.OrderType.Bid ){
					if( this.filterBigPendingOrders != 0 && volume >= this.filterBigPendingOrders ){
						myFillRectangle(ref genRect, colorBigPendingOrdersColor, (float)((volume / this.filterBigPendingOrders)*bigPendingOrdersOpacity));
					}
					else{
						myFillRectangle(ref genRect, colorBidPendingOrdersColor, orderOpacity);
					}
					// !- debemos saber el % para poder filtrar correctamente
					if( orderOpacity > this.filterTextPendingOrdersPer ){
						myDrawText(volume.ToString(), ref this.genRect, colorBidPendingOrdersTextColor, bookmapMinFontWidth, bookmapMinFontHeight, bookmapTextFormat, bidPendingOrdersTextOpacity);
					}
				}
			}
			public void renderOrdersLadder(int barIndex)
			{
				VolumeAnalysis.OrderBookLadder orderBookLadder = this.bookMap.getOrderBookLadder(barIndex);
				if( orderBookLadder == null )
					return;
				
				long maxPendingOrderVolume = orderBookLadder.MaxOrderVolume;
				this.barX = CHART_CONTROL.GetXByBarIndex(CHART_BARS, barIndex);
				this.genRect.X = this.barX - (this.W / 2f);
				this.genRect.Width = this.W;
				this.genRect.Height = this.H;
				
				VolumeAnalysis.OrderInfo orderInfo;
				double price;
				int bars = this.wyckoffBars.Count;
				foreach(var order in orderBookLadder)
				{
					price = order.Key;
					// !- no mostramos el nivel de la escalera de precios por donde el precio de mercado paso.
					if( price == orderBookLadder.MarketPrice ){
						// ! evitar error: The given key X was not present in the dictionary
						if( bars > barIndex)//if( this.wyckoffBars.BarExists(barIndex) )
							this.renderMarketOrder(barIndex, price, maxPendingOrderVolume);
						
						continue;
					}
					
					long volume = order.Value.Volume;
					float orderOpacity = (float)Math2.Percent(maxPendingOrderVolume, volume) / 100.0f;
					if( orderOpacity >= this.filterPendingOrdersPer ){
						this.genRect.Y = CHART_SCALE.GetYByValue(price) - this.H / 2f;
						this.renderPendingOrders(order.Key, volume, order.Value.Type, orderOpacity);
					}
				}
			}
			/// !- Mostramos el libro de ordenes
			public void renderOrderBookLadder()
			{
				if( cummulativeOrderBookLadder == null ){
					return;
				}
				
				double price;
				long maxOrder;
				float maxOrderPercent;
				VolumeAnalysis.OrderInfo orderInfo;
				float half_margin_right = this.marginRight / 2f;
				
				this.genRect.X = this.PanelW - this.marginRight + (this.W / 2f);
				this.genRect.Width = half_margin_right;
				this.genRect.Height = this.H;
				
				this.ordersBookRect.X = this.PanelW - this.marginRight + (this.W / 2f);
				this.ordersBookRect.Height = this.H;
				
				maxOrder = cummulativeOrderBookLadder.MaxOrderVolume;
				foreach(var order in cummulativeOrderBookLadder)
				{
					price = order.Key;
					orderInfo = order.Value;
					
					maxOrderPercent = (float)Math2.Percent(maxOrder, orderInfo.Volume) / 100f;
					ordersBookRect.Y = CHART_SCALE.GetYByValue(price) - (this.H / 2f);
					ordersBookRect.Width = half_margin_right * maxOrderPercent;
					
					this.genRect.Y = CHART_SCALE.GetYByValue(price) - (this.H / 2f);
					if(orderInfo.Type == VolumeAnalysis.OrderType.Ask ){
						myFillRectangle(ref ordersBookRect, colorAskOrderBookColor, bidOrderBookOpacity);
						myDrawRectangle(ref ordersBookRect, SharpDX.Color.Green, bidOrderBookOpacity, 1);
						
						myDrawText(orderInfo.Volume.ToString(), ref this.genRect, colorAskOrderBookTextColor, this.orderBookMinFontWidth, this.orderBookMinFontHeight, orderbookTextFormat, askOrderBookTextOpacity);
					}
					else{
						myFillRectangle(ref ordersBookRect, colorBidOrderBookColor, askOrderBookOpacity);
						myDrawRectangle(ref ordersBookRect, SharpDX.Color.Red, askOrderBookOpacity, 1);
						
						myDrawText(orderInfo.Volume.ToString(), ref this.genRect, colorBidOrderBookTextColor, this.orderBookMinFontWidth, this.orderBookMinFontHeight, orderbookTextFormat, bidOrderBookTextOpacity);
					}
				}
			}
			/// !- Mostramos el volume profile de la sesion
			public void renderCummulativeMarketOrderLadder()
			{
				if( cummulativeMarketOrderLadder == null ){
					return;
				}
				/// !- calculamos en tiempo real el maximo y minimo de la escalera
				this.cummulativeMarketOrderLadder.CalculateMinAndMax(ref minMarketOL, ref maxMarketOL);
				long maxMarketTotal= maxMarketOL.Total;
				foreach(var marketOrder in cummulativeMarketOrderLadder){
					this.renderCummulative(marketOrder.Key, marketOrder.Value, maxMarketTotal);
				}
			}
			public void renderBackground()
			{
				float halfW = this.W / 2f;
				float midline_x  = this.PanelW - this.marginRight + halfW;
				float backgroundH = System.Convert.ToSingle(CHART_SCALE.Height);
				
				backgroundRect.X = midline_x;
				backgroundRect.Y = 0;
				backgroundRect.Width = this.marginRight;
				backgroundRect.Height = System.Convert.ToSingle(this.PanelH);
				myFillRectangle(ref backgroundRect, colorBackgroundColor, backgroundOpacity);
				
				midline_x = this.PanelW - (this.marginRight/2f) + halfW;
				background_StVerticalVec.X = midline_x;
				background_EndVerticalVec.X = midline_x;
				background_StVerticalVec.Y = backgroundH;
				background_EndVerticalVec.Y = 0;
				myDrawLine(ref background_StVerticalVec, ref background_EndVerticalVec, colorVerticalLinesColor);
				
				midline_x = this.PanelW - this.marginRight + halfW;
				background_StVerticalVec.X = midline_x;
				background_EndVerticalVec.X = midline_x;
				background_StVerticalVec.Y = backgroundH;
				background_EndVerticalVec.Y = 0;
				myDrawLine(ref background_StVerticalVec, ref background_EndVerticalVec, colorVerticalLinesColor);
			}
		}
		
		#endregion
		#region GLOBAL_VARIABLES
		
		private WyckoffBookMap wyckoffBM;
		private VolumeAnalysis.WyckoffBars wyckoffBars;
		private VolumeAnalysis.BookMap bookMap;
		private VolumeAnalysis.PriceLadder marketOrderLadder;
		private VolumeAnalysis.OrderBookLadder orderBookLadder;
		
		#endregion
		#region INDICATOR_SETUP
		
		private void setBookMapDatabase()
		{
			_SaveSession = false;
			_SessionSaveFilePath = string.Empty;
			_SessionLoadFilePath = string.Empty;
			_FilterSessionPendingOrdersPer = 0;//100f;
		}
		private void setBookMapStyle()
		{
			_BookmapTextFont = new SimpleFont();
			_BookmapTextFont.Family = new FontFamily("Arial");
			_BookmapTextFont.Size = 10f;
			_BookmapTextFont.Bold = false;
			_BookmapTextFont.Italic= false;
			_BookmapMinFontWidth = 15f;
			_BookmapMinFontHeight = 15f;
			
			_BidPendingOrdersColor = Brushes.LightBlue;//Brushes.Brown;
			_AskPendingOrdersColor = Brushes.LightBlue;//Brushes.DarkSeaGreen;
			_BidPendingOrdersTextColor = Brushes.MidnightBlue;
			_AskPendingOrdersTextColor = Brushes.MidnightBlue;
			_BidPendingOrdersTextOpacity = 100f;
			_AskPendingOrdersTextOpacity = 100f;
			
			_BidMarketOrdersColor = Brushes.Maroon;
			_AskMarketOrdersColor = Brushes.SeaGreen;
			_BidMarketOrdersTextColor = Brushes.IndianRed;
			_AskMarketOrdersTextColor = Brushes.MediumAquamarine;
			_BigPendingOrdersColor = Brushes.OrangeRed;
			_BidMarketOrdersTextOpacity = 100f;
			_AskMarketOrdersTextOpacity = 100f;
			_BigPendingOrdersOpacity = 70f;
			
			_TotalMarketOrdersColor = Brushes.DarkGoldenrod;
			_TotalMarketOrdersTextColor = Brushes.WhiteSmoke;
			_TotalMarketOrdersTextOpacity = 70f;
			
			_BookMarginRight = 200;
			_ShowMarketOrdersText = true;
		}
		private void setBookMapCalculationsAndFilters()
		{
			// !- por defecto 50 niveles
			_LadderRange = _BookMapEnums.LadderRange.Levels50;
			_MarketBarsCalculation = _BookMapEnums.MarketBarsCalculation.EachTick;
			_MarketOrdersCalculation = _BookMapEnums.MarketOrdersCalculation.Delta;
			// !- mostrar ordenes desde % en adelante
			_FilterPendingOrdersPer = 0;
			_FilterTextPendingOrdersPer = 95;
			// !- filtro para grandes ordenes a mercado
			_AggresiveMarketOrdersFilter = 50;
			// !- 0 para ignorar el filtro de volumen
			_FilterBigPendingOrders = 600;
		}
		private void setCummulativeBookCalculations()
		{
			_MarketCummulativeCalculation = _BookMapEnums.MarketCummulativeCalculation.BidAndAsk;
		}
		private void setCummulativeBookStyle()
		{
			_CummulativeBookTextFont = new SimpleFont();
			_CummulativeBookTextFont.Family = new FontFamily("Arial");
			_CummulativeBookTextFont.Size = 10f;
			_CummulativeBookTextFont.Bold = false;
			_CummulativeBookTextFont.Italic= false;
			_CummulativeBookMinFontWidth = 14;
			_CummulativeBookMinFontHeight= 14;
			
			_BidMarketCummulativeColor = Brushes.Maroon;
			_AskMarketCummulativeColor = Brushes.SeaGreen;
			_BidMarketCummulativeTextColor = Brushes.LightPink;
			_AskMarketCummulativeTextColor = Brushes.LightYellow;
			_TotalMarketCummulativeColor = Brushes.SteelBlue;
			_TotalMarketCummulativeTextColor = Brushes.Goldenrod;
			_TotalMarketCummulativeTextOpacity = 100f;
			_BidMarketCummulativeTextOpacity = 100f;
			_AskMarketCummulativeTextOpacity = 100f;
		}
		private void setOrderBookStyle()
		{
			_OrderBookTextFont = new SimpleFont();
			_OrderBookTextFont.Family = new FontFamily("Arial");
			_OrderBookTextFont.Size = 10f;
			_OrderBookTextFont.Bold = true;
			_OrderBookTextFont.Italic= false;
			_OrderBookTextMinFontWidth = 14;
			_OrderBookTextMinFontHeight= 14;
			
			_BidOrderBookColor = Brushes.Maroon;
			_AskOrderBookColor = Brushes.SeaGreen;
			_BidOrderBookTextColor = Brushes.WhiteSmoke;
			_AskOrderBookTextColor = Brushes.WhiteSmoke;
			_BidOrderBookTextOpacity = 100f;
			_AskOrderBookTextOpacity = 100f;
			_BidOrderBookOpacity = 100f;
			_AskOrderBookOpacity = 100f;
		}
		private void setOrderBookBackground()
		{
			_BackgroundColor = Brushes.DarkSlateGray;
			_BackgroundColorOpacity = 20f;
			_VerticalLinesColor = Brushes.DarkSlateGray;
		}
		
		#endregion
		
		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description									= @"";
				Name										= "Book Map";
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
				
				setBookMapDatabase();
				setBookMapStyle();
				setBookMapCalculationsAndFilters();
				setCummulativeBookCalculations();
				setCummulativeBookStyle();
				setOrderBookStyle();
				setOrderBookBackground();
				
				marketOrderLadder = new VolumeAnalysis.PriceLadder();
				wyckoffBM = new WyckoffBookMap();
			}
			else if (State == State.Configure)
			{
				/// !- el orden no importa
				/// 
				wyckoffBM.setBookmapFontStyle(_BookmapTextFont);
				wyckoffBM.setBookmapMinSizeFont(_BookmapMinFontWidth, _BookmapMinFontHeight);
				
				wyckoffBM.setOrderBookFontStyle(_OrderBookTextFont);
				wyckoffBM.setOrderBookMinSizeFont(_OrderBookTextMinFontWidth, _OrderBookTextMinFontHeight);
				
				wyckoffBM.setCummulativeBookFontStyle(_CummulativeBookTextFont);
				wyckoffBM.setCummulativeBookMinSizeFont(_CummulativeBookMinFontWidth, _CummulativeBookMinFontHeight);
				
				wyckoffBM.setBidAskPendingOrdersColor(_BidPendingOrdersColor, _AskPendingOrdersColor);
				wyckoffBM.setBidAskPendingOrdersFontColor(
					_BidPendingOrdersTextColor, _BidPendingOrdersTextOpacity,
					_AskPendingOrdersTextColor, _AskPendingOrdersTextOpacity
				);
				wyckoffBM.setBidAskMarketOrdersColor(_BidMarketOrdersColor, _AskMarketOrdersColor);
				wyckoffBM.setBigPendingOrdersColor(_BigPendingOrdersColor, _BigPendingOrdersOpacity);
				wyckoffBM.setBidAskMarketOrdersFontColor(
					_BidMarketOrdersTextColor, _BidMarketOrdersTextOpacity,
					_AskMarketOrdersTextColor, _AskMarketOrdersTextOpacity
				);
				wyckoffBM.setShowMarketOrdersText(_ShowMarketOrdersText);
				wyckoffBM.setFilterPendingOrders(_FilterPendingOrdersPer, _FilterTextPendingOrdersPer);
				wyckoffBM.setFilterBigPendingOrders(_FilterBigPendingOrders);
				wyckoffBM.setFilterAggresiveMarketOrders(_AggresiveMarketOrdersFilter);
				
				wyckoffBM.setMarketOrdersCalculation(_MarketOrdersCalculation, _MarketBarsCalculation);
				wyckoffBM.setTotalMarketOrdersColor(_TotalMarketOrdersColor);
				wyckoffBM.setTotalMarketOrdersFontColor(_TotalMarketOrdersTextColor, _TotalMarketOrdersTextOpacity);
				
				wyckoffBM.setBidAskMarketCummulativeColor(_BidMarketCummulativeColor, _AskMarketCummulativeColor);
				wyckoffBM.setBidAskMarketCummulativeFontColor(
					_BidMarketCummulativeTextColor, _BidMarketCummulativeTextOpacity,
					_AskMarketCummulativeTextColor, _AskMarketCummulativeTextOpacity,
					_TotalMarketCummulativeTextColor, _TotalMarketCummulativeTextOpacity
				);
				wyckoffBM.setTotalMarketCummulativeColor(_TotalMarketCummulativeColor);
				wyckoffBM.setMarketCummulativeCalculation(_MarketCummulativeCalculation);
				wyckoffBM.setMarginRight(_BookMarginRight);
				
				wyckoffBM.setBidAskOrderBookColor(
					_BidOrderBookColor, _BidOrderBookOpacity,
					_AskOrderBookColor, _AskOrderBookOpacity
				);
				wyckoffBM.setBidAskOrderBookFontColor(
					_BidOrderBookTextColor, _BidOrderBookTextOpacity,
					_AskOrderBookTextColor, _AskOrderBookTextOpacity
				);
				wyckoffBM.setBackgroundColor(_BackgroundColor, _BackgroundColorOpacity);
				wyckoffBM.setVerticalLinesColor(_VerticalLinesColor);
				
				Calculate = Calculate.OnEachTick;
			}
			else if (State == State.DataLoaded)
			{
				int ladderRange = this.getLadderRange(_LadderRange);
				
				bookMap = new VolumeAnalysis.BookMap(Bars);
				// !- nivel maximo de la escalera de precios
				bookMap.setLadderRange(ladderRange);
				bookMap.setFilterSessionPercent(_FilterSessionPendingOrdersPer);
				if( this._SaveSession ){
					if( _SessionSaveFilePath.IsNullOrEmpty() ){
						// !- Desde la ultima barra cargada...
						DateTime tmp = this.Bars.LastBarTime;
						string sessionTime = tmp.Year.ToString() + '_' + tmp.Month.ToString() + '_' + tmp.Day.ToString();
						// !- siempre cargamos el nombre del archivo con la ruta asociada a la propiedad de NT8
						_SessionSaveFilePath =
							NinjaTrader.Core.Globals.UserDataDir + "session" + sessionTime + ".bm";//NinjaTrader.Core.Globals.UserDataDir + "session" + Bars.LastBarTime.ToString() + ".bm";
					}
					bookMap.SaveSessionFile(_SessionSaveFilePath);
				}
				if( !this._SessionLoadFilePath.IsNullOrEmpty() ){
					VolumeAnalysis.BookMap.SessionError err = bookMap.LoadSessionFile(_SessionLoadFilePath);
					Print(string.Format("[+] Session error:{0}", err));
				}
				wyckoffBM.setBookMap(bookMap);
				
				wyckoffBars = new VolumeAnalysis.WyckoffBars(Bars);
				wyckoffBM.setWyckoffBars(wyckoffBars);
				wyckoffBM.setCummulativeMarketOrdersLadder(marketOrderLadder);
				
				orderBookLadder = new VolumeAnalysis.OrderBookLadder(TickSize);
				// !- nivel maximo de la escalera de precios
				orderBookLadder.SetLadderRange(ladderRange);
				wyckoffBM.setOrderBookLadder(orderBookLadder);
				//Print( Bars.LastBarTime.CompareTo(new DateTime(2020,4,7,  1,0,0)) );//Bars.GetBar(new DateTime(2020,4,8,  2, 0, 44)));
			}
			else if (State == State.Realtime)
			{
				wyckoffBM.setRealtime(true);
			}
			else if(State == State.Terminated)
			{
				if( ChartControl != null )
					ChartControl.Properties.BarMarginRight = 0;
			}
		}
		protected override void OnRender(ChartControl chartControl, ChartScale chartScale)
		{
			base.OnRender(chartControl, chartScale);
			if( !wyckoffBM.IsRealtime || IsInHitTest == null || chartControl == null || ChartBars.Bars == null ){
				return;
			}
			// !- margen derecho de la pantalla
			chartControl.Properties.BarMarginRight = _BookMarginRight;
			
			// 1- Altura minima de un tick
			// 2- Ancho de barra en barra
			wyckoffBM.setHW(chartScale.GetPixelsForDistance(TickSize), chartControl.Properties.BarDistance);
			wyckoffBM.setChartPanelHW(ChartPanel.H, ChartPanel.W);
			// !- Apuntamos al target de renderizado
			wyckoffBM.setRenderTarget(chartControl, chartScale, ChartBars, RenderTarget);
			
			int fromIndex = ChartBars.FromIndex;
			int toIndex = ChartBars.ToIndex;
			//try{
				for (int index = fromIndex; index <= toIndex; index++){
					wyckoffBM.renderOrdersLadder(index);
				}
				
				wyckoffBM.renderOrderBookLadder();
				wyckoffBM.renderCummulativeMarketOrderLadder();
			//} catch{ /*Print("Exception ocurred");*/ }
			wyckoffBM.renderBackground();
		}
		// !- necesario para obtener el libro de ordenes(Level II)
		protected override void OnMarketDepth(MarketDepthEventArgs depthMarketArgs)
		{
			bookMap.onMarketDepth(depthMarketArgs);
			orderBookLadder.AddOrder(Bars.LastPrice, depthMarketArgs);
			// !- Renderizamos nuevamente
			ForceRefresh();
		}
		protected override void OnMarketData(MarketDataEventArgs MarketArgs){
			if( !wyckoffBars.onMarketData(MarketArgs) )
				return;
			
			marketOrderLadder.AddPrice(MarketArgs);
		}
		
		#region Properties
		
		/// !- Book Map DB
		
		[NinjaScriptProperty]
		[Display(Name="Session save file path", Order=0, GroupName="Book Map Database")]
		public string _SessionSaveFilePath { get; set; } 
		
		[NinjaScriptProperty]
		[Display(Name="Session load file path", Order=1, GroupName="Book Map Database")]
		[PropertyEditor ("NinjaTrader.Gui.Tools.FilePathPicker" , Filter= "Any Files (*.bm)|*.bm" )]
		public string _SessionLoadFilePath { get; set; } 
		
		[NinjaScriptProperty]
		[Display(Name="Realtime save session", Order=2, GroupName="Book Map Database")]
		public bool _SaveSession
		{ get; set; }
		
		[NinjaScriptProperty]
		[Range(0.0f, 100f)]
		[Display(Name="Load pending orders from(%)", Order=4, GroupName="Book Map Database")]
		public float _FilterSessionPendingOrdersPer
		{ get; set; }
		
		/// !- Book Map Calculation
		
		[NinjaScriptProperty]
		[Display(Name = "Ladder range", Order = 0, GroupName = "Book Map Calculation")]
		public _BookMapEnums.LadderRange _LadderRange
		{ get; set; }
		
		[NinjaScriptProperty]
		[Display(Name = "Market orders calculation", Order = 1, GroupName = "Book Map Calculation")]
		public _BookMapEnums.MarketOrdersCalculation _MarketOrdersCalculation
		{ get; set; }
		
		[NinjaScriptProperty]
		[Display(Name = "Market bars calculation", Order = 1, GroupName = "Book Map Calculation")]
		public _BookMapEnums.MarketBarsCalculation _MarketBarsCalculation
		{ get; set; }
		
		/// !- Book Map Filters
		
		[NinjaScriptProperty]
		[Range(0, long.MaxValue)]
		[Display(Name = "Aggresive market orders(0 to ignore)", Order = 0, GroupName = "Book Map Filters")]
		public long _AggresiveMarketOrdersFilter
		{ get; set; }
		
		[NinjaScriptProperty]
		[Range(0, long.MaxValue)]
		[Display(Name="Big pending orders(0 to ignore)", Order=1, GroupName="Book Map Filters")]
		public long _FilterBigPendingOrders
		{ get; set; }
		
		[NinjaScriptProperty]
		[Range(0.0f, 100f)]
		[Display(Name="Show pending orders from(%)", Order=2, GroupName="Book Map Filters")]
		public float _FilterPendingOrdersPer
		{ get; set; }
		
		[NinjaScriptProperty]
		[Range(0.0f, 100f)]
		[Display(Name="Show text pending orders from(%)", Order=3, GroupName="Book Map Filters")]
		public float _FilterTextPendingOrdersPer
		{ get; set; }
		
		[NinjaScriptProperty]
		[Display(Name="Show market orders text", Order=4, GroupName="Book Map Style")]
		public bool _ShowMarketOrdersText
		{ get; set; }
		
		/// !- Book Map Style
		
		[NinjaScriptProperty]
		[Range(120, 600)]
		[Display(Name = "Book margin right", Order = 0, GroupName = "Book Map Style")]
		public int _BookMarginRight
		{ get; set; }
		
		[XmlIgnore]
		[Display(Name="Background", Order=1, GroupName="Book Map Style")]
		public Brush _BackgroundColor
		{ get; set; }
		[Browsable(false)]
		public string _BackgroundColorSerializable
		{
			get { return Serialize.BrushToString(_BackgroundColor); }
			set { _BackgroundColor = Serialize.StringToBrush(value); }
		}
		[NinjaScriptProperty]
		[Range(0.0f, 100f)]
		[Display(Name="Background opacity(%)", Order=2, GroupName="Book Map Style")]
		public float _BackgroundColorOpacity
		{ get; set; }
		
		[XmlIgnore]
		[Display(Name="Vertical lines", Order=3, GroupName="Book Map Style")]
		public Brush _VerticalLinesColor
		{ get; set; }
		[Browsable(false)]
		public string _VerticalLinesColorSerializable
		{
			get { return Serialize.BrushToString(_VerticalLinesColor); }
			set { _VerticalLinesColor = Serialize.StringToBrush(value); }
		}
		
		[XmlIgnore]
		[Display(Name="Bid pending orders", Order=4, GroupName="Book Map Style")]
		public Brush _BidPendingOrdersColor
		{ get; set; }
		[Browsable(false)]
		public string _BidPendingOrdersColorSerializable
		{
			get { return Serialize.BrushToString(_BidPendingOrdersColor); }
			set { _BidPendingOrdersColor = Serialize.StringToBrush(value); }
		}
		[XmlIgnore]
		[Display(Name="Bid market orders", Order=5, GroupName="Book Map Style")]
		public Brush _BidMarketOrdersColor
		{ get; set; }
		[Browsable(false)]
		public string _BidMarketOrdersColorSerializable
		{
			get { return Serialize.BrushToString(_BidMarketOrdersColor); }
			set { _BidMarketOrdersColor = Serialize.StringToBrush(value); }
		}
		
		[XmlIgnore]
		[Display(Name="Ask pending orders", Order=6, GroupName="Book Map Style")]
		public Brush _AskPendingOrdersColor
		{ get; set; }
		[Browsable(false)]
		public string _AskPendingOrdersColorSerializable
		{
			get { return Serialize.BrushToString(_AskPendingOrdersColor); }
			set { _AskPendingOrdersColor = Serialize.StringToBrush(value); }
		}
		[XmlIgnore]
		[Display(Name="Ask market orders", Order=7, GroupName="Book Map Style")]
		public Brush _AskMarketOrdersColor
		{ get; set; }
		[Browsable(false)]
		public string _AskMarketOrdersColorSerializable
		{
			get { return Serialize.BrushToString(_AskMarketOrdersColor); }
			set { _AskMarketOrdersColor = Serialize.StringToBrush(value); }
		}
		
		[XmlIgnore]
		[Display(Name="Total market orders", Order=8, GroupName="Book Map Style")]
		public Brush _TotalMarketOrdersColor
		{ get; set; }
		[Browsable(false)]
		public string _TotalMarketOrdersColorSerializable
		{
			get { return Serialize.BrushToString(_TotalMarketOrdersColor); }
			set { _TotalMarketOrdersColor = Serialize.StringToBrush(value); }
		}
		
		[XmlIgnore]
		[Display(Name="Big pending orders", Order=9, GroupName="Book Map Style")]
		public Brush _BigPendingOrdersColor
		{ get; set; }
		[Browsable(false)]
		public string _BigPendingOrdersColorSerializable
		{
			get { return Serialize.BrushToString(_BigPendingOrdersColor); }
			set { _BigPendingOrdersColor = Serialize.StringToBrush(value); }
		}
		[NinjaScriptProperty]
		[Range(0.0f, 100f)]
		[Display(Name="Big pending opacity(%)", Order=10, GroupName="Book Map Style")]
		public float _BigPendingOrdersOpacity
		{ get; set; }
		
		[NinjaScriptProperty]
		[Display(Name="Font", Order=11, GroupName="Book Map Style")]
		public SimpleFont _BookmapTextFont
		{ get; set; }
		[NinjaScriptProperty]
		[Range(1.0f, float.MaxValue)]
		[Display(Name="Min font width", Order=12, GroupName="Book Map Style")]
		public float _BookmapMinFontWidth
		{ get; set; }
		[NinjaScriptProperty]
		[Range(1.0f, float.MaxValue)]
		[Display(Name="Min font height", Order=13, GroupName="Book Map Style")]
		public float _BookmapMinFontHeight
		{ get; set; }
		
		[XmlIgnore]
		[Display(Name="Bid pending orders text", Order=14, GroupName="Book Map Style")]
		public Brush _BidPendingOrdersTextColor
		{ get; set; }
		[Browsable(false)]
		public string _BidPendingOrdersTextColorSerializable
		{
			get { return Serialize.BrushToString(_BidPendingOrdersTextColor); }
			set { _BidPendingOrdersTextColor = Serialize.StringToBrush(value); }
		}
		[NinjaScriptProperty]
		[Range(0.0f, 100f)]
		[Display(Name="Bid pending orders opacity(%)", Order=15, GroupName="Book Map Style")]
		public float _BidPendingOrdersTextOpacity
		{ get; set; }
		
		[XmlIgnore]
		[Display(Name="Ask pending orders text", Order=16, GroupName="Book Map Style")]
		public Brush _AskPendingOrdersTextColor
		{ get; set; }
		[Browsable(false)]
		public string _AskOrdersTextColorSerializable
		{
			get { return Serialize.BrushToString(_AskPendingOrdersTextColor); }
			set { _AskPendingOrdersTextColor = Serialize.StringToBrush(value); }
		}
		[NinjaScriptProperty]
		[Range(0.0f, 100f)]
		[Display(Name="Ask pending orders opacity(%)", Order=17, GroupName="Book Map Style")]
		public float _AskPendingOrdersTextOpacity
		{ get; set; }
		
		[XmlIgnore]
		[Display(Name="Bid market orders text", Order=18, GroupName="Book Map Style")]
		public Brush _BidMarketOrdersTextColor
		{ get; set; }
		[Browsable(false)]
		public string _BidMarketOrdersTextColorSerializable
		{
			get { return Serialize.BrushToString(_BidMarketOrdersTextColor); }
			set { _BidMarketOrdersTextColor = Serialize.StringToBrush(value); }
		}
		[NinjaScriptProperty]
		[Range(0.0f, 100f)]
		[Display(Name="Bid market orders opacity(%)", Order=19, GroupName="Book Map Style")]
		public float _BidMarketOrdersTextOpacity
		{ get; set; }
		
		[XmlIgnore]
		[Display(Name="Ask market orders text", Order=20, GroupName="Book Map Style")]
		public Brush _AskMarketOrdersTextColor
		{ get; set; }
		[Browsable(false)]
		public string _AskMarketOrdersTextColorSerializable
		{
			get { return Serialize.BrushToString(_AskMarketOrdersTextColor); }
			set { _AskMarketOrdersTextColor = Serialize.StringToBrush(value); }
		}
		[NinjaScriptProperty]
		[Range(0.0f, 100f)]
		[Display(Name="Ask market orders opacity(%)", Order=21, GroupName="Book Map Style")]
		public float _AskMarketOrdersTextOpacity
		{ get; set; }
		
		[XmlIgnore]
		[Display(Name="Total market orders text", Order=22, GroupName="Book Map Style")]
		public Brush _TotalMarketOrdersTextColor
		{ get; set; }
		[Browsable(false)]
		public string _TotalMarketOrdersTextColorSerializable
		{
			get { return Serialize.BrushToString(_TotalMarketOrdersTextColor); }
			set { _TotalMarketOrdersTextColor = Serialize.StringToBrush(value); }
		}
		[NinjaScriptProperty]
		[Range(0.0f, 100f)]
		[Display(Name="Total market orders opacity(%)", Order=23, GroupName="Book Map Style")]
		public float _TotalMarketOrdersTextOpacity
		{ get; set; }
		
		/// !- Book Cummulative Style
		
		[NinjaScriptProperty]
		[Display(Name = "Market Cummulative Calculation", Order = 0, GroupName = "Cummulative Book Calculation")]
		public _BookMapEnums.MarketCummulativeCalculation _MarketCummulativeCalculation
		{ get; set; }
		
//		[NinjaScriptProperty]
//		[Display(Name="Show Cummulative Book", Order=1, GroupName="Cummulative Book Calculation")]
//		public bool _ShowBookCummulative
//		{ get; set; }
		
		/// !- Book Cummulative Style
		
		[XmlIgnore]
		[Display(Name="Bid market orders", Order=1, GroupName="Cummulative Book Style")]
		public Brush _BidMarketCummulativeColor
		{ get; set; }
		[Browsable(false)]
		public string _BidMarketCummulativeColorSerializable
		{
			get { return Serialize.BrushToString(_BidMarketCummulativeColor); }
			set { _BidMarketCummulativeColor = Serialize.StringToBrush(value); }
		}
		
		[XmlIgnore]
		[Display(Name="Ask market orders", Order=2, GroupName="Cummulative Book Style")]
		public Brush _AskMarketCummulativeColor
		{ get; set; }
		[Browsable(false)]
		public string _AskMarketCummulativeColorSerializable
		{
			get { return Serialize.BrushToString(_AskMarketCummulativeColor); }
			set { _AskMarketCummulativeColor = Serialize.StringToBrush(value); }
		}
		
		[XmlIgnore]
		[Display(Name="Total market orders", Order=3, GroupName="Cummulative Book Style")]
		public Brush _TotalMarketCummulativeColor
		{ get; set; }
		[Browsable(false)]
		public string _TotalMarketCummulativeColorSerializable
		{
			get { return Serialize.BrushToString(_TotalMarketCummulativeColor); }
			set { _TotalMarketCummulativeColor = Serialize.StringToBrush(value); }
		}
		
		[NinjaScriptProperty]
		[Display(Name="Font", Order=4, GroupName="Cummulative Book Style")]
		public SimpleFont _CummulativeBookTextFont
		{ get; set; }
		[NinjaScriptProperty]
		[Range(1.0f, float.MaxValue)]
		[Display(Name="Min font width", Order=5, GroupName="Cummulative Book Style")]
		public float _CummulativeBookMinFontWidth
		{ get; set; }
		[NinjaScriptProperty]
		[Range(1.0f, float.MaxValue)]
		[Display(Name="Min font height", Order=6, GroupName="Cummulative Book Style")]
		public float _CummulativeBookMinFontHeight
		{ get; set; }
		
		[XmlIgnore]
		[Display(Name="Bid text", Order=7, GroupName="Cummulative Book Style")]
		public Brush _BidMarketCummulativeTextColor
		{ get; set; }
		[Browsable(false)]
		public string _BidMarketCummulativeTextColorSerializable
		{
			get { return Serialize.BrushToString(_BidMarketCummulativeTextColor); }
			set { _BidMarketCummulativeTextColor = Serialize.StringToBrush(value); }
		}
		[NinjaScriptProperty]
		[Range(0.0f, 100f)]
		[Display(Name="Bid text opacity(%)", Order=8, GroupName="Cummulative Book Style")]
		public float _BidMarketCummulativeTextOpacity
		{ get; set; }
		
		[XmlIgnore]
		[Display(Name="Ask text", Order=9, GroupName="Cummulative Book Style")]
		public Brush _AskMarketCummulativeTextColor
		{ get; set; }
		[Browsable(false)]
		public string _AskMarketCummulativeTextColorSerializable
		{
			get { return Serialize.BrushToString(_AskMarketCummulativeTextColor); }
			set { _AskMarketCummulativeTextColor = Serialize.StringToBrush(value); }
		}
		[NinjaScriptProperty]
		[Range(0.0f, 100f)]
		[Display(Name="Ask text opacity(%)", Order=10, GroupName="Cummulative Book Style")]
		public float _AskMarketCummulativeTextOpacity
		{ get; set; }
		
		[XmlIgnore]
		[Display(Name="Total text", Order=11, GroupName="Cummulative Book Style")]
		public Brush _TotalMarketCummulativeTextColor
		{ get; set; }
		[Browsable(false)]
		public string _TotalMarketCummulativeTextColorSerializable
		{
			get { return Serialize.BrushToString(_TotalMarketCummulativeTextColor); }
			set { _TotalMarketCummulativeTextColor = Serialize.StringToBrush(value); }
		}
		[NinjaScriptProperty]
		[Range(0.0f, 100f)]
		[Display(Name="Total text opacity(%)", Order=12, GroupName="Cummulative Book Style")]
		public float _TotalMarketCummulativeTextOpacity
		{ get; set; }
		
		/// !- Order book style
		/// 
		[XmlIgnore]
		[Display(Name="Bid orders", Order=0, GroupName="Order Book Style")]
		public Brush _BidOrderBookColor
		{ get; set; }
		[Browsable(false)]
		public string _BidOrderBookColorSerializable
		{
			get { return Serialize.BrushToString(_BidOrderBookColor); }
			set { _BidOrderBookColor = Serialize.StringToBrush(value); }
		}
		[NinjaScriptProperty]
		[Range(0.0f, 100f)]
		[Display(Name="Bid orders opacity(%)", Order=1, GroupName="Order Book Style")]
		public float _BidOrderBookOpacity
		{ get; set; }
		
		[XmlIgnore]
		[Display(Name="Ask orders", Order=2, GroupName="Order Book Style")]
		public Brush _AskOrderBookColor
		{ get; set; }
		[Browsable(false)]
		public string _AskOrderBookColorSerializable
		{
			get { return Serialize.BrushToString(_AskOrderBookColor); }
			set { _AskOrderBookColor = Serialize.StringToBrush(value); }
		}
		[NinjaScriptProperty]
		[Range(0.0f, 100f)]
		[Display(Name="Ask orders opacity(%)", Order=3, GroupName="Order Book Style")]
		public float _AskOrderBookOpacity
		{ get; set; }
		
		[NinjaScriptProperty]
		[Display(Name="Font", Order=4, GroupName="Order Book Style")]
		public SimpleFont _OrderBookTextFont
		{ get; set; }
		[NinjaScriptProperty]
		[Range(1.0f, float.MaxValue)]
		[Display(Name="Min font width", Order=5, GroupName="Order Book Style")]
		public float _OrderBookTextMinFontWidth
		{ get; set; }
		[NinjaScriptProperty]
		[Range(1.0f, float.MaxValue)]
		[Display(Name="Min font height", Order=6, GroupName="Order Book Style")]
		public float _OrderBookTextMinFontHeight
		{ get; set; }
		
		[XmlIgnore]
		[Display(Name="Bid text", Order=7, GroupName="Order Book Style")]
		public Brush _BidOrderBookTextColor
		{ get; set; }
		[Browsable(false)]
		public string _BidOrderBookTextColorSerializable
		{
			get { return Serialize.BrushToString(_BidOrderBookTextColor); }
			set { _BidOrderBookTextColor = Serialize.StringToBrush(value); }
		}
		[NinjaScriptProperty]
		[Range(0.0f, 100f)]
		[Display(Name="Bid text opacity(%)", Order=8, GroupName="Order Book Style")]
		public float _BidOrderBookTextOpacity
		{ get; set; }
		
		[XmlIgnore]
		[Display(Name="Ask text", Order=9, GroupName="Order Book Style")]
		public Brush _AskOrderBookTextColor
		{ get; set; }
		[Browsable(false)]
		public string _AskOrderBookTextColorSerializable
		{
			get { return Serialize.BrushToString(_AskOrderBookTextColor); }
			set { _AskOrderBookTextColor = Serialize.StringToBrush(value); }
		}
		[NinjaScriptProperty]
		[Range(0.0f, 100f)]
		[Display(Name="Ask text opacity(%)", Order=10, GroupName="Order Book Style")]
		public float _AskOrderBookTextOpacity
		{ get; set; }
		
		
		#endregion
	}
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private WyckoffZen.Bookmap[] cacheBookmap;
		public WyckoffZen.Bookmap Bookmap(string _sessionSaveFilePath, string _sessionLoadFilePath, bool _saveSession, float _filterSessionPendingOrdersPer, _BookMapEnums.LadderRange _ladderRange, _BookMapEnums.MarketOrdersCalculation _marketOrdersCalculation, _BookMapEnums.MarketBarsCalculation _marketBarsCalculation, long _aggresiveMarketOrdersFilter, long _filterBigPendingOrders, float _filterPendingOrdersPer, float _filterTextPendingOrdersPer, bool _showMarketOrdersText, int _bookMarginRight, float _backgroundColorOpacity, float _bigPendingOrdersOpacity, SimpleFont _bookmapTextFont, float _bookmapMinFontWidth, float _bookmapMinFontHeight, float _bidPendingOrdersTextOpacity, float _askPendingOrdersTextOpacity, float _bidMarketOrdersTextOpacity, float _askMarketOrdersTextOpacity, float _totalMarketOrdersTextOpacity, _BookMapEnums.MarketCummulativeCalculation _marketCummulativeCalculation, SimpleFont _cummulativeBookTextFont, float _cummulativeBookMinFontWidth, float _cummulativeBookMinFontHeight, float _bidMarketCummulativeTextOpacity, float _askMarketCummulativeTextOpacity, float _totalMarketCummulativeTextOpacity, float _bidOrderBookOpacity, float _askOrderBookOpacity, SimpleFont _orderBookTextFont, float _orderBookTextMinFontWidth, float _orderBookTextMinFontHeight, float _bidOrderBookTextOpacity, float _askOrderBookTextOpacity)
		{
			return Bookmap(Input, _sessionSaveFilePath, _sessionLoadFilePath, _saveSession, _filterSessionPendingOrdersPer, _ladderRange, _marketOrdersCalculation, _marketBarsCalculation, _aggresiveMarketOrdersFilter, _filterBigPendingOrders, _filterPendingOrdersPer, _filterTextPendingOrdersPer, _showMarketOrdersText, _bookMarginRight, _backgroundColorOpacity, _bigPendingOrdersOpacity, _bookmapTextFont, _bookmapMinFontWidth, _bookmapMinFontHeight, _bidPendingOrdersTextOpacity, _askPendingOrdersTextOpacity, _bidMarketOrdersTextOpacity, _askMarketOrdersTextOpacity, _totalMarketOrdersTextOpacity, _marketCummulativeCalculation, _cummulativeBookTextFont, _cummulativeBookMinFontWidth, _cummulativeBookMinFontHeight, _bidMarketCummulativeTextOpacity, _askMarketCummulativeTextOpacity, _totalMarketCummulativeTextOpacity, _bidOrderBookOpacity, _askOrderBookOpacity, _orderBookTextFont, _orderBookTextMinFontWidth, _orderBookTextMinFontHeight, _bidOrderBookTextOpacity, _askOrderBookTextOpacity);
		}

		public WyckoffZen.Bookmap Bookmap(ISeries<double> input, string _sessionSaveFilePath, string _sessionLoadFilePath, bool _saveSession, float _filterSessionPendingOrdersPer, _BookMapEnums.LadderRange _ladderRange, _BookMapEnums.MarketOrdersCalculation _marketOrdersCalculation, _BookMapEnums.MarketBarsCalculation _marketBarsCalculation, long _aggresiveMarketOrdersFilter, long _filterBigPendingOrders, float _filterPendingOrdersPer, float _filterTextPendingOrdersPer, bool _showMarketOrdersText, int _bookMarginRight, float _backgroundColorOpacity, float _bigPendingOrdersOpacity, SimpleFont _bookmapTextFont, float _bookmapMinFontWidth, float _bookmapMinFontHeight, float _bidPendingOrdersTextOpacity, float _askPendingOrdersTextOpacity, float _bidMarketOrdersTextOpacity, float _askMarketOrdersTextOpacity, float _totalMarketOrdersTextOpacity, _BookMapEnums.MarketCummulativeCalculation _marketCummulativeCalculation, SimpleFont _cummulativeBookTextFont, float _cummulativeBookMinFontWidth, float _cummulativeBookMinFontHeight, float _bidMarketCummulativeTextOpacity, float _askMarketCummulativeTextOpacity, float _totalMarketCummulativeTextOpacity, float _bidOrderBookOpacity, float _askOrderBookOpacity, SimpleFont _orderBookTextFont, float _orderBookTextMinFontWidth, float _orderBookTextMinFontHeight, float _bidOrderBookTextOpacity, float _askOrderBookTextOpacity)
		{
			if (cacheBookmap != null)
				for (int idx = 0; idx < cacheBookmap.Length; idx++)
					if (cacheBookmap[idx] != null && cacheBookmap[idx]._SessionSaveFilePath == _sessionSaveFilePath && cacheBookmap[idx]._SessionLoadFilePath == _sessionLoadFilePath && cacheBookmap[idx]._SaveSession == _saveSession && cacheBookmap[idx]._FilterSessionPendingOrdersPer == _filterSessionPendingOrdersPer && cacheBookmap[idx]._LadderRange == _ladderRange && cacheBookmap[idx]._MarketOrdersCalculation == _marketOrdersCalculation && cacheBookmap[idx]._MarketBarsCalculation == _marketBarsCalculation && cacheBookmap[idx]._AggresiveMarketOrdersFilter == _aggresiveMarketOrdersFilter && cacheBookmap[idx]._FilterBigPendingOrders == _filterBigPendingOrders && cacheBookmap[idx]._FilterPendingOrdersPer == _filterPendingOrdersPer && cacheBookmap[idx]._FilterTextPendingOrdersPer == _filterTextPendingOrdersPer && cacheBookmap[idx]._ShowMarketOrdersText == _showMarketOrdersText && cacheBookmap[idx]._BookMarginRight == _bookMarginRight && cacheBookmap[idx]._BackgroundColorOpacity == _backgroundColorOpacity && cacheBookmap[idx]._BigPendingOrdersOpacity == _bigPendingOrdersOpacity && cacheBookmap[idx]._BookmapTextFont == _bookmapTextFont && cacheBookmap[idx]._BookmapMinFontWidth == _bookmapMinFontWidth && cacheBookmap[idx]._BookmapMinFontHeight == _bookmapMinFontHeight && cacheBookmap[idx]._BidPendingOrdersTextOpacity == _bidPendingOrdersTextOpacity && cacheBookmap[idx]._AskPendingOrdersTextOpacity == _askPendingOrdersTextOpacity && cacheBookmap[idx]._BidMarketOrdersTextOpacity == _bidMarketOrdersTextOpacity && cacheBookmap[idx]._AskMarketOrdersTextOpacity == _askMarketOrdersTextOpacity && cacheBookmap[idx]._TotalMarketOrdersTextOpacity == _totalMarketOrdersTextOpacity && cacheBookmap[idx]._MarketCummulativeCalculation == _marketCummulativeCalculation && cacheBookmap[idx]._CummulativeBookTextFont == _cummulativeBookTextFont && cacheBookmap[idx]._CummulativeBookMinFontWidth == _cummulativeBookMinFontWidth && cacheBookmap[idx]._CummulativeBookMinFontHeight == _cummulativeBookMinFontHeight && cacheBookmap[idx]._BidMarketCummulativeTextOpacity == _bidMarketCummulativeTextOpacity && cacheBookmap[idx]._AskMarketCummulativeTextOpacity == _askMarketCummulativeTextOpacity && cacheBookmap[idx]._TotalMarketCummulativeTextOpacity == _totalMarketCummulativeTextOpacity && cacheBookmap[idx]._BidOrderBookOpacity == _bidOrderBookOpacity && cacheBookmap[idx]._AskOrderBookOpacity == _askOrderBookOpacity && cacheBookmap[idx]._OrderBookTextFont == _orderBookTextFont && cacheBookmap[idx]._OrderBookTextMinFontWidth == _orderBookTextMinFontWidth && cacheBookmap[idx]._OrderBookTextMinFontHeight == _orderBookTextMinFontHeight && cacheBookmap[idx]._BidOrderBookTextOpacity == _bidOrderBookTextOpacity && cacheBookmap[idx]._AskOrderBookTextOpacity == _askOrderBookTextOpacity && cacheBookmap[idx].EqualsInput(input))
						return cacheBookmap[idx];
			return CacheIndicator<WyckoffZen.Bookmap>(new WyckoffZen.Bookmap(){ _SessionSaveFilePath = _sessionSaveFilePath, _SessionLoadFilePath = _sessionLoadFilePath, _SaveSession = _saveSession, _FilterSessionPendingOrdersPer = _filterSessionPendingOrdersPer, _LadderRange = _ladderRange, _MarketOrdersCalculation = _marketOrdersCalculation, _MarketBarsCalculation = _marketBarsCalculation, _AggresiveMarketOrdersFilter = _aggresiveMarketOrdersFilter, _FilterBigPendingOrders = _filterBigPendingOrders, _FilterPendingOrdersPer = _filterPendingOrdersPer, _FilterTextPendingOrdersPer = _filterTextPendingOrdersPer, _ShowMarketOrdersText = _showMarketOrdersText, _BookMarginRight = _bookMarginRight, _BackgroundColorOpacity = _backgroundColorOpacity, _BigPendingOrdersOpacity = _bigPendingOrdersOpacity, _BookmapTextFont = _bookmapTextFont, _BookmapMinFontWidth = _bookmapMinFontWidth, _BookmapMinFontHeight = _bookmapMinFontHeight, _BidPendingOrdersTextOpacity = _bidPendingOrdersTextOpacity, _AskPendingOrdersTextOpacity = _askPendingOrdersTextOpacity, _BidMarketOrdersTextOpacity = _bidMarketOrdersTextOpacity, _AskMarketOrdersTextOpacity = _askMarketOrdersTextOpacity, _TotalMarketOrdersTextOpacity = _totalMarketOrdersTextOpacity, _MarketCummulativeCalculation = _marketCummulativeCalculation, _CummulativeBookTextFont = _cummulativeBookTextFont, _CummulativeBookMinFontWidth = _cummulativeBookMinFontWidth, _CummulativeBookMinFontHeight = _cummulativeBookMinFontHeight, _BidMarketCummulativeTextOpacity = _bidMarketCummulativeTextOpacity, _AskMarketCummulativeTextOpacity = _askMarketCummulativeTextOpacity, _TotalMarketCummulativeTextOpacity = _totalMarketCummulativeTextOpacity, _BidOrderBookOpacity = _bidOrderBookOpacity, _AskOrderBookOpacity = _askOrderBookOpacity, _OrderBookTextFont = _orderBookTextFont, _OrderBookTextMinFontWidth = _orderBookTextMinFontWidth, _OrderBookTextMinFontHeight = _orderBookTextMinFontHeight, _BidOrderBookTextOpacity = _bidOrderBookTextOpacity, _AskOrderBookTextOpacity = _askOrderBookTextOpacity }, input, ref cacheBookmap);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.WyckoffZen.Bookmap Bookmap(string _sessionSaveFilePath, string _sessionLoadFilePath, bool _saveSession, float _filterSessionPendingOrdersPer, _BookMapEnums.LadderRange _ladderRange, _BookMapEnums.MarketOrdersCalculation _marketOrdersCalculation, _BookMapEnums.MarketBarsCalculation _marketBarsCalculation, long _aggresiveMarketOrdersFilter, long _filterBigPendingOrders, float _filterPendingOrdersPer, float _filterTextPendingOrdersPer, bool _showMarketOrdersText, int _bookMarginRight, float _backgroundColorOpacity, float _bigPendingOrdersOpacity, SimpleFont _bookmapTextFont, float _bookmapMinFontWidth, float _bookmapMinFontHeight, float _bidPendingOrdersTextOpacity, float _askPendingOrdersTextOpacity, float _bidMarketOrdersTextOpacity, float _askMarketOrdersTextOpacity, float _totalMarketOrdersTextOpacity, _BookMapEnums.MarketCummulativeCalculation _marketCummulativeCalculation, SimpleFont _cummulativeBookTextFont, float _cummulativeBookMinFontWidth, float _cummulativeBookMinFontHeight, float _bidMarketCummulativeTextOpacity, float _askMarketCummulativeTextOpacity, float _totalMarketCummulativeTextOpacity, float _bidOrderBookOpacity, float _askOrderBookOpacity, SimpleFont _orderBookTextFont, float _orderBookTextMinFontWidth, float _orderBookTextMinFontHeight, float _bidOrderBookTextOpacity, float _askOrderBookTextOpacity)
		{
			return indicator.Bookmap(Input, _sessionSaveFilePath, _sessionLoadFilePath, _saveSession, _filterSessionPendingOrdersPer, _ladderRange, _marketOrdersCalculation, _marketBarsCalculation, _aggresiveMarketOrdersFilter, _filterBigPendingOrders, _filterPendingOrdersPer, _filterTextPendingOrdersPer, _showMarketOrdersText, _bookMarginRight, _backgroundColorOpacity, _bigPendingOrdersOpacity, _bookmapTextFont, _bookmapMinFontWidth, _bookmapMinFontHeight, _bidPendingOrdersTextOpacity, _askPendingOrdersTextOpacity, _bidMarketOrdersTextOpacity, _askMarketOrdersTextOpacity, _totalMarketOrdersTextOpacity, _marketCummulativeCalculation, _cummulativeBookTextFont, _cummulativeBookMinFontWidth, _cummulativeBookMinFontHeight, _bidMarketCummulativeTextOpacity, _askMarketCummulativeTextOpacity, _totalMarketCummulativeTextOpacity, _bidOrderBookOpacity, _askOrderBookOpacity, _orderBookTextFont, _orderBookTextMinFontWidth, _orderBookTextMinFontHeight, _bidOrderBookTextOpacity, _askOrderBookTextOpacity);
		}

		public Indicators.WyckoffZen.Bookmap Bookmap(ISeries<double> input , string _sessionSaveFilePath, string _sessionLoadFilePath, bool _saveSession, float _filterSessionPendingOrdersPer, _BookMapEnums.LadderRange _ladderRange, _BookMapEnums.MarketOrdersCalculation _marketOrdersCalculation, _BookMapEnums.MarketBarsCalculation _marketBarsCalculation, long _aggresiveMarketOrdersFilter, long _filterBigPendingOrders, float _filterPendingOrdersPer, float _filterTextPendingOrdersPer, bool _showMarketOrdersText, int _bookMarginRight, float _backgroundColorOpacity, float _bigPendingOrdersOpacity, SimpleFont _bookmapTextFont, float _bookmapMinFontWidth, float _bookmapMinFontHeight, float _bidPendingOrdersTextOpacity, float _askPendingOrdersTextOpacity, float _bidMarketOrdersTextOpacity, float _askMarketOrdersTextOpacity, float _totalMarketOrdersTextOpacity, _BookMapEnums.MarketCummulativeCalculation _marketCummulativeCalculation, SimpleFont _cummulativeBookTextFont, float _cummulativeBookMinFontWidth, float _cummulativeBookMinFontHeight, float _bidMarketCummulativeTextOpacity, float _askMarketCummulativeTextOpacity, float _totalMarketCummulativeTextOpacity, float _bidOrderBookOpacity, float _askOrderBookOpacity, SimpleFont _orderBookTextFont, float _orderBookTextMinFontWidth, float _orderBookTextMinFontHeight, float _bidOrderBookTextOpacity, float _askOrderBookTextOpacity)
		{
			return indicator.Bookmap(input, _sessionSaveFilePath, _sessionLoadFilePath, _saveSession, _filterSessionPendingOrdersPer, _ladderRange, _marketOrdersCalculation, _marketBarsCalculation, _aggresiveMarketOrdersFilter, _filterBigPendingOrders, _filterPendingOrdersPer, _filterTextPendingOrdersPer, _showMarketOrdersText, _bookMarginRight, _backgroundColorOpacity, _bigPendingOrdersOpacity, _bookmapTextFont, _bookmapMinFontWidth, _bookmapMinFontHeight, _bidPendingOrdersTextOpacity, _askPendingOrdersTextOpacity, _bidMarketOrdersTextOpacity, _askMarketOrdersTextOpacity, _totalMarketOrdersTextOpacity, _marketCummulativeCalculation, _cummulativeBookTextFont, _cummulativeBookMinFontWidth, _cummulativeBookMinFontHeight, _bidMarketCummulativeTextOpacity, _askMarketCummulativeTextOpacity, _totalMarketCummulativeTextOpacity, _bidOrderBookOpacity, _askOrderBookOpacity, _orderBookTextFont, _orderBookTextMinFontWidth, _orderBookTextMinFontHeight, _bidOrderBookTextOpacity, _askOrderBookTextOpacity);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.WyckoffZen.Bookmap Bookmap(string _sessionSaveFilePath, string _sessionLoadFilePath, bool _saveSession, float _filterSessionPendingOrdersPer, _BookMapEnums.LadderRange _ladderRange, _BookMapEnums.MarketOrdersCalculation _marketOrdersCalculation, _BookMapEnums.MarketBarsCalculation _marketBarsCalculation, long _aggresiveMarketOrdersFilter, long _filterBigPendingOrders, float _filterPendingOrdersPer, float _filterTextPendingOrdersPer, bool _showMarketOrdersText, int _bookMarginRight, float _backgroundColorOpacity, float _bigPendingOrdersOpacity, SimpleFont _bookmapTextFont, float _bookmapMinFontWidth, float _bookmapMinFontHeight, float _bidPendingOrdersTextOpacity, float _askPendingOrdersTextOpacity, float _bidMarketOrdersTextOpacity, float _askMarketOrdersTextOpacity, float _totalMarketOrdersTextOpacity, _BookMapEnums.MarketCummulativeCalculation _marketCummulativeCalculation, SimpleFont _cummulativeBookTextFont, float _cummulativeBookMinFontWidth, float _cummulativeBookMinFontHeight, float _bidMarketCummulativeTextOpacity, float _askMarketCummulativeTextOpacity, float _totalMarketCummulativeTextOpacity, float _bidOrderBookOpacity, float _askOrderBookOpacity, SimpleFont _orderBookTextFont, float _orderBookTextMinFontWidth, float _orderBookTextMinFontHeight, float _bidOrderBookTextOpacity, float _askOrderBookTextOpacity)
		{
			return indicator.Bookmap(Input, _sessionSaveFilePath, _sessionLoadFilePath, _saveSession, _filterSessionPendingOrdersPer, _ladderRange, _marketOrdersCalculation, _marketBarsCalculation, _aggresiveMarketOrdersFilter, _filterBigPendingOrders, _filterPendingOrdersPer, _filterTextPendingOrdersPer, _showMarketOrdersText, _bookMarginRight, _backgroundColorOpacity, _bigPendingOrdersOpacity, _bookmapTextFont, _bookmapMinFontWidth, _bookmapMinFontHeight, _bidPendingOrdersTextOpacity, _askPendingOrdersTextOpacity, _bidMarketOrdersTextOpacity, _askMarketOrdersTextOpacity, _totalMarketOrdersTextOpacity, _marketCummulativeCalculation, _cummulativeBookTextFont, _cummulativeBookMinFontWidth, _cummulativeBookMinFontHeight, _bidMarketCummulativeTextOpacity, _askMarketCummulativeTextOpacity, _totalMarketCummulativeTextOpacity, _bidOrderBookOpacity, _askOrderBookOpacity, _orderBookTextFont, _orderBookTextMinFontWidth, _orderBookTextMinFontHeight, _bidOrderBookTextOpacity, _askOrderBookTextOpacity);
		}

		public Indicators.WyckoffZen.Bookmap Bookmap(ISeries<double> input , string _sessionSaveFilePath, string _sessionLoadFilePath, bool _saveSession, float _filterSessionPendingOrdersPer, _BookMapEnums.LadderRange _ladderRange, _BookMapEnums.MarketOrdersCalculation _marketOrdersCalculation, _BookMapEnums.MarketBarsCalculation _marketBarsCalculation, long _aggresiveMarketOrdersFilter, long _filterBigPendingOrders, float _filterPendingOrdersPer, float _filterTextPendingOrdersPer, bool _showMarketOrdersText, int _bookMarginRight, float _backgroundColorOpacity, float _bigPendingOrdersOpacity, SimpleFont _bookmapTextFont, float _bookmapMinFontWidth, float _bookmapMinFontHeight, float _bidPendingOrdersTextOpacity, float _askPendingOrdersTextOpacity, float _bidMarketOrdersTextOpacity, float _askMarketOrdersTextOpacity, float _totalMarketOrdersTextOpacity, _BookMapEnums.MarketCummulativeCalculation _marketCummulativeCalculation, SimpleFont _cummulativeBookTextFont, float _cummulativeBookMinFontWidth, float _cummulativeBookMinFontHeight, float _bidMarketCummulativeTextOpacity, float _askMarketCummulativeTextOpacity, float _totalMarketCummulativeTextOpacity, float _bidOrderBookOpacity, float _askOrderBookOpacity, SimpleFont _orderBookTextFont, float _orderBookTextMinFontWidth, float _orderBookTextMinFontHeight, float _bidOrderBookTextOpacity, float _askOrderBookTextOpacity)
		{
			return indicator.Bookmap(input, _sessionSaveFilePath, _sessionLoadFilePath, _saveSession, _filterSessionPendingOrdersPer, _ladderRange, _marketOrdersCalculation, _marketBarsCalculation, _aggresiveMarketOrdersFilter, _filterBigPendingOrders, _filterPendingOrdersPer, _filterTextPendingOrdersPer, _showMarketOrdersText, _bookMarginRight, _backgroundColorOpacity, _bigPendingOrdersOpacity, _bookmapTextFont, _bookmapMinFontWidth, _bookmapMinFontHeight, _bidPendingOrdersTextOpacity, _askPendingOrdersTextOpacity, _bidMarketOrdersTextOpacity, _askMarketOrdersTextOpacity, _totalMarketOrdersTextOpacity, _marketCummulativeCalculation, _cummulativeBookTextFont, _cummulativeBookMinFontWidth, _cummulativeBookMinFontHeight, _bidMarketCummulativeTextOpacity, _askMarketCummulativeTextOpacity, _totalMarketCummulativeTextOpacity, _bidOrderBookOpacity, _askOrderBookOpacity, _orderBookTextFont, _orderBookTextMinFontWidth, _orderBookTextMinFontHeight, _bidOrderBookTextOpacity, _askOrderBookTextOpacity);
		}
	}
}

#endregion
