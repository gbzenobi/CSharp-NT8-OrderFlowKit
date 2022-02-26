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
//using NinjaTrader.NinjaScript.AddOns.ZMath;
using System.Collections.Concurrent;

namespace NinjaTrader.NinjaScript.AddOns
{
	#region SIGHT_ENGINE
	
	namespace SightEngine
	{
		//public static class Debug{ public static void toFile(string info){ File.AppendAllText(NinjaTrader.Core.Globals.UserDataDir + "Debug.txt", info + Environment.NewLine); } }
		
		public enum MarketType
		{
			BEARISH, BULLISH
		}
		public enum BarType
		{
			BUY, SELL, INCERTITUDE, UNKNOWN
		}
		public enum BarClass
		{
			ENGULFING, SWALLOWED, BULLISH, BEARISH, UNKNOWN
		}
		public static class TimePeriod
		{
			public delegate bool TIME(DateTime t1, DateTime t2, int p);
			public static bool MINUTES(DateTime t1, DateTime t2, int Period)
			{
				return Math.Abs((t2 - t1).TotalMinutes) >= Period;
			}
			public static bool HOURS(DateTime t1, DateTime t2, int Period)
			{
				return Math.Abs((t2 - t1).TotalHours) >= Period;
			}
			public static bool DAYS(DateTime t1, DateTime t2, int Period)
			{
				return Math.Abs((t2 - t1).TotalDays) >= Period;
			}
		}
		
		#region MATH2
		
		public static class Math2
		{
			public static bool isInRange<T>(T Value, T Begin, T End) where T : IComparable
			{
				return (Value.CompareTo(Begin) > 0 && Value.CompareTo(End) < 0);
			}
			public static bool isBounded<T>(T Value, T Begin, T End) where T : IComparable
			{
				return (Value.CompareTo(Begin) >= 0 && Value.CompareTo(End) <= 0);
			}
//			public static bool isGreater<T>(T Value1, T Value2) where T : IComparable
//			{
//				return Value2.CompareTo(Value1) > 0;
//			}
			public static bool isLess<T>(T Value1, T Value2) where T : IComparable
			{
				return Value2.CompareTo(Value1) < 0;
			}
			public static double neverLessThanZero(double Value)
			{
				if(Value < 0)
					Value = 0;
				return Value;
			}
			public static int getMaximunValue(int Value, int maximunValue)
			{
				if(Value > maximunValue)
					Value = maximunValue;
				return Value;
			}
			public static double calculateDistance(double Begin, double End)
			{
				return Math.Abs(Begin - End);
			}
			public static double Percent(double Total, double Cuantity, int roundDigits)
			{ 
				return Math.Round((Cuantity*100)/Total, roundDigits);
			}
			public static double Percent(double Total, double Cuantity)
			{ 
				return Percent(Total, Cuantity, 2);
			}
			public static bool isPercent(double Percent)
			{
				return isBounded(Percent, 0, 100);
			}
			public static int boolToInt(bool v)
			{
				return v ? 1 : 0;
			}
			public static double Atan2inDeg(double y, double x)
			{
				return Math.Round(Math.Atan2(y, x) * (180/3.14), 2);
			}
			public class BoundedValue<T>
			{
				public BoundedValue(){}
				public BoundedValue(T Min, T Max)
				{
					this.Min = Min;
					this.Max = Max;
				}
				
				public T Min, Max;
			}
//			public class Decimal
//			{
//				public decimal Value;
//				public decimal Error;
//			}
		}
		
		#endregion
		#region STR_UTILS
		
		public static class StrUtils
		{
			public static int extractInfo(string info, out string extracted_info, char first_key, char end_key, int start_position)
			{
				extracted_info= string.Empty;
				int str_len = info.Length;
				if( start_position >= str_len )
					return -1;
				for(;start_position < str_len; start_position++){
					if( info[start_position] == first_key ){
						if( start_position > 0)
							start_position++;
						for(;start_position < str_len; start_position++)
						{
							if( info[start_position] == end_key ){
								return start_position;
							}
							extracted_info+= info[start_position];
						}
					}
				}
				
				return -1;
			}
			public static int extractInfo(string info, out string extracted_info, char first_key, char end_key)
			{
				return extractInfo(info, out extracted_info, first_key, end_key, 0);
			}
			public static int extractInfo(string info, out string extracted_info, char end_key, int start_position)
			{
				return extractInfo(info, out extracted_info, info[start_position], end_key, start_position);
			}
			public static int extractInfo(string info, out string extracted_info, char end_key)
			{
				return extractInfo(info, out extracted_info, info[0], end_key, 0);
			}
		}
		
		#endregion
		#region MAIN
		
		#region CORE_CLASS
		
		public interface ICore{}
		// !- Me permite hace mis propias implementaciones
		public class SECore<K, C>//: Dictionary<int, C>, IEnumerable<KeyValuePair<int, C>>
		{
			private SortedDictionary<K, C> This;
			
			public SECore()
			{
				this.This = new SortedDictionary<K, C>();
			}
			public SECore(Dictionary<K, C> This)
			{
				this.This = new SortedDictionary<K, C>(This);
			}
			
			#region GENERIC_IMPLEMENTATIONS
			
			public IEnumerator<KeyValuePair<K, C>> GetEnumerator()
			{
				return This.GetEnumerator();
			}
			public KeyValuePair<K, C> First
			{
				get{ return This.First(); }
			}
			public KeyValuePair<K, C> Last
			{
				get{ return This.Last(); }
			}
			public bool ContainsKey(K key)
			{
				return This.ContainsKey(key);
			}
			public SortedDictionary<K, C>.KeyCollection Keys
			{
				get{ return This.Keys; }
			}
			public IEnumerable<KeyValuePair<K, C>> Skip(int skipFrom)
			{
				return This.Skip(skipFrom);
			}
      		public IEnumerable<KeyValuePair<K, C>> Take(int takeTo)
			{
				return This.Take(takeTo);
			}
			public bool Remove(K key)
			{
				return This.Remove(key);
			}
			public int Count
			{
				get{ return This.Count; }
			}
			public void Clear()
			{
				This.Clear();
			}
			
			#endregion
			#region MY_IMPLEMENTATIONS
			
			public C this[K index]
			{
				get{ return This[index]; }
		        set{ This[index] = value; }
			}
			// *- si el acceso al valor no existe no genera una excepcion, si no un NULL
			public C At(K index)
			{
				if( !This.ContainsKey(index) )
					return default(C);
				return This[index];
			}
			// *- no generamos una excepcion si ya existe el item
			public bool Add(K index, C obj)
			{
				if( This.ContainsKey(index) )
					return false;
				
				This.Add(index, obj);
				return true;
			}
			//protected KeyValuePair<int, C> ElementAtOrDefault(int index){ return This.ElementAtOrDefault(index); }
			public bool isNull
			{
				get { return This == null || This.Count == 0; }
			}
			
