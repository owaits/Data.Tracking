using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Reflection;
using System.Threading.Tasks;

namespace Oarw.Data.Tracking.Blazor.Input
{
    public partial class InputTimeSpan : InputBase<TimeSpan?>
    {
        enum TimeResolution
        {
            [Display(Name = "Minute(s)")]
            Minute,
            [Display(Name = "Hour(s)")]
            Hour,
            [Display(Name = "Day(s)")]
            Day,
            [Display(Name = "Week(s)")]
            Week,
            [Display(Name = "Year(s)")]
            Year
        }

        [Parameter]
        public bool Small { get; set; }

        [Parameter]
        public bool ShowOrder { get; set; } = true;

        [Parameter]
        public string Placeholder { get; set; }

        [Parameter]
        public RenderFragment Append { get; set; }

        private double? Offset
        {
            get
            {
                if (CurrentValue == null)
                    return null;

                switch(Resolution)
                {
                    case TimeResolution.Minute:
                        return Math.Abs(((TimeSpan)CurrentValue).TotalMinutes);
                    case TimeResolution.Hour:
                        return Math.Abs(((TimeSpan)CurrentValue).TotalHours);
                    case TimeResolution.Day:
                        return Math.Abs(((TimeSpan)CurrentValue).TotalDays);
                    case TimeResolution.Week:
                        return Math.Abs(((TimeSpan)CurrentValue).TotalDays / 7);
                    case TimeResolution.Year:
                        return Math.Abs(((TimeSpan)CurrentValue).TotalDays / 365);
                }

                return null;
            }
            set
            {
                switch(Resolution)
                {
                    case TimeResolution.Minute:
                        CurrentValue = (value == null ? null : TimeSpan.FromMinutes((double)value));
                        break;
                    case TimeResolution.Hour:
                        CurrentValue = (value == null? null : TimeSpan.FromHours((double) value));
                        break;
                    case TimeResolution.Day:
                        CurrentValue = (value == null ? null : TimeSpan.FromDays((double)value));
                        break;
                    case TimeResolution.Week:
                        CurrentValue = (value == null ? null : TimeSpan.FromDays((double)value * 7));
                        break;

                    case TimeResolution.Year:
                        CurrentValue = (value == null ? null : TimeSpan.FromDays((double)value * 365));
                        break;
                }
                
                ValueChanged.InvokeAsync(CurrentValue);
            }
        }

        private TimeResolution Resolution { get; set; }

        private bool Before
        {
            get { return CurrentValue < TimeSpan.Zero; }
            set
            {
                if(Before != value)
                {
                    CurrentValue = -CurrentValue;
                }                
            }
        }

        protected override async Task OnInitializedAsync()
        {
            if (CurrentValue != null)
            {
                if (((TimeSpan)CurrentValue).TotalDays % 365 == 0)
                    Resolution = TimeResolution.Year;
                else if (((TimeSpan)CurrentValue).TotalDays % 1 == 0)
                    Resolution = TimeResolution.Day;
                else if (((TimeSpan)CurrentValue).TotalHours % 1 == 0)
                    Resolution = TimeResolution.Hour;
                else if (((TimeSpan)CurrentValue).TotalMinutes % 1 == 0)
                    Resolution = TimeResolution.Minute;

                StateHasChanged();
            }

            await Task.CompletedTask;
        }

        protected override bool TryParseValueFromString(string value, out TimeSpan? result, [NotNullWhen(false)] out string validationErrorMessage)
        {
            result = TimeSpan.Parse(value);
            validationErrorMessage = null;
            return true;
        }
    }
}
