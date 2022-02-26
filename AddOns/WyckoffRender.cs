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
using NinjaTrader.Gui.Tools;
#endregion

using System.IO;

namespace NinjaTrader.NinjaScript.AddOns
{
	namespace WyckoffRenderUtils
	{
		public static class Debug{ public static void toFile(string info){ File.AppendAllText(NinjaTrader.Core.Globals.UserDataDir + "Debug.txt", info + Environment.NewLine); } }
		
		public class WyckoffRenderControl
		{
			protected ChartControl CHART_CONTROL;
			protected ChartScale CHART_SCALE;
			protected ChartBars CHART_BARS;
			protected SharpDX.Direct2D1.RenderTarget RENDER_TARGET;
			
			protected float W;
			protected float H;
			protected float PanelH;
			protected float PanelW;
			
			public void setRenderTarget(ChartControl chartControl, ChartScale chartScale, ChartBars chartBars, SharpDX.Direct2D1.RenderTarget renderTarget)
			{
				this.CHART_CONTROL = chartControl;
				this.CHART_SCALE = chartScale;
				this.CHART_BARS = chartBars;
				this.RENDER_TARGET = renderTarget;
			}
			public void setHW(float H, float W)
			{
				this.H = H;
				this.W = W;
			}
			public void setChartPanelHW(float PanelH, float PanelW)
			{
				this.PanelH = PanelH;
				this.PanelW = PanelW;
			}
			protected void setFontStyle(SimpleFont font, out SharpDX.DirectWrite.TextFormat textFormat)
			{
				SharpDX.DirectWrite.FontWeight fw;
				SharpDX.DirectWrite.FontStyle fs;
				
				if( font.Bold == true ) fw = SharpDX.DirectWrite.FontWeight.Bold;
				else fw = SharpDX.DirectWrite.FontWeight.Normal;
				
				if( font.Italic == true ) fs = SharpDX.DirectWrite.FontStyle.Italic;
				else fs = SharpDX.DirectWrite.FontStyle.Normal;
				
				textFormat = new SharpDX.DirectWrite.TextFormat(
					NinjaTrader.Core.Globals.DirectWriteFactory,
					font.ToString(),
					fw,
					fs,
					SharpDX.DirectWrite.FontStretch.Normal,
					(float)font.Size
				);
				textFormat.ParagraphAlignment = SharpDX.DirectWrite.ParagraphAlignment.Center; 
				textFormat.TextAlignment = SharpDX.DirectWrite.TextAlignment.Center;
			}
			protected SharpDX.Direct2D1.DashStyle DashStyleHelperToDX(DashStyleHelper dashStyle)
			{
				switch( dashStyle )
				{
					case DashStyleHelper.Dash:
					{
						return SharpDX.Direct2D1.DashStyle.Dash;
					}
					case DashStyleHelper.DashDot:
					{
						return SharpDX.Direct2D1.DashStyle.DashDot;
					}
					case DashStyleHelper.DashDotDot:
					{
						return SharpDX.Direct2D1.DashStyle.DashDotDot;
					}
					case DashStyleHelper.Dot:
					{
						return SharpDX.Direct2D1.DashStyle.Dot;
					}
				}
				return SharpDX.Direct2D1.DashStyle.Solid;
			}
			
