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

//This namespace holds Indicators in this folder and is required. Do not change it. 
namespace NinjaTrader.NinjaScript.Indicators
{
	public class CI_ATR_Grid : Indicator
	{
		private CustomEnumNamespace.RangeTimeframe rangeTFType;

		public CI_ATR_Grid()
		{
			VendorLicense("CrystalIndicators", "VolatilityAnalysisBundle", "www.crystalindicators.com",
				"info@crystalindicators.com", null);
		}

		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description									= @"ATR Grid from Crystal Indicators. crystalindicators.com";
				Name										= "CI ATR Grid";
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
				IsSuspendedWhileInactive					= true;

				Period = 15;
				Level1 = 0.5;
				Level2 = 0.7;

				rangeTFType = CustomEnumNamespace.RangeTimeframe.Day;

				Level0507AreaColor = Brushes.Yellow;
				Level071AreaColor = Brushes.Orange;
				
				AddPlot(Brushes.Red, "P1");
				AddPlot(Brushes.Orange, "P07");
				AddPlot(Brushes.Yellow, "P05");
				AddPlot(Brushes.Yellow, "M05");
				AddPlot(Brushes.Orange, "M07");
				AddPlot(Brushes.Red, "M1");
			}
			else if (State == State.Configure)
			{
				//AddDataSeries(Data.BarsPeriodType.Minute, 1440);
				AddDataSeries(Data.BarsPeriodType.Minute, (int) rangeTFType);
			} 
		}

		protected override void OnBarUpdate()
		{
			if (BarsInProgress == 1) return;

			if (CurrentBars[1] < 1) return;

			if ((rangeTFType.Equals(CustomEnumNamespace.RangeTimeframe.Day) &&
				BarsPeriods[0].BarsPeriodType == BarsPeriodType.Minute &&
				BarsPeriods[0].Value <= 240 &&
				(1440 % BarsPeriods[0].Value == 0)) ||
				(rangeTFType.Equals(CustomEnumNamespace.RangeTimeframe.Hour) &&
				BarsPeriods[0].BarsPeriodType == BarsPeriodType.Minute &&
				BarsPeriods[0].Value <= 15 &&
				(60 % BarsPeriods[0].Value == 0)))
            {
				double prevATR = ATR(BarsArray[1], 15)[1];
				double prevClose = Closes[1][0];
				double prevHigh = Highs[1][0];
				double prevLow = Lows[1][0];

				P1[0] = prevClose + prevATR;
				P07[0] = prevClose + prevATR * Level2;
				P05[0] = prevClose + prevATR * Level1;

				Draw.Region(this, "P1-P07", CurrentBars[0], 0, P1, P07, Brushes.Transparent, Level071AreaColor, 50);
				Draw.Region(this, "P07-P05", CurrentBars[0], 0, P07, P05, Brushes.Transparent, Level0507AreaColor, 50);

				M05[0] = prevClose - prevATR * Level1;
				M07[0] = prevClose - prevATR * Level2;
				M1[0] = prevClose - prevATR;

				Draw.Region(this, "M05-M07", CurrentBars[0], 0, M05, M07, Brushes.Transparent, Level0507AreaColor, 50);
				Draw.Region(this, "M07-M1", CurrentBars[0], 0, M07, M1, Brushes.Transparent, Level071AreaColor, 50);
			}
		}

		#region Properties

		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name = "Period", Order = 1, GroupName = "Parameters")]
		public int Period
		{ get; set; }

		[NinjaScriptProperty]
		[Range(0, double.MaxValue)]
		[Display(Name = "Level1", Order = 2, GroupName = "Parameters")]
		public double Level1
		{ get; set; }

		[NinjaScriptProperty]
		[Range(0, double.MaxValue)]
		[Display(Name = "Level2", Order = 3, GroupName = "Parameters")]
		public double Level2
		{ get; set; }

		[NinjaScriptProperty]
		[Display(Name = "Range Timeframe", Order = 4, GroupName = "Parameters", Description = "Choose a Timeframe for a Range.")]
		public CustomEnumNamespace.RangeTimeframe RangeTFType
		{
			get { return rangeTFType; }
			set { rangeTFType = value; }
		}

		[NinjaScriptProperty]
		[XmlIgnore]
		[Display(Name = "0.7-1 Area Color", Order = 5, GroupName = "Parameters")]
		public Brush Level071AreaColor
		{ get; set; }

		[NinjaScriptProperty]
		[XmlIgnore]
		[Display(Name = "0.5-0.7 Area Color", Order = 6, GroupName = "Parameters")]
		public Brush Level0507AreaColor
		{ get; set; }

		[Browsable(false)]
		[XmlIgnore]
		public Series<double> P1
		{
			get { return Values[0]; }
		}

		[Browsable(false)]
		[XmlIgnore]
		public Series<double> P07
		{
			get { return Values[1]; }
		}

		[Browsable(false)]
		[XmlIgnore]
		public Series<double> P05
		{
			get { return Values[2]; }
		}

		[Browsable(false)]
		[XmlIgnore]
		public Series<double> M05
		{
			get { return Values[3]; }
		}

		[Browsable(false)]
		[XmlIgnore]
		public Series<double> M07
		{
			get { return Values[4]; }
		}

		[Browsable(false)]
		[XmlIgnore]
		public Series<double> M1
		{
			get { return Values[5]; }
		}
		#endregion

	}
}