			#endregion
		}
		public class SEBars<T> : SECore<int , T>{}
		
		#endregion
		#region VOLUME_ANALYSIS
		public static class VolumeAnalysis{
			public enum PeriodMode
			{
//				Bars,
				Minutes,
				Hours,
				Days
			}
			public enum VolumeType
			{
				BidAsk,
				Total,
				Delta
			}
			
			public class MarketOrder
			{
				public MarketOrder()
				{
					this.Clear();
				}
				private long bid;
				private long ask;
				private long total;
				private long delta;
				//private int secs;
				
				public void Clear()
				{
					bid = ask = total = delta = 0;
				}
				public void CalculateSigmaVolume(WyckoffBars.Bar wyckoffBar)
				{
					ask+= wyckoffBar.Ask;
					bid+= wyckoffBar.Bid;
					total+= wyckoffBar.Total;
					delta+= wyckoffBar.Delta;
				}
				public void CalculateSigmaVolume(MarketDataEventArgs MarketArgs)
				{
					long v = MarketArgs.Volume;
					double price = MarketArgs.Price;
					
					total+= v;
					if(price >= MarketArgs.Ask){
						ask+= v;
						delta+= v;
					}
			    	else if(price <= MarketArgs.Bid){
						bid+= v;
						delta-= v;
					}
					//contracts++;
					//secs += MarketArgs.Time.Second;
				}
				public void CalculateSigmaVolume(MarketOrder volume)
				{
					ask+= volume.Ask;
					bid+= volume.Bid;
					delta+= volume.Delta;
					total+= volume.Total;
				}
				public void CalculateSigmaVolume(long Bid, long Ask, long Delta, long Total)
				{
					bid+= Bid;
					ask+= Ask;
					delta+= Delta;
					total+= Total;
				}
				
				public long Total
				{
					get{ return this.total; }
				}
				public long Delta
				{
					get{ return this.delta; }
				}
				public long Bid
				{
					get{ return this.bid; }
				}
				public long Ask
				{
					get{ return this.ask; }
				}
				//public long Contracts
				//{
					//get{ return this.contracts; }
				//}
			}
			public class PriceLadder
			{
				private ConcurrentDictionary<double, MarketOrder> ladder;
				private double minPrice;
				private double maxPrice;
				private double lowPrice;
				private double highPrice;
				
				public PriceLadder()
				{
					this.ladder = new ConcurrentDictionary<double, MarketOrder>();
					this.minPrice = this.lowPrice = double.MaxValue;
					this.maxPrice = this.highPrice = 0;
				}
				
				public double LowPrice
				{
					get{ return this.lowPrice; }
				}
				public double HighPrice
				{
					get{ return this.highPrice; }
				}
				public void CalculateMinAndMax(ref MarketOrder minVolume, ref MarketOrder maxVolume,
					out double MinPrice, out double MaxPrice)
				{
					minVolume.Clear();
					maxVolume.Clear();
					
					MarketOrder mo;
					MinPrice = -1;
					MaxPrice = -1;
					long m_ask = long.MaxValue;
					long m_bid = long.MaxValue;
					long m_delta = long.MaxValue;
					long m_total = long.MaxValue;
					long M_ask = 0;
					long M_bid = 0;
					long M_delta = 0;
					long M_total = 0;
					long ask;
					long bid;
					long delta;
					long total;
					double price;
					
					foreach(var pl in ladder)
					{
						mo		= pl.Value;
						ask		= mo.Ask;
						bid		= mo.Bid;
						delta	= mo.Delta;
						total	= mo.Total;
						
						// !- Al decir que siempre es mayor o igual O menor o igual
						// actualizamos siempre el ciclo al ultimo precio
						// ...
						// !- Valor minimo
						if( ask <= m_ask )
							m_ask = ask;
						if( bid <= m_bid )
							m_bid = bid;
						if( delta <= m_delta )
							m_delta = delta;
						if( total <= m_total ){
							m_total = total;
							MinPrice = pl.Key;
						}
						// !- Valor maximo
						if( ask >= M_ask )
							M_ask = ask;
						if( bid >= M_bid )
							M_bid = bid;
						if( Math.Abs(delta) >= Math.Abs(M_delta) )
							M_delta = delta;
						if( total >= M_total ){
							M_total = total;
							MaxPrice = pl.Key;
						}
//						price = pl.Key;
//						// !- precio mas alto y bajo de la escalera respectivamente
//						if( price > this.highPrice )
//							this.highPrice = price;
//						if( price < this.lowPrice )
//							this.lowPrice = price;
					}
					minVolume.CalculateSigmaVolume(m_bid, m_ask, m_delta, m_total);
					maxVolume.CalculateSigmaVolume(M_bid, M_ask, M_delta, M_total);
				}
				public void CalculateMinAndMax(ref MarketOrder minVolume, ref MarketOrder maxVolume)
				{
					// !- Inutil
					double mpv = -1;
					double Mpv = -1;
					this.CalculateMinAndMax(ref minVolume, ref maxVolume, out mpv, out Mpv);
				}
				public void CalculateTotalVolume(ref MarketOrder totalVol)
				{
					totalVol.Clear();
					foreach(var pl in ladder){
						totalVol.CalculateSigmaVolume(pl.Value);
					}
				}
				private void _setPriceLadder(double price)
				{
					if( price > this.highPrice )
						this.highPrice = price;
					if( price < this.lowPrice )
						this.lowPrice = price;
					// !- si no existe el nivel de precio en el ladder lo creamos
					if( !ladder.ContainsKey(price) ){
						ladder[price] = new MarketOrder();
					}
				}
				public void AddPrice(double price, MarketOrder volume)
				{
					this._setPriceLadder(price);
					ladder[price].CalculateSigmaVolume(volume);
				}
				public void AddPrice(MarketDataEventArgs MarketArgs)
				{
					double price = MarketArgs.Price;
					
					this._setPriceLadder(price);
					ladder[price].CalculateSigmaVolume(MarketArgs);
				}
				
				#region DEFAULT_IMPS
				
				public IEnumerator<KeyValuePair<double, MarketOrder>> GetEnumerator(){
					return ladder.GetEnumerator();
				}
				public MarketOrder this[double price]
				{
					get{ return ladder[price]; }
			        set{ ladder[price] = value; }
				}
				public bool TryRemove(double price, out MarketOrder marketOrder){
					return ladder.TryRemove(price, out marketOrder);
				}
				public bool PriceExists(double price){
					return ladder.ContainsKey(price);
				}
				public void Clear()
				{
					this.ladder.Clear();
				}
				public int Count
				{
					get{ return this.ladder.Count; }
				}
				
				#endregion
			}
			#region VOLUME_PROFILE
			
