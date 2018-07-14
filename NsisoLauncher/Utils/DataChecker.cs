using System.Text.RegularExpressions;

namespace NsisoLauncher.Utils
{
    public static class DataChecker
    {
        //验证小数格式
        public static bool IsDecimal(string str_decimal)
        {
            return Regex.IsMatch(str_decimal, @"^[0-9]+\.[0-9]{2}$");
        }

        //验证是否为数字
        public static bool IsNumber(string str_number)
        {
            return Regex.IsMatch(str_number, @"^[0-9]*$");
        }

        //验证正整数
        public static bool IsIntNumber(string str_intNumber)
        {
            return Regex.IsMatch(str_intNumber, @"^\+?[1-9][0-9]*$");
        }

        //验证大小写
        public static bool IsUpChar(string str_UpChar)
        {
            return Regex.IsMatch(str_UpChar, @"^[A-Z]+$");
        }
        public static bool IsLowerChar(string str_UpChar)
        {
            return Regex.IsMatch(str_UpChar, @"^[a-z]+$");
        }

        //验证是否为字母
        public static bool IsLetter(string str_Letter)
        {
            return Regex.IsMatch(str_Letter, @"^[A-Za-z]+$");
        }

        //验证是否为中文
        public static bool IsChinese(string str_chinese)
        {
            return Regex.IsMatch(str_chinese, @"^[\u4e00-\u9fa5]{1,}$");
        }

        //验证邮箱
        public static bool IsEmail(string str_Email)
        {
            return Regex.IsMatch(str_Email,
            @"^(([\w\.]+)@(([[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}))|((\w+\.?)+)@([a-zA-Z]{2,4}|[0-9]{1,3})(\.[a-zA-Z]{2,4}))$");
        }
    }
}