			protected void myDrawText(string text, ref SharpDX.RectangleF rect, SharpDX.Color color, float fontWidth, float fontHeight, SharpDX.DirectWrite.TextFormat textFormat, float brushOpacity)
			{
				if( this.W >= fontWidth && this.H >= fontHeight ){
					SharpDX.Direct2D1.Brush dxBrush = new SharpDX.Direct2D1.SolidColorBrush(RENDER_TARGET, color);
					if( !dxBrush.IsDisposed ){
						dxBrush.Opacity = brushOpacity;
						RENDER_TARGET.DrawText(text, textFormat, rect, dxBrush);
						dxBrush.Dispose();
					}
				}
			}
			protected void myFillRectangle(ref SharpDX.RectangleF rect, SharpDX.Color color, float opacity)
			{
				SharpDX.Direct2D1.Brush dxBrush = new SharpDX.Direct2D1.SolidColorBrush(RENDER_TARGET, color);
				if( !dxBrush.IsDisposed ){
					dxBrush.Opacity = opacity;
					RENDER_TARGET.FillRectangle(rect, dxBrush);	
					dxBrush.Dispose();
				}
			}
			protected void myDrawRectangle(ref SharpDX.RectangleF rect, SharpDX.Color color, float opacity, float strokeWidth)
			{
				SharpDX.Direct2D1.Brush dxBrush = new SharpDX.Direct2D1.SolidColorBrush(RENDER_TARGET, color);
				if( !dxBrush.IsDisposed ){
					dxBrush.Opacity = opacity;
					RENDER_TARGET.DrawRectangle(rect, dxBrush, strokeWidth);
					dxBrush.Dispose();
				}
			}
			protected void myDrawEllipse(ref SharpDX.Direct2D1.Ellipse ellipse, SharpDX.Color color, float opacity, float strokeWidth)
			{
				SharpDX.Direct2D1.Brush dxBrush = new SharpDX.Direct2D1.SolidColorBrush(RENDER_TARGET, color);
				if( !dxBrush.IsDisposed ){
					dxBrush.Opacity = opacity;
					RENDER_TARGET.DrawEllipse(ellipse, dxBrush, strokeWidth);
					dxBrush.Dispose();
				}
			}
			protected void myFillEllipse(ref SharpDX.Direct2D1.Ellipse ellipse, SharpDX.Color color, float opacity)
			{
				SharpDX.Direct2D1.Brush dxBrush = new SharpDX.Direct2D1.SolidColorBrush(RENDER_TARGET, color);
				if( !dxBrush.IsDisposed ){
					dxBrush.Opacity = opacity;
					RENDER_TARGET.FillEllipse(ellipse, dxBrush);
					dxBrush.Dispose();
				}
			}
			protected void myDrawLine(ref SharpDX.Vector2 startVec, ref SharpDX.Vector2 endVec,
				SharpDX.Color color, float opacity,
				float strokeWidth, SharpDX.Direct2D1.StrokeStyle strokeStyle)
			{
				SharpDX.Direct2D1.Brush dxBrush = new SharpDX.Direct2D1.SolidColorBrush(RENDER_TARGET, color);
				if( !dxBrush.IsDisposed ){
					dxBrush.Opacity = opacity;
					RENDER_TARGET.DrawLine(startVec, endVec, dxBrush, strokeWidth, strokeStyle);
					dxBrush.Dispose();
				}
			}
			protected void myDrawLine(ref SharpDX.Vector2 startVec, ref SharpDX.Vector2 endVec, SharpDX.Color color)
			{
				this.myDrawLine(ref startVec, ref endVec, color, 1.0f, 1.0f, null);
			}
			
