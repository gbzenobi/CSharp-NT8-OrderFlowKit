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

public static class _OrderFlowEnums
{
	public enum Calculation
	{
		BidAsk,
		TotalDelta,
		Total,
		Delta
	}
	public enum Representation
	{
		Volume,
		Percent
	}
	public enum Style
	{
		Profile,
		HeatMap
	}
	public enum Position
	{
		Left,
		//Center,
		Right
	}
}

namespace NinjaTrader.NinjaScript.Indicators.WyckoffZen
{
	public class OrderFlow : Indicator
	{
		#region MAIN
		
		private class WyckoffOrderFlow : WyckoffRenderControl
		{
			private SharpDX.RectangleF Rect, Rect2;
			private Action _renderOFText;
			private Action<float, float> renderClusterRect;
			private SharpDX.DirectWrite.TextFormat volumeTextFormat;
			private SharpDX.Vector2 beg_maxClusterVec;
			private SharpDX.Vector2 end_maxClusterVec;
			
			// !- setup brushes
			private SharpDX.Color colorBidClusterColor;
			private SharpDX.Color colorAskClusterColor;
			private SharpDX.Color colorTotalClusterColor;
			private SharpDX.Color colorBidFontColor;
			private SharpDX.Color colorAskFontColor;
			private SharpDX.Color colorTotalFontColor;
			private SharpDX.Color colorMaxVolumeClusterColor;
			private SharpDX.Color colorMinVolumeClusterColor;
			private SharpDX.Color colorPOCLines;
			private SharpDX.Color colorPOILines;
			
			// !- setup opacity
			private float brushBidFontOpacity;
			private float brushAskFontOpacity;
			private float totalFontOpacity;
			private float maxClusterOpacity;
			private float minClusterOpacity;
			private float clustersOpacity;
			private float POCLinesOpacity;
			private float POILinesOpacity;
			// !- setup calculation
			private float minFontWidth;
			private float minFontHeight;
			private _OrderFlowEnums.Position orderFlowPosition;
			private _OrderFlowEnums.Style orderFlowStyle;
			private _OrderFlowEnums.Calculation orderFlowCalculation;
			private _OrderFlowEnums.Representation orderFlowRepresentation;
			// !- setup renders
			private bool showClusterPOC;
			private bool showClusterPOI;
			private bool showPOCSLines;
			private bool showPOISLines;
			private bool showText;
			private bool showOrderFlow;
			
			// !- setup lines style
			private float POCLines_strokeWidth;
			private SharpDX.Direct2D1.StrokeStyle POCLines_strokeStyle;
			private float POILines_strokeWidth;
			private SharpDX.Direct2D1.StrokeStyle POILines_strokeStyle;
			
			// !- Para copiado de datos internos;
			private float barX;
			private float barY;
			private VolumeAnalysis.WyckoffBars wyckoffBars;
			private VolumeAnalysis.WyckoffBars.Bar currentBar;
			private VolumeAnalysis.MarketOrder volumeInfo;
			private bool realTime;
			
			public WyckoffOrderFlow()
			{
				this.volumeInfo = new VolumeAnalysis.MarketOrder();
				this.Rect = new SharpDX.RectangleF();
				this.Rect2= new SharpDX.RectangleF();
				this.realTime = false;
				
				this.beg_maxClusterVec = new SharpDX.Vector2();
				this.end_maxClusterVec = new SharpDX.Vector2();
			}
			
			#region SET_ORDER_FLOW_STYLE
			
