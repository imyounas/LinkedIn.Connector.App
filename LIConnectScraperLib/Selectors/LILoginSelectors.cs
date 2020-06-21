
namespace LIConnectScraperLib.Selectors
{
   public class LILoginSelectors
    {

        public static string LogInRedirectUrl = "https://www.linkedin.com/start/join?trk=login_reg_redirect";
        public static string LogInRedirectUrl2 = "https://www.linkedin.com/redirect";
        public static string LogInRedirectUrl3 = "https://www.linkedin.com/m/login/";
        public static string LogInRedirectUrl4 = "www.google.com";
        public static string LogInRedirectUrl5 = "chrome:";
        public static string LogInRedirectUrl6 = "https://www.linkedin.com/m/login/";

        public static string LoginButtonIdClick = "btn-primary";
        public static string UserNameTextId = "session_key-login";
        public static string PasswordTextId = "session_password-login";
        // public static string SignInAnchorClsClick = "#uno-reg-join > div > div > div > div.content-container > div.reg-content-wrapper.single > div > div > p > a";
        //public static string RegisterationFromFoundCSSSel = "#uno-reg-join > div > div > div > div.content-container > div.reg-content-wrapper.single > div > div > form";
        public static string SignInAnchorClsClick = "sign-in-link";
        public static string RegisterationFromFoundCSSSel = "form.join-linkedin-form";
    }


    public class LIDirectLoginSelectors
    {
        public static string LoginUrl = "https://www.linkedin.com/";
        public static string LoginFormCSSSel = "form.login-form";
        public static string HomeUrlAfterLogin = "https://www.linkedin.com/feed/";
        public static string JobUrlAfterLogin = "https://www.linkedin.com/jobs/";
        public static string LoginButtonIdClick = "login-submit";
        public static string UserNameTextId = "login-email";
        public static string PasswordTextId = "login-password";
        public static string ProfileNavButtonId = "profile-nav-item";
    }
}
