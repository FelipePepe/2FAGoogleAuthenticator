using _2FAGoogleAuthenticator.ViewModel;
using Google.Authenticator;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace _2FAGoogleAuthenticator.Controllers
{
    public class HomeController : Controller
    {
        private const string key = "****-##-****"; // any 10-12 char string for use as private key in the Google Authenticator app

        public ActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Login(LoginModel login)
        {
            string message = string.Empty;
            bool status = false;

            // check username and password form out database here
            // for demo I am going to use Admin as Username and Password1 as Password static value

            if (login.UserName.Equals("Admin") && login.Password.Equals("Password1"))
            {
                status = true;
                message = "Login Successful";
                Session["Username"] = login.UserName;

                TwoFactorAuthenticator tfa = new TwoFactorAuthenticator();
                string userUniqueKey = login.UserName + key; // as Its a demo, I have done this way, but user any encrypted value.
                Session["UserUniqueKey"] = userUniqueKey;

                var setupInfo = tfa.GenerateSetupCode("Dotnet Awesome", login.UserName, userUniqueKey, false, 3);

                ViewBag.BarcodeImageUrl = setupInfo.QrCodeSetupImageUrl;
                ViewBag.SetupCode = setupInfo.ManualEntryKey;
            }
            else
            {
                message = "Invalid Username or Password";
            }

            ViewBag.Message = message;
            ViewBag.Status = status;


            return View();
        }

        public ActionResult MyProfile()
        {
            if (Session["UserName"] == null || Session["isValid2FA"] == null || !(bool)Session["IsValid2FA"])
            {
                return RedirectToAction("Login");
            }

            ViewBag.Message = "Wellcome " + Session["UserName"].ToString();

            return View();
        }

        public ActionResult Verify2FA()
        {
            var token = Request["passcode"];
            TwoFactorAuthenticator tfa = new TwoFactorAuthenticator();

            string UserUniqueKey = Session["UserUniqueKey"].ToString();

            bool isValid = tfa.ValidateTwoFactorPIN(UserUniqueKey, token);

            Session["IsValid2FA"] = isValid;

            if (isValid)
            {
                return RedirectToAction("MyProfile", "Home");
            }
            else
            {
                return RedirectToAction("Login", "Home");
            }

        }
    }
}