			public void setFontStyle(SimpleFont font)
			{
				base.setFontStyle(font, out volumeTextFormat);
			}
			public void setBidAskClusterColor(Brush brushBidClusterColor, Brush brushAskClusterColor)
			{
				this.colorBidClusterColor = WyckoffRenderControl.BrushToColor(brushBidClusterColor);
				this.colorAskClusterColor = WyckoffRenderControl.BrushToColor(brushAskClusterColor);
			}
			public void setMaxMinVolumeClusterColor(
				Brush brushMaxVolumeClusterColor, float maxClusterOpacity,
				Brush brushMinVolumeClusterColor, float minClusterOpacity
				)
			{
				this.colorMaxVolumeClusterColor = WyckoffRenderControl.BrushToColor(brushMaxVolumeClusterColor);
				this.maxClusterOpacity = maxClusterOpacity / 100f;
				this.colorMinVolumeClusterColor = WyckoffRenderControl.BrushToColor(brushMinVolumeClusterColor);
				this.minClusterOpacity = minClusterOpacity / 100f;
			}
			public void setPOCPOILines(
				Brush brushPOCLines, float POCLines_strokeWidth, DashStyleHelper POCstrokeStyle, float POCLinesOpacity,
				Brush brushPOILines, float POILines_strokeWidth, DashStyleHelper POIstrokeStyle, float POILinesOpacity
				)
			{
				this.colorPOCLines = WyckoffRenderControl.BrushToColor(brushPOCLines);
				this.POCLinesOpacity = POCLinesOpacity / 100;
				this.POCLines_strokeWidth = POCLines_strokeWidth;
				SharpDX.Direct2D1.StrokeStyleProperties POCLines_strokeStyleProperties = new SharpDX.Direct2D1.StrokeStyleProperties();
				POCLines_strokeStyleProperties.DashStyle = DashStyleHelperToDX(POCstrokeStyle);
				
				this.POCLines_strokeStyle = new SharpDX.Direct2D1.StrokeStyle(NinjaTrader.Core.Globals.D2DFactory, POCLines_strokeStyleProperties);
				this.colorPOILines = WyckoffRenderControl.BrushToColor(brushPOILines);
				this.POILinesOpacity = POILinesOpacity / 100;
				this.POILines_strokeWidth = POCLines_strokeWidth;
				SharpDX.Direct2D1.StrokeStyleProperties POILines_strokeStyleProperties = new SharpDX.Direct2D1.StrokeStyleProperties();
				POILines_strokeStyleProperties.DashStyle = DashStyleHelperToDX(POIstrokeStyle);
				this.POILines_strokeStyle = new SharpDX.Direct2D1.StrokeStyle(NinjaTrader.Core.Globals.D2DFactory, POILines_strokeStyleProperties);
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
			public void setTotalFontColor(Brush brushTotalClusterColor, Brush brushTotalFontColor, float totalOpacity)
			{
				this.colorTotalClusterColor = WyckoffRenderControl.BrushToColor(brushTotalClusterColor);
				this.colorTotalFontColor = WyckoffRenderControl.BrushToColor(brushTotalFontColor);
				this.totalFontOpacity = totalOpacity / 100f;
			}
			public void setMinSizeFont(float minFontWidth, float minFontHeight)
			{
				this.minFontWidth = minFontWidth;
				this.minFontHeight = minFontHeight;
			}
			public void setShows(
				bool showClusterPOC, bool showClusterPOI,
				bool showPOCSLines, bool showPOISLines,
				bool showText, bool showOrderFlow)
			{
				this.showClusterPOC = showClusterPOC;
				this.showClusterPOI = showClusterPOI;
				this.showPOCSLines = showPOCSLines;
				this.showPOISLines = showPOISLines;
				this.showText = showText;
				this.showOrderFlow = showOrderFlow;
			}
			public void setPosition(_OrderFlowEnums.Position orderFlowPosition)
			{
				this.orderFlowPosition = orderFlowPosition;
			}
			public void setStyle(_OrderFlowEnums.Style orderFlowStyle)
			{
				this.orderFlowStyle = orderFlowStyle;
			}
			public void setClustersOpacity(float clustersOpacity)
			{
				this.clustersOpacity = clustersOpacity / 100f;
			}
			
			#endregion
			#region RENDER_INFORMATION
			
			private void __renderBidAskText()
			{
				long B = 0;
				long A = 0;
				string C = "";
				switch( this.orderFlowRepresentation )
				{
					case _OrderFlowEnums.Representation.Volume:
					{
						B = this.volumeInfo.Bid;
						A = this.volumeInfo.Ask;
						break;
					}
					case _OrderFlowEnums.Representation.Percent:
					{
						long total = this.volumeInfo.Total;
						B = (long)Math2.Percent(total, this.volumeInfo.Bid);
						A = (long)Math2.Percent(total, this.volumeInfo.Ask);
						// !- agregamos el simbolo de %
						C = "%";
						break;
					}
				}
				long D = volumeInfo.Delta;
				if( D > 0 ){
					myDrawText(string.Format("{0}x{1}"+C, B, A), ref Rect, colorAskFontColor, -1,-1, volumeTextFormat, brushAskFontOpacity);
				}
				else{
					myDrawText(string.Format("{0}x{1}"+C, B, A), ref Rect, colorBidFontColor, -1,-1, volumeTextFormat, brushBidFontOpacity);
				}
			}
			private void __renderTotalDeltaText()
			{
				long T = 0;
				long D = 0;
				string C = "";
				switch( this.orderFlowRepresentation )
				{
					case _OrderFlowEnums.Representation.Volume:
					{
						T = this.volumeInfo.Total;
						D = this.volumeInfo.Delta;
						break;
					}
					case _OrderFlowEnums.Representation.Percent:
					{
						long total = this.volumeInfo.Total;
						T = total;
						D = (long)Math2.Percent(total, Math.Abs(this.volumeInfo.Delta));
						// !- agregamos el simbolo de %
						C = "%";
						break;
					}
				}
				if( this.volumeInfo.Delta >= 0 ){
					myDrawText(string.Format("{0}x{1}"+C, T, D), ref Rect, colorAskFontColor, -1,-1, volumeTextFormat, brushAskFontOpacity);
				}
				else{
					myDrawText(string.Format("{0}x{1}"+C, T, D), ref Rect, colorBidFontColor, -1,-1, volumeTextFormat, brushBidFontOpacity);
				}
			}
			private void __renderTotalText()
			{
				myDrawText(volumeInfo.Total.ToString(), ref Rect, colorTotalFontColor, -1,-1, volumeTextFormat, totalFontOpacity);
			}
			private void __renderDeltaText()
			{
				long D = this.volumeInfo.Delta;
				string s_D = string.Empty;
				switch( this.orderFlowRepresentation )
				{
					case _OrderFlowEnums.Representation.Volume:
					{
						s_D = D.ToString();
						break;
					}
					case _OrderFlowEnums.Representation.Percent:
					{
						long total = this.volumeInfo.Total;
						s_D = Math2.Percent(total, Math.Abs(D)).ToString();
						// !- agregamos el simbolo de %
						s_D+= "%";
						break;
					}
				}
				
				if( D >= 0 ){
					myDrawText(s_D.ToString(), ref Rect, colorAskFontColor, -1,-1, volumeTextFormat, brushAskFontOpacity);
				}
				else{
					myDrawText(s_D.ToString(), ref Rect, colorBidFontColor, -1,-1, volumeTextFormat, brushBidFontOpacity);
				}
			}
			public void setCalculation(_OrderFlowEnums.Calculation orderFlowCalculation)
			{
				switch( orderFlowCalculation )
				{
					//case _OrderFlowEnums.Calculation.TotalBidAsk:
					case _OrderFlowEnums.Calculation.BidAsk:
					{
						this._renderOFText = this.__renderBidAskText;
						this.renderClusterRect = this.__renderBidAsk;
						break;
					}
					case _OrderFlowEnums.Calculation.TotalDelta:
					{
						this._renderOFText = this.__renderTotalDeltaText;
						this.renderClusterRect = this.__renderDelta;
						break;
					}
					case _OrderFlowEnums.Calculation.Total:
					{
						this._renderOFText = this.__renderTotalText;
						this.renderClusterRect = this.__renderTotal;
						break;
					}
					case _OrderFlowEnums.Calculation.Delta:
					{
						this._renderOFText = this.__renderDeltaText;
						this.renderClusterRect = this.__renderDelta;
						break;
					}
				}
				this.orderFlowCalculation = orderFlowCalculation;
			}
			public void setRepresentation(_OrderFlowEnums.Representation orderFlowRepresentation)
			{
				this.orderFlowRepresentation = orderFlowRepresentation;
			}
			
			#endregion
			
			public void setWyckoffBars(VolumeAnalysis.WyckoffBars wyckoffBars)
			{
				this.wyckoffBars = wyckoffBars;
			}
			public void setRealtime(bool isRealtime)
			{
				this.realTime = isRealtime;
			}
			public bool IsRealtime
			{
				get{ return this.realTime; }
			}
			// *- para la posicion del order flow, clusters y texto, si el resultado es negativo
			// se invierte la posicion
			private bool calculateXPosition(out float barXpos)
			{
				barXpos = 0;
				switch(orderFlowPosition)
				{
					case _OrderFlowEnums.Position.Right:
					{
						barXpos = (W / 4f);// - (BarW*4f);
						break;
					}
					case _OrderFlowEnums.Position.Left:
					{
						barXpos = -(W / 4f);
						return true;
					}
//					case _OrderFlowEnums.Position.Center:
//					{
//						barXpos = -(W / 4f);
//						break;
//					}
				}
				return false;
			}
			private float calculateXPositionFont()
			{
				if( orderFlowPosition == _OrderFlowEnums.Position.Left )//switch(orderFlowPosition)
				{
					return -W;
					// !- No necesitamos calcular la fuente
					//case _OrderFlowEnums.Position.Right:{ return 0; }
					//case _OrderFlowEnums.Position.Left:{ return -W; }
					//case _OrderFlowEnums.Position.Center:{ return -(W / 2f); }
				}
				return 0;
			}
			private float calculateClusterPOCPercent()
			{
				switch( this.orderFlowCalculation )
				{
//					case _OrderFlowEnums.Calculation.TotalBidAsk:
//					{
//						long vol;
//						if( this.volumeInfo.Delta >= 0 )
//							vol = this.volumeInfo.Ask;
//						else
//							vol = this.volumeInfo.Bid;
//						return (float)Math2.Percent(this.currentBar.MaxClusterVolume.Total, vol);
//					}
					case _OrderFlowEnums.Calculation.BidAsk:
					case _OrderFlowEnums.Calculation.Total:
					{
						// !- Obtenemos el porcentaje de volumen a partir del cluster maximo, en este punto
						// @maxClusterVolume representa el 100% y @vs.Total es el volumen en cada nivel
						// de precio, entonces si: maxClusterVolume == vs.Total el porcentaje sera 100%
						return (float)Math2.Percent(this.currentBar.MaxClusterVolume.Total, this.volumeInfo.Total);
					}
					case _OrderFlowEnums.Calculation.TotalDelta:
					{
						return (float)Math2.Percent(this.currentBar.MaxClusterVolume.Total, Math.Abs(this.volumeInfo.Delta));
					}
					case _OrderFlowEnums.Calculation.Delta:
					{
						return (float)Math2.Percent(Math.Abs(this.currentBar.MaxClusterVolume.Delta), Math.Abs(this.volumeInfo.Delta));
					}
				}
				return 0;
			}
			
			private void __renderBidAsk(float clusterPOC_per, float opacity)
			{
				bool invertSign = false;
				long vol = this.currentBar.MaxClusterVolume.Total;
				float bidPer = (float)Math2.Percent(vol, this.volumeInfo.Bid) / 100f;
				float askPer = (float)Math2.Percent(vol, this.volumeInfo.Ask) / 100f;
				this.Rect2.Y = this.Rect.Y;
				this.Rect2.Height = this.Rect.Height;
				
				// !- calculamos el estilo(profile, heatmap)
				switch( orderFlowStyle )
				{
					case _OrderFlowEnums.Style.Profile:
					{
						float bar_x;
						invertSign = this.calculateXPosition(out bar_x);
						
						Rect.X = barX + bar_x;//barX + (W / 4f);
						Rect2.X= Rect.X;
						
						// !- Anchura maxima: W / 2
						// el cual representa la anchura total de cada cluster, a partir de esto el
						// calculo (% * W) nos dara que cantidad de pixeles corresponde a cada cluster
						float width = W / 2f;
						float width_per, width_per2;
						
						width_per = (float)Math.Round(bidPer * width);
						width_per2= (float)Math.Round(askPer * width);
						if( invertSign ){
							width_per = -width_per;
							width_per2= -width_per2;
						}
						Rect.Width = width_per;
						Rect2.Width= width_per2;
						break;
					}
					case _OrderFlowEnums.Style.HeatMap:
					{
						Rect.X = barX - (W / 2f);
						Rect2.X= Rect.X;
						//float width = W; // 2f;
						//if( invertSign )
							//width = -width;
						Rect.Width = this.W;
						Rect2.Width= this.W;
						break;
					}
				}
				
				myFillRectangle(ref Rect, colorBidClusterColor, bidPer);
				myFillRectangle(ref Rect2,colorAskClusterColor, askPer);
			}
			private void __renderDelta(float clusterPOC_per, float opacity)
			{
				long D = this.volumeInfo.Delta;
				if( D == 0 )
					return;
				switch( this.orderFlowStyle )
				{
					case _OrderFlowEnums.Style.Profile:
					{
						float bar_x;
						bool invertSign = this.calculateXPosition(out bar_x);
						
						Rect.X = barX + bar_x;//barX + (W / 4f);
						// !- Anchura maxima: W / 2
						// el cual representa la anchura total de cada cluster, a partir de esto el
						// calculo (% * W) nos dara que cantidad de pixeles corresponde a cada cluster
						float width = W / 2f;
						float width_per = (float)Math.Round((clusterPOC_per * width) / 100);
						if( invertSign )
							width_per= -width_per;
						Rect.Width = width_per;
						break;
					}
					case _OrderFlowEnums.Style.HeatMap:
					{
						Rect.X = barX - (W / 2f);
						Rect.Width = W;
						break;
					}
				}
				if( D >= 0 )
					myFillRectangle(ref Rect, colorAskClusterColor, clustersOpacity);
				else
					myFillRectangle(ref Rect, colorBidClusterColor, clustersOpacity);
			}
			private void __renderTotal(float clusterPOC_per, float opacity)
			{
				switch( this.orderFlowStyle )
				{
					case _OrderFlowEnums.Style.Profile:
					{
						float bar_x;
						bool invertSign = this.calculateXPosition(out bar_x);
						
						Rect.X = barX + bar_x;//barX + (W / 4f);
						// !- Anchura maxima: W / 2
						// el cual representa la anchura total de cada cluster, a partir de esto el
						// calculo (% * W) nos dara que cantidad de pixeles corresponde a cada cluster
						float width = W / 2f;
						float width_per = (float)Math.Round((clusterPOC_per * width) / 100);
						if( invertSign )
							width_per= -width_per;
						
						Rect.Width = width_per;
						break;
					}
					case _OrderFlowEnums.Style.HeatMap:
					{
						Rect.X = barX - (W / 2f);
						Rect.Width = W;
						break;
					}
				}
				myFillRectangle(ref Rect, colorTotalClusterColor, opacity);
			}
			private void renderCluster()
			{
				if( !this.showOrderFlow ){
					return;
				}
				this.Rect.Y = barY - (this.H / 2f);
				this.Rect.Height = this.H;
				
				float clusterPOC_per = this.calculateClusterPOCPercent();
				float opacity = this.clustersOpacity == 0 ? 1.0f : (clusterPOC_per / 100f) * this.clustersOpacity;
				this.renderClusterRect(clusterPOC_per, opacity);
			}
			private void renderMinMaxCluster(double price)
			{
				Rect.Y = this.barY - (H / 2f);
				Rect.Height = H;
				
				switch( orderFlowStyle )
				{
					case _OrderFlowEnums.Style.Profile:{
						float bar_x;
						bool invertSign = this.calculateXPosition(out bar_x);
						Rect.X = barX + bar_x;
						float minmax_clus_width = W / 2f;
						if( invertSign )
							minmax_clus_width = -minmax_clus_width;
						Rect.Width = minmax_clus_width;
						
						break;
					}
					case _OrderFlowEnums.Style.HeatMap:{
						Rect.X = barX - (W / 2f);
						Rect.Width = W;
						
						break;
					}
				}
				
				// !- cluster maximo, es necesario que el precio actual coincida con el precio del cluster maximo
				// de otro modo si el volumen total es identico(se repite en la vela mas de una vez) el renderizado
				// mostratra mas de un cluster maximo confundiendo cual fue el ultimo maximo...
				if( this.showClusterPOC && Math2.Percent(currentBar.MaxClusterVolume.Total, volumeInfo.Total) == 100 && currentBar.MaxClusterPrice == price ){
					myDrawRectangle(ref Rect, colorMaxVolumeClusterColor, maxClusterOpacity, 1.5f);
				}
				// !- cluster minimo, misma logica
				if( this.showClusterPOI && Math2.Percent(currentBar.MinClusterVolume.Total, volumeInfo.Total) == 100 && currentBar.MinClusterPrice == price ){
					myFillRectangle(ref Rect, colorMinVolumeClusterColor, minClusterOpacity);
				}
			}
			private void renderText()
			{
				// !- renderizamos el texto
				if( this.showText && W >= minFontWidth && H >= minFontHeight )
				{
					switch( orderFlowStyle )
					{
						case _OrderFlowEnums.Style.Profile:
						{
							Rect.X = this.barX + this.calculateXPositionFont();
							break;
						}
						case _OrderFlowEnums.Style.HeatMap:
						{
							Rect.X = this.barX - (W / 2f);
							break;
						}
					}
					Rect.Y = this.barY - (H / 2f);
					Rect.Width = W;
					Rect.Height = H;
					
					this._renderOFText();//RenderTarget.DrawText(string.Format("{0}x{1}", T, D), volumeTextFormat, Rect, Brushes.CornflowerBlue.ToDxBrush(RenderTarget, 1.0f));
				}
			}
			// !- Renderizado para onRender
			private void renderBarCluster(double price)
			{
				renderCluster();
				renderMinMaxCluster(price);
				renderText();
			}
			private void renderLines(int barIndex)
			{
				if( barIndex > 0 ){
					int prevBarIndex = barIndex - 1;
					VolumeAnalysis.WyckoffBars.Bar prevBar = this.wyckoffBars[prevBarIndex];
					float prevBarX = CHART_CONTROL.GetXByBarIndex(CHART_BARS, prevBarIndex);
					
					beg_maxClusterVec.X = this.barX;
					end_maxClusterVec.X = prevBarX;
					
					if( this.orderFlowStyle == _OrderFlowEnums.Style.Profile )
					{
						switch( this.orderFlowPosition )
						{
							case _OrderFlowEnums.Position.Right:
							{
								float barW = W / 2f;
								beg_maxClusterVec.X += barW;//(W / 4f);
								end_maxClusterVec.X += barW;
								
								break;
							}
							case _OrderFlowEnums.Position.Left:
							{
								float barW = W / 2f;
								beg_maxClusterVec.X -= barW;
								end_maxClusterVec.X -= barW;
								break;
							}
							/* // podemos omitirlo es redundante
							case _OrderFlowEnums.Position.Center:
							{
								beg_maxClusterVec.X = this.barX;
								end_maxClusterVec.X = prevBarX;
								
								break;
							}*/
						}
						/* // podemos omitirlo es redundante
						case _OrderFlowEnums.Style.HeatMap:
						{
							break;
						}*/
					}
				
					// !- POC Lines
					if(this.showPOCSLines){
						beg_maxClusterVec.Y = CHART_SCALE.GetYByValue(currentBar.MaxClusterPrice);
						end_maxClusterVec.Y = CHART_SCALE.GetYByValue(prevBar.MaxClusterPrice);
						myDrawLine(ref beg_maxClusterVec, ref end_maxClusterVec,
							colorPOCLines, POCLinesOpacity,
							POCLines_strokeWidth, POCLines_strokeStyle);
					}
					// !- POI lines
					if(this.showPOISLines){
						beg_maxClusterVec.Y = CHART_SCALE.GetYByValue(currentBar.MinClusterPrice);
						end_maxClusterVec.Y = CHART_SCALE.GetYByValue(prevBar.MinClusterPrice);
						
						myDrawLine(ref beg_maxClusterVec, ref end_maxClusterVec,
							colorPOILines, POILinesOpacity,
							POILines_strokeWidth, POILines_strokeStyle);
					}
				}
			}
			public void renderBarClusters(int barIndex, bool realtimeCalculation)
			{
				// !- Optimizamos para calculos en tiempo-real
				if( realtimeCalculation ){
					//if( !this.wyckoffBars.BarExists(barIndex) ) return;
					this.currentBar = this.wyckoffBars.CurrentBar;//this.currentBar = this.wyckoffBars[barIndex];
					this.currentBar.CalculateMinAndMaxCluster();
				}
				else{
					this.currentBar = this.wyckoffBars[barIndex];
				}
				this.barX = CHART_CONTROL.GetXByBarIndex(CHART_BARS, barIndex);
				// -- renderizamos las lineas de POCs y POIs si estas fueron calculadas
				renderLines(barIndex);
				
				double price;
				foreach(var wb in currentBar){
					price = wb.Key;
					this.barY = CHART_SCALE.GetYByValue(price);
					// !- informacion de volumen
					this.volumeInfo = wb.Value;
					// !- renderizamos el cluster precio a precio
					renderBarCluster(price);
				}
			}
		}
		
