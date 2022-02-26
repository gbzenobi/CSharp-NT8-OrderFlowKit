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

public static class _VolumeAnalysisProfileEnums
{
	public enum Formula
	{
		Total,
		Delta,
		BidAsk,
		TotalAndBidAsk,
		TotalAndDelta,
		TotalAndDeltaAndBidAsk
	}
	public enum RenderInfo
	{
		BidAsk,
		TotalAndDelta,
		Total,
		Delta		
	}
}

namespace NinjaTrader.NinjaScript.Indicators.WyckoffZen
{
	public class VolumeAnalysisProfile : Indicator
	{
		#region MAIN
		
		public class WyckoffVolumeProfile : WyckoffRenderControl
		{
			private VolumeAnalysis.WyckoffBars wyckoffBars;
			private VolumeAnalysis.Profile marketVolumeProfile;
			private VolumeAnalysis.Profile rangeVolumeProfile;
			
			private float POCOpacity;
			private float POIOpacity;
			private float selectionOpacity;
			private float eraseOpacity;
			private int timeFrame;
			private bool isRealtime;
			private DateTime beginTime;
			private Func<int, int> calculateBars;
			private Action<int, int, int, double, VolumeAnalysis.MarketOrder> renderVolumeFormula;
			private Action<int, VolumeAnalysis.MarketOrder, VolumeAnalysis.Profile.Ladder> renderVolumeInfo;
			private SharpDX.DirectWrite.TextFormat volumeTextFormat;
			private SharpDX.RectangleF Rect;
			private float minFontWidth;
			private float minFontHeight;
			private bool showTotalVolumeInfo;
			private bool showDeltaInfo;
			private bool showFont;
			private bool showPOC;
			private bool showPOI;
			
			private SharpDX.Direct2D1.Brush gColor;
			private SharpDX.Direct2D1.Brush gColorFont;
			
			private SharpDX.Color colorBid;
			private SharpDX.Color colorAsk;
			private SharpDX.Color colorTotal;
			private SharpDX.Color colorPOC;
			private SharpDX.Color colorPOI;
			private SharpDX.Color colorFont;
			private SharpDX.Color colorSelection;
			private SharpDX.Color colorErase;
			
			private bool key_AddProfile;
			private bool click_AddProfile;
			private bool key_DeleteProfile;
			private SharpDX.Vector2 currentMouseCoords;
			private SharpDX.Vector2 firstMouseCoords;
			private SharpDX.RectangleF rectCoords;
			
			public WyckoffVolumeProfile()
			{
				this.Rect = new SharpDX.RectangleF();
				this.isRealtime= false;
				this.key_AddProfile = this.click_AddProfile = false;
				this.key_DeleteProfile = false;
				this.currentMouseCoords = new SharpDX.Vector2();
				this.firstMouseCoords = new SharpDX.Vector2();
				this.rectCoords = new SharpDX.Rectangle();
			}
			
			#region SETS
			
			public void setFontStyle(SimpleFont font)
			{
				base.setFontStyle(font, out volumeTextFormat);
			}
			public void setShowFont(bool showFont, float minFontWidth, float minFontHeight)
			{
				this.showFont= showFont;
				this.minFontWidth = minFontWidth;
				this.minFontHeight = minFontHeight;
			}
			
			public void setRealtime(bool isRealtime){ this.isRealtime = isRealtime; }
			public bool IsRealtime{ get{ return this.isRealtime; } }
			// !- obtenemos las barras segun la temporalidad elegida
			public int getCalculatedBars(int period)
			{
				return this.calculateBars(period);
			}
			public void setWyckoffBars(VolumeAnalysis.WyckoffBars wyckoffBars)
			{
				this.wyckoffBars = wyckoffBars;
				// !- Valor del timeframe en el que estamos
				this.timeFrame = wyckoffBars.NT8Bars.BarsType.BarsPeriod.Value;
				//this.lastBar = wyckoffBars.NT8Bars.Count - 1;
			}
			public void setVolumeProfile(VolumeAnalysis.Profile marketVolumeProfile, VolumeAnalysis.Profile rangeVolumeProfile)
			{
				this.marketVolumeProfile = marketVolumeProfile;
				this.rangeVolumeProfile = rangeVolumeProfile;
			}
			
			public void setProfileBidAskColor(Brush brushBidColor, Brush brushAskColor, Brush brushTotalColor)
			{
				this.colorBid = WyckoffRenderControl.BrushToColor(brushBidColor);
				this.colorAsk = WyckoffRenderControl.BrushToColor(brushAskColor);
				this.colorTotal = WyckoffRenderControl.BrushToColor(brushTotalColor);
			}
			public void setProfileCalculationColor(Brush brushPOCColor, Brush brushPOIColor)
			{
				this.colorPOC = WyckoffRenderControl.BrushToColor(brushPOCColor);
				this.colorPOI = WyckoffRenderControl.BrushToColor(brushPOIColor);
			}
			public void setCursorStyle(Brush brushSelectionColor, float selectionOpacity, Brush brushEraseColor, float eraseOpacity)
			{
				this.colorSelection = WyckoffRenderControl.BrushToColor(brushSelectionColor);
				this.selectionOpacity = selectionOpacity / 100f;
				this.colorErase = WyckoffRenderControl.BrushToColor(brushEraseColor);
				this.eraseOpacity = eraseOpacity / 100f;
			}
			public void setFontColor(Brush brushFontColor)
			{
				this.colorFont = WyckoffRenderControl.BrushToColor(brushFontColor);
			}
			public void setCalculationsOpacity(float POCOpacity, float POIOpacity)
			{
				this.POCOpacity = POCOpacity / 100.0f;
				this.POIOpacity = POIOpacity / 100.0f;
			}
			public void setShowInfo(bool showTotalVolumeInfo, bool showDeltaInfo)
			{
				this.showTotalVolumeInfo = showTotalVolumeInfo;
				this.showDeltaInfo = showDeltaInfo;
			}
			public void setShowCalculations(bool showPOC, bool showPOI)
			{
				this.showPOC = showPOC;
				this.showPOI = showPOI;
			}
			
			#endregion
			// !- debe ser cargada desde DataLoaded
			#region BARS_CALCULATION
			
			// !- Calculos solo usados para el render de rango de barras en la pantalla
//			private int _defaultTotalBars(int bars){ return bars; }
			private int _TotalBarsByMinutes(int Minutes){ return Minutes/this.timeFrame; }
			private int _TotalBarsByHours(int Hours){ return (Hours*60)/this.timeFrame; }
			private int _TotalBarsByDays(int Days){ return (Days*24*60)/this.timeFrame; }
			public void setBarsPeriodFormula(VolumeAnalysis.PeriodMode periodMode)
			{
				switch( periodMode )
				{
//					case VolumeAnalysis.PeriodMode.Bars:
//					{
//						this.calculateBars = this._defaultTotalBars;
//						break;
//					}
					case VolumeAnalysis.PeriodMode.Minutes:
					{
						this.calculateBars = this._TotalBarsByMinutes;
						break;
					}
					case VolumeAnalysis.PeriodMode.Hours:
					{
						this.calculateBars = this._TotalBarsByHours;
						break;
					}
					case VolumeAnalysis.PeriodMode.Days:
					{
						this.calculateBars = this._TotalBarsByDays;
						break;
					}
				}
			}
			
