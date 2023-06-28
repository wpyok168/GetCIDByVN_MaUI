﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GetCIDByVN
{
    public class MultiParamterConverter : IMultiValueConverter
    {
       // public static object ConverterObject;
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            //必须新new一个，否则拿不到数据，因为values在返回之后，就会被清空了
            //return values.Clone();
            return values.ToArray();    

            //Tuple<object, object> tuple = new Tuple<object, object>(values[0], values[1]);
            //return tuple;

            //ConverterObject = values;
            //string str = values.GetType().ToString();
            //return values.ToArray();
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}