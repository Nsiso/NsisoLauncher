using NsisoLauncherCore.Modules;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace NsisoLauncher.Utils
{
    public class BooleanConverter<T> : IValueConverter
    {
        public BooleanConverter(T trueValue, T falseValue)
        {
            True = trueValue;
            False = falseValue;
        }

        public T True { get; set; }
        public T False { get; set; }

        public virtual object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value is bool && ((bool)value) ? True : False;
        }

        public virtual object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value is T && EqualityComparer<T>.Default.Equals((T)value, True);
        }
    }

    public sealed class BooleanToVisibilityConverter : BooleanConverter<Visibility>
    {
        public BooleanToVisibilityConverter() :
            base(Visibility.Visible, Visibility.Collapsed)
        { }
    }

    [ValueConversion(typeof(bool), typeof(bool))]
    public class BoolToOppositeBoolConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter,
           CultureInfo culture)
        {
            if (targetType != typeof(bool))
            {
                throw new InvalidOperationException("The target must be a boolean");
            }

            return !(bool)value;

        }

        public object ConvertBack(object value, Type targetType, object parameter,
            CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }

    [ValueConversion(typeof(bool), typeof(Visibility))]
    public class OppositeBooleanToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool isVisible = (bool)value;
            return new System.Windows.Controls.BooleanToVisibilityConverter().Convert(!isVisible, targetType, parameter, culture);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return !(bool)new System.Windows.Controls.BooleanToVisibilityConverter().ConvertBack(value, targetType, parameter, culture);
        }
    }

    [ValueConversion(typeof(string), typeof(int))]
    public class StringToIntConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string str = value.ToString();
            if (string.IsNullOrEmpty(str))
            {
                return 0;
            }
            else
            {
                return int.Parse(str);
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value.ToString();
        }
    }

    [ValueConversion(typeof(string), typeof(Visibility))]
    public class StringNotEmptyToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string str = (string)value;
            if (string.IsNullOrWhiteSpace(str))
            {
                return Visibility.Collapsed;
            }
            else
            {
                return Visibility.Visible;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }

    public class ObjectNotEmptyToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
            {
                return Visibility.Collapsed;
            }
            else
            {
                return Visibility.Visible;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }

    public class ObjectNotEmptyToBooleanConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }

    public class VisualBrushTargetConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            var parentControl = values[0] as FrameworkElement;
            var targetControl = values[1] as FrameworkElement;

            var transformedPos = targetControl.TransformToVisual(parentControl).Transform(new Point());
            var transformedSize = targetControl.TransformToVisual(parentControl).Transform(new Point(targetControl.RenderSize.Width, targetControl.RenderSize.Height));

            transformedSize = new Point(transformedSize.X - transformedPos.X, transformedSize.Y - transformedPos.Y);
            return new Rect(transformedPos.X,
                            transformedPos.Y,
                            transformedSize.X,
                            transformedSize.Y);
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            return null;
        }
    }

    public class ComparisonConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value?.Equals(parameter);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value?.Equals(true) == true ? parameter : Binding.DoNothing;
        }
    }

    [ValueConversion(typeof(NsisoLauncherCore.Net.Server.ServerInfo.StateType), typeof(SolidColorBrush))]
    public class ServerStateToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            NsisoLauncherCore.Net.Server.ServerInfo.StateType type = (NsisoLauncherCore.Net.Server.ServerInfo.StateType)value;
            switch (type)
            {
                case NsisoLauncherCore.Net.Server.ServerInfo.StateType.GOOD:
                    return new SolidColorBrush(Colors.Green);
                default:
                    return new SolidColorBrush(Colors.Red);
            }
        }

        object IValueConverter.ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    [ValueConversion(typeof(NsisoLauncherCore.Net.Server.ServerInfo.StateType), typeof(MahApps.Metro.IconPacks.PackIconFontAwesomeKind))]
    public class ServerStateToIconTypeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            NsisoLauncherCore.Net.Server.ServerInfo.StateType type = (NsisoLauncherCore.Net.Server.ServerInfo.StateType)value;
            switch (type)
            {
                case NsisoLauncherCore.Net.Server.ServerInfo.StateType.GOOD:
                    return MahApps.Metro.IconPacks.PackIconFontAwesomeKind.CircleSolid;
                case NsisoLauncherCore.Net.Server.ServerInfo.StateType.NO_RESPONSE:
                    return MahApps.Metro.IconPacks.PackIconFontAwesomeKind.ExclamationCircleSolid;
                case NsisoLauncherCore.Net.Server.ServerInfo.StateType.BAD_CONNECT:
                    return MahApps.Metro.IconPacks.PackIconFontAwesomeKind.ExclamationCircleSolid;
                case NsisoLauncherCore.Net.Server.ServerInfo.StateType.EXCEPTION:
                    return MahApps.Metro.IconPacks.PackIconFontAwesomeKind.ExclamationCircleSolid;
                default:
                    return MahApps.Metro.IconPacks.PackIconFontAwesomeKind.ExclamationCircleSolid;
            }
        }

        object IValueConverter.ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    [ValueConversion(typeof(NsisoLauncherCore.Net.Server.ServerInfo.StateType), typeof(Visibility))]
    public class ServerStateToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            NsisoLauncherCore.Net.Server.ServerInfo.StateType type = (NsisoLauncherCore.Net.Server.ServerInfo.StateType)value;
            string mode = (string)parameter;
            switch (type)
            {
                case NsisoLauncherCore.Net.Server.ServerInfo.StateType.GOOD:
                    if (mode == "reverse")
                    {
                        return Visibility.Collapsed;
                    }
                    else
                    {
                        return Visibility.Visible;
                    }
                default:
                    if (mode == "reverse")
                    {
                        return Visibility.Visible;
                    }
                    else
                    {
                        return Visibility.Collapsed;
                    }
            }
        }

        object IValueConverter.ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    [ValueConversion(typeof(AuthenticationType), typeof(Visibility))]
    public class UsernamePasswordVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            AuthenticationType type = (AuthenticationType)value;
            string mode = (string)parameter;
            if (mode == "username")
            {
                switch (type)
                {
                    case AuthenticationType.MICROSOFT:
                        return Visibility.Collapsed;

                    default:
                        return Visibility.Visible;
                }
            }
            else
            {
                switch (type)
                {
                    case AuthenticationType.OFFLINE:
                        return Visibility.Collapsed;

                    case AuthenticationType.MICROSOFT:
                        return Visibility.Collapsed;

                    default:
                        return Visibility.Visible;
                }
            }
        }

        object IValueConverter.ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