			#endregion
			#region RANGE_PROFILE_KEY_ACTIVATION
			
			// !- este evento se dispara continuamente mientras este pulsada la tecla en cuestion
			private void resetMouseCoords()
			{
				this.firstMouseCoords.X = currentMouseCoords.X = 0;
				//this.firstMouseCoords.Y = currentMouseCoords.Y = 0;
			}
			public bool onKeyDown(System.Windows.Input.KeyEventArgs e)
		    {
		        if(e.Key == Key.LeftCtrl && !this.click_AddProfile){// || e.Key == Key.Space || e.Key == Key.LeftAlt || e.Key == Key.LeftShift)
		            this.key_AddProfile = true;
					return true;
		        }
				if(e.Key == Key.LeftShift){
					this.key_DeleteProfile = true;
					return true;
				}
				return false;
		    }
		    public bool onKeyUp(System.Windows.Input.KeyEventArgs e){
				if(e.Key == Key.LeftCtrl){
					this.key_AddProfile = this.click_AddProfile = false;
					this.resetMouseCoords();
					return true;
		        }
				if(e.Key == Key.LeftShift){
					this.key_DeleteProfile = false;
					this.resetMouseCoords();
					return true;
				}
				return false;
		    }
			public bool mouseMoveEvent(MouseEventArgs mouseEvent, ChartPanel chartPanel)
			{
				if( this.key_AddProfile )//bool isDown = Keyboard.IsKeyDown( this.keyCode );
				{
					Point mouseCoords = mouseEvent.GetPosition(chartPanel);
					int mX = ChartingExtensions.ConvertToHorizontalPixels(mouseCoords.X, CHART_CONTROL.PresentationSource); //int mX = this.chartControl.MouseDownPoint.X.ConvertToHorizontalPixels(chartControl.PresentationSource);
					//int mY = ChartingExtensions.ConvertToVerticalPixels(mouseCoords.Y, this.chartControl.PresentationSource);
					//int barX = this.chartControl.GetXByBarIndex(this.chartBars, this.chartBars.GetBarIdxByX(this.chartControl, mX));
					//int barY = this.chartScale.GetYByValue(this.chartScale.GetValueByY(mY));
					if( this.firstMouseCoords.X == 0 ){//&& this.firstMouseCoords.Y == 0 ){
						this.firstMouseCoords.X = mX;
						//this.firstMouseCoords.Y = mY;
					}
					else{
						this.currentMouseCoords.X = mX;
						//this.currentMouseCoords.Y = mY;
					}
					return true;
				}
				if( this.key_DeleteProfile ){
					Point mouseCoords = mouseEvent.GetPosition(chartPanel);
					int mX = ChartingExtensions.ConvertToHorizontalPixels(mouseCoords.X, CHART_CONTROL.PresentationSource); //int mX = this.chartControl.MouseDownPoint.X.ConvertToHorizontalPixels(chartControl.PresentationSource);
//					int mY = ChartingExtensions.ConvertToVerticalPixels(mouseCoords.Y, this.chartControl.PresentationSource);
					int barIndex = this.rangeVolumeProfile.GetProfileInRange(CHART_BARS.GetBarIdxByX(CHART_CONTROL, mX));
					// !- si no esta dentro del rango reseteamos las coordenadas
					if( barIndex == -1 ){
						this.resetMouseCoords();
						return false;
					}
					if( this.firstMouseCoords.X == 0 ){
						this.firstMouseCoords.X = CHART_CONTROL.GetXByBarIndex(CHART_BARS, this.rangeVolumeProfile.StartBarIndex(barIndex));
					}
					else{
						this.currentMouseCoords.X = CHART_CONTROL.GetXByBarIndex(CHART_BARS, barIndex);
					}
				}
//				if( Keyboard.IsKeyUp( this.keyCode ) ){
//					this.keyPressed = 0;
//					isDown = false;
//				}
				return false;
			}
			public bool mouseClicked()//MouseButtonEventArgs mouseEvent)
			{
				// convert e.GetPosition for different dpi settings
				//clickPoint.X = ChartingExtensions.ConvertToHorizontalPixels(mouseEvent.GetPosition(this.chartControl as IInputElement).X, ChartControl.PresentationSource);
				//clickPoint.Y = ChartingExtensions.ConvertToVerticalPixels(mouseEvent.GetPosition(this.chartControl as IInputElement).Y, ChartControl.PresentationSource);
				//convertedPrice = Instrument.MasterInstrument.RoundToTickSize(chartScale.GetValueByY((float)clickPoint.Y));
				//DateTime convertedTime = ChartControl.GetTimeBySlotIndex((int)ChartControl.GetSlotIndexByX((int)clickPoint.X));
				//int mousePointX = chartControl.MouseDownPoint.X.ConvertToHorizontalPixels(chartControl.PresentationSource);
				if( this.key_AddProfile ){
					int firstBarX = CHART_BARS.GetBarIdxByX(CHART_CONTROL, (int)this.firstMouseCoords.X);
					int lastBarX = CHART_BARS.GetBarIdxByX(CHART_CONTROL, (int)this.currentMouseCoords.X);
					// !- minimos de barras: 2
					if( Math.Abs(firstBarX - lastBarX) <= 1 ){
						return false;
					}
					this.rangeVolumeProfile.AddRangeProfile(Math.Min(firstBarX, lastBarX), Math.Max(firstBarX, lastBarX));
					this.click_AddProfile= true;
					this.key_AddProfile = false;
					
					return true;
				}
				if( this.key_DeleteProfile ){
					int barIndex = CHART_BARS.GetBarIdxByX(CHART_CONTROL, (int)this.currentMouseCoords.X);
					barIndex = this.rangeVolumeProfile.GetProfileInRange(barIndex);
					if( this.rangeVolumeProfile.Exists(barIndex) ){
						this.rangeVolumeProfile.RemoveProfile( barIndex );
						// !- reseteamos para no ver las coordenadas del perfil borrado
						this.resetMouseCoords();
						return true;
					}
				}
				
				return false;
			}
			
			#endregion
			#region RENDER_VOLUME_INFO
			