			public class Profile
			{
				private SEBars<Profile.Ladder> internalProfileBars;
				private Func<int, bool> calculateProfilePeriod;
				private Ladder realtimeProfileLadder;
				private bool _realtimeLockCalcs;
				private Bars NT8Bars;
				private WyckoffBars internalWBars;
				private int Period;
				private int startBarIndex;
				private int lastBarIndex;
				private double minPrice;
				private double maxPrice;
				
				public Profile(WyckoffBars wyckoffBars)
				{
					this.internalProfileBars = new SEBars<Profile.Ladder> ();
					this.internalWBars = wyckoffBars;
					this.NT8Bars = this.internalWBars.NT8Bars;
					// !- Por defecto 1 dia de perfil de volumen
					this.Period = 1;
					this.calculateProfilePeriod = this._calculateDaysPeriod;
					this.startBarIndex = 0;
					this.realtimeProfileLadder= null;
					//this.beginTime = internalWBars.NT8Bars.GetTime(0); //  tiempo en que empezamos el profile
				}
				// !- esta clase tiene como objetivo optimzar los calculos en tiempo real, evitando
				// sobrecargar recursos innecesariamente
				public class Ladder : PriceLadder
				{
					private MarketOrder _minVolume;
					private MarketOrder _maxVolume;
					private MarketOrder _profileVolume;					
					private double _minPrice;
					private double _maxPrice;
					private int startIndex;
					private int endIndex;
					
					public Ladder(int startBarIndex, int endBarIndex)
					{
						this.startIndex = startBarIndex;
						this.endIndex = endBarIndex;
						this._minVolume = new MarketOrder();
						this._maxVolume = new MarketOrder();
						this._profileVolume = new MarketOrder();
					}
					public void setStartBarIndex(int barIndex)
					{
						this.startIndex= barIndex;
					}
					public void setEndBarIndex(int barIndex)
					{
						this.endIndex= barIndex;
					}
					// !- Optmizacion del Profile, de este modo hacemos los calculos una ves y los guardamos
					public void CalculateMinAndMax()
					{
						base.CalculateMinAndMax(ref _minVolume, ref _maxVolume, out _minPrice, out _maxPrice);
					}
					public void CalculateTotalVolume()
					{
						base.CalculateTotalVolume(ref _profileVolume);
					}
					public MarketOrder MinVolume
					{
						get{ return this._minVolume; }
					}
					public MarketOrder MaxVolume
					{
						get{ return this._maxVolume; }
					}
					public double MinLadderPrice
					{
						get{ return this._minPrice; }
					}
					public double MaxLadderPrice
					{
						get{ return this._maxPrice; }
					}
					public MarketOrder ProfileVolume
					{
						get{ return this._profileVolume; }
					}
					public int StartBarIndex
					{
						get{ return this.startIndex; }
					}
					public int EndBarIndex
					{
						get{ return this.endIndex; }
					}
					public int TotalBars
					{
						get{ return this.endIndex - this.startIndex; }
					}
				}
				#region VOLUME_PROFILE_PERIOD
				
				// !- Calculos para el rango del volume profile
				public void setTimePeriod(int Period, PeriodMode periodMode)
				{
					switch(periodMode)
					{
//						case PeriodMode.Bars:
//						{
//							this.calculateProfilePeriod = this._calculateBarsPeriod;
//							break;
//						}
						case PeriodMode.Minutes:
						{
							this.calculateProfilePeriod = this._calculateMinutesPeriod;
							break;
						}
						case PeriodMode.Hours:
						{
							this.calculateProfilePeriod = this._calculateHoursPeriod;
							break;
						}
						case PeriodMode.Days:
						{
							this.calculateProfilePeriod = this._calculateDaysPeriod;
							break;
						}
					}
					this.Period = Period;
				}
//				private bool _calculateBarsPeriod(int barIndex)
//				{
//					int currBar = this.internalWBars.CurrentBarIndex;
//					if( currBar < Period )
//						return false;
//					return currBar%Period == 0;
//				}
				private bool _calculateMinutesPeriod(int barIndex)
				{
					return (this.NT8Bars.GetTime(barIndex) - this.NT8Bars.GetTime(this.startBarIndex)).TotalMinutes > this.Period;
				}
				private bool _calculateHoursPeriod(int barIndex)
				{
					return (this.NT8Bars.GetTime(barIndex) - this.NT8Bars.GetTime(this.startBarIndex)).TotalHours > this.Period;
				}
				private bool _calculateDaysPeriod(int barIndex)
				{
					DateTime currTime = this.NT8Bars.GetTime(barIndex);
					DateTime prevTime = this.NT8Bars.GetTime(this.startBarIndex);
					int currDay = currTime.Day;
					int prevDay = prevTime.Day;
					
					return ( currDay != prevDay && Math.Abs(currDay - prevDay) >= this.Period );//return (- ).TotalDays >= this.Period;
				}
				
				#endregion
				public void setRealtimeCalculations(bool realtimeCalculation)
				{
					if( realtimeCalculation ){
						if( this.realtimeProfileLadder != null ){
							this.realtimeProfileLadder.Clear();
							this.realtimeProfileLadder = null;
						}
						this.realtimeProfileLadder = new Ladder(-1, -1);
					}
					this._realtimeLockCalcs = false;
				}
				// !- Agregamos el perfil dinamicamente(mercado real)
				private void AddRealtimeProfile(int currentBar, MarketDataEventArgs MarketArgs)
				{
					if( !this._realtimeLockCalcs ){
						WyckoffBars.Bar bar;
						// !- empezamos desde el ultimo volume profile
						for(int i = this.startBarIndex;i <= currentBar; i++){
							bar = this.internalWBars[i];
							foreach(var b in bar){
								realtimeProfileLadder.AddPrice(b.Key, b.Value);
							}
						}
						// !- lo necesitamos para los calculos de graficos
						realtimeProfileLadder.setStartBarIndex(this.startBarIndex);
						this._realtimeLockCalcs = true;
					}
					// !- agregamos la nueva informacion que llegue
					realtimeProfileLadder.AddPrice(MarketArgs);
					realtimeProfileLadder.setEndBarIndex(currentBar);
					// !- costoso pero necesario...
					realtimeProfileLadder.CalculateMinAndMax();
					realtimeProfileLadder.CalculateTotalVolume();
				}
				private bool _addProfile(int beginIndex, int endIndex)
				{
					Ladder pl = new Ladder(beginIndex, endIndex);
					int totalBars = pl.TotalBars;
					int startIndex = endIndex - totalBars;
					
					WyckoffBars.Bar bar;
					// !- Hasta la barra actual
					for(;startIndex <= endIndex; startIndex++)
					{
						bar = this.internalWBars[startIndex];
						// *- iteramos cada nivel de precio de la barra actual
						foreach(var b in bar){
							pl.AddPrice(b.Key, b.Value);
						}
					}
					// !- Hacemos los calculos una vez, optimizando asi la informacion de volumen
					pl.CalculateMinAndMax();
					pl.CalculateTotalVolume();
					return this.internalProfileBars.Add(endIndex, pl);
				}
				// !- agregamos el perfil en el rango de barras seleccionado por el usuario
				public bool AddRangeProfile(int beginIndex, int endIndex)
				{
					return _addProfile(beginIndex, endIndex);
				}
				// !- Agregamos el perfil estaticamente
				public void AddMarketProfile(int barIndex, MarketDataEventArgs MarketArgs)
				{
					if( this.internalWBars == null ){
						return;
					}
					if( this.internalWBars.IsNewBar && calculateProfilePeriod(barIndex)  ){
						// *- al crear un nuevo perfil podemos borrar el buffer de tiempo real y limpiar los calculos
						this.realtimeProfileLadder.Clear();
						this._realtimeLockCalcs = false;
						
						bool added = this._addProfile(this.startBarIndex, barIndex);
						this.startBarIndex = barIndex;// + 1;	
					}
					if( this.internalWBars.IsMarketRealtime && this.realtimeProfileLadder != null ){
						this.AddRealtimeProfile(this.internalWBars.CurrentBarIndex, MarketArgs);
					}
				}
				