		#endregion
		#region GLOBAL_VARIABLES
		
		private VolumeAnalysis.WyckoffBars wyckoffBars;
		private WyckoffOrderFlow wyckoffOF;
		
		#endregion
		
		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				wyckoffOF = new WyckoffOrderFlow();
				
				Description									= @"";
				Name										= "Order Flow";
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
				
				// !- Setup de estilo
				_TextFont = new SimpleFont();
				_TextFont.Family = new FontFamily("Arial");
				_TextFont.Size = 10f;
				_TextFont.Bold = false;
				_TextFont.Italic= false;
				
				_MinFontWidth = 10f; _MinFontHeight= 10f;
				_BidTextVolumeColor = Brushes.LightCoral;
				_BidTextOpacity = 100f;
				_AskTextVolumeColor = Brushes.DarkSeaGreen;
				_AskTextOpacity = 100f;
				_TotalTextVolumeColor = Brushes.Beige;
				_TotalTextOpacity = 80f;
				
				_AskClusterVolumeColor = Brushes.SeaGreen;
				_BidClusterVolumeColor = Brushes.Red;
				_TotalClusterVolumeColor = Brushes.LightSkyBlue;
				_MaxClusterVolumeColor = Brushes.PowderBlue;
				_MaxClusterOpacity = 60f;
				_MinClusterVolumeColor = Brushes.Violet;
				_MinClusterOpacity = 20f;
				// !- Por defecto tiene un 70% de opacidad cada nivel de cluster
				_ClustersOpacity = 70f;
				