			private bool _setVolumeInfo(int barY, VolumeAnalysis.Profile.Ladder profileLadder)
			{
				if( this.showFont && this.W >= this.minFontWidth && this.H >= this.minFontHeight ){
					int totalBars	= profileLadder.TotalBars;
					int startBar	= profileLadder.StartBarIndex;
					
					this.Rect.X = CHART_CONTROL.GetXByBarIndex(CHART_BARS, startBar) - (this.W / 2);
					this.Rect.Y = barY - (this.H / 2f);
					this.Rect.Width = totalBars + this.W;
					this.Rect.Height = this.H;
					return true;
				}
				return false;
			}
			private void _renderBidAskVolumeInfo(int barY, VolumeAnalysis.MarketOrder marketOrder, VolumeAnalysis.Profile.Ladder profileLadder)
			{
				if( this._setVolumeInfo(barY, profileLadder) ){
					myDrawText(string.Format("{0} x {1}", marketOrder.Bid, marketOrder.Ask), ref Rect, colorFont, -1, -1, volumeTextFormat, 1.0f);
				}
			}
			private void _renderTotalDeltaVolumeInfo(int barY, VolumeAnalysis.MarketOrder marketOrder, VolumeAnalysis.Profile.Ladder profileLadder)
			{
				if( this._setVolumeInfo(barY, profileLadder) ){
					myDrawText(string.Format("{0} x {1}", marketOrder.Total, marketOrder.Delta), ref Rect, colorFont, -1, -1, volumeTextFormat, 1.0f);
				}
			}
			private void _renderDeltaVolumeInfo(int barY, VolumeAnalysis.MarketOrder marketOrder, VolumeAnalysis.Profile.Ladder profileLadder)
			{
				if( this._setVolumeInfo(barY, profileLadder) ){
					myDrawText(marketOrder.Delta.ToString(), ref Rect, colorFont, -1, -1, volumeTextFormat, 1.0f);
				}
			}
			private void _renderTotalVolumeInfo(int barY, VolumeAnalysis.MarketOrder marketOrder, VolumeAnalysis.Profile.Ladder profileLadder)
			{
				if( this._setVolumeInfo(barY, profileLadder) ){
					myDrawText(marketOrder.Total.ToString(), ref Rect, colorFont, -1, -1, volumeTextFormat, 1.0f);
				}
			}
			public void setVolumeRenderInfo(_VolumeAnalysisProfileEnums.RenderInfo renderInfo)
			{
				switch( renderInfo )
				{
					case _VolumeAnalysisProfileEnums.RenderInfo.BidAsk:
					{
						this.renderVolumeInfo = this._renderBidAskVolumeInfo;
						break;
					}
					case _VolumeAnalysisProfileEnums.RenderInfo.Total:
					{
						this.renderVolumeInfo = this._renderTotalVolumeInfo;
						break;
					}
					case _VolumeAnalysisProfileEnums.RenderInfo.Delta:
					{
						this.renderVolumeInfo = this._renderDeltaVolumeInfo;
						break;
					}
					case _VolumeAnalysisProfileEnums.RenderInfo.TotalAndDelta:
					{
						this.renderVolumeInfo = this._renderTotalDeltaVolumeInfo;
						break;
					}
				}
			}
			
			#endregion
			#region RENDER_VOLUME_FORMULA
			
			private void _renderTotalVolume(
				int currentBar, int totalBars, int barY,
				double maxVolume, VolumeAnalysis.MarketOrder marketOrder)
			{
				double volPercent = Math2.Percent(maxVolume, marketOrder.Total);
				this.calculatePriceLadder(currentBar, totalBars, barY, volPercent);
				myFillRectangle(ref Rect, colorTotal, (float)volPercent / 100f);
			}
			private void _renderDeltaVolume(
				int currentBar, int totalBars, int barY,
				double maxVolume, VolumeAnalysis.MarketOrder marketOrder)
			{
				double delta = marketOrder.Delta;
				double volPercent = Math2.Percent(maxVolume, Math.Abs(delta));
				
				calculatePriceLadder(currentBar, totalBars, barY, volPercent);
				if( delta < 0 ){
					myFillRectangle(ref Rect, colorBid, 1.0f);
				}
				else{
					myFillRectangle(ref Rect, colorAsk, 1.0f);
				}
			}
			private void _renderBidAskVolume(
				int currentBar, int totalBars, int barY,
				double maxVolume, VolumeAnalysis.MarketOrder marketOrder)
			{
				double volPercent;
				
				volPercent = Math2.Percent(maxVolume, marketOrder.Bid);
				this.calculatePriceLadder(currentBar, totalBars, barY, volPercent);
				myFillRectangle(ref Rect, colorBid, (float)volPercent / 100f);
				
				volPercent = Math2.Percent(maxVolume, marketOrder.Ask);
				this.calculatePriceLadder(currentBar, totalBars, barY, volPercent);
				myFillRectangle(ref Rect, colorAsk, (float)volPercent / 100f);
			}
			private void _renderTotalAndBidAsk(int currentBar, int totalBars, int barY,
				double maxVolume, VolumeAnalysis.MarketOrder marketOrder)
			{
				_renderTotalVolume (currentBar, totalBars, barY, maxVolume, marketOrder);
				_renderBidAskVolume(currentBar, totalBars, barY, maxVolume, marketOrder);
			}
			private void _renderTotalAndDelta(int currentBar, int totalBars, int barY,
				double maxVolume, VolumeAnalysis.MarketOrder marketOrder)
			{
				_renderTotalVolume(currentBar, totalBars, barY, maxVolume, marketOrder);
				_renderDeltaVolume(currentBar, totalBars, barY, maxVolume, marketOrder);
			}
			private void _renderTotalAndDeltaAndBidAsk(int currentBar, int totalBars, int barY,
				double maxVolume, VolumeAnalysis.MarketOrder marketOrder)
			{
				_renderTotalVolume(currentBar, totalBars, barY, maxVolume, marketOrder);
				_renderDeltaVolume(currentBar, totalBars, barY, maxVolume, marketOrder);
				_renderBidAskVolume(currentBar, totalBars, barY, maxVolume, marketOrder);
			}
			
			public void setVolumeFormula(_VolumeAnalysisProfileEnums.Formula volumeFormula)
			{
				switch( volumeFormula )
				{
					case _VolumeAnalysisProfileEnums.Formula.Total:
					{
						this.renderVolumeFormula = this._renderTotalVolume;
						break;
					}
					case _VolumeAnalysisProfileEnums.Formula.Delta:
					{
						this.renderVolumeFormula = this._renderDeltaVolume;
						break;
					}
					case _VolumeAnalysisProfileEnums.Formula.BidAsk:
					{
						this.renderVolumeFormula = this._renderBidAskVolume;
						break;
					}
					case _VolumeAnalysisProfileEnums.Formula.TotalAndBidAsk:
					{
						this.renderVolumeFormula = this._renderTotalAndBidAsk;
						break;
					}
					case _VolumeAnalysisProfileEnums.Formula.TotalAndDelta:
					{
						this.renderVolumeFormula = this._renderTotalAndDelta;
						break;
					}
					case _VolumeAnalysisProfileEnums.Formula.TotalAndDeltaAndBidAsk:
					{
						this.renderVolumeFormula = this._renderTotalAndDeltaAndBidAsk;
						break;
					}
				}
			}
			
			#endregion
			#region RENDER_PROFILE
			
