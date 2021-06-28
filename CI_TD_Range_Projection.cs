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
using SharpDX.Direct2D1;
using SharpDX;

//This namespace holds Indicators in this folder and is required. Do not change it. 
namespace NinjaTrader.NinjaScript.Indicators
{
	public class CI_TD_Range_Projection : Indicator
	{
		private CustomEnumNamespace.RangeTimeframes rangeTimeframe;
		private double xValue;
		private double upperRangeValue;
		private double lowerRangeValue;
		private int timeFrameNumber;
		private BarsPeriodType timeFrameType;
		private double upsideLevel;
		private double downsideLevel;

		public CI_TD_Range_Projection()
		{
			VendorLicense("CrystalIndicators", "VolatilityAnalysisBundle", "www.crystalindicators.com",
				"info@crystalindicators.com", null);
		}

		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description									= @"TD Range Projection by Crystal Indicators";
				Name										= "CI TD Range Projection";
				Calculate									= Calculate.OnPriceChange;
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

				ShowToleranceLevels							= true;
				rangeTimeframe								= CustomEnumNamespace.RangeTimeframes.Day;
				UpperBorderColor							= Brushes.Green;
				LowerBorderColor							= Brushes.Red;
				RangeAreaColor								= Brushes.DodgerBlue;
				RangeAreaTransparency						= 20;
				RangeTextLabelColor							= Brushes.Orange;
				UpsideLevelColor							= Brushes.Lime;
				DownsideLevelColor							= Brushes.Magenta;
				ToleranceLevelsAreaColor					= Brushes.Gray;
				ToleranceLevelsAreaTransparency				= 20;
				ToleranceLevelsTextLabelColor				= Brushes.Firebrick;
			}
			else if (State == State.Configure)
			{
				switch(rangeTimeframe)
                {
					case CustomEnumNamespace.RangeTimeframes.Minutes60:
						timeFrameNumber = 60;
						timeFrameType = Data.BarsPeriodType.Minute;
						AddDataSeries(timeFrameType, timeFrameNumber);
						break;
					case CustomEnumNamespace.RangeTimeframes.Minutes240:
						timeFrameNumber = 240;
						timeFrameType = Data.BarsPeriodType.Minute;
						AddDataSeries(timeFrameType, timeFrameNumber);
						break;
					case CustomEnumNamespace.RangeTimeframes.Day:
						timeFrameNumber = 1;
						timeFrameType = Data.BarsPeriodType.Day;
						AddDataSeries(timeFrameType, timeFrameNumber);
						break;
					case CustomEnumNamespace.RangeTimeframes.Week:
						timeFrameNumber = 1;
						timeFrameType = Data.BarsPeriodType.Week;
						AddDataSeries(timeFrameType, timeFrameNumber);
						break;
					case CustomEnumNamespace.RangeTimeframes.Month:
						timeFrameNumber = 1;
						timeFrameType = Data.BarsPeriodType.Month;
						AddDataSeries(timeFrameType, timeFrameNumber);
						break;

				}
				//AddDataSeries(Data.BarsPeriodType.Minute, (int) rangeTimeframe);
			}
		}

		protected override void OnBarUpdate()
		{
			if (CurrentBars[1] < 1) return;
			
			//Calculation of X value for Range Projection
			xValue = calculateXValue(Closes[1][1], Highs[1][1], Lows[1][1], Opens[1][1]);
			upperRangeValue = xValue - Lows[1][1];
			lowerRangeValue = xValue - Highs[1][1];

			if (ShowToleranceLevels)
            {
				//Calculation of upside and downside levels
				double trueRangeValue = calculateTrueRange() * 0.15;
				if (trueRangeValue < 0)
					trueRangeValue = Highs[1][1] - Lows[1][1];

				upsideLevel = Opens[1][0] + trueRangeValue;
				downsideLevel = Opens[1][0] - trueRangeValue;

				//Print("trueRangeValue - " + trueRangeValue);
				//Print("upsideLevel - " + upsideLevel + " downsideLevel - " + downsideLevel);
			}
			

			//Check if we use the lower timeframe on the chart than range's timeframe 
			double weightChart = calculateWeight(BarsPeriods[0].BarsPeriodType, BarsPeriods[0].Value);
			double weightRange = calculateWeight(timeFrameType, timeFrameNumber);

			//Draw area on the lower timeframe on the chart
			if (weightChart < weightRange)
            {
				DateTime start = BarsArray[0].GetTime(BarsArray[0].GetBar(Times[1][1]) + 1);
				DateTime end = Times[1][0];

				//Print("start - " + start);
				//Print("end - " + end);
				//Print("CurrentBar - " + CurrentBars[0]);
				//Print("startId - " + startId);
				//Print("endId - " + endId);

				//NinjaTrader.Gui.Tools.SimpleFont simpleFont = chartControl.Properties.LabelFont ?? new NinjaTrader.Gui.Tools.SimpleFont();
				NinjaTrader.Gui.Tools.SimpleFont simpleFont = new NinjaTrader.Gui.Tools.SimpleFont();

				//Draw Tolerance Levels
				if (ShowToleranceLevels)
				{
					Draw.Rectangle(this, "TD Tolerance Levels Area", true, start, upsideLevel, end, downsideLevel,
								Brushes.Transparent, ToleranceLevelsAreaColor, ToleranceLevelsAreaTransparency);
					Draw.Line(this, "Upside Level Border", true, start, upsideLevel, end, upsideLevel, UpsideLevelColor,
						DashStyleHelper.Dash, 3);
					Draw.Line(this, "Downside Level Border", true, start, downsideLevel, end, downsideLevel,
						DownsideLevelColor, DashStyleHelper.Dash, 3);
					Draw.Text(this, "Upside Level Text", true,
						Instrument.MasterInstrument.RoundToTickSize(upsideLevel).ToString() + " - Upside Tolerance Level",
						end, upsideLevel, (int)simpleFont.Size, ToleranceLevelsTextLabelColor, simpleFont,
						TextAlignment.Left, Brushes.Transparent, Brushes.Transparent, 100);
					Draw.Text(this, "Downside Level Text", true,
						Instrument.MasterInstrument.RoundToTickSize(downsideLevel).ToString() + " - Downside Tolerance Level",
						end, downsideLevel, -(int)simpleFont.Size, ToleranceLevelsTextLabelColor, simpleFont,
						TextAlignment.Left, Brushes.Transparent, Brushes.Transparent, 100);
				}

				//Draw Range Projection
				Draw.Rectangle(this, "TD Range Projection Area", true, start, upperRangeValue, end, lowerRangeValue,
                                Brushes.Transparent, RangeAreaColor, RangeAreaTransparency);
                Draw.Line(this, "Upper Range Border", true, start, upperRangeValue, end, upperRangeValue, UpperBorderColor,
					DashStyleHelper.Solid, 3);
				Draw.Line(this, "Lower Range Border", true, start, lowerRangeValue, end, lowerRangeValue,
					LowerBorderColor, DashStyleHelper.Solid, 3);
				Draw.Text(this, "Upper Range Border Text", true, 
					Instrument.MasterInstrument.RoundToTickSize(upperRangeValue).ToString() + " - Upper Range Border",
					end, upperRangeValue, (int) simpleFont.Size, RangeTextLabelColor, simpleFont,
					TextAlignment.Right, Brushes.Transparent, Brushes.Transparent, 100);
				Draw.Text(this, "Lower Range Border Text", true,
					Instrument.MasterInstrument.RoundToTickSize(lowerRangeValue).ToString() + " - Lower Range Border",
					end, lowerRangeValue, -(int)simpleFont.Size, RangeTextLabelColor, simpleFont,
					TextAlignment.Right, Brushes.Transparent, Brushes.Transparent, 100);
            }

		}

		protected override void OnRender(ChartControl chartControl, ChartScale chartScale)
        {
            base.OnRender(chartControl, chartScale);

			if (BarsPeriods[0].BarsPeriodType.ToString().Equals(rangeTimeframe.ToString())
				&& BarsPeriods[0].Value == timeFrameNumber)
				calculateCoordinatesForRendering(chartControl, chartScale);

		}

		private double calculateTrueRange()
        {
			double trueRange = -1;
			double trueMin = -1;
			double trueMax = -1;
			int i = 1;
			while (i < CurrentBars[1] - 2)
            {
				if (Lows[1][i] < Lows[1][i+2])
                {
					trueMin = Lows[1][i];
					break;
                }
				i++;
            }
			int j = 1;
			while (j < CurrentBars[1] - 2)
			{
				if (Highs[1][j] > Highs[1][j + 2])
                {
					trueMax = Highs[1][j];
					break;
                }
				j++;
            }
			
			if (trueMax > 0 && trueMin > 0)
            {
				trueRange = trueMax - trueMin;
            }

			return trueRange;
        }

		private double calculateWeight(BarsPeriodType barsPeriodType, int value)
        {
			double weight = 0;
			switch (barsPeriodType)
            {
				case BarsPeriodType.Minute:
					weight = value;
					break;
				case BarsPeriodType.Day:
					weight = value * 1440;
					break;
				case BarsPeriodType.Week:
					weight = value * 1440 * 7;
					break;
				case BarsPeriodType.Month:
					weight = value * 1440 * 30;
					break;
            }
			return weight;
        }

		private void calculateCoordinatesForRendering(ChartControl chartControl, ChartScale chartScale)
		{
            float start = chartControl.GetXByBarIndex(ChartBars, CurrentBars[0]) + 20;
            float end = start + ChartPanel.W / 15;
            string highText = "Upper Range Border " + timeFrameNumber + " " + timeFrameType.ToString();
            string lowText = "Lower Range Border " + timeFrameNumber + " " + timeFrameType.ToString();
            string highLevelText = "Upside Tolerance Level " + timeFrameNumber + " " + timeFrameType.ToString();
            string lowLevelText = "Downside Tolerance Level " + timeFrameNumber + " " + timeFrameType.ToString();
			
			renderLines(chartControl, chartScale, upperRangeValue, lowerRangeValue, start, end, highText, lowText,
				UpperBorderColor, LowerBorderColor, RangeTextLabelColor);
			
			if (ShowToleranceLevels)
				renderLines(chartControl, chartScale, upsideLevel, downsideLevel, start, end, highLevelText, lowLevelText,
					UpsideLevelColor, DownsideLevelColor, ToleranceLevelsTextLabelColor);
		}

		private void renderLines(ChartControl chartControl, ChartScale chartScale,
			double high, double low, float startX, float endX, string highText, string lowText,
            System.Windows.Media.Brush highLineBrushVal, System.Windows.Media.Brush lowLineBrushVal,
			System.Windows.Media.Brush textColor)
		{
			//Lines
			//SharpDX.Direct2D1.Brush highLineBrush = Brushes.Green.ToDxBrush(RenderTarget);
			//SharpDX.Direct2D1.Brush highLineBrush = UpperBorderColor.ToDxBrush(RenderTarget);
			SharpDX.Direct2D1.Brush highLineBrush = highLineBrushVal.ToDxBrush(RenderTarget);
			//SharpDX.Direct2D1.Brush lowLineBrush = Brushes.Red.ToDxBrush(RenderTarget);
			//SharpDX.Direct2D1.Brush lowLineBrush = LowerBorderColor.ToDxBrush(RenderTarget);
			SharpDX.Direct2D1.Brush lowLineBrush = lowLineBrushVal.ToDxBrush(RenderTarget);
			Vector2 highStartPoint = new Vector2(startX, chartScale.GetYByValue(high));
			Vector2 highEndPoint = new Vector2(endX, chartScale.GetYByValue(high));
			Vector2 lowStartPoint = new Vector2(startX, chartScale.GetYByValue(low));
			Vector2 lowEndPoint = new Vector2(endX, chartScale.GetYByValue(low));

			RenderTarget.DrawLine(highStartPoint, highEndPoint, highLineBrush, 3);
			RenderTarget.DrawLine(lowStartPoint, lowEndPoint, lowLineBrush, 3);

			highLineBrush.Dispose();
			lowLineBrush.Dispose();

			//Text
			//SharpDX.Direct2D1.Brush textBrush = System.Windows.Media.Brushes.Orange.ToDxBrush(RenderTarget);
			SharpDX.Direct2D1.Brush textBrush = textColor.ToDxBrush(RenderTarget);
			SimpleFont simpleFont = chartControl.Properties.LabelFont ?? new SimpleFont();
			SharpDX.DirectWrite.TextFormat textFormat = simpleFont.ToDirectWriteTextFormat();
			//Vector2 highTextPoint = new Vector2(startX, chartScale.GetYByValue(high) + 2);
			//Vector2 lowTextPoint = new Vector2(startX, chartScale.GetYByValue(low) - 20);
			//string highText;
			//string lowText;

			//highText = "Upper Range Border " + timeFrameNumber + " " + timeFrameType.ToString();
			//lowText = "Lower Range Border " + timeFrameNumber + " " + timeFrameType.ToString();

			//SharpDX.DirectWrite.TextLayout highTextLayout =
			//	new SharpDX.DirectWrite.TextLayout(NinjaTrader.Core.Globals.DirectWriteFactory, highText, textFormat,
			//	ChartPanel.W, textFormat.FontSize);
			//SharpDX.DirectWrite.TextLayout lowTextLayout =
			//	new SharpDX.DirectWrite.TextLayout(NinjaTrader.Core.Globals.DirectWriteFactory, lowText, textFormat,
			//	ChartPanel.W, textFormat.FontSize);
			//RenderTarget.DrawTextLayout(highTextPoint, highTextLayout, textBrush);
			//RenderTarget.DrawTextLayout(lowTextPoint, lowTextLayout, textBrush);

			//TextPrice
			//Vector2 highTextPricePoint = new Vector2(startX, chartScale.GetYByValue(high) - 20);
			Vector2 highTextPricePoint = new Vector2(endX + 5, chartScale.GetYByValue(high) - (int)simpleFont.Size);
			//Vector2 lowTextPricePoint = new Vector2(startX, chartScale.GetYByValue(low) + 2);
			Vector2 lowTextPricePoint = new Vector2(endX + 5, chartScale.GetYByValue(low) - (int)simpleFont.Size);
			SharpDX.DirectWrite.TextLayout highTextPriceLayout =
				new SharpDX.DirectWrite.TextLayout(NinjaTrader.Core.Globals.DirectWriteFactory,
				Instrument.MasterInstrument.RoundToTickSize(high).ToString() + " - " + highText,
				textFormat, ChartPanel.W, textFormat.FontSize);
			SharpDX.DirectWrite.TextLayout lowTextPriceLayout =
				new SharpDX.DirectWrite.TextLayout(NinjaTrader.Core.Globals.DirectWriteFactory,
				Instrument.MasterInstrument.RoundToTickSize(low).ToString() + " - " + lowText,
				textFormat, ChartPanel.W, textFormat.FontSize);
			RenderTarget.DrawTextLayout(highTextPricePoint, highTextPriceLayout, textBrush);
			RenderTarget.DrawTextLayout(lowTextPricePoint, lowTextPriceLayout, textBrush);

			textBrush.Dispose();
			textFormat.Dispose();
			//highTextLayout.Dispose();
			//lowTextLayout.Dispose();
			highTextPriceLayout.Dispose();
			lowTextPriceLayout.Dispose();
		}

		private double calculateXValue(double closePrev, double highPrev, double lowPrev, double openPrev)
		{
			double xValue;
			if (closePrev > openPrev)
			{
				xValue = (highPrev * 2 + lowPrev + closePrev) / 2;
			}
			else if (closePrev < openPrev)
			{
				xValue = (highPrev + lowPrev * 2 + closePrev) / 2;
			}
			else if (closePrev == openPrev)
			{
				xValue = (highPrev + lowPrev + closePrev * 2) / 2;
			}
			else
				xValue = closePrev;
			return xValue;
		}

		#region Properties
		[NinjaScriptProperty]
		[Display(Name = "Range and Levels Timeframe", Order = 1, GroupName = "Parameters", Description = "Choose a Timeframe for a Range.")]
		public CustomEnumNamespace.RangeTimeframes RangeTimeframe
		{
			get { return rangeTimeframe; }
			set { rangeTimeframe = value; }
		}

		[NinjaScriptProperty]
		[XmlIgnore]
		[Display(Name="Range Projection Upper Border Color", Description="Upper Border Color", Order=2, GroupName="Parameters")]
		public System.Windows.Media.Brush UpperBorderColor
		{ get; set; }

		[Browsable(false)]
		public string UpperBorderColorSerializable
		{
			get { return Serialize.BrushToString(UpperBorderColor); }
			set { UpperBorderColor = Serialize.StringToBrush(value); }
		}			

		[NinjaScriptProperty]
		[XmlIgnore]
		[Display(Name="Range Projection Lower Border Color", Description="Lower Border Color", Order=3, GroupName="Parameters")]
		public System.Windows.Media.Brush LowerBorderColor
		{ get; set; }

		[Browsable(false)]
		public string LowerBorderColorSerializable
		{
			get { return Serialize.BrushToString(LowerBorderColor); }
			set { LowerBorderColor = Serialize.StringToBrush(value); }
		}			

		[NinjaScriptProperty]
		[XmlIgnore]
		[Display(Name="Range Projection Area Color", Description="Range Area Color", Order=4, GroupName="Parameters")]
		public System.Windows.Media.Brush RangeAreaColor
		{ get; set; }

		[Browsable(false)]
		public string RangeAreaColorSerializable
		{
			get { return Serialize.BrushToString(RangeAreaColor); }
			set { RangeAreaColor = Serialize.StringToBrush(value); }
		}			

		[NinjaScriptProperty]
		[Range(0, int.MaxValue)]
		[Display(Name="Range Projection Area Transparency", Description="Range Area Transparency", Order=5, GroupName="Parameters")]
		public int RangeAreaTransparency
		{ get; set; }

		[NinjaScriptProperty]
		[XmlIgnore]
		[Display(Name = "Range Projection Text Label Color", Description = "Range Area Text Label Color", Order = 6, GroupName = "Parameters")]
		public System.Windows.Media.Brush RangeTextLabelColor
		{ get; set; }

		[Browsable(false)]
		public string RangeTextLabelColorSerializable
		{
			get { return Serialize.BrushToString(RangeTextLabelColor); }
			set { RangeTextLabelColor = Serialize.StringToBrush(value); }
		}

		[NinjaScriptProperty]
		[Display(Name = "Show Tolerance Levels", Order = 7, GroupName = "Parameters")]
		public bool ShowToleranceLevels
		{ get; set; }

		[NinjaScriptProperty]
		[XmlIgnore]
		[Display(Name = "Upside Tolerance Level Color", Description = "Upside Tolerance Level Color", Order = 8, GroupName = "Parameters")]
		public System.Windows.Media.Brush UpsideLevelColor
		{ get; set; }

		[Browsable(false)]
		public string UpsideLevelColorSerializable
		{
			get { return Serialize.BrushToString(UpsideLevelColor); }
			set { UpsideLevelColor = Serialize.StringToBrush(value); }
		}

		[NinjaScriptProperty]
		[XmlIgnore]
		[Display(Name = "Downside Tolerance Level Color", Description = "Downside Tolerance Level Color", Order = 9, GroupName = "Parameters")]
		public System.Windows.Media.Brush DownsideLevelColor
		{ get; set; }

		[Browsable(false)]
		public string DownsideLevelColorSerializable
		{
			get { return Serialize.BrushToString(DownsideLevelColor); }
			set { DownsideLevelColor = Serialize.StringToBrush(value); }
		}

		[NinjaScriptProperty]
		[XmlIgnore]
		[Display(Name = "Tolerance Levels Projection Area Color", Description = "Tolerance Levels Area Color", Order = 10, GroupName = "Parameters")]
		public System.Windows.Media.Brush ToleranceLevelsAreaColor
		{ get; set; }

		[Browsable(false)]
		public string ToleranceLevelsAreaColorSerializable
		{
			get { return Serialize.BrushToString(ToleranceLevelsAreaColor); }
			set { ToleranceLevelsAreaColor = Serialize.StringToBrush(value); }
		}

		[NinjaScriptProperty]
		[Range(0, int.MaxValue)]
		[Display(Name = "Tolerance Levels Area Transparency", Description = "Tolerance Levels Area Transparency", Order = 11, GroupName = "Parameters")]
		public int ToleranceLevelsAreaTransparency
		{ get; set; }

		[NinjaScriptProperty]
		[XmlIgnore]
		[Display(Name = "Tolerance Levels Text Label Color", Description = "Tolerance Levels Text Label Color", Order = 10, GroupName = "Parameters")]
		public System.Windows.Media.Brush ToleranceLevelsTextLabelColor
		{ get; set; }

		[Browsable(false)]
		public string ToleranceLevelsTextLabelColorSerializable
		{
			get { return Serialize.BrushToString(ToleranceLevelsTextLabelColor); }
			set { ToleranceLevelsTextLabelColor = Serialize.StringToBrush(value); }
		}
		#endregion

	}
}