				public  IEnumerator<KeyValuePair<int, Ladder>> GetEnumerator()
				{
					return this.internalProfileBars.GetEnumerator();
				}
				public  Ladder GetProfile(int barIndex)
				{
					return this.internalProfileBars[barIndex];
				}
				public void RemoveProfile(int barIndex)
				{
					this.internalProfileBars.Remove(barIndex);
				}
				public Ladder GetRealtimeProfile
				{
					get{ return this.realtimeProfileLadder; }
				}
				public int LastProfileIndex
				{
					get{ return this.startBarIndex; }
				}
				// !- con esta funcion podemos obtener un determinado perfil de volumen
				// en el rango que haya sido creado si el indice de la barra pasada como
				// argumento se encuentra dentro del rango de este
				public int GetProfileInRange(int barIndex)
				{
					int idx;
					foreach( var p in this.internalProfileBars )
					{
						idx = p.Key;
						if( Math2.isBounded(barIndex, p.Value.StartBarIndex, idx) ){
							return idx;
						}
					}
					return -1;
				}
				public void Clear()
				{
					this.internalProfileBars.Clear();
				}
				public bool Exists(int barIndex)
				{
					return internalProfileBars.ContainsKey(barIndex);
				}
				public MarketOrder GetProfileVolume(int barIndex)
				{
					return internalProfileBars[barIndex].ProfileVolume;
				}
				public double GetLadderHighPrice(int barIndex)
				{
					return internalProfileBars[barIndex].HighPrice;
				}
				public double GetLadderLowPrice(int barIndex)
				{
					return internalProfileBars[barIndex].LowPrice;
				}
				public MarketOrder GetLadderMaxVolume(int barIndex)
				{
					return internalProfileBars[barIndex].MaxVolume;
				}
				public MarketOrder GetLadderMinVolume(int barIndex)
				{
					return internalProfileBars[barIndex].MinVolume;
				}
				public int TotalBars(int barIndex)
				{
					return this.internalProfileBars[barIndex].TotalBars; //barIndex - internalProfileBars[barIndex].StartBarIndex;
				}
				public int TotalProfiles
				{
					get { return this.internalProfileBars.Count; } 
				}
				public int StartBarIndex(int barIndex)
				{
					return internalProfileBars[barIndex].StartBarIndex;
				}
			}
			
			#endregion
			#region BOOKMAP_CORE
			
			public enum OrderType
			{
				Bid, Ask,
//				BidRemoved, AskRemoved,
				Unknown
			}
			public class OrderInfo
			{
				public long Volume;
				public OrderType Type;
				
				public OrderInfo(){}
				public OrderInfo(long Volume, OrderType orderType){
					this.Volume = Volume;
					this.Type = orderType;
				}
			}
			public class OrderBookLadder
			{
				private ConcurrentDictionary<double, OrderInfo> orderLadder;
				// !- Mayor cluster de ladder
				private long maxOrderVolume;
				private double maxOrderPrice;
				// !- Alto mas alto del ladder ASK
				private long highOrderVolume;
				private double highOrderPrice;
				// !- Alto mas alto del ladder BID
				private long lowOrderVolume;
				private double lowOrderPrice;
				// !- Precio
				private double marketPrice;
				private double tickSize;
				
				private int priceLadderRange;
				private bool isDefaultLadder;
				