			public void renderMessageInfo(string textInfo, int X, int Y, SharpDX.Color4 textColor, float fontSize) //SharpDX.Color4 textLayoutColor, int fontSize)//SharpDX.Color.Beige, SharpDX.Color.White
			{
				SharpDX.Vector2 startPoint = new SharpDX.Vector2(X, Y);
				SharpDX.DirectWrite.TextFormat textFormat= new SharpDX.DirectWrite.TextFormat(Core.Globals.DirectWriteFactory, "Arial", fontSize);
				SharpDX.DirectWrite.TextLayout textLayout = new SharpDX.DirectWrite.TextLayout(Core.Globals.DirectWriteFactory,
					textInfo, textFormat, this.PanelW, this.PanelH);
				SharpDX.RectangleF msgRect = new SharpDX.RectangleF(startPoint.X, startPoint.Y,
					textLayout.Metrics.Width, textLayout.Metrics.Height);
				SharpDX.Direct2D1.SolidColorBrush textDXBrush = new SharpDX.Direct2D1.SolidColorBrush(RENDER_TARGET, textColor);
	 			
				// execute the render target draw rectangle with desired values
//				if(textLayoutColor != null){
//					SharpDX.Direct2D1.SolidColorBrush layoutDXBrush = new SharpDX.Direct2D1.SolidColorBrush(Target, textLayoutColor);
//					Target.DrawRectangle(msgRect, layoutDXBrush);
//					layoutDXBrush.Dispose();
//				}
				// execute the render target text layout command with desired values
				RENDER_TARGET.DrawTextLayout(startPoint, textLayout, textDXBrush);
				
				textLayout.Dispose();
				textFormat.Dispose();
				textDXBrush.Dispose();
			}
			
			private void calculatePriceLadder(
				int currentBar, int totalBars, int barY,
				double volumePercent)
			{
				// !- Calculamos las barras que hay en el periodo de tiempo elejido
				int barXfrom = CHART_CONTROL.GetXByBarIndex(CHART_BARS, currentBar - totalBars);
				int _barXto = currentBar - totalBars - (int)Math.Round((volumePercent * totalBars) / 100);
				//R.Width = W;
				Rect.X = barXfrom - (this.W / 2f);
				Rect.Y = barY - (this.H / 2f);
				Rect.Width = barXfrom - CHART_CONTROL.GetXByBarIndex(CHART_BARS, _barXto) + this.W;
				Rect.Height= this.H;
			}
			// !- para POCs y POIs
			private void renderPO(
				int currentBar, int totalBars, int barY,
				double maxLadderVolume, double currentVolume, SharpDX.Color color, float opacity)
			{
				double volPercent = Math2.Percent(maxLadderVolume, currentVolume);
				// !- El valor maximo de volumen representa el 100%, el cual es el POC
				if( volPercent == 100 )
				{
					this.calculatePriceLadder(currentBar, totalBars, barY, volPercent);
					myFillRectangle(ref Rect, color, opacity);
				}
			}
			// !- informacion delta y total del profile
			private void renderProfileInfo(int currentBar, VolumeAnalysis.Profile.Ladder profileLadder)
			{
				int totalBars = profileLadder.TotalBars;
				//if( (this.W - 1) > totalBars ) return;
				int barXfrom = CHART_CONTROL.GetXByBarIndex(CHART_BARS, currentBar - totalBars);
				int barXto = CHART_CONTROL.GetXByBarIndex(CHART_BARS, currentBar);
				
				Rect.Width = barXto - barXfrom;
				Rect.Height = this.H;
				Rect.X = barXfrom;
				// !- Obtenemos el precio mas alto del perfil
				VolumeAnalysis.MarketOrder marketOrder = profileLadder.ProfileVolume;
				// !- Renderizamos la funcion correcta de texto(elejida por el usuario)
				if( this.showTotalVolumeInfo ){
					//Rect.Y = chartScale.GetYByValue(vp.GetLadderHighPrice(currentBar)) - (this.H * 8);
					Rect.Y = CHART_SCALE.GetYByValue(profileLadder.HighPrice) - (this.H * 4f);
					//Target.DrawText("V:", this.volumeTextFormat, Rect, brushFontColor.ToDxBrush(Target));
					myDrawText(string.Format("V:{0}", marketOrder.Total), ref Rect, colorTotal, -1, -1, volumeTextFormat, 1.0f);
				}
				if( this.showDeltaInfo ){
					Rect.Y = CHART_SCALE.GetYByValue(profileLadder.LowPrice) + (this.H * 4f);
					//Target.DrawText("D:", this.volumeTextFormat, Rect, brushFontColor.ToDxBrush(Target));
					long delta = marketOrder.Delta;
					if( delta >= 0 ){
						myDrawText(string.Format("D:{0}", delta), ref Rect, colorAsk, -1, -1, volumeTextFormat, 1.0f);
					}
					else{
						myDrawText(string.Format("D:{0}", delta), ref Rect, colorBid, -1, -1, volumeTextFormat, 1.0f);
					}
				}
			}
			private void _renderProfile(int barIndex, VolumeAnalysis.Profile.Ladder profileLadder, int totalBars)
			{
				VolumeAnalysis.MarketOrder mo;
				long totalVol;
				int barY;
				double price;
				double maxLadderVol = profileLadder.MaxVolume.Total;
				double minLadderVol = profileLadder.MinVolume.Total;
				double maxLadderPrice = profileLadder.MaxLadderPrice;
				double minLadderPrice = profileLadder.MinLadderPrice;
				
				//Stopwatch sw = new Stopwatch(); sw.Start();
				foreach(var p in profileLadder)
				{
					// !- Informacion de volumen del precio en el ladder
					price = p.Key;
					mo = p.Value;
					
					barY = CHART_SCALE.GetYByValue(p.Key);
					totalVol = mo.Total;
					// !- Renderizamos el volumen segun la formula(Total, Bid, Ask, Delta, etc..)
					renderVolumeFormula(barIndex, totalBars, barY, maxLadderVol, mo);
					// !- Renderizamos el POC(Point of control)
					/// REVISAR ESTO EN UN FUTURO...
					if( this.showPOC && price == maxLadderPrice  ){
						renderPO(barIndex, totalBars, barY, maxLadderVol, totalVol, this.colorPOC, POCOpacity);
					}
					// !- Renderizamos el POI(Point of imbalance)
					if( this.showPOI && price == minLadderPrice ){
						renderPO(barIndex, totalBars, barY, minLadderVol, totalVol, this.colorPOI, POIOpacity);
					}
					// !- Renderizamos en texto la informacion de volumen total
					renderVolumeInfo(barY, mo, profileLadder);
				}
//				int barX = chartControl.GetXByBarIndex(chartBars, barIndex);
//				Target.DrawLine(new SharpDX.Vector2(barX, chartScale.GetYByValue(pl.HighPrice)), new SharpDX.Vector2(barX, chartScale.GetYByValue(pl.LowPrice)), Brushes.Salmon.ToDxBrush(Target, 0.3f), this.W / 2f);
				//sw.Stop(); Print(string.Format("ms:{0}", sw.ElapsedMilliseconds));
				renderProfileInfo(barIndex, profileLadder);
			}
			private void _renderCursor(bool keyIsPressed, SharpDX.Color color, float opacity)
			{
				if( !keyIsPressed || this.firstMouseCoords.X == 0 || this.currentMouseCoords.X == 0 ){
					return;
				}
//				Target.DrawLine(this.currentMouseCoords, this.firstMouseCoords, Brushes.Goldenrod.ToDxBrush(Target, 0.7f), 1.0f);
				this.rectCoords.X = currentMouseCoords.X;
				this.rectCoords.Y = 0;//currentMouseCoords.Y - this.PanelH;
				this.rectCoords.Width = firstMouseCoords.X - currentMouseCoords.X;
				this.rectCoords.Height = this.PanelH;//*2f;//firstMouseCoords.Y - currentMouseCoords.Y
				myFillRectangle(ref rectCoords, color, opacity);
				//Target.DrawLine(this.currentMouseCoords, this.firstMouseCoords, Brushes.Crimson.ToDxBrush(Target, 1.0f), 1.0f);
			}
			public void renderCursor()
			{
				_renderCursor(this.key_AddProfile, this.colorSelection, selectionOpacity);
				_renderCursor(this.key_DeleteProfile, this.colorErase, eraseOpacity);
			}
			public void renderProfile(int barIndex)
			{
				if( this.marketVolumeProfile == null ){
					return;
				}
				if( marketVolumeProfile.Exists(barIndex) ){
					/// !- quitamos una barra de adelante y una de atras, no las necesitamos(de otro modo habria desbordamiento de grafico)
					// esto sucede en la funcion .calculatePriceLadder(...) ya que los calculos de perfil de volumen en tiempo de mercado exigen
					// que la barra hasta donde se calcula el N-volume profile siempre sea + 1(es decir termina donde empieza al siguiente)
					_renderProfile(barIndex - 1, marketVolumeProfile.GetProfile(barIndex), marketVolumeProfile.TotalBars(barIndex) - 1);
				}
			}
			public void renderRealtimeProfile()
			{
				VolumeAnalysis.Profile.Ladder tmp = this.marketVolumeProfile.GetRealtimeProfile;
				if( tmp == null ){
					return;
				}
				_renderProfile(this.wyckoffBars.CurrentBarIndex, tmp, tmp.TotalBars);
			}
			public void renderRangeProfile()
			{
				if( this.rangeVolumeProfile == null ){
					return;
				}
				foreach(var vp in this.rangeVolumeProfile){
					_renderProfile(vp.Key, vp.Value, vp.Value.TotalBars);
				}
			}
			