				// !- Estilo de lineas POCs y POIs
				_POCLinesColor = Brushes.WhiteSmoke;
				_POCLinesOpacity = 70f;
				_POCLines_strokeWidth = 1.0f;
				_POCLinesStrokeStyle = DashStyleHelper.Solid;//SharpDX.Direct2D1.DashStyle.Solid;
				_POILinesColor = Brushes.Violet;
				_POILinesOpacity = 40f;
				_POILines_strokeWidth = 1.0f;
				_POILinesStrokeStyle = DashStyleHelper.Dash;//SharpDX.Direct2D1.DashStyle.Dash;
				
				// !- Calculos del order flow por defecto
				_OrderFlowCalculation = _OrderFlowEnums.Calculation.BidAsk;
				_OrderFlowRepresentation = _OrderFlowEnums.Representation.Volume;
				_OrderFlowPosition = _OrderFlowEnums.Position.Right;
				
				_ShowClusterPOC = true;
				_ShowPOCSLines = false;
				_ShowClusterPOI = false;
				_ShowPOISLines = false;
				_ShowText = true;
				_ShowOrderFlow = true;
				// !- calculamos la ultima barra creada en tiempo real/mercado (?)
				_RealtimeHeuristic = true;
			}
			else if (State == State.Configure)
			{
				wyckoffOF.setFontStyle(_TextFont);
				wyckoffOF.setCalculation(_OrderFlowCalculation);
				wyckoffOF.setRepresentation(_OrderFlowRepresentation);
				wyckoffOF.setPosition(_OrderFlowPosition);
				wyckoffOF.setStyle(_OrderFlowStyle);
				wyckoffOF.setBidAskClusterColor(_BidClusterVolumeColor, _AskClusterVolumeColor);
				wyckoffOF.setBidAskFontColor(_BidTextVolumeColor, _BidTextOpacity,
					_AskTextVolumeColor, _AskTextOpacity);
				wyckoffOF.setTotalFontColor(_TotalClusterVolumeColor, _TotalTextVolumeColor, _TotalTextOpacity);
				wyckoffOF.setMaxMinVolumeClusterColor(_MaxClusterVolumeColor, _MaxClusterOpacity,
					_MinClusterVolumeColor, _MinClusterOpacity);
				wyckoffOF.setPOCPOILines(
					_POCLinesColor, _POCLines_strokeWidth, _POCLinesStrokeStyle, _POCLinesOpacity,
					_POILinesColor, _POILines_strokeWidth, _POILinesStrokeStyle, _POILinesOpacity);
				wyckoffOF.setMinSizeFont(_MinFontWidth, _MinFontHeight);
				wyckoffOF.setClustersOpacity(_ClustersOpacity);
				wyckoffOF.setShows(_ShowClusterPOC, _ShowClusterPOI,
				_ShowPOCSLines, _ShowPOISLines,
				_ShowText, _ShowOrderFlow);
				
				Calculate= Calculate.OnEachTick;
			}
			else if(State == State.DataLoaded)
			{
				wyckoffBars = new VolumeAnalysis.WyckoffBars(Bars);
				wyckoffOF.setWyckoffBars(wyckoffBars);
			}
			else if(State == State.Realtime)
			{
				wyckoffOF.setRealtime(true);
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
			if( !wyckoffOF.IsRealtime || IsInHitTest == null ||
				chartControl == null || ChartBars.Bars == null ){
				return;
			}
			
			#region RENDER_ORDER_FLOW
			
			float W = chartControl.Properties.BarDistance;
			
			if( this._OrderFlowStyle == _OrderFlowEnums.Style.HeatMap ){
				ChartControl.Properties.BarMarginRight = (int)(W / 4);
			}
			if( this._OrderFlowPosition == _OrderFlowEnums.Position.Right ){
				ChartControl.Properties.BarMarginRight = (int)(W / 1.5);
			}
			
			//renderOF.BarW = (float)chartControl.BarWidth;
			// 1- Altura minima de un tick
			// 2- Ancho de barra en barra
			wyckoffOF.setHW(chartScale.GetPixelsForDistance(TickSize), W);
			// !- Apuntamos al target de renderizado
			wyckoffOF.setRenderTarget(chartControl, chartScale, ChartBars, RenderTarget);
			
			int fromIndex = ChartBars.FromIndex;
			int toIndex = ChartBars.ToIndex;
			// ?- es la ultima barra generada
			if( toIndex == wyckoffBars.CurrentBarIndex )
			{
				if( _RealtimeHeuristic ){
					wyckoffOF.renderBarClusters(toIndex, true);
				}
				// -- no cargamos la ultima barra en creacion
				toIndex--;
			}
			
			for(int barIndex = fromIndex; barIndex <= toIndex; barIndex++){
				wyckoffOF.renderBarClusters(barIndex, false);
			}
			
			#endregion
		}
		protected override void OnMarketData(MarketDataEventArgs MarketArgs){
			wyckoffBars.onMarketData(MarketArgs);
		}
		
