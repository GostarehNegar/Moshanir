using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace GN.Library.Helpers
{
    /// <summary>
    /// An interface to work with 'Mobile Phone Numbers'.
    /// Use Parse method to parse a mobile phone number.
    /// </summary>
    public interface IMobilePhoneHelper
    {
        bool IsMobile { get; }
        /// <summary>
        /// Returns true if it's a valid number.
        /// </summary>
        bool IsValid { get; }
        /// <summary>
        /// Parses a mobile phone and returns an instance of helper.
        /// Examples:
        /// +989121877626 => +989121877626
        /// 989121877626 => +989121877626
        /// 00989121877626 => +989121877626
        /// 09121877626 => +989121877626
        /// 9121877626 =>+989121877626
        /// Use the 'IsValid' property of the returned object to check
        /// if its valid obile phone.
        /// </summary>
        /// <param name="phone"></param>
        /// <returns></returns>
        IMobilePhoneHelper Parse(string phone);
        /// <summary>
        /// Returns the local format. 
        /// Given +989121877626, it retuns '09121877626'.
        /// </summary>
        string AsLocalPhone { get; }
        /// <summary>
        /// Retuns the phone number in international format.
        /// For instance +989121877626.
        /// </summary>
        string AsIntlPhone { get; }
        /// <summary>
        /// Retuns Telegram friemdly format. That is 
        /// the international format without the leading '+' sing.
        /// </summary>
        string AsTelegramFriendly { get; }
        /// <summary>
        /// The mobile county code. 
        /// Given +989121877626, it retuns '98'.
        /// </summary>
        string CountryCode { get; }
        /// <summary>
        /// Returns the 'Operator Code'.
        /// For example given '+989121877626' it returns '0912'.
        /// </summary>
        string OperatorCode { get; }

        bool Equals(string other);
        bool Equals(IMobilePhoneHelper other);
    }

    public class MobilePhoneHelper : IMobilePhoneHelper
    {
        private string _phone;
        public bool IsMobile => IsValid && _phone.StartsWith("+989");
        private static bool IsDigitOnly(string numberString)
        {
            return Regex.IsMatch(numberString, @"^[0-9]*$");
        }
        private static bool isValidLocalPhone(string phone)
        {
            return !string.IsNullOrWhiteSpace(phone) && phone.StartsWith("0") && phone.Length > 10 && IsDigitOnly(phone);
        }
        private static bool IsValidOperatorCode(string code)
        {
            return !string.IsNullOrWhiteSpace(code) && code.Length > 3 && code.StartsWith("0") &&
                code[1] != '0';
        }
        private static bool IsValidCountryCode(string code)
        {
            return !string.IsNullOrWhiteSpace(code) && code.Length == 2;
        }
        public IMobilePhoneHelper Parse(string phone)
        {
            var country_code = "";
            var area_code = "";
            _phone = phone ?? "";
            if (!string.IsNullOrWhiteSpace(phone))
            {
                phone = phone.Trim().Replace(" ", "");
                //if (!phone.StartsWith("+") && !phone.StartsWith("00") && !phone.StartsWith("0") && phone.Length > 10)
                //{
                //    phone = "+" + phone;
                //}
                if (phone.StartsWith("+"))
                {
                    if (phone.Length > 4)
                    {
                        country_code = country_code = phone.Substring(1, 2);
                        phone = "0" + phone.Substring(3);
                    }
                    else
                    {
                        country_code = "INVALID";
                    }
                }
                if (phone.StartsWith("00"))
                {
                    if (phone.Length > 4)
                    {
                        country_code = country_code = phone.Substring(2, 2);
                        phone = "0" + phone.Substring(4);
                    }
                    else
                    {
                        country_code = "INVALID";
                    }
                }
                if (phone.StartsWith("98"))
                {
                    phone = phone.Substring(2, phone.Length - 2);
                    country_code = "98";
                }
                if (phone.StartsWith("09"))
                {
                    phone = phone.Substring(1, phone.Length - 1);
                }
                else if (phone.StartsWith("0") && phone.Length > 3)
                {
                    area_code = phone.Substring(1, 2);
                    phone = phone.Substring(3);
                }
                else
                {
                    area_code = "21";

                }


                country_code = string.IsNullOrWhiteSpace(country_code) ? "98" : country_code;
                _phone = string.Format("+{0}{1}{2}", country_code, area_code, phone);
            }


            return this;
        }

        public bool Equals(string other)
        {
            return Equals(new MobilePhoneHelper().Parse(other));
        }
        public bool Equals(IMobilePhoneHelper other)
        {
            return other != null && other.IsValid && other.AsIntlPhone == this.AsIntlPhone;
        }
        public string Local
        {
            get
            {
                var result = _phone.Length < 3 ? "" : "0" + _phone.Substring(3, _phone.Length - 3);
                return isValidLocalPhone(result) ? result : null;
            }
        }
        public string OperatorCode
        {
            get
            {
                var local = Local;
                var result = local.Length > 4 ? local.Substring(0, 4) : "";
                return IsValidOperatorCode(result) ? result : null;
            }
        }
        public bool IsValid
        {
            get
            {
                return isValidLocalPhone(Local) && IsValidOperatorCode(OperatorCode) && IsValidCountryCode(CountryCode);
            }
        }
        public string AsLocalPhone
        {
            get
            {
                return Local;
            }
        }
        public string AsIntlPhone
        {
            get { return _phone; }
        }
        public string AsTelegramFriendly
        {
            get
            {
                return !string.IsNullOrWhiteSpace(_phone) && _phone.Length > 1
                    ? _phone.Substring(1, _phone.Length - 1)
                    : null;
            }
        }
        public string CountryCode
        {
            get
            {
                var result = !string.IsNullOrWhiteSpace(_phone) && _phone.Length > 3
                    ? _phone.Substring(1, 2)
                    : null;
                return IsValidCountryCode(result) ? result : null;
            }
        }

        public static IMobilePhoneHelper Create(string phhone)
        {
            return new MobilePhoneHelper().Parse(phhone);
        }
    }
}