				public OrderBookLadder(double tickSize)
				{
					this.orderLadder = new ConcurrentDictionary<double, OrderInfo>();
					this.maxOrderVolume = 0;
					this.highOrderVolume = 0;
					this.highOrderPrice = 0;
					this.lowOrderVolume = long.MaxValue;
					this.lowOrderPrice = double.MaxValue;
					this.marketPrice = 0;
					this.tickSize = tickSize;
					// !- por defecto 10 niveles de precio
					this.priceLadderRange = 10;
					this.isDefaultLadder= true;
				}
				private void _setLadderMinAndMax(MarketDepthEventArgs depthMarketArgs)
				{
					double price = depthMarketArgs.Price;
					long volume = depthMarketArgs.Volume;
					
					if( volume >= this.maxOrderVolume )
					{
						this.maxOrderPrice = price;
						this.maxOrderVolume = volume;
					}
					// !- alto mas alto del ladder ASK
					if( depthMarketArgs.MarketDataType == MarketDataType.Ask )
					{
						if( price >= this.highOrderPrice ){
							this.highOrderPrice = price;
							this.highOrderVolume = volume;
						}
						return;
					}
					// !- bajo mas bajo del ladder BID
					if( depthMarketArgs.MarketDataType == MarketDataType.Bid )
					{
						if( price <= this.lowOrderPrice ){
							this.lowOrderPrice = price;
							this.lowOrderVolume = volume;
						}
					}
				}
				private void _setLadderMinAndMax(double price, OrderInfo orderInfo)
				{
					long volume = orderInfo.Volume;
					if( volume >= this.maxOrderVolume )
					{
						this.maxOrderVolume = volume;
						this.maxOrderPrice = price;
					}
					// !- alto mas alto del ladder ASK
					if( orderInfo.Type == OrderType.Ask )
					{
						if( price >= this.highOrderPrice ){
							this.highOrderVolume = volume;
							this.highOrderPrice = price;
						}
						return;
					}
					// !- bajo mas bajo del ladder BID
					if( orderInfo.Type == OrderType.Bid )
					{
						if( price <= this.lowOrderPrice ){
							this.lowOrderVolume = volume;
							this.lowOrderPrice = price;
						}
					}
				}
				// !- True: excede el rango de la escalera de precios
				private bool exceedsPriceLadder(MarketDepthEventArgs depthMarketArgs)
				{
					// !- por defecto son 10 niveles
					if( this.isDefaultLadder ){
						return false;
					}
					double dist = Math.Abs(depthMarketArgs.Price - this.marketPrice) / this.tickSize;
					if( depthMarketArgs.MarketDataType == MarketDataType.Ask ){
						if( dist >= this.priceLadderRange)
							return true;
					}
					else if( depthMarketArgs.MarketDataType == MarketDataType.Bid ){
						if( dist >= (this.priceLadderRange + 1) )
							return true;
					}
					
					return false;
				}
				private bool exceedsPriceLadder(double price, OrderInfo orderInfo)
				{
					// !- por defecto son 10 niveles
					if( this.isDefaultLadder ){
						return false;
					}
					if( orderInfo.Volume == 0 ){
						return true;
					}
					double dist = Math.Abs(price - this.marketPrice) / this.tickSize;
					if( orderInfo.Type == OrderType.Ask ){
						if( dist >= this.priceLadderRange)
							return true;
					}
					else if( orderInfo.Type == OrderType.Bid ){
						if( dist >= (this.priceLadderRange + 1))
							return true;
					}
					return false;
				}
				public bool PriceExists(double price)
				{
					return this.orderLadder.ContainsKey(price);
				}
				// !- si el rango permitido de la escalera de precios supera los 10 niveles
				// guardamos los 10+X niveles en el diccionario de precios, estos quedaran
				// en esos niveles(por donde el precio pase/paso) hasta que el precio pase
				// nuevamente...
				public void SetLadderRange(int priceLadderRange)
				{
					// !- por defecto solo se muetran 10 niveles de precio del libro de ordenes
					if( priceLadderRange <= 10 ){						
						this.isDefaultLadder = true;
					}
					else{
						this.priceLadderRange = priceLadderRange;
						this.isDefaultLadder = false;
					}
				}
				// !- seteamos el precio actual de mercado, el cual representa
				// el "centro" de la escalera de precios
				public void SetMarketPrice(double marketPrice)
				{
					this.marketPrice = marketPrice;
				}
				// !- obtiene la informacion de mercado en la escalera de precios si esta existe
				// de otro modo la crea
				private OrderInfo getOrderInfo(MarketDepthEventArgs depthMarketArgs, bool newOrder)
				{
					OrderInfo orderInfo;
					// !- si no existe la orden la creamos
					if( newOrder == true ){
						orderInfo = new OrderInfo();
					}
					else{
						orderInfo = this.orderLadder[depthMarketArgs.Price];
					}
					orderInfo.Type = OrderType.Unknown;
					if( depthMarketArgs.MarketDataType == MarketDataType.Ask )
						orderInfo.Type = OrderType.Ask;
					else if( depthMarketArgs.MarketDataType == MarketDataType.Bid )
						orderInfo.Type = OrderType.Bid;
					
					// !- actualizamos las ordenes entrantes si por defecto solo permitimos
					// 10 niveles de precio
					if( this.isDefaultLadder ){
						orderInfo.Volume = depthMarketArgs.Volume;
						return orderInfo;
					}
					// !- no actualizamos el volumen de orden, asi aun si la orden fue removida
					// dejamos la huella de esta.
					if( depthMarketArgs.Operation != Operation.Remove ){
						orderInfo.Volume = depthMarketArgs.Volume;
					}
					return orderInfo;
				}
				public void AddOrder(double marketPrice, MarketDepthEventArgs depthMarketArgs)
				{
					double price = depthMarketArgs.Price;
					// !- precio del mercado
					this.marketPrice = marketPrice;
					// !- movemos el minimo y maximo de la escalera de precio
					this._setLadderMinAndMax(depthMarketArgs);
					// -- Si el nivel de precio existe actualizamos los valores
					if( orderLadder.ContainsKey(price)){
						// -- Si la orden fue actualiza en el nivel de precio entonces,
						// actualizamos ese nivel en el ladder
						orderLadder[price] = this.getOrderInfo(depthMarketArgs, false);
					}
					else{
						if( this.exceedsPriceLadder(depthMarketArgs) )
							return;
						// -- Una nueva orden a mercado ha sido lanzada, creamos el nivel de precio en el ladder
						orderLadder[price] = this.getOrderInfo(depthMarketArgs, true);
					}
				}
				public void AddOrder(MarketDepthEventArgs depthMarketArgs)
				{
					this.AddOrder(depthMarketArgs.Price, depthMarketArgs);
				}
				public void AddOrder(double price, OrderInfo orderInfo)
				{
					// !- si la orden fue removida o excede el nivel de precio configurado
					if( this.exceedsPriceLadder(price, orderInfo) || orderLadder.ContainsKey(price) )
						return;
					orderLadder[price] = new OrderInfo(orderInfo.Volume, orderInfo.Type);
					this._setLadderMinAndMax(price, orderInfo);
				}
				public void RemoveOrder(double price)
				{
					OrderInfo tmp;
					this.orderLadder.TryRemove(price, out tmp);
				}
				
				public long MaxOrderVolume
				{
					get{ return this.maxOrderVolume; }
				}
				public double MaxOrderPrice
				{
					get{ return this.maxOrderPrice; }
				}
				public long HighOrderVolume
				{
					get{ return this.highOrderVolume; }
				}
				public double HighOrderPrice
				{
					get{ return this.highOrderPrice; }
				}
				public long LowOrderVolume
				{
					get{ return this.lowOrderVolume; }
				}
				public double LowOrderPrice
				{
					get{ return this.lowOrderPrice; }
				}
				public double MarketPrice
				{
					get{ return this.marketPrice; }
				}
				public IEnumerator<KeyValuePair<double, OrderInfo>> GetEnumerator(){
					return orderLadder.GetEnumerator();
				}
				public OrderInfo this[double price]
				{
					get{ return this.orderLadder[price]; }
			        set{ this.orderLadder[price] = value; }
				}
				public int Count
				{
					get{ return this.orderLadder.Count; }
				}
				public void Clear()
				{
					this.orderLadder.Clear();
				}
			}
			public class BookMap
			{
				private int currBarIndex;
				private bool firstOrder;
				private double tickSize;
				private string sessionFile;
				private float filterSessionPercent;
				private Bars NT8_Bars;
				private Dictionary<DateTime, OrderBookLadder> bookMap;
				private SortedDictionary<double, OrderInfo> orderBookDB;
				private int ladderRange;
				private DateTime lastMarketTime;
				