		#region Properties
		
		[NinjaScriptProperty]
		[Display(Name="Formula", Order=0, GroupName="Order flow calculation")]
		public _OrderFlowEnums.Calculation _OrderFlowCalculation
		{ get; set; }
		
		[NinjaScriptProperty]
		[Display(Name="Representation", Order=1, GroupName="Order flow calculation")]
		public _OrderFlowEnums.Representation _OrderFlowRepresentation
		{ get; set; }
		
		[NinjaScriptProperty]
		[Display(Name="Position(Profile)", Order=2, GroupName="Order flow calculation")]
		public _OrderFlowEnums.Position _OrderFlowPosition
		{ get; set; }
		
		[NinjaScriptProperty]
		[Display(Name="Style", Order=3, GroupName="Order flow calculation")]
		public _OrderFlowEnums.Style _OrderFlowStyle
		{ get; set; }
		
		[NinjaScriptProperty]
		[Display(Name="Realtime heuristic", Order=4, GroupName="Order flow calculation")]
		public bool _RealtimeHeuristic
		{ get; set; }
		
		[XmlIgnore]
		[Display(Name="Ask clusters", Order=0, GroupName="Order flow style")]
		public Brush _AskClusterVolumeColor
		{ get; set; }
		[Browsable(false)]
		public string _AskClusterVolumeColorSerializable
		{
			get { return Serialize.BrushToString(_AskClusterVolumeColor); }
			set { _AskClusterVolumeColor = Serialize.StringToBrush(value); }
		}
		[XmlIgnore]
		[Display(Name="Bid clusters", Order=1, GroupName="Order flow style")]
		public Brush _BidClusterVolumeColor
		{ get; set; }
		[Browsable(false)]
		public string _BidClusterVolumeColorSerializable
		{
			get { return Serialize.BrushToString(_BidClusterVolumeColor); }
			set { _BidClusterVolumeColor = Serialize.StringToBrush(value); }
		}
		[XmlIgnore]
		[Display(Name="Total clusters", Order=2, GroupName="Order flow style")]
		public Brush _TotalClusterVolumeColor
		{ get; set; }
		[Browsable(false)]
		public string _TotalClusterVolumeColorSerializable
		{
			get { return Serialize.BrushToString(_TotalClusterVolumeColor); }
			set { _TotalClusterVolumeColor = Serialize.StringToBrush(value); }
		}
		