namespace CustomEnumNamespace
{
	public enum RangeTimeframe
	{
		Hour = 60,
		Day = 1440
	}
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private CI_ATR_Grid[] cacheCI_ATR_Grid;
		public CI_ATR_Grid CI_ATR_Grid(int period, double level1, double level2, CustomEnumNamespace.RangeTimeframe rangeTFType, Brush level071AreaColor, Brush level0507AreaColor)
		{
			return CI_ATR_Grid(Input, period, level1, level2, rangeTFType, level071AreaColor, level0507AreaColor);
		}

		public CI_ATR_Grid CI_ATR_Grid(ISeries<double> input, int period, double level1, double level2, CustomEnumNamespace.RangeTimeframe rangeTFType, Brush level071AreaColor, Brush level0507AreaColor)
		{
			if (cacheCI_ATR_Grid != null)
				for (int idx = 0; idx < cacheCI_ATR_Grid.Length; idx++)
					if (cacheCI_ATR_Grid[idx] != null && cacheCI_ATR_Grid[idx].Period == period && cacheCI_ATR_Grid[idx].Level1 == level1 && cacheCI_ATR_Grid[idx].Level2 == level2 && cacheCI_ATR_Grid[idx].RangeTFType == rangeTFType && cacheCI_ATR_Grid[idx].Level071AreaColor == level071AreaColor && cacheCI_ATR_Grid[idx].Level0507AreaColor == level0507AreaColor && cacheCI_ATR_Grid[idx].EqualsInput(input))
						return cacheCI_ATR_Grid[idx];
			return CacheIndicator<CI_ATR_Grid>(new CI_ATR_Grid(){ Period = period, Level1 = level1, Level2 = level2, RangeTFType = rangeTFType, Level071AreaColor = level071AreaColor, Level0507AreaColor = level0507AreaColor }, input, ref cacheCI_ATR_Grid);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.CI_ATR_Grid CI_ATR_Grid(int period, double level1, double level2, CustomEnumNamespace.RangeTimeframe rangeTFType, Brush level071AreaColor, Brush level0507AreaColor)
		{
			return indicator.CI_ATR_Grid(Input, period, level1, level2, rangeTFType, level071AreaColor, level0507AreaColor);
		}

		public Indicators.CI_ATR_Grid CI_ATR_Grid(ISeries<double> input , int period, double level1, double level2, CustomEnumNamespace.RangeTimeframe rangeTFType, Brush level071AreaColor, Brush level0507AreaColor)
		{
			return indicator.CI_ATR_Grid(input, period, level1, level2, rangeTFType, level071AreaColor, level0507AreaColor);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.CI_ATR_Grid CI_ATR_Grid(int period, double level1, double level2, CustomEnumNamespace.RangeTimeframe rangeTFType, Brush level071AreaColor, Brush level0507AreaColor)
		{
			return indicator.CI_ATR_Grid(Input, period, level1, level2, rangeTFType, level071AreaColor, level0507AreaColor);
		}

		public Indicators.CI_ATR_Grid CI_ATR_Grid(ISeries<double> input , int period, double level1, double level2, CustomEnumNamespace.RangeTimeframe rangeTFType, Brush level071AreaColor, Brush level0507AreaColor)
		{
			return indicator.CI_ATR_Grid(input, period, level1, level2, rangeTFType, level071AreaColor, level0507AreaColor);
		}
	}
}

#endregion