				public BookMap(Bars bars)
				{
					this.NT8_Bars = bars;
					this.tickSize = bars.Instrument.MasterInstrument.TickSize;
					this.bookMap = new Dictionary<DateTime, OrderBookLadder>();
					this.orderBookDB = null;
					this.currBarIndex = 0;
					this.filterSessionPercent = 0;
					this.firstOrder = true;
					this.sessionFile= string.Empty;
					// !- 10 niveles en la escalera de precios por defecto
					this.ladderRange= 10;
				}
				public Bars NT8Bars
				{
					get{ return this.NT8_Bars; }
				}
				
				public OrderBookLadder getOrderBookLadder(int barIndex)
				{
					DateTime barTime = this.NT8_Bars.GetTime(barIndex);
					if( !bookMap.ContainsKey(barTime) )
						return null;
					return bookMap[barTime];
				}
				public void setLadderRange(int ladderRange)
				{
					this.ladderRange = ladderRange;
				}
				
				#region BOOKMAP_SESSION_LOADER
				
				public enum SessionError{
					SUCCESSFUL_LOADING,
					FILE_PATH_ERROR,
					TIMESTAMP_ERROR,
					SESSION_NOT_EXIST,
					LADDER_LOADING_ERROR,
					TIMEFRAME_ERROR,
					INSTRUMENT_ERROR
				}
				public void SaveSessionFile(string sessionFile)
				{
					this.sessionFile = sessionFile;
					// !- si no existe lo creamos
					if( orderBookDB == null ){
						this.orderBookDB = new SortedDictionary<double, OrderInfo>();
					}
					else{
						this.orderBookDB.Clear();
					}
				}
				public void setFilterSessionPercent(float filterPercent)
				{
					this.filterSessionPercent = filterPercent;
				}
				private SessionError getInstrumentInfo(string line)
				{
					string[] instrument_info = line.Split(new char[]{' '});
					// !- periodo
					if( !instrument_info[0].Equals( this.NT8_Bars.BarsType.BarsPeriod.Value.ToString()) ||
						// !- tiempo: minute, second, etc..
						!instrument_info[1].Equals( "Second" ) ){//this.NT8_Bars.BarsType.BarsPeriod ) ){
						return SessionError.TIMEFRAME_ERROR;
					}
					if( !instrument_info[2].Equals( this.NT8_Bars.Instrument.MasterInstrument.Name ) ){
						return SessionError.INSTRUMENT_ERROR;
					}
					return SessionError.SUCCESSFUL_LOADING;
				}
				private int loadNewBar(string line, ref DateTime orderBookLadderTimestamp)
				{
					string s_orderBookLadderTimestamp = string.Empty;
					// !- extraemos el timestamp de la barra(copiamos hasta el BID del ladder)
					int bar_time_pos = StrUtils.extractInfo(line, out s_orderBookLadderTimestamp, '(');
					// -- Convertimos la informacion extraida a DateTime
					if( !DateTime.TryParse(s_orderBookLadderTimestamp, out orderBookLadderTimestamp) ){
						return -1;
					}
					bookMap[orderBookLadderTimestamp] = new OrderBookLadder(this.tickSize);
					bookMap[orderBookLadderTimestamp].SetLadderRange(this.ladderRange);	
					
					return bar_time_pos;
				}
				private int loadLadderInfo(string line, int start_pos, ref OrderBookLadder orderBookLadder, char first_key, char end_key, OrderType orderType)
				{
					string info = string.Empty;
					double price;
					long volume;
					// !- extramos la informacion del ladder y la 'spliteamos' para obtener el precio y volumen
					int last_pos = StrUtils.extractInfo(line, out info, first_key, end_key, start_pos);
					if( last_pos == -1 )
						return -1;
					string[] order_info = info.Split(new char[]{';'});
					OrderInfo orderInfo = new OrderInfo();
					
					foreach(string s in order_info){
						int s_pos = s.IndexOf(' ');
						if( s_pos == -1 )
							continue;
						
						// !- copiamos el precio y lo convertimos a double
						if(!double.TryParse(s.Substring(0, s_pos), out price))
							return -1;
						if(double.IsNaN(price) || double.IsInfinity(price))
							return -1;
						// !- copiamos el volumen y lo convertimos a long
						if(!long.TryParse(s.Substring(s_pos + 1), out volume))
							return -1;
						
						orderInfo.Volume = volume;
						orderInfo.Type = orderType;
						orderBookLadder.AddOrder(price, orderInfo);
					}
					
					return last_pos;
				}
				private bool copyLastLadderInfo(int barIndex, ref OrderBookLadder orderBookLadder)
				{
					int prevBarIndex = barIndex - 1;
					if( prevBarIndex < 0 )
						return false;
					OrderBookLadder orderLadder = this.getOrderBookLadder(prevBarIndex);
					if( orderLadder == null )
						return false;
					// !- copiamos la escalera anterior de precios a la barra actual
					//bookMap[orderBookLadderTimestamp].SetMarketPrice(this.NT8_Bars.GetClose(prevBarIndex));
					foreach(var order in orderLadder){
						orderBookLadder.AddOrder(order.Key, order.Value);
					}
					return true;
				}
				private void filterLadderInfo(ref OrderBookLadder orderBookLadder)
				{
					float volPer = 0;
					long maxVolumeLadder = orderBookLadder.MaxOrderVolume;
					List<double> removals = new List<double>();
					foreach(var order in orderBookLadder){
						if( (float)Math2.Percent(maxVolumeLadder, order.Value.Volume) < this.filterSessionPercent ){
							removals.Add(order.Key);
						}
					}
					foreach(double price in removals){
						orderBookLadder.RemoveOrder(price);
					}
				}
				public SessionError LoadSessionFile(string sessionFile)
				{
					SessionError err = SessionError.SUCCESSFUL_LOADING;
					if( bookMap == null || sessionFile.IsNullOrEmpty() ){
						return SessionError.FILE_PATH_ERROR;
					}
					// !- si en el mapa ya existia informacion la descartamos...
					bookMap.Clear();
					using (StreamReader bookMapSessionFile = File.OpenText(sessionFile))
				    {
						string line;
						bool is_first_line = true;
						int barIndex = -1;
						int totalBars = this.NT8_Bars.Count;
						DateTime orderBookLadderTimestamp = new DateTime();
						
						while ((line = bookMapSessionFile.ReadLine()) != null)
						{
							if( line.IsNullOrEmpty() )
								continue;
							if( line[0] == '#' ){
								// !- omitimos el #
								err = getInstrumentInfo(line.Substring(1));
								if( err != SessionError.SUCCESSFUL_LOADING )
									break;
								continue;
							}
							
							int timestamp_pos = loadNewBar(line, ref orderBookLadderTimestamp);
							if( timestamp_pos < 0 ){
								err = SessionError.TIMESTAMP_ERROR;
								break;
							}
							
							OrderBookLadder orderBookLadder = new OrderBookLadder(this.tickSize);
							orderBookLadder.SetLadderRange(this.ladderRange);
							/// !- ya que .GetBar es expensiva buscamos(solo una vez) el tiempo correspondiente
							/// a la primera barra guardada en la sesion, luego solo incrementamos el puntero
							/// a medida que se crean las barras generadas por el bookmap
							if( !is_first_line ){
								// !- la fecha extraida de la sesion no existe(porque probablemente el grafico no haya llegado a tal punto)
								if( this.NT8_Bars.LastBarTime.CompareTo(orderBookLadderTimestamp) < 0 ){
									err = SessionError.SESSION_NOT_EXIST;
									break;
								}
								barIndex = this.NT8_Bars.GetBar(orderBookLadderTimestamp);
							}
							else{
								barIndex++;
							}
							// !- las barras aun no existen, no necesitamos cargar nada...
							if( barIndex > totalBars )
								break;
							// !- el precio actual de mercado en el momento en que se creo el ladder
							orderBookLadder.SetMarketPrice(this.NT8_Bars.GetClose(barIndex));
							
							// !- primero copiamos el ladder Bid(porque en este orden fue guardado)
							int bid_pos = loadLadderInfo(line, timestamp_pos, ref orderBookLadder, '(', ')', OrderType.Bid);
							if( bid_pos < 0 ){
								err = SessionError.LADDER_LOADING_ERROR;
								break;
							}
							// !- finalmente copiamos el ladder Ask
							int ask_pos = loadLadderInfo(line, bid_pos, ref orderBookLadder, '{', '}', OrderType.Ask);
							if( ask_pos < 0 ){
								err = SessionError.LADDER_LOADING_ERROR;
								break;
							}
							// !- copiamos los precios de la escalera de precios previa para ir formando
							// el mapa de liquidez
							if( !is_first_line )
							{
								if( !copyLastLadderInfo( barIndex, ref orderBookLadder ) ){
									err = SessionError.LADDER_LOADING_ERROR;
									break;
								}
							}
							// !- si el % del filtro de liquidez es mayor que 0 filtramos...
							if( this.filterSessionPercent > 0 ){
								filterLadderInfo(ref orderBookLadder);
							}
							// !- copiamos el ladder al bookmap
							bookMap[orderBookLadderTimestamp] = orderBookLadder;
							
							is_first_line = false;
						}
						// !- liberamos el archivo...
						bookMapSessionFile.Close();
						bookMapSessionFile.Dispose();
					}
					
					return err;
				}
				