namespace CustomEnumNamespace
{
	public enum RangeTimeframes
	{
		//Minutes15 = 15,
		//Minutes30 = 30,
		Minutes60 = 60,
		Minutes240 = 240,
		Day = 1440,
		Week,
		Month
	}
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private CI_TD_Range_Projection[] cacheCI_TD_Range_Projection;
		public CI_TD_Range_Projection CI_TD_Range_Projection(CustomEnumNamespace.RangeTimeframes rangeTimeframe, System.Windows.Media.Brush upperBorderColor, System.Windows.Media.Brush lowerBorderColor, System.Windows.Media.Brush rangeAreaColor, int rangeAreaTransparency, System.Windows.Media.Brush rangeTextLabelColor, bool showToleranceLevels, System.Windows.Media.Brush upsideLevelColor, System.Windows.Media.Brush downsideLevelColor, System.Windows.Media.Brush toleranceLevelsAreaColor, int toleranceLevelsAreaTransparency, System.Windows.Media.Brush toleranceLevelsTextLabelColor)
		{
			return CI_TD_Range_Projection(Input, rangeTimeframe, upperBorderColor, lowerBorderColor, rangeAreaColor, rangeAreaTransparency, rangeTextLabelColor, showToleranceLevels, upsideLevelColor, downsideLevelColor, toleranceLevelsAreaColor, toleranceLevelsAreaTransparency, toleranceLevelsTextLabelColor);
		}

