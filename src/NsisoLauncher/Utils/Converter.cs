using NsisoLauncher.Config;
using NsisoLauncherCore.Modules;
using NsisoLauncherCore.Net;
using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace NsisoLauncher.Utils
{
    public class BoolToOppositeBoolConverter : IValueConverter
    {
        #region IValueConverter Members
        public object Convert(object value, Type targetType, object parameter,
            CultureInfo culture)
        {
            if (targetType != typeof(bool))
                throw new InvalidOperationException("The target must be a boolean");
            return !(bool)value;

        }

        public object ConvertBack(object value, Type targetType, object parameter,
            CultureInfo culture)
        {
            throw new NotSupportedException();
        }
        #endregion
    }

    //public class SettingDirRadioButtonConverter : IValueConverter
    //{
    //    #region IValueConverter Members
    //    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    //    {
    //        GameDirEnum s = (GameDirEnum)value;
    //        return s == (GameDirEnum)int.Parse(parameter.ToString());
    //    }

    //    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    //    {
    //        bool isChecked = (bool)value;
    //        if (!isChecked)
    //        {
    //            return null;
    //        }
    //        return (GameDirEnum)int.Parse(parameter.ToString());
    //    }
    //    #endregion
    //}

    //public class SettingCGRadioButtonConverter : IValueConverter
    //{
    //    #region IValueConverter Members
    //    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    //    {
    //        GCType s = (GCType)value;
    //        return s == (GCType)int.Parse(parameter.ToString());
    //    }

    //    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    //    {
    //        bool isChecked = (bool)value;
    //        if (!isChecked)
    //        {
    //            return null;
    //        }
    //        return (GCType)int.Parse(parameter.ToString());
    //    }
    //    #endregion
    //}

    //public class SettingDownloadRadioButtonConverter : IValueConverter
    //{
    //    #region IValueConverter Members
    //    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    //    {
    //        DownloadSource s = (DownloadSource)value;
    //        return s == (DownloadSource)int.Parse(parameter.ToString());
    //    }

    //    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    //    {
    //        bool isChecked = (bool)value;
    //        if (!isChecked)
    //        {
    //            return null;
    //        }
    //        return (DownloadSource)int.Parse(parameter.ToString());
    //    }
    //    #endregion
    //}

    //public class SettingLoginTypeRadioButtonConverter : IValueConverter
    //{
    //    #region IValueConverter Members
    //    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    //    {
    //        AuthenticationType s = (AuthenticationType)value;
    //        return s == (AuthenticationType)int.Parse(parameter.ToString());
    //    }

    //    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    //    {
    //        bool isChecked = (bool)value;
    //        if (!isChecked)
    //        {
    //            return null;
    //        }
    //        return (AuthenticationType)int.Parse(parameter.ToString());
    //    }
    //    #endregion
    //}

    public class StringToIntConverter : IValueConverter
    {
        #region IValueConverter Members
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string str = (string)value;
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
        #endregion
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
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return value?.Equals(parameter);
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return value?.Equals(true) == true ? parameter : Binding.DoNothing;
        }
    }
}