				#endregion
				#region BOOKMAP_SESSION_REGISTER
				
				private void addSessionOrder(MarketDepthEventArgs depthMarketArgs)
				{
					if( orderBookDB == null )
						return;
					long volume = depthMarketArgs.Volume;
					if( volume == 0 )
						return;
					
					double price = depthMarketArgs.Price;
					OrderType orderType = OrderType.Unknown;
					if( depthMarketArgs.MarketDataType == MarketDataType.Ask )
						orderType = OrderType.Ask;
					if( depthMarketArgs.MarketDataType == MarketDataType.Bid )
						orderType = OrderType.Bid;
					
					// -- Si el nivel de precio existe actualizamos los valores
					if( orderBookDB.ContainsKey(price)){
						// -- Si la orden fue actualiza en el nivel de precio entonces,
						// actualizamos ese nivel en el ladder
						orderBookDB[price].Type = orderType;
						orderBookDB[price].Volume = volume;
					}
					else{
						// -- Una nueva orden a mercado ha sido lanzada, creamos el nivel de precio en el ladder
						orderBookDB[price] = new OrderInfo(volume, orderType);
					}
				}
				private void saveLastOrderBook()
				{
					if( orderBookDB == null )
						return;
					// !- lo ejecutamos solo la primera vez(es decir la primer orden)
					if( firstOrder ){
						string instrument_info = this.NT8_Bars.BarsType.BarsPeriod.ToString() + ' ';
						instrument_info += this.NT8_Bars.Instrument.MasterInstrument.Name.ToString();
						
						File.AppendAllText(this.sessionFile, '#' + instrument_info + '\n');
						firstOrder = false;
						return;
					}
					
					string askInfo = string.Empty;
					string bidInfo = string.Empty;
					double price;
					long volume;
					foreach(var order in orderBookDB){
						volume= order.Value.Volume;
						price = order.Key;
						if( order.Value.Type == OrderType.Ask ){
							askInfo += price;
							askInfo += ' ';
							askInfo += volume;
							askInfo += ';';
						}
						if( order.Value.Type == OrderType.Bid ){
							bidInfo += price;
							bidInfo += ' ';
							bidInfo += volume;
							bidInfo += ';';
						}
					}
					
					File.AppendAllText(this.sessionFile, lastMarketTime.ToString() +
						'(' + bidInfo + ')' +
						'{' + askInfo + '}' +
						'\n'
					);
					
					orderBookDB.Clear();
				}
				
				#endregion
				private void copyLastLadder(DateTime barTime)
				{
					int prevBarIndex = this.currBarIndex - 2;
					if( prevBarIndex < 0 )
						return;
					OrderBookLadder orderLadder = this.getOrderBookLadder(prevBarIndex);//bookMap[this.NT8_Bars.GetTime(prevBarIndex)];
					if( orderLadder == null )
						return;
					// !- copiamos la escalera anterior de precios a la barra actual
					bookMap[barTime].SetMarketPrice(this.NT8_Bars.LastPrice);//bookMap[toBarTime].SetMarketPrice(orderLadder.MarketPrice);
					foreach(var order in orderLadder){
						bookMap[barTime].AddOrder(order.Key, order.Value);
					}
					// !- limpiamos la informacion de maxima orden de la escalera de precios
					//bookMap[toBarTime].resetMaxOrderInfo();
				}
				// !- nivel II
				public void onMarketDepth(MarketDepthEventArgs depthMarketArgs)
				{
					double price = depthMarketArgs.Price;
					long volume = depthMarketArgs.Volume;
					this.currBarIndex = this.NT8_Bars.Count;
					DateTime currBarTime = this.NT8_Bars.GetTime(currBarIndex);
					
					// -- Si no existe la barra la creamos
					if( !bookMap.ContainsKey(currBarTime) ){
						bookMap[currBarTime] = new OrderBookLadder(this.tickSize);
						bookMap[currBarTime].SetLadderRange(this.ladderRange);
						this.copyLastLadder(currBarTime);
						this.saveLastOrderBook();
					}
					bookMap[currBarTime].AddOrder(this.NT8_Bars.LastPrice, depthMarketArgs);
					
					this.addSessionOrder(depthMarketArgs);
					// !- ultimo tiempo de barra creado, necesario para la DB
					this.lastMarketTime = currBarTime;
				}
			}
			
			#endregion
			#region WYCKOFF_BARS_CLASS
			
