using Amazon.Extensions.CognitoAuthentication;
using Amazon.AspNetCore.Identity.Cognito;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using WebAdvert.Web.Models.Accounts;
using System;

namespace WebAdvert.Web.Controllers
{
    public class AccountController : Controller
    {
        private readonly SignInManager<CognitoUser> _signInManager;
        private readonly UserManager<CognitoUser> _userManager;
        private readonly CognitoUserPool _pool;
        public AccountController(SignInManager<CognitoUser> signInManager, UserManager<CognitoUser> userManager, CognitoUserPool pool)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _pool = pool;
        }

        public async Task<IActionResult> Signup()
        {
            var model = new SignupModel();
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Signup(SignupModel signupModel)
        {
            try
            {
                if (ModelState.IsValid)
                {                   
                   
                        var user = _pool.GetUser(signupModel.Email);
                        if (user.Status != null)
                        {
                            ModelState.AddModelError("UserExists", "User with this email already exists");
                            return View(signupModel);
                        }

                        user.Attributes.Add(CognitoAttribute.Name.ToString(), signupModel.Email);
                        var createdUser = await _userManager.CreateAsync(user, signupModel.Password).ConfigureAwait(false);

                        if (createdUser.Succeeded) return RedirectToAction("Confirm");
                    
                    return View(signupModel);

                }
                return View(signupModel);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [HttpGet]
        public async Task<IActionResult> Confirm(ConfirmModel model)
        {
            return View(model);
        }

        [HttpPost]
        [ActionName("Confirm")]
        public async Task<IActionResult> ConfirmPost(ConfirmModel model)
        {
            if(ModelState.IsValid)
            {
                var user = _userManager.FindByEmailAsync(model.Email).Result;
                if (user == null)
                {
                    ModelState.AddModelError("NotFound", "A user with the given email address was not found");
                    return View(model);
                }

                //var result = await _userManager.ConfirmEmailAsync(user, model.Code);
                var result = await ((CognitoUserManager<CognitoUser>)_userManager)
                    .ConfirmSignUpAsync(user, model.Code, true).ConfigureAwait(false);
                if (result.Succeeded) return RedirectToAction("Index", "Home");

                foreach (var item in result.Errors) 
                    ModelState.AddModelError(item.Code, item.Description);

            }
            return View(model);
        }

        [HttpGet]
        public IActionResult Login(LoginModel model)
        {
            return View(model);
        }

        [HttpPost]
        [ActionName("Login")]
        public async Task<IActionResult> LoginPost(LoginModel model)
        {
            if (ModelState.IsValid)
            {
                var result = await _signInManager.PasswordSignInAsync(model.Email,
                    model.Password, model.RememberMe, false).ConfigureAwait(false);
                if (result.Succeeded)
                    return RedirectToAction("Index", "Home");
                ModelState.AddModelError("LoginError", "Email and password do not match");
            }

            return View("Login", model);
        }

    }

}
        
    