			public static SharpDX.Color BrushToColor(Brush brushToConvert)
			{
				string brushColor= brushToConvert.ToString();
				// # A
				if( brushColor.Equals(Brushes.AliceBlue.ToString()) ) return SharpDX.Color.AliceBlue;
				if( brushColor.Equals(Brushes.AntiqueWhite.ToString()) ) return SharpDX.Color.AntiqueWhite;
				if( brushColor.Equals(Brushes.Aqua.ToString()) ) return SharpDX.Color.Aqua;
				if( brushColor.Equals(Brushes.Aquamarine.ToString()) ) return SharpDX.Color.Aquamarine;
				if( brushColor.Equals(Brushes.Azure.ToString()) ) return SharpDX.Color.Azure;
				// # B
				if( brushColor.Equals(Brushes.Beige.ToString()) ) return SharpDX.Color.Beige;
				if( brushColor.Equals(Brushes.Bisque.ToString()) ) return SharpDX.Color.Bisque;
				if( brushColor.Equals(Brushes.Black.ToString()) ) return SharpDX.Color.Black;
				if( brushColor.Equals(Brushes.BlanchedAlmond.ToString()) ) return SharpDX.Color.BlanchedAlmond;
				if( brushColor.Equals(Brushes.Blue.ToString()) ) return SharpDX.Color.Blue;
				if( brushColor.Equals(Brushes.BlueViolet.ToString()) ) return SharpDX.Color.BlueViolet;
				if( brushColor.Equals(Brushes.Brown.ToString()) ) return SharpDX.Color.Brown;
				if( brushColor.Equals(Brushes.BurlyWood.ToString()) ) return SharpDX.Color.BurlyWood;
				// # C
				if( brushColor.Equals(Brushes.CadetBlue.ToString()) ) return SharpDX.Color.CadetBlue;
				if( brushColor.Equals(Brushes.Chartreuse.ToString()) ) return SharpDX.Color.Chartreuse;
				if( brushColor.Equals(Brushes.Chocolate.ToString()) ) return SharpDX.Color.Chocolate;
				if( brushColor.Equals(Brushes.Coral.ToString()) ) return SharpDX.Color.Coral;
				if( brushColor.Equals(Brushes.CornflowerBlue.ToString()) ) return SharpDX.Color.CornflowerBlue;
				if( brushColor.Equals(Brushes.Cornsilk.ToString()) ) return SharpDX.Color.Cornsilk;
				if( brushColor.Equals(Brushes.Crimson.ToString()) ) return SharpDX.Color.Crimson;
				if( brushColor.Equals(Brushes.Cyan.ToString()) ) return SharpDX.Color.Cyan;
				// # D
				if( brushColor.Equals(Brushes.DarkBlue.ToString()) ) return SharpDX.Color.DarkBlue;
				if( brushColor.Equals(Brushes.DarkCyan.ToString()) ) return SharpDX.Color.DarkCyan;
				if( brushColor.Equals(Brushes.DarkGoldenrod.ToString()) ) return SharpDX.Color.DarkGoldenrod;
				if( brushColor.Equals(Brushes.DarkGray.ToString()) ) return SharpDX.Color.DarkGray;
				if( brushColor.Equals(Brushes.DarkGreen.ToString()) ) return SharpDX.Color.DarkGreen;
				if( brushColor.Equals(Brushes.DarkKhaki.ToString()) ) return SharpDX.Color.DarkKhaki;
				if( brushColor.Equals(Brushes.DarkMagenta.ToString()) ) return SharpDX.Color.DarkMagenta;
				if( brushColor.Equals(Brushes.DarkOliveGreen.ToString()) ) return SharpDX.Color.DarkOliveGreen;
				if( brushColor.Equals(Brushes.DarkOrange.ToString()) ) return SharpDX.Color.DarkOrange;
				if( brushColor.Equals(Brushes.DarkOrchid.ToString()) ) return SharpDX.Color.DarkOrchid;
				if( brushColor.Equals(Brushes.DarkRed.ToString()) ) return SharpDX.Color.DarkRed;
				if( brushColor.Equals(Brushes.DarkSalmon.ToString()) ) return SharpDX.Color.DarkSalmon;
				if( brushColor.Equals(Brushes.DarkSeaGreen.ToString()) ) return SharpDX.Color.DarkSeaGreen;
				if( brushColor.Equals(Brushes.DarkSlateBlue.ToString()) ) return SharpDX.Color.DarkSlateBlue;
				if( brushColor.Equals(Brushes.DarkSlateGray.ToString()) ) return SharpDX.Color.DarkSlateGray;
				if( brushColor.Equals(Brushes.DarkTurquoise.ToString()) ) return SharpDX.Color.DarkTurquoise;
				if( brushColor.Equals(Brushes.DarkViolet.ToString()) ) return SharpDX.Color.DarkViolet;
				if( brushColor.Equals(Brushes.DeepPink.ToString()) ) return SharpDX.Color.DeepPink;
				if( brushColor.Equals(Brushes.DeepSkyBlue.ToString()) ) return SharpDX.Color.DeepSkyBlue;
				if( brushColor.Equals(Brushes.DimGray.ToString()) ) return SharpDX.Color.DimGray;
				if( brushColor.Equals(Brushes.DodgerBlue.ToString()) ) return SharpDX.Color.DodgerBlue;
				// # E
				if( brushColor.Equals(Brushes.Firebrick.ToString()) ) return SharpDX.Color.Firebrick;
				if( brushColor.Equals(Brushes.FloralWhite.ToString()) ) return SharpDX.Color.FloralWhite;
				if( brushColor.Equals(Brushes.ForestGreen.ToString()) ) return SharpDX.Color.ForestGreen;
				if( brushColor.Equals(Brushes.Fuchsia.ToString()) ) return SharpDX.Color.Fuchsia;
				// # G
				if( brushColor.Equals(Brushes.Gainsboro.ToString()) ) return SharpDX.Color.Gainsboro;
				if( brushColor.Equals(Brushes.GhostWhite.ToString()) ) return SharpDX.Color.GhostWhite;
				if( brushColor.Equals(Brushes.Gold.ToString()) ) return SharpDX.Color.Gold;
				if( brushColor.Equals(Brushes.Goldenrod.ToString()) ) return SharpDX.Color.Goldenrod;
				if( brushColor.Equals(Brushes.Gray.ToString()) ) return SharpDX.Color.Gray;
				if( brushColor.Equals(Brushes.Green.ToString()) ) return SharpDX.Color.Green;
				if( brushColor.Equals(Brushes.GreenYellow.ToString()) ) return SharpDX.Color.GreenYellow;
				// # E
				if( brushColor.Equals(Brushes.Honeydew.ToString()) ) return SharpDX.Color.Honeydew;
				if( brushColor.Equals(Brushes.HotPink.ToString()) ) return SharpDX.Color.HotPink;
				// # I
				if( brushColor.Equals(Brushes.IndianRed.ToString()) ) return SharpDX.Color.IndianRed;
				if( brushColor.Equals(Brushes.Indigo.ToString()) ) return SharpDX.Color.Indigo;
				if( brushColor.Equals(Brushes.Ivory.ToString()) ) return SharpDX.Color.Ivory;
				// # K
				if( brushColor.Equals(Brushes.Khaki.ToString()) ) return SharpDX.Color.Khaki;
				// # L
				if( brushColor.Equals(Brushes.Lavender.ToString()) ) return SharpDX.Color.Lavender;
				if( brushColor.Equals(Brushes.LavenderBlush.ToString()) ) return SharpDX.Color.LavenderBlush;
				if( brushColor.Equals(Brushes.LawnGreen.ToString()) ) return SharpDX.Color.LawnGreen;
				if( brushColor.Equals(Brushes.LemonChiffon.ToString()) ) return SharpDX.Color.LemonChiffon;
				if( brushColor.Equals(Brushes.LightBlue.ToString()) ) return SharpDX.Color.LightBlue;
				if( brushColor.Equals(Brushes.LightCoral.ToString()) ) return SharpDX.Color.LightCoral;
				if( brushColor.Equals(Brushes.LightCyan.ToString()) ) return SharpDX.Color.LightCyan;
				if( brushColor.Equals(Brushes.LightGoldenrodYellow.ToString()) ) return SharpDX.Color.LightGoldenrodYellow;
				if( brushColor.Equals(Brushes.LightGray.ToString()) ) return SharpDX.Color.LightGray;
				if( brushColor.Equals(Brushes.LightGreen.ToString()) ) return SharpDX.Color.LightGreen;
				if( brushColor.Equals(Brushes.LightPink.ToString()) ) return SharpDX.Color.LightPink;
				if( brushColor.Equals(Brushes.LightSalmon.ToString()) ) return SharpDX.Color.LightSalmon;
				if( brushColor.Equals(Brushes.LightSeaGreen.ToString()) ) return SharpDX.Color.LightSeaGreen;
				if( brushColor.Equals(Brushes.LightSkyBlue.ToString()) ) return SharpDX.Color.LightSkyBlue;
				if( brushColor.Equals(Brushes.LightSlateGray.ToString()) ) return SharpDX.Color.LightSlateGray;
				if( brushColor.Equals(Brushes.LightSteelBlue.ToString()) ) return SharpDX.Color.LightSteelBlue;
				if( brushColor.Equals(Brushes.LightYellow.ToString()) ) return SharpDX.Color.LightYellow;
				if( brushColor.Equals(Brushes.Lime.ToString()) ) return SharpDX.Color.Lime;
				if( brushColor.Equals(Brushes.LimeGreen.ToString()) ) return SharpDX.Color.LimeGreen;
				if( brushColor.Equals(Brushes.Linen.ToString()) ) return SharpDX.Color.Linen;
				// # M
				if( brushColor.Equals(Brushes.Magenta.ToString()) ) return SharpDX.Color.Magenta;
				if( brushColor.Equals(Brushes.Maroon.ToString()) ) return SharpDX.Color.Maroon;
				if( brushColor.Equals(Brushes.MediumAquamarine.ToString()) ) return SharpDX.Color.MediumAquamarine;
				if( brushColor.Equals(Brushes.MediumBlue.ToString()) ) return SharpDX.Color.MediumBlue;
				if( brushColor.Equals(Brushes.MediumOrchid.ToString()) ) return SharpDX.Color.MediumOrchid;
				if( brushColor.Equals(Brushes.MediumPurple.ToString()) ) return SharpDX.Color.MediumPurple;
				if( brushColor.Equals(Brushes.MediumSeaGreen.ToString()) ) return SharpDX.Color.MediumSeaGreen;
				if( brushColor.Equals(Brushes.MediumSlateBlue.ToString()) ) return SharpDX.Color.MediumSlateBlue;
				if( brushColor.Equals(Brushes.MediumSpringGreen.ToString()) ) return SharpDX.Color.MediumSpringGreen;
				if( brushColor.Equals(Brushes.MediumTurquoise.ToString()) ) return SharpDX.Color.MediumTurquoise;
				if( brushColor.Equals(Brushes.MediumVioletRed.ToString()) ) return SharpDX.Color.MediumVioletRed;
				if( brushColor.Equals(Brushes.MidnightBlue.ToString()) ) return SharpDX.Color.MidnightBlue;
				if( brushColor.Equals(Brushes.MintCream.ToString()) ) return SharpDX.Color.MintCream;
				if( brushColor.Equals(Brushes.MistyRose.ToString()) ) return SharpDX.Color.MintCream;
				if( brushColor.Equals(Brushes.Moccasin.ToString()) ) return SharpDX.Color.MintCream;
				// # N
				if( brushColor.Equals(Brushes.NavajoWhite.ToString()) ) return SharpDX.Color.NavajoWhite;
				if( brushColor.Equals(Brushes.Navy.ToString()) ) return SharpDX.Color.Navy;
				// # O
				if( brushColor.Equals(Brushes.OldLace.ToString()) ) return SharpDX.Color.OldLace;
				if( brushColor.Equals(Brushes.Olive.ToString()) ) return SharpDX.Color.Olive;
				if( brushColor.Equals(Brushes.OliveDrab.ToString()) ) return SharpDX.Color.OliveDrab;
				if( brushColor.Equals(Brushes.Orange.ToString()) ) return SharpDX.Color.Orange;
				if( brushColor.Equals(Brushes.OrangeRed.ToString()) ) return SharpDX.Color.OrangeRed;
				if( brushColor.Equals(Brushes.Orchid.ToString()) ) return SharpDX.Color.Orchid;
				// # P
				if( brushColor.Equals(Brushes.PaleGoldenrod.ToString()) ) return SharpDX.Color.PaleGoldenrod;
				if( brushColor.Equals(Brushes.PaleGreen.ToString()) ) return SharpDX.Color.PaleGreen;
				if( brushColor.Equals(Brushes.PaleTurquoise.ToString()) ) return SharpDX.Color.PaleTurquoise;
				if( brushColor.Equals(Brushes.PaleVioletRed.ToString()) ) return SharpDX.Color.PaleVioletRed;
				if( brushColor.Equals(Brushes.PapayaWhip.ToString()) ) return SharpDX.Color.PapayaWhip;
				if( brushColor.Equals(Brushes.PeachPuff.ToString()) ) return SharpDX.Color.PeachPuff;
				if( brushColor.Equals(Brushes.Peru.ToString()) ) return SharpDX.Color.Peru;
				if( brushColor.Equals(Brushes.Pink.ToString()) ) return SharpDX.Color.Pink;
				if( brushColor.Equals(Brushes.Plum.ToString()) ) return SharpDX.Color.Plum;
				if( brushColor.Equals(Brushes.PowderBlue.ToString()) ) return SharpDX.Color.PowderBlue;
				if( brushColor.Equals(Brushes.Purple.ToString()) ) return SharpDX.Color.Purple;
				// # R
				if( brushColor.Equals(Brushes.Red.ToString()) ) return SharpDX.Color.Red;
				if( brushColor.Equals(Brushes.RosyBrown.ToString()) ) return SharpDX.Color.RosyBrown;
				if( brushColor.Equals(Brushes.RoyalBlue.ToString()) ) return SharpDX.Color.RoyalBlue;
				// # S
				if( brushColor.Equals(Brushes.SaddleBrown.ToString()) ) return SharpDX.Color.SaddleBrown;
				if( brushColor.Equals(Brushes.Salmon.ToString()) ) return SharpDX.Color.Salmon;
				if( brushColor.Equals(Brushes.SandyBrown.ToString()) ) return SharpDX.Color.SandyBrown;
				if( brushColor.Equals(Brushes.SeaGreen.ToString()) ) return SharpDX.Color.SeaGreen;
				if( brushColor.Equals(Brushes.SeaShell.ToString()) ) return SharpDX.Color.SeaShell;
				if( brushColor.Equals(Brushes.Sienna.ToString()) ) return SharpDX.Color.Sienna;
				if( brushColor.Equals(Brushes.Silver.ToString()) ) return SharpDX.Color.Silver;
				if( brushColor.Equals(Brushes.SkyBlue.ToString()) ) return SharpDX.Color.SkyBlue;
				if( brushColor.Equals(Brushes.SlateBlue.ToString()) ) return SharpDX.Color.SlateBlue;
				if( brushColor.Equals(Brushes.SlateGray.ToString()) ) return SharpDX.Color.SlateGray;
				if( brushColor.Equals(Brushes.Snow.ToString()) ) return SharpDX.Color.Snow;
				if( brushColor.Equals(Brushes.SpringGreen.ToString()) ) return SharpDX.Color.SpringGreen;
				if( brushColor.Equals(Brushes.SteelBlue.ToString()) ) return SharpDX.Color.SteelBlue;
				// # T
				if( brushColor.Equals(Brushes.Tan.ToString()) ) return SharpDX.Color.Tan;
				if( brushColor.Equals(Brushes.Teal.ToString()) ) return SharpDX.Color.Teal;
				if( brushColor.Equals(Brushes.Thistle.ToString()) ) return SharpDX.Color.Thistle;
				if( brushColor.Equals(Brushes.Tomato.ToString()) ) return SharpDX.Color.Tomato;
				//if( brushToConvert == Brushes.Transparent ) return SharpDX.Color.Transparent;
				if( brushColor.Equals(Brushes.Turquoise.ToString()) ) return SharpDX.Color.Turquoise;
				// # V
				if( brushColor.Equals(Brushes.Violet.ToString()) ) return SharpDX.Color.Violet;
				// # W
				if( brushColor.Equals(Brushes.Wheat.ToString()) ) return SharpDX.Color.Wheat;
				if( brushColor.Equals(Brushes.White.ToString()) ) return SharpDX.Color.White;
				if( brushColor.Equals(Brushes.WhiteSmoke.ToString()) ) return SharpDX.Color.WhiteSmoke;
				// # Y
				if( brushColor.Equals(Brushes.Yellow.ToString()) ) return SharpDX.Color.Yellow;
				if( brushColor.Equals(Brushes.YellowGreen.ToString()) ) return SharpDX.Color.YellowGreen;
				
				return SharpDX.Color.Transparent;
			}
			
			
		}
	}
}