			public class WyckoffBars : SEBars<WyckoffBars.Bar>
			{
				private bool isNewBar;
				private long minClusterVolumeFilter;
				private VolumeType volumeType;
				private int currentBar;
				private int lastBarLoaded;
				private double tickSize;
				protected Bars NT8_Bars;
				
				public WyckoffBars(Bars bars)
				{
					this.isNewBar = false;
					this.minClusterVolumeFilter = -1;
					this.NT8_Bars = bars;
					this.lastBarLoaded = bars.Count - 1;
					this.tickSize = bars.Instrument.MasterInstrument.TickSize;
					this.currentBar = 0;
					this[currentBar] = new Bar();
					//DateTime dt = bars.GetTime(bars.Count - 1);
					//this.lastBarTime = new DateTime(dt.Year, dt.Month, dt.Day, dt.Hour, dt.Minute, dt.Second);
					//this.lastBarTime = this.lastBarTime.AddSeconds(-5);
					// !- Inicializamos las barras cargadas hasta el momento
					//for(int i = 0; i <= barsCount; i++) this[i] = new Bar();
				}
				
				public class Bar : MarketOrder
				{
					private PriceLadder pvLadder;
					private MarketOrder barOrderFlow;
					private MarketOrder minCluster;
					private MarketOrder maxCluster;
					private double minClusterPrice;
					private double maxClusterPrice;
					//public DateTime Time;
					
					public Bar()
					{
						this.pvLadder = new PriceLadder();
						this.minCluster = new MarketOrder();
						this.maxCluster = new MarketOrder();
					}
					public IEnumerator<KeyValuePair<double, MarketOrder>> GetEnumerator(){
						return pvLadder.GetEnumerator();
					}
					// !- la funcion recibe ordenes a mercado, por lo tanto la estructura $MarketDataEventArgs
					// cambia de informacion continuamente; precio, bid, ask, volume....
					public void CalculateMarketInfo(MarketDataEventArgs MarketArgs)
					{
						pvLadder.AddPrice(MarketArgs);
						this.CalculateSigmaVolume(MarketArgs);
					}
					public void CalculateMinAndMaxCluster()
					{
						pvLadder.CalculateMinAndMax(ref this.minCluster, ref this.maxCluster,
							out this.minClusterPrice, out this.maxClusterPrice);
					}
					public void FilterClusterVolume(long minVolume, VolumeType volType)
					{
						//List<double> priceRemovals = new List<double>();
						double price;
						foreach(var pl in pvLadder)
						{
							price = pl.Key;
							switch( volType )
							{
								case VolumeType.BidAsk:
								{
									if( Math.Abs(pl.Value.Total) < minVolume ){
										//priceRemovals.Add(pl.Key);
										MarketOrder mo = pl.Value;
										this.pvLadder.TryRemove(price, out mo);
									}
									break;
								}
								case VolumeType.Delta:
								{
									if( Math.Abs(pl.Value.Delta) < minVolume ){
										//priceRemovals.Add(pl.Key);
										MarketOrder mo = pl.Value;
										this.pvLadder.TryRemove(price, out mo);
									}
									break;
								}
							}
							
						}
						/*foreach(var p in priceRemovals){
							this.pvLadder.Remove(p);
						}*/
					}
//					public MarketOrder AtPrice(double price)
//					{
//						if( !this.pvLadder.PriceExists(price) )
//							return null;
//						return this.pvLadder[price];
//					}
					public bool PriceExists(double price)
					{
						return this.pvLadder.PriceExists(price);
					}
					public MarketOrder MaxClusterVolume
					{
						get{ return maxCluster; }
					}
					public MarketOrder MinClusterVolume
					{
						get{ return minCluster; }
					}
					public double MinClusterPrice
					{
						get{ return this.minClusterPrice; }
					}
					public double MaxClusterPrice
					{
						get{ return this.maxClusterPrice; }
					}
				}
				public void enableMinClusterVolumeFilter(long minClusterVolumeFilter, VolumeType volumeType)
				{
					this.minClusterVolumeFilter = minClusterVolumeFilter;
					this.volumeType = volumeType;
				}
				public void disableMinClusterVolumeFilter()
				{
					this.minClusterVolumeFilter = -1;
				}
				public bool onMarketData(MarketDataEventArgs MarketArgs)
				{
					if( MarketArgs.MarketDataType != MarketDataType.Last )
						return false;
					
					this.isNewBar = false;
					DateTime barTime = NT8_Bars.GetTime(currentBar);
					// *- El tiempo de la orden de mercado supera al finalizado del tiempo de la barra actual
					// => una nueva barra fue creada
					if( MarketArgs.Time.CompareTo(barTime) > 0 )
					{
						// !- Hacemos los calculos del cluster una vez, optmizando asi futuros calculos innecesarios...
						this[currentBar].CalculateMinAndMaxCluster();
						// !- Solo si activamos el filtro de volumen
						if( this.minClusterVolumeFilter != -1 )
							this[currentBar].FilterClusterVolume(this.minClusterVolumeFilter, this.volumeType);
						
						// !- Tiempo de finalizado de la barra en mercado
						//this[currentBar].Time = MarketArgs.Time;
						// !- pasamos a la siguiente barra
						this.currentBar++;
						// !- creamos la nueva barra
						this[currentBar] = new Bar();
						
						this.isNewBar = true;
						//return true;
					}
					// !- Calculos de volumen en cada barra creada
					this[currentBar].CalculateMarketInfo(MarketArgs);
					// !- Ultimo tiempo de mercado
					//this[CurrentBar].Time = MarketArgs.Time;
					
					return true;
				}
				// !- Cuando la barra termina de crearse esta retornara True, luego al pasar
				// a la siguiente nueva barra retornara False, repitiendose asi este ciclo.
				public bool IsNewBar
				{
					get{ return this.isNewBar; }
				}
				public bool BarExists(int barIndex)
				{
					return this.ContainsKey(barIndex);
				}
				//public bool IsRealtime{ get{ return currentBar >= lastBarIndex; } }
				public int CurrentBarIndex
				{
					get{ return this.currentBar; }
				}
				public Bar CurrentBar
				{
					get{ return this[currentBar]; }
				}
				// !- variable constante, una vez corriendo el mercado este valor NO aumentara
				public int LastBarLoadedIndex
				{
					get{ return this.lastBarLoaded; }
				}
				public bool IsMarketRealtime
				{
					get{ return this.CurrentBarIndex >= this.lastBarLoaded; }
				}
				
				public Bar PreviousBar{ get{ return this[currentBar - 1]; } }
				public Bars NT8Bars
				{
					get{ return this.NT8_Bars; }
				}
				public double TickSize
				{
					get{ return this.tickSize; }
				}
			}
			
			#endregion
		}
		#endregion
		
		#endregion
	}
	
	#endregion
}