		[XmlIgnore]
		[Display(Name="Max cluster", Order=3, GroupName="Order flow style")]
		public Brush _MaxClusterVolumeColor
		{ get; set; }
		[Browsable(false)]
		public string _MaxClusterVolumeColorSerializable
		{
			get { return Serialize.BrushToString(_MaxClusterVolumeColor); }
			set { _MaxClusterVolumeColor = Serialize.StringToBrush(value); }
		}
		
		[XmlIgnore]
		[Display(Name="Min cluster", Order=4, GroupName="Order flow style")]
		public Brush _MinClusterVolumeColor
		{ get; set; }
		[Browsable(false)]
		public string _MinClusterVolumeColorSerializable
		{
			get { return Serialize.BrushToString(_MinClusterVolumeColor); }
			set { _MinClusterVolumeColor = Serialize.StringToBrush(value); }
		}
		
		[NinjaScriptProperty]
		[Range(0.0f, 100f)]
		[Display(Name="Max cluster opacity", Order=5, GroupName="Order flow style")]
		public float _MaxClusterOpacity
		{ get; set; }
		[NinjaScriptProperty]
		[Range(0.0f, 100f)]
		[Display(Name="Min cluster opacity", Order=6, GroupName="Order flow style")]
		public float _MinClusterOpacity
		{ get; set; }
		[NinjaScriptProperty]
		[Range(0.0f, 100f)]
		[Display(Name="Clusters opacity", Order=7, GroupName="Order flow style")]
		public float _ClustersOpacity
		{ get; set; }
		
		[XmlIgnore]
		[Display(Name="POC lines", Order=8, GroupName="Order flow style")]
		public Brush _POCLinesColor
		{ get; set; }
		[Browsable(false)]
		public string _POCLinesColorSerializable
		{
			get { return Serialize.BrushToString(_POCLinesColor); }
			set { _POCLinesColor = Serialize.StringToBrush(value); }
		}
		[XmlIgnore]
		[Display(Name="POI lines", Order=9, GroupName="Order flow style")]
		public Brush _POILinesColor
		{ get; set; }
		[Browsable(false)]
		public string _POILinesColorSerializable
		{
			get { return Serialize.BrushToString(_POILinesColor); }
			set { _POILinesColor = Serialize.StringToBrush(value); }
		}
		[NinjaScriptProperty]
		[Range(0.5f, 10f)]
		[Display(Name="POC lines width", Order=10, GroupName="Order flow style")]
		public float _POCLines_strokeWidth
		{ get; set; }
		[NinjaScriptProperty]
		[Display(Name="POC lines style", Order=11, GroupName="Order flow style")]
		public DashStyleHelper _POCLinesStrokeStyle
		{ get; set; }
		[NinjaScriptProperty]
		[Range(0.0f, 100f)]
		[Display(Name="POC lines opacity", Order=12, GroupName="Order flow style")]
		public float _POCLinesOpacity
		{ get; set; }
		
		[NinjaScriptProperty]
		[Range(0.5f, 10f)]
		[Display(Name="POI lines width", Order=13, GroupName="Order flow style")]
		public float _POILines_strokeWidth
		{ get; set; }
		[NinjaScriptProperty]
		[Display(Name="POI lines style", Order=14, GroupName="Order flow style")]
		public DashStyleHelper _POILinesStrokeStyle
		{ get; set; }
		[NinjaScriptProperty]
		[Range(0.0f, 100f)]
		[Display(Name="POI lines opacity", Order=15, GroupName="Order flow style")]
		public float _POILinesOpacity
		{ get; set; }
		
		[XmlIgnore]
		[Display(Name="Ask text", Order=16, GroupName="Order flow style")]
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
		[Display(Name="Ask text opacity", Order=17, GroupName="Order flow style")]
		public float _AskTextOpacity
		{ get; set; }
		
		[XmlIgnore]
		[Display(Name="Bid text", Order=18, GroupName="Order flow style")]
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
		[Display(Name="Bid text opacity", Order=19, GroupName="Order flow style")]
		public float _BidTextOpacity
		{ get; set; }
		
		[XmlIgnore]
		[Display(Name="Total text", Order=20, GroupName="Order flow style")]
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
		[Display(Name="Total text opacity", Order=21, GroupName="Order flow style")]
		public float _TotalTextOpacity
		{ get; set; }
		
		[NinjaScriptProperty]
		[Display(Name="Font", Order=22, GroupName="Order flow style")]
		public SimpleFont _TextFont
		{ get; set; }
		
		[NinjaScriptProperty]
		[Range(1.0f, float.MaxValue)]
		[Display(Name="Min font width", Order=23, GroupName="Order flow style")]
		public float _MinFontWidth
		{ get; set; }
		
		[NinjaScriptProperty]
		[Range(1.0f, float.MaxValue)]
		[Display(Name="Min font height", Order=24, GroupName="Order flow style")]
		public float _MinFontHeight
		{ get; set; }
		
		
		[NinjaScriptProperty]
		[Display(Name="Cluster POC", Order=0, GroupName="Order flow show")]
		public bool _ShowClusterPOC
		{ get; set; }
		[NinjaScriptProperty]
		[Display(Name="POC lines", Order=1, GroupName="Order flow show")]
		public bool _ShowPOCSLines
		{ get; set; }
		
		[NinjaScriptProperty]
		[Display(Name="Cluster POI", Order=2, GroupName="Order flow show")]
		public bool _ShowClusterPOI
		{ get; set; }
		[NinjaScriptProperty]
		[Display(Name="POI lines", Order=3, GroupName="Order flow show")]
		public bool _ShowPOISLines
		{ get; set; }
		
		[NinjaScriptProperty]
		[Display(Name="Text", Order=4, GroupName="Order flow show")]
		public bool _ShowText
		{ get; set; }
		
		[NinjaScriptProperty]
		[Display(Name="Order flow", Order=5, GroupName="Order flow show")]
		public bool _ShowOrderFlow
		{ get; set; }
		