		public CI_TD_Range_Projection CI_TD_Range_Projection(ISeries<double> input, CustomEnumNamespace.RangeTimeframes rangeTimeframe, System.Windows.Media.Brush upperBorderColor, System.Windows.Media.Brush lowerBorderColor, System.Windows.Media.Brush rangeAreaColor, int rangeAreaTransparency, System.Windows.Media.Brush rangeTextLabelColor, bool showToleranceLevels, System.Windows.Media.Brush upsideLevelColor, System.Windows.Media.Brush downsideLevelColor, System.Windows.Media.Brush toleranceLevelsAreaColor, int toleranceLevelsAreaTransparency, System.Windows.Media.Brush toleranceLevelsTextLabelColor)
		{
			if (cacheCI_TD_Range_Projection != null)
				for (int idx = 0; idx < cacheCI_TD_Range_Projection.Length; idx++)
					if (cacheCI_TD_Range_Projection[idx] != null && cacheCI_TD_Range_Projection[idx].RangeTimeframe == rangeTimeframe && cacheCI_TD_Range_Projection[idx].UpperBorderColor == upperBorderColor && cacheCI_TD_Range_Projection[idx].LowerBorderColor == lowerBorderColor && cacheCI_TD_Range_Projection[idx].RangeAreaColor == rangeAreaColor && cacheCI_TD_Range_Projection[idx].RangeAreaTransparency == rangeAreaTransparency && cacheCI_TD_Range_Projection[idx].RangeTextLabelColor == rangeTextLabelColor && cacheCI_TD_Range_Projection[idx].ShowToleranceLevels == showToleranceLevels && cacheCI_TD_Range_Projection[idx].UpsideLevelColor == upsideLevelColor && cacheCI_TD_Range_Projection[idx].DownsideLevelColor == downsideLevelColor && cacheCI_TD_Range_Projection[idx].ToleranceLevelsAreaColor == toleranceLevelsAreaColor && cacheCI_TD_Range_Projection[idx].ToleranceLevelsAreaTransparency == toleranceLevelsAreaTransparency && cacheCI_TD_Range_Projection[idx].ToleranceLevelsTextLabelColor == toleranceLevelsTextLabelColor && cacheCI_TD_Range_Projection[idx].EqualsInput(input))
						return cacheCI_TD_Range_Projection[idx];
			return CacheIndicator<CI_TD_Range_Projection>(new CI_TD_Range_Projection(){ RangeTimeframe = rangeTimeframe, UpperBorderColor = upperBorderColor, LowerBorderColor = lowerBorderColor, RangeAreaColor = rangeAreaColor, RangeAreaTransparency = rangeAreaTransparency, RangeTextLabelColor = rangeTextLabelColor, ShowToleranceLevels = showToleranceLevels, UpsideLevelColor = upsideLevelColor, DownsideLevelColor = downsideLevelColor, ToleranceLevelsAreaColor = toleranceLevelsAreaColor, ToleranceLevelsAreaTransparency = toleranceLevelsAreaTransparency, ToleranceLevelsTextLabelColor = toleranceLevelsTextLabelColor }, input, ref cacheCI_TD_Range_Projection);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.CI_TD_Range_Projection CI_TD_Range_Projection(CustomEnumNamespace.RangeTimeframes rangeTimeframe, System.Windows.Media.Brush upperBorderColor, System.Windows.Media.Brush lowerBorderColor, System.Windows.Media.Brush rangeAreaColor, int rangeAreaTransparency, System.Windows.Media.Brush rangeTextLabelColor, bool showToleranceLevels, System.Windows.Media.Brush upsideLevelColor, System.Windows.Media.Brush downsideLevelColor, System.Windows.Media.Brush toleranceLevelsAreaColor, int toleranceLevelsAreaTransparency, System.Windows.Media.Brush toleranceLevelsTextLabelColor)
		{
			return indicator.CI_TD_Range_Projection(Input, rangeTimeframe, upperBorderColor, lowerBorderColor, rangeAreaColor, rangeAreaTransparency, rangeTextLabelColor, showToleranceLevels, upsideLevelColor, downsideLevelColor, toleranceLevelsAreaColor, toleranceLevelsAreaTransparency, toleranceLevelsTextLabelColor);
		}

		public Indicators.CI_TD_Range_Projection CI_TD_Range_Projection(ISeries<double> input , CustomEnumNamespace.RangeTimeframes rangeTimeframe, System.Windows.Media.Brush upperBorderColor, System.Windows.Media.Brush lowerBorderColor, System.Windows.Media.Brush rangeAreaColor, int rangeAreaTransparency, System.Windows.Media.Brush rangeTextLabelColor, bool showToleranceLevels, System.Windows.Media.Brush upsideLevelColor, System.Windows.Media.Brush downsideLevelColor, System.Windows.Media.Brush toleranceLevelsAreaColor, int toleranceLevelsAreaTransparency, System.Windows.Media.Brush toleranceLevelsTextLabelColor)
		{
			return indicator.CI_TD_Range_Projection(input, rangeTimeframe, upperBorderColor, lowerBorderColor, rangeAreaColor, rangeAreaTransparency, rangeTextLabelColor, showToleranceLevels, upsideLevelColor, downsideLevelColor, toleranceLevelsAreaColor, toleranceLevelsAreaTransparency, toleranceLevelsTextLabelColor);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.CI_TD_Range_Projection CI_TD_Range_Projection(CustomEnumNamespace.RangeTimeframes rangeTimeframe, System.Windows.Media.Brush upperBorderColor, System.Windows.Media.Brush lowerBorderColor, System.Windows.Media.Brush rangeAreaColor, int rangeAreaTransparency, System.Windows.Media.Brush rangeTextLabelColor, bool showToleranceLevels, System.Windows.Media.Brush upsideLevelColor, System.Windows.Media.Brush downsideLevelColor, System.Windows.Media.Brush toleranceLevelsAreaColor, int toleranceLevelsAreaTransparency, System.Windows.Media.Brush toleranceLevelsTextLabelColor)
		{
			return indicator.CI_TD_Range_Projection(Input, rangeTimeframe, upperBorderColor, lowerBorderColor, rangeAreaColor, rangeAreaTransparency, rangeTextLabelColor, showToleranceLevels, upsideLevelColor, downsideLevelColor, toleranceLevelsAreaColor, toleranceLevelsAreaTransparency, toleranceLevelsTextLabelColor);
		}

		public Indicators.CI_TD_Range_Projection CI_TD_Range_Projection(ISeries<double> input , CustomEnumNamespace.RangeTimeframes rangeTimeframe, System.Windows.Media.Brush upperBorderColor, System.Windows.Media.Brush lowerBorderColor, System.Windows.Media.Brush rangeAreaColor, int rangeAreaTransparency, System.Windows.Media.Brush rangeTextLabelColor, bool showToleranceLevels, System.Windows.Media.Brush upsideLevelColor, System.Windows.Media.Brush downsideLevelColor, System.Windows.Media.Brush toleranceLevelsAreaColor, int toleranceLevelsAreaTransparency, System.Windows.Media.Brush toleranceLevelsTextLabelColor)
		{
			return indicator.CI_TD_Range_Projection(input, rangeTimeframe, upperBorderColor, lowerBorderColor, rangeAreaColor, rangeAreaTransparency, rangeTextLabelColor, showToleranceLevels, upsideLevelColor, downsideLevelColor, toleranceLevelsAreaColor, toleranceLevelsAreaTransparency, toleranceLevelsTextLabelColor);
		}
	}
}

#endregion