			#endregion
		} // WyckoffVolumeProfile
		
		#endregion
		#region GLOBAL_VARIABLES
		
		private VolumeAnalysis.WyckoffBars wyckoffBars;
		private VolumeAnalysis.Profile volumeProfile;
		private VolumeAnalysis.Profile rangeVolumeProfile;
		private WyckoffVolumeProfile wyckoffVP;
		
		#endregion
		#region INDICATOR_SETUP
		
		private void setStyle()
		{
			_TotalVolColor = Brushes.CornflowerBlue;
			_AskVolColor = Brushes.Green;
			_BidVolColor = Brushes.Red;
			_POCColor = Brushes.Tan;
			_POIColor = Brushes.Indigo;
			_SelectionColor = Brushes.Navy;
			_SelectionOpacity = 10f;
			_EraseColor = Brushes.Crimson;
			_EraseOpacity = 10f;
			
			_FontColor = Brushes.LightYellow;			
			// *- 90%
			_POCOpacity = 90f;
			// *- 80%
			_POIOpacity = 80f;
			_TextOpacity = 100f;
		}
		private void setCalculations()
		{
			_PeriodMode = VolumeAnalysis.PeriodMode.Days;
			_VolumeFormula = _VolumeAnalysisProfileEnums.Formula.TotalAndBidAsk;
			_VolumeRenderInfo = _VolumeAnalysisProfileEnums.RenderInfo.Total;
			// !- 1 dia de perfil de volumen por defecto
			_Period = 1;
			_showTotalVolumeInfo = true;
			_showDeltaInfo = true;
			_ShowPOC = true;
			_ShowPOI = true;
			_RealtimeHeuristic = true;
			// !- si este valor es false entonces el perfil de volumen NO es calculado sobre el grafico
			_EnableMarketProfile= true;
		}
		private void setFontStyle()
		{
			_TextFont = new SimpleFont();
			_TextFont.Family = new FontFamily("Arial");
			_TextFont.Size = 10f;
			_TextFont.Bold = false;
			_TextFont.Italic= false;
			_ShowText = true;
			_MinFontWidth = 1f;
			_MinFontHeight = 8f;
		}
		
		#endregion
		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description									= @"";
				Name										= "Volume Analysis Profile";
				Calculate									= Calculate.OnBarClose;
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
				
				setStyle();
				setCalculations();
				setFontStyle();
				