		#endregion
	}
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private WyckoffZen.OrderFlow[] cacheOrderFlow;
		public WyckoffZen.OrderFlow OrderFlow(_OrderFlowEnums.Calculation _orderFlowCalculation, _OrderFlowEnums.Representation _orderFlowRepresentation, _OrderFlowEnums.Position _orderFlowPosition, _OrderFlowEnums.Style _orderFlowStyle, bool _realtimeHeuristic, float _maxClusterOpacity, float _minClusterOpacity, float _clustersOpacity, float _pOCLines_strokeWidth, DashStyleHelper _pOCLinesStrokeStyle, float _pOCLinesOpacity, float _pOILines_strokeWidth, DashStyleHelper _pOILinesStrokeStyle, float _pOILinesOpacity, float _askTextOpacity, float _bidTextOpacity, float _totalTextOpacity, SimpleFont _textFont, float _minFontWidth, float _minFontHeight, bool _showClusterPOC, bool _showPOCSLines, bool _showClusterPOI, bool _showPOISLines, bool _showText, bool _showOrderFlow)
		{
			return OrderFlow(Input, _orderFlowCalculation, _orderFlowRepresentation, _orderFlowPosition, _orderFlowStyle, _realtimeHeuristic, _maxClusterOpacity, _minClusterOpacity, _clustersOpacity, _pOCLines_strokeWidth, _pOCLinesStrokeStyle, _pOCLinesOpacity, _pOILines_strokeWidth, _pOILinesStrokeStyle, _pOILinesOpacity, _askTextOpacity, _bidTextOpacity, _totalTextOpacity, _textFont, _minFontWidth, _minFontHeight, _showClusterPOC, _showPOCSLines, _showClusterPOI, _showPOISLines, _showText, _showOrderFlow);
		}

