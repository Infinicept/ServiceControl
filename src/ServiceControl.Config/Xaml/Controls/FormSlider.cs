﻿namespace ServiceControl.Config.Xaml.Controls
{
    using System;
    using System.ComponentModel;
    using System.Text;
    using System.Windows;
    using System.Windows.Controls;

    [DefaultEvent("ValueChanged")]
    [DefaultProperty("Value")]
    [TemplatePart(Name = SliderPartName, Type = typeof(Slider))]
    public class FormSlider : Slider
    {
        static FormSlider()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(FormSlider), new FrameworkPropertyMetadata(typeof(FormSlider)));
        }

        public string Header
        {
            get { return (string)GetValue(HeaderProperty); }
            set { SetValue(HeaderProperty, value); }
        }

        public string Summary
        {
            get { return (string)GetValue(SummaryProperty); }
            set { SetValue(SummaryProperty, value); }
        }

        public string Explanation
        {
            get { return (string)GetValue(ExplanationProperty); }
            set { SetValue(ExplanationProperty, value); }
        }

        public TimeSpanUnits Units
        {
            get { return (TimeSpanUnits)GetValue(UnitsProperty); }
            set { SetValue(UnitsProperty, value); }
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            if (GetTemplateChild(SliderPartName) is Slider slider)
            {
                if (GetTemplateChild("PART_SliderDown") is Button downButton)
                {
                    downButton.Click += (sender, args) =>
                    {
                        var newValue = slider.Value - slider.SmallChange;
                        slider.Value = newValue < slider.Minimum ? slider.Minimum : newValue;
                    };
                }

                if (GetTemplateChild("PART_SliderUp") is Button upButton)
                {
                    upButton.Click += (sender, args) =>
                    {
                        var newValue = slider.Value + slider.SmallChange;
                        slider.Value = newValue > slider.Maximum ? slider.Maximum : newValue;
                    };
                }
            }
        }

        protected override void OnValueChanged(double oldValue, double newValue)
        {
            base.OnValueChanged(oldValue, newValue);

            var period = Units == TimeSpanUnits.Days
                ? TimeSpan.FromDays(Math.Truncate(Value))
                : TimeSpan.FromHours(Math.Truncate(Value));

            UpdateSummary(period);
        }

        void UpdateSummary(TimeSpan period)
        {
            var s = new StringBuilder();
            if (period.TotalHours < 24)
            {
                s.AppendFormat("{0} Hours", period.Hours);
            }
            else
            {
                s.AppendFormat("{0} Day{1}", period.Days, period.Days > 1 ? "s" : string.Empty);
                if (period.Hours != 0)
                {
                    s.AppendFormat(" {0} Hour{1}", period.Hours, period.Hours > 1 ? "s" : string.Empty);
                }
            }

            Summary = s.ToString();
        }

        const string SliderPartName = "PART_Slider";

        public static readonly DependencyProperty ExplanationProperty =
            DependencyProperty.Register("Explanation", typeof(string), typeof(FormSlider), new PropertyMetadata(string.Empty));

        public static readonly DependencyProperty SummaryProperty =
            DependencyProperty.Register("Summary", typeof(string), typeof(FormSlider), new PropertyMetadata(string.Empty));

        public static readonly DependencyProperty HeaderProperty =
            DependencyProperty.Register("Header", typeof(string), typeof(FormSlider), new PropertyMetadata(string.Empty));

        public static readonly DependencyProperty UnitsProperty =
            DependencyProperty.Register("Units", typeof(TimeSpanUnits), typeof(FormSlider));
    }

    public enum TimeSpanUnits
    {
        Hours,
        Days
    }
}