				wyckoffVP = new WyckoffVolumeProfile();
			}
			else if (State == State.Configure)
			{
				wyckoffVP.setProfileBidAskColor(_BidVolColor, _AskVolColor, _TotalVolColor);
				wyckoffVP.setProfileCalculationColor(_POCColor, _POIColor);
				wyckoffVP.setCursorStyle(_SelectionColor, _SelectionOpacity,_EraseColor, _EraseOpacity);
				wyckoffVP.setFontStyle(_TextFont);
				wyckoffVP.setFontColor(_FontColor);
				wyckoffVP.setShowFont(_ShowText, _MinFontWidth, _MinFontHeight);
				
				wyckoffVP.setShowCalculations(_ShowPOC, _ShowPOI);
				wyckoffVP.setVolumeRenderInfo(_VolumeRenderInfo);
				
				wyckoffVP.setCalculationsOpacity(_POCOpacity, _POIOpacity);
				wyckoffVP.setVolumeFormula(_VolumeFormula);
				wyckoffVP.setShowInfo(_showTotalVolumeInfo, _showDeltaInfo);
				
				// !- Seteamos la formula correcta para calcular las barras totales en cada perfil de volumen
				wyckoffVP.setBarsPeriodFormula(_PeriodMode);
				
				Calculate = Calculate.OnEachTick;
			}
			else if(State == State.DataLoaded)
			{
				wyckoffBars = new VolumeAnalysis.WyckoffBars(Bars);
				wyckoffVP.setWyckoffBars(wyckoffBars);
				
				/// !- menor a 6 barras el volume profile da error
				if( wyckoffVP.getCalculatedBars(_Period)  < 6){
					wyckoffBars = null;
					return;
				}
				
				if( this._EnableMarketProfile ){
					volumeProfile = new VolumeAnalysis.Profile(wyckoffBars);
					volumeProfile.setTimePeriod(_Period, _PeriodMode);
					volumeProfile.setRealtimeCalculations(_RealtimeHeuristic);
				}
				rangeVolumeProfile = new VolumeAnalysis.Profile(wyckoffBars);
				// !- no necesitamos tiempo real
				rangeVolumeProfile.setRealtimeCalculations(false);
				// !- no necesitamos determinar el periodo temporal ya que es escogido por el usuario
				//rangeVolumeProfile.setTimePeriod(_Period, _PeriodMode);
				
				wyckoffVP.setFontStyle(_TextFont);
				wyckoffVP.setVolumeProfile(volumeProfile, rangeVolumeProfile);
				
				if( ChartPanel != null && ChartControl != null )
				{
//					wyckoffVP.setRangeKey(_RangeKey);
					// !- activamos las funciones de teclado
					ChartPanel.KeyDown+= new System.Windows.Input.KeyEventHandler(OnKeyDown);
            		ChartPanel.KeyUp += new System.Windows.Input.KeyEventHandler(OnKeyUp);
					ChartControl.MouseLeftButtonDown += MouseClicked;
					ChartControl.MouseMove += MouseMoveEvent;
				}
			}
			else if(State == State.Realtime)
			{
				if( wyckoffBars != null )
					wyckoffVP.setRealtime(true);
			}
			else if(State == State.Terminated)
        	{
	            if( wyckoffBars != null && ChartPanel != null && ChartControl != null)
	            {
	                ChartPanel.KeyDown -= OnKeyDown;
	                ChartPanel.KeyUp -= OnKeyUp;
					ChartControl.MouseLeftButtonDown -= MouseClicked;
					ChartControl.MouseMove -= MouseMoveEvent;
	            }
	        }
		}
		#region MOUSE_AND_KEY_EVENTS
		
		private void MouseMoveEvent(object sender, MouseEventArgs e){
			if( wyckoffVP.mouseMoveEvent(e, ChartPanel) ){
				ForceRefresh();
			}
		}
		private void OnKeyDown(object sender, System.Windows.Input.KeyEventArgs e){
			if( wyckoffVP.onKeyDown(e) ){
				ForceRefresh();
			}
		}
	    public void OnKeyUp(object sender, System.Windows.Input.KeyEventArgs e){
			if( wyckoffVP.onKeyUp(e) ){
				ForceRefresh();
			}
		}
		private void MouseClicked(object sender, MouseButtonEventArgs e)
		{
			if( wyckoffVP.mouseClicked() ){
				ForceRefresh();
			}
		}
		
		#endregion
		protected override void OnRender(ChartControl chartControl, ChartScale chartScale)
		{
			base.OnRender(chartControl, chartScale);
			if( wyckoffBars == null ){
				wyckoffVP.setChartPanelHW(ChartPanel.H, ChartPanel.W);
				wyckoffVP.setRenderTarget(chartControl, chartScale, ChartBars, RenderTarget);
				wyckoffVP.renderMessageInfo(string.Format("Bars number error:{0} minimum required for volume profile:6",  wyckoffVP.getCalculatedBars(_Period)), ChartPanel.W / 3, ChartPanel.H / 2, SharpDX.Color.Beige, 14);
			}
			if( !wyckoffVP.IsRealtime || IsInHitTest == null || chartControl == null || ChartBars.Bars == null )
				return;
			// 1- Altura minima de un tick
			// 2- Ancho de barra en barra
			wyckoffVP.setHW(chartScale.GetPixelsForDistance(TickSize), chartControl.Properties.BarDistance);
			wyckoffVP.setChartPanelHW(ChartPanel.H, ChartPanel.W);
			// !- Apuntamos al target de renderizado
			wyckoffVP.setRenderTarget(chartControl, chartScale, ChartBars, RenderTarget);
			
			if( this._EnableMarketProfile ){
				int fromIndex = ChartBars.FromIndex;
				int toIndex = ( ChartBars.ToIndex + wyckoffVP.getCalculatedBars(_Period) ) - 1;
				try{
					for(int barIndex = fromIndex; barIndex <= toIndex; barIndex++){
						wyckoffVP.renderProfile(barIndex);
					}
					if( this._RealtimeHeuristic ){
						wyckoffVP.renderRealtimeProfile();
					}
				} catch{ }
			}			
			wyckoffVP.renderRangeProfile();
			wyckoffVP.renderCursor();
		}
		protected override void OnMarketData(MarketDataEventArgs MarketArgs)
		{
			if( wyckoffBars == null ){
				return;
			}
			if( !wyckoffBars.onMarketData(MarketArgs) ){
				return;
			}
			if( this._EnableMarketProfile ){
				volumeProfile.AddMarketProfile(CurrentBar, MarketArgs);
			}
		}
		
		#region Properties
		
		// !- Setup
		[NinjaScriptProperty]
		[Display(Name="Formula", Order=0, GroupName="Volume Profile Calculations")]
		public _VolumeAnalysisProfileEnums.Formula _VolumeFormula
		{ get; set; }
		
		[NinjaScriptProperty]
		[Display(Name="Ladder information", Order=1, GroupName="Volume Profile Calculations")]
		public _VolumeAnalysisProfileEnums.RenderInfo _VolumeRenderInfo
		{ get; set; }
		
		[NinjaScriptProperty]
		[Display(Name="Time", Order=2, GroupName="Volume Profile Calculations")]
		public VolumeAnalysis.PeriodMode _PeriodMode
		{ get; set; }
		
		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="Period", Order=3, GroupName="Volume Profile Calculations")]
		public int _Period
		{ get; set; }
		
		[NinjaScriptProperty]
		[Display(Name="Show POC", Order=4, GroupName="Volume Profile Calculations")]
		public bool _ShowPOC
		{ get; set; }
		
		[NinjaScriptProperty]
		[Display(Name="Show POI", Order=5, GroupName="Volume Profile Calculations")]
		public bool _ShowPOI
		{ get; set; }
		
		[NinjaScriptProperty]
		[Display(Name="Realtime heuristic", Order=6, GroupName="Volume Profile Calculations")]
		public bool _RealtimeHeuristic
		{ get; set; }
		
		[NinjaScriptProperty]
		[Display(Name="Enable market profile", Order=7, GroupName="Volume Profile Calculations")]
		public bool _EnableMarketProfile
		{ get; set; }
		
		// !- Style
		[XmlIgnore]
		[Display(Name="Total volume color", Order=1, GroupName="Volume Profile Style")]
		public Brush _TotalVolColor
		{ get; set; }
		[Browsable(false)]
		public string _TotalVolColorSerializable
		{
			get { return Serialize.BrushToString(_TotalVolColor); }
			set { _TotalVolColor = Serialize.StringToBrush(value); }
		}
		[XmlIgnore]
		[Display(Name="Bid volume color", Order=2, GroupName="Volume Profile Style")]
		public Brush _BidVolColor
		{ get; set; }
		[Browsable(false)]
		public string _BidVolColorSerializable
		{
			get { return Serialize.BrushToString(_BidVolColor); }
			set { _BidVolColor = Serialize.StringToBrush(value); }
		}
		[XmlIgnore]
		[Display(Name="Ask volume color", Order=3, GroupName="Volume Profile Style")]
		public Brush _AskVolColor
		{ get; set; }
		[Browsable(false)]
		public string _AskVolColorSerializable
		{
			get { return Serialize.BrushToString(_AskVolColor); }
			set { _AskVolColor = Serialize.StringToBrush(value); }
		}
		
		// *- POC color style
		[XmlIgnore]
		[Display(Name="POC color", Order=4, GroupName="Volume Profile Style")]
		public Brush _POCColor
		{ get; set; }
		[Browsable(false)]
		public string _POCColorSerializable
		{
			get { return Serialize.BrushToString(_POCColor); }
			set { _POCColor = Serialize.StringToBrush(value); }
		}
		[NinjaScriptProperty]
		[Range(1.0f, 100.0f)]
		[Display(Name="POC opacity %", Order=5, GroupName="Volume Profile Style")]
		public float _POCOpacity
		{ get; set; }
		
		// *- POI color style
		[XmlIgnore]
		[Display(Name="POI color", Order=6, GroupName="Volume Profile Style")]
		public Brush _POIColor
		{ get; set; }
		[Browsable(false)]
		public string _POIColorSerializable
		{
			get { return Serialize.BrushToString(_POIColor); }
			set { _POIColor = Serialize.StringToBrush(value); }
		}
		[NinjaScriptProperty]
		[Range(1.0f, 100.0f)]
		[Display(Name="POI opacity %", Order=7, GroupName="Volume Profile Style")]
		public float _POIOpacity
		{ get; set; }
		
		[XmlIgnore]
		[Display(Name="Selection color", Order=8, GroupName="Volume Profile Style")]
		public Brush _SelectionColor
		{ get; set; }
		[Browsable(false)]
		public string _SelectionColorSerializable
		{
			get { return Serialize.BrushToString(_SelectionColor); }
			set { _SelectionColor = Serialize.StringToBrush(value); }
		}
		[NinjaScriptProperty]
		[Range(1.0f, 100.0f)]
		[Display(Name="Selection opacity %", Order=9, GroupName="Volume Profile Style")]
		public float _SelectionOpacity
		{ get; set; }
		
		[XmlIgnore]
		[Display(Name="Erase color", Order=10, GroupName="Volume Profile Style")]
		public Brush _EraseColor
		{ get; set; }
		[Browsable(false)]
		public string _EraseColorSerializable
		{
			get { return Serialize.BrushToString(_EraseColor); }
			set { _EraseColor = Serialize.StringToBrush(value); }
		}
		[NinjaScriptProperty]
		[Range(1.0f, 100.0f)]
		[Display(Name="Erase opacity %", Order=11, GroupName="Volume Profile Style")]
		public float _EraseOpacity
		{ get; set; }
		
		[XmlIgnore]
		[Display(Name="Font Color", Order=12, GroupName="Volume Profile Style")]
		public Brush _FontColor
		{ get; set; }
		[Browsable(false)]
		public string _FontColorSerializable
		{
			get { return Serialize.BrushToString(_FontColor); }
			set { _FontColor = Serialize.StringToBrush(value); }
		}
		[NinjaScriptProperty]
		[Range(1.0f, 100.0f)]
		[Display(Name="Text opacity %", Order=13, GroupName="Volume Profile Style")]
		public float _TextOpacity
		{ get; set; }
		
		[NinjaScriptProperty]
		[Display(Name="Font", Order=14, GroupName="Volume Profile Style")]
		public SimpleFont _TextFont
		{ get; set; }
		
		[NinjaScriptProperty]
		[Display(Name="Show text", Order=15, GroupName="Volume Profile Style")]
		public bool _ShowText
		{ get; set; }
		
		[NinjaScriptProperty]
		[Range(1.0f, float.MaxValue)]
		[Display(Name="Min font width", Order=16, GroupName="Volume Profile Style")]
		public float _MinFontWidth
		{ get; set; }
		[NinjaScriptProperty]
		[Range(1.0f, float.MaxValue)]
		[Display(Name="Min font height", Order=17, GroupName="Volume Profile Style")]
		public float _MinFontHeight
		{ get; set; }
		
		// !- Info
		[NinjaScriptProperty]
		[Display(Name="Total volume", Order=0, GroupName="Volume Profile Information")]
		public bool _showTotalVolumeInfo
		{ get; set; }
		
		[NinjaScriptProperty]
		[Display(Name="Delta", Order=1, GroupName="Volume Profile Information")]
		public bool _showDeltaInfo
		{ get; set; }
		
		#endregion
	}
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private WyckoffZen.VolumeAnalysisProfile[] cacheVolumeAnalysisProfile;
		public WyckoffZen.VolumeAnalysisProfile VolumeAnalysisProfile(_VolumeAnalysisProfileEnums.Formula _volumeFormula, _VolumeAnalysisProfileEnums.RenderInfo _volumeRenderInfo, VolumeAnalysis.PeriodMode _periodMode, int _period, bool _showPOC, bool _showPOI, bool _realtimeHeuristic, bool _enableMarketProfile, float _pOCOpacity, float _pOIOpacity, float _selectionOpacity, float _eraseOpacity, float _textOpacity, SimpleFont _textFont, bool _showText, float _minFontWidth, float _minFontHeight, bool _showTotalVolumeInfo, bool _showDeltaInfo)
		{
			return VolumeAnalysisProfile(Input, _volumeFormula, _volumeRenderInfo, _periodMode, _period, _showPOC, _showPOI, _realtimeHeuristic, _enableMarketProfile, _pOCOpacity, _pOIOpacity, _selectionOpacity, _eraseOpacity, _textOpacity, _textFont, _showText, _minFontWidth, _minFontHeight, _showTotalVolumeInfo, _showDeltaInfo);
		}

		public WyckoffZen.VolumeAnalysisProfile VolumeAnalysisProfile(ISeries<double> input, _VolumeAnalysisProfileEnums.Formula _volumeFormula, _VolumeAnalysisProfileEnums.RenderInfo _volumeRenderInfo, VolumeAnalysis.PeriodMode _periodMode, int _period, bool _showPOC, bool _showPOI, bool _realtimeHeuristic, bool _enableMarketProfile, float _pOCOpacity, float _pOIOpacity, float _selectionOpacity, float _eraseOpacity, float _textOpacity, SimpleFont _textFont, bool _showText, float _minFontWidth, float _minFontHeight, bool _showTotalVolumeInfo, bool _showDeltaInfo)
		{
			if (cacheVolumeAnalysisProfile != null)
				for (int idx = 0; idx < cacheVolumeAnalysisProfile.Length; idx++)
					if (cacheVolumeAnalysisProfile[idx] != null && cacheVolumeAnalysisProfile[idx]._VolumeFormula == _volumeFormula && cacheVolumeAnalysisProfile[idx]._VolumeRenderInfo == _volumeRenderInfo && cacheVolumeAnalysisProfile[idx]._PeriodMode == _periodMode && cacheVolumeAnalysisProfile[idx]._Period == _period && cacheVolumeAnalysisProfile[idx]._ShowPOC == _showPOC && cacheVolumeAnalysisProfile[idx]._ShowPOI == _showPOI && cacheVolumeAnalysisProfile[idx]._RealtimeHeuristic == _realtimeHeuristic && cacheVolumeAnalysisProfile[idx]._EnableMarketProfile == _enableMarketProfile && cacheVolumeAnalysisProfile[idx]._POCOpacity == _pOCOpacity && cacheVolumeAnalysisProfile[idx]._POIOpacity == _pOIOpacity && cacheVolumeAnalysisProfile[idx]._SelectionOpacity == _selectionOpacity && cacheVolumeAnalysisProfile[idx]._EraseOpacity == _eraseOpacity && cacheVolumeAnalysisProfile[idx]._TextOpacity == _textOpacity && cacheVolumeAnalysisProfile[idx]._TextFont == _textFont && cacheVolumeAnalysisProfile[idx]._ShowText == _showText && cacheVolumeAnalysisProfile[idx]._MinFontWidth == _minFontWidth && cacheVolumeAnalysisProfile[idx]._MinFontHeight == _minFontHeight && cacheVolumeAnalysisProfile[idx]._showTotalVolumeInfo == _showTotalVolumeInfo && cacheVolumeAnalysisProfile[idx]._showDeltaInfo == _showDeltaInfo && cacheVolumeAnalysisProfile[idx].EqualsInput(input))
						return cacheVolumeAnalysisProfile[idx];
			return CacheIndicator<WyckoffZen.VolumeAnalysisProfile>(new WyckoffZen.VolumeAnalysisProfile(){ _VolumeFormula = _volumeFormula, _VolumeRenderInfo = _volumeRenderInfo, _PeriodMode = _periodMode, _Period = _period, _ShowPOC = _showPOC, _ShowPOI = _showPOI, _RealtimeHeuristic = _realtimeHeuristic, _EnableMarketProfile = _enableMarketProfile, _POCOpacity = _pOCOpacity, _POIOpacity = _pOIOpacity, _SelectionOpacity = _selectionOpacity, _EraseOpacity = _eraseOpacity, _TextOpacity = _textOpacity, _TextFont = _textFont, _ShowText = _showText, _MinFontWidth = _minFontWidth, _MinFontHeight = _minFontHeight, _showTotalVolumeInfo = _showTotalVolumeInfo, _showDeltaInfo = _showDeltaInfo }, input, ref cacheVolumeAnalysisProfile);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.WyckoffZen.VolumeAnalysisProfile VolumeAnalysisProfile(_VolumeAnalysisProfileEnums.Formula _volumeFormula, _VolumeAnalysisProfileEnums.RenderInfo _volumeRenderInfo, VolumeAnalysis.PeriodMode _periodMode, int _period, bool _showPOC, bool _showPOI, bool _realtimeHeuristic, bool _enableMarketProfile, float _pOCOpacity, float _pOIOpacity, float _selectionOpacity, float _eraseOpacity, float _textOpacity, SimpleFont _textFont, bool _showText, float _minFontWidth, float _minFontHeight, bool _showTotalVolumeInfo, bool _showDeltaInfo)
		{
			return indicator.VolumeAnalysisProfile(Input, _volumeFormula, _volumeRenderInfo, _periodMode, _period, _showPOC, _showPOI, _realtimeHeuristic, _enableMarketProfile, _pOCOpacity, _pOIOpacity, _selectionOpacity, _eraseOpacity, _textOpacity, _textFont, _showText, _minFontWidth, _minFontHeight, _showTotalVolumeInfo, _showDeltaInfo);
		}

		public Indicators.WyckoffZen.VolumeAnalysisProfile VolumeAnalysisProfile(ISeries<double> input , _VolumeAnalysisProfileEnums.Formula _volumeFormula, _VolumeAnalysisProfileEnums.RenderInfo _volumeRenderInfo, VolumeAnalysis.PeriodMode _periodMode, int _period, bool _showPOC, bool _showPOI, bool _realtimeHeuristic, bool _enableMarketProfile, float _pOCOpacity, float _pOIOpacity, float _selectionOpacity, float _eraseOpacity, float _textOpacity, SimpleFont _textFont, bool _showText, float _minFontWidth, float _minFontHeight, bool _showTotalVolumeInfo, bool _showDeltaInfo)
		{
			return indicator.VolumeAnalysisProfile(input, _volumeFormula, _volumeRenderInfo, _periodMode, _period, _showPOC, _showPOI, _realtimeHeuristic, _enableMarketProfile, _pOCOpacity, _pOIOpacity, _selectionOpacity, _eraseOpacity, _textOpacity, _textFont, _showText, _minFontWidth, _minFontHeight, _showTotalVolumeInfo, _showDeltaInfo);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.WyckoffZen.VolumeAnalysisProfile VolumeAnalysisProfile(_VolumeAnalysisProfileEnums.Formula _volumeFormula, _VolumeAnalysisProfileEnums.RenderInfo _volumeRenderInfo, VolumeAnalysis.PeriodMode _periodMode, int _period, bool _showPOC, bool _showPOI, bool _realtimeHeuristic, bool _enableMarketProfile, float _pOCOpacity, float _pOIOpacity, float _selectionOpacity, float _eraseOpacity, float _textOpacity, SimpleFont _textFont, bool _showText, float _minFontWidth, float _minFontHeight, bool _showTotalVolumeInfo, bool _showDeltaInfo)
		{
			return indicator.VolumeAnalysisProfile(Input, _volumeFormula, _volumeRenderInfo, _periodMode, _period, _showPOC, _showPOI, _realtimeHeuristic, _enableMarketProfile, _pOCOpacity, _pOIOpacity, _selectionOpacity, _eraseOpacity, _textOpacity, _textFont, _showText, _minFontWidth, _minFontHeight, _showTotalVolumeInfo, _showDeltaInfo);
		}

		public Indicators.WyckoffZen.VolumeAnalysisProfile VolumeAnalysisProfile(ISeries<double> input , _VolumeAnalysisProfileEnums.Formula _volumeFormula, _VolumeAnalysisProfileEnums.RenderInfo _volumeRenderInfo, VolumeAnalysis.PeriodMode _periodMode, int _period, bool _showPOC, bool _showPOI, bool _realtimeHeuristic, bool _enableMarketProfile, float _pOCOpacity, float _pOIOpacity, float _selectionOpacity, float _eraseOpacity, float _textOpacity, SimpleFont _textFont, bool _showText, float _minFontWidth, float _minFontHeight, bool _showTotalVolumeInfo, bool _showDeltaInfo)
		{
			return indicator.VolumeAnalysisProfile(input, _volumeFormula, _volumeRenderInfo, _periodMode, _period, _showPOC, _showPOI, _realtimeHeuristic, _enableMarketProfile, _pOCOpacity, _pOIOpacity, _selectionOpacity, _eraseOpacity, _textOpacity, _textFont, _showText, _minFontWidth, _minFontHeight, _showTotalVolumeInfo, _showDeltaInfo);
		}
	}
}

#endregion