		public WyckoffZen.OrderFlow OrderFlow(ISeries<double> input, _OrderFlowEnums.Calculation _orderFlowCalculation, _OrderFlowEnums.Representation _orderFlowRepresentation, _OrderFlowEnums.Position _orderFlowPosition, _OrderFlowEnums.Style _orderFlowStyle, bool _realtimeHeuristic, float _maxClusterOpacity, float _minClusterOpacity, float _clustersOpacity, float _pOCLines_strokeWidth, DashStyleHelper _pOCLinesStrokeStyle, float _pOCLinesOpacity, float _pOILines_strokeWidth, DashStyleHelper _pOILinesStrokeStyle, float _pOILinesOpacity, float _askTextOpacity, float _bidTextOpacity, float _totalTextOpacity, SimpleFont _textFont, float _minFontWidth, float _minFontHeight, bool _showClusterPOC, bool _showPOCSLines, bool _showClusterPOI, bool _showPOISLines, bool _showText, bool _showOrderFlow)
		{
			if (cacheOrderFlow != null)
				for (int idx = 0; idx < cacheOrderFlow.Length; idx++)
					if (cacheOrderFlow[idx] != null && cacheOrderFlow[idx]._OrderFlowCalculation == _orderFlowCalculation && cacheOrderFlow[idx]._OrderFlowRepresentation == _orderFlowRepresentation && cacheOrderFlow[idx]._OrderFlowPosition == _orderFlowPosition && cacheOrderFlow[idx]._OrderFlowStyle == _orderFlowStyle && cacheOrderFlow[idx]._RealtimeHeuristic == _realtimeHeuristic && cacheOrderFlow[idx]._MaxClusterOpacity == _maxClusterOpacity && cacheOrderFlow[idx]._MinClusterOpacity == _minClusterOpacity && cacheOrderFlow[idx]._ClustersOpacity == _clustersOpacity && cacheOrderFlow[idx]._POCLines_strokeWidth == _pOCLines_strokeWidth && cacheOrderFlow[idx]._POCLinesStrokeStyle == _pOCLinesStrokeStyle && cacheOrderFlow[idx]._POCLinesOpacity == _pOCLinesOpacity && cacheOrderFlow[idx]._POILines_strokeWidth == _pOILines_strokeWidth && cacheOrderFlow[idx]._POILinesStrokeStyle == _pOILinesStrokeStyle && cacheOrderFlow[idx]._POILinesOpacity == _pOILinesOpacity && cacheOrderFlow[idx]._AskTextOpacity == _askTextOpacity && cacheOrderFlow[idx]._BidTextOpacity == _bidTextOpacity && cacheOrderFlow[idx]._TotalTextOpacity == _totalTextOpacity && cacheOrderFlow[idx]._TextFont == _textFont && cacheOrderFlow[idx]._MinFontWidth == _minFontWidth && cacheOrderFlow[idx]._MinFontHeight == _minFontHeight && cacheOrderFlow[idx]._ShowClusterPOC == _showClusterPOC && cacheOrderFlow[idx]._ShowPOCSLines == _showPOCSLines && cacheOrderFlow[idx]._ShowClusterPOI == _showClusterPOI && cacheOrderFlow[idx]._ShowPOISLines == _showPOISLines && cacheOrderFlow[idx]._ShowText == _showText && cacheOrderFlow[idx]._ShowOrderFlow == _showOrderFlow && cacheOrderFlow[idx].EqualsInput(input))
						return cacheOrderFlow[idx];
			return CacheIndicator<WyckoffZen.OrderFlow>(new WyckoffZen.OrderFlow(){ _OrderFlowCalculation = _orderFlowCalculation, _OrderFlowRepresentation = _orderFlowRepresentation, _OrderFlowPosition = _orderFlowPosition, _OrderFlowStyle = _orderFlowStyle, _RealtimeHeuristic = _realtimeHeuristic, _MaxClusterOpacity = _maxClusterOpacity, _MinClusterOpacity = _minClusterOpacity, _ClustersOpacity = _clustersOpacity, _POCLines_strokeWidth = _pOCLines_strokeWidth, _POCLinesStrokeStyle = _pOCLinesStrokeStyle, _POCLinesOpacity = _pOCLinesOpacity, _POILines_strokeWidth = _pOILines_strokeWidth, _POILinesStrokeStyle = _pOILinesStrokeStyle, _POILinesOpacity = _pOILinesOpacity, _AskTextOpacity = _askTextOpacity, _BidTextOpacity = _bidTextOpacity, _TotalTextOpacity = _totalTextOpacity, _TextFont = _textFont, _MinFontWidth = _minFontWidth, _MinFontHeight = _minFontHeight, _ShowClusterPOC = _showClusterPOC, _ShowPOCSLines = _showPOCSLines, _ShowClusterPOI = _showClusterPOI, _ShowPOISLines = _showPOISLines, _ShowText = _showText, _ShowOrderFlow = _showOrderFlow }, input, ref cacheOrderFlow);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.WyckoffZen.OrderFlow OrderFlow(_OrderFlowEnums.Calculation _orderFlowCalculation, _OrderFlowEnums.Representation _orderFlowRepresentation, _OrderFlowEnums.Position _orderFlowPosition, _OrderFlowEnums.Style _orderFlowStyle, bool _realtimeHeuristic, float _maxClusterOpacity, float _minClusterOpacity, float _clustersOpacity, float _pOCLines_strokeWidth, DashStyleHelper _pOCLinesStrokeStyle, float _pOCLinesOpacity, float _pOILines_strokeWidth, DashStyleHelper _pOILinesStrokeStyle, float _pOILinesOpacity, float _askTextOpacity, float _bidTextOpacity, float _totalTextOpacity, SimpleFont _textFont, float _minFontWidth, float _minFontHeight, bool _showClusterPOC, bool _showPOCSLines, bool _showClusterPOI, bool _showPOISLines, bool _showText, bool _showOrderFlow)
		{
			return indicator.OrderFlow(Input, _orderFlowCalculation, _orderFlowRepresentation, _orderFlowPosition, _orderFlowStyle, _realtimeHeuristic, _maxClusterOpacity, _minClusterOpacity, _clustersOpacity, _pOCLines_strokeWidth, _pOCLinesStrokeStyle, _pOCLinesOpacity, _pOILines_strokeWidth, _pOILinesStrokeStyle, _pOILinesOpacity, _askTextOpacity, _bidTextOpacity, _totalTextOpacity, _textFont, _minFontWidth, _minFontHeight, _showClusterPOC, _showPOCSLines, _showClusterPOI, _showPOISLines, _showText, _showOrderFlow);
		}

		public Indicators.WyckoffZen.OrderFlow OrderFlow(ISeries<double> input , _OrderFlowEnums.Calculation _orderFlowCalculation, _OrderFlowEnums.Representation _orderFlowRepresentation, _OrderFlowEnums.Position _orderFlowPosition, _OrderFlowEnums.Style _orderFlowStyle, bool _realtimeHeuristic, float _maxClusterOpacity, float _minClusterOpacity, float _clustersOpacity, float _pOCLines_strokeWidth, DashStyleHelper _pOCLinesStrokeStyle, float _pOCLinesOpacity, float _pOILines_strokeWidth, DashStyleHelper _pOILinesStrokeStyle, float _pOILinesOpacity, float _askTextOpacity, float _bidTextOpacity, float _totalTextOpacity, SimpleFont _textFont, float _minFontWidth, float _minFontHeight, bool _showClusterPOC, bool _showPOCSLines, bool _showClusterPOI, bool _showPOISLines, bool _showText, bool _showOrderFlow)
		{
			return indicator.OrderFlow(input, _orderFlowCalculation, _orderFlowRepresentation, _orderFlowPosition, _orderFlowStyle, _realtimeHeuristic, _maxClusterOpacity, _minClusterOpacity, _clustersOpacity, _pOCLines_strokeWidth, _pOCLinesStrokeStyle, _pOCLinesOpacity, _pOILines_strokeWidth, _pOILinesStrokeStyle, _pOILinesOpacity, _askTextOpacity, _bidTextOpacity, _totalTextOpacity, _textFont, _minFontWidth, _minFontHeight, _showClusterPOC, _showPOCSLines, _showClusterPOI, _showPOISLines, _showText, _showOrderFlow);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.WyckoffZen.OrderFlow OrderFlow(_OrderFlowEnums.Calculation _orderFlowCalculation, _OrderFlowEnums.Representation _orderFlowRepresentation, _OrderFlowEnums.Position _orderFlowPosition, _OrderFlowEnums.Style _orderFlowStyle, bool _realtimeHeuristic, float _maxClusterOpacity, float _minClusterOpacity, float _clustersOpacity, float _pOCLines_strokeWidth, DashStyleHelper _pOCLinesStrokeStyle, float _pOCLinesOpacity, float _pOILines_strokeWidth, DashStyleHelper _pOILinesStrokeStyle, float _pOILinesOpacity, float _askTextOpacity, float _bidTextOpacity, float _totalTextOpacity, SimpleFont _textFont, float _minFontWidth, float _minFontHeight, bool _showClusterPOC, bool _showPOCSLines, bool _showClusterPOI, bool _showPOISLines, bool _showText, bool _showOrderFlow)
		{
			return indicator.OrderFlow(Input, _orderFlowCalculation, _orderFlowRepresentation, _orderFlowPosition, _orderFlowStyle, _realtimeHeuristic, _maxClusterOpacity, _minClusterOpacity, _clustersOpacity, _pOCLines_strokeWidth, _pOCLinesStrokeStyle, _pOCLinesOpacity, _pOILines_strokeWidth, _pOILinesStrokeStyle, _pOILinesOpacity, _askTextOpacity, _bidTextOpacity, _totalTextOpacity, _textFont, _minFontWidth, _minFontHeight, _showClusterPOC, _showPOCSLines, _showClusterPOI, _showPOISLines, _showText, _showOrderFlow);
		}

		public Indicators.WyckoffZen.OrderFlow OrderFlow(ISeries<double> input , _OrderFlowEnums.Calculation _orderFlowCalculation, _OrderFlowEnums.Representation _orderFlowRepresentation, _OrderFlowEnums.Position _orderFlowPosition, _OrderFlowEnums.Style _orderFlowStyle, bool _realtimeHeuristic, float _maxClusterOpacity, float _minClusterOpacity, float _clustersOpacity, float _pOCLines_strokeWidth, DashStyleHelper _pOCLinesStrokeStyle, float _pOCLinesOpacity, float _pOILines_strokeWidth, DashStyleHelper _pOILinesStrokeStyle, float _pOILinesOpacity, float _askTextOpacity, float _bidTextOpacity, float _totalTextOpacity, SimpleFont _textFont, float _minFontWidth, float _minFontHeight, bool _showClusterPOC, bool _showPOCSLines, bool _showClusterPOI, bool _showPOISLines, bool _showText, bool _showOrderFlow)
		{
			return indicator.OrderFlow(input, _orderFlowCalculation, _orderFlowRepresentation, _orderFlowPosition, _orderFlowStyle, _realtimeHeuristic, _maxClusterOpacity, _minClusterOpacity, _clustersOpacity, _pOCLines_strokeWidth, _pOCLinesStrokeStyle, _pOCLinesOpacity, _pOILines_strokeWidth, _pOILinesStrokeStyle, _pOILinesOpacity, _askTextOpacity, _bidTextOpacity, _totalTextOpacity, _textFont, _minFontWidth, _minFontHeight, _showClusterPOC, _showPOCSLines, _showClusterPOI, _showPOISLines, _showText, _showOrderFlow);
		}
	}
}

#endregion
