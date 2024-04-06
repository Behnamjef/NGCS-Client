using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace NetworkAdapter.Services
{
    public static class ApplicationConsts
    {
        public static string GameKey = "HokmGG";
        public static int NormalMatchWinScore = 3;
        public static int NormalMatchDrawScore = 1;
        public static string DefaultLeadboardName = "default";
        public static string langPersian = "persian";
        public static string langEnglish = "english";


        private static Regex emailReg = new Regex(@"^[a-zA-Z0-9_.+-]+@[a-zA-Z0-9-]+\.[a-zA-Z0-9-.]+$");
        private static Regex phoneRegex = new(@"^(?:0|98|\+98|\+980|0098|098|00980)?(9\d{9})$");

        public static bool IsEmailValid(string email)
        {
            return emailReg.IsMatch(email);
        }
        public static bool IsPhoneNumberValid(string phoneNumber)
        {
            return phoneRegex.IsMatch(phoneNumber);
        }

    }



    #region Enums
    public enum noticeTypes : int
    {
        news = 1,
        popup = 0,
    }
    #endregion
}
