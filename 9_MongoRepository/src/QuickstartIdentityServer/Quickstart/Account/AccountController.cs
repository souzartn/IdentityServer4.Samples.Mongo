// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using IdentityModel;
using IdentityServer4.Services;
using IdentityServer4.Stores;
using IdentityServer4.Test;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Authentication;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Security.Principal;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.MongoDB;

namespace IdentityServer4.Quickstart.UI
{
    /// <summary>
    /// This sample controller implements a typical login/logout/provision workflow for local and external accounts.
    /// The login service encapsulates the interactions with the user data store. This data store is in-memory only and cannot be used for production!
    /// The interaction service provides a way for the UI to communicate with identityserver for validation and context retrieval
    /// </summary>
    [SecurityHeaders]
    public class AccountController : Controller
    {
        private readonly IIdentityServerInteractionService _interaction;
        private readonly AccountService _account;

        //Asp Idenitty Classes
        private readonly UserManager<IdentityUser> _userManager;

        private readonly SignInManager<IdentityUser> _signInManager;

        public AccountController(
            IIdentityServerInteractionService interaction,
            IClientStore clientStore,
            IHttpContextAccessor httpContextAccessor,
            UserManager<IdentityUser> userManager, SignInManager<IdentityUser> signInManager)
        {
            // if the TestUserStore is not in DI, then we'll just use the global users collection
            _signInManager = signInManager;
            _userManager = userManager;

            _interaction = interaction;
            _account = new AccountService(interaction, httpContextAccessor, clientStore);
        }

        /// <summary>
        /// Show login page
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Login(string returnUrl)
        {
            var vm = await _account.BuildLoginViewModelAsync(returnUrl);

            if (vm.IsExternalLoginOnly)
            {
                // only one option for logging in
                return await ExternalLogin(vm.ExternalProviders.First().AuthenticationScheme, returnUrl);
            }

            return View(vm);
        }

        /// <summary>
        /// Handle postback from username/password login
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginInputModel model)
        {
            if (ModelState.IsValid)
            {
                var result =
                    await _signInManager.PasswordSignInAsync(model.Username, model.Password, model.RememberLogin,
                        false);

                // validate username/password against in-memory store
                if (result.Succeeded)
                {
                    AuthenticationProperties props = null;
                    // only set explicit expiration here if persistent. 
                    // otherwise we reply upon expiration configured in cookie middleware.
                    if (AccountOptions.AllowRememberLogin && model.RememberLogin)
                    {
                        props = new AuthenticationProperties
                        {
                            IsPersistent = true,
                            ExpiresUtc = DateTimeOffset.UtcNow.Add(AccountOptions.RememberMeLoginDuration)
                        };
                    }
                    ;

                    // issue authentication cookie with subject ID and username
                    var user = await _userManager.FindByNameAsync(model.Username);
                    await HttpContext.Authentication.SignInAsync(user.Id, user.UserName, props);

                    // make sure the returnUrl is still valid, and if yes - redirect back to authorize endpoint
                    if (_interaction.IsValidReturnUrl(model.ReturnUrl))
                    {
                        return Redirect(model.ReturnUrl);
                    }

                    return Redirect("~/");
                }

                ModelState.AddModelError("", AccountOptions.InvalidCredentialsErrorMessage);
            }

            // something went wrong, show form with error
            var vm = await _account.BuildLoginViewModelAsync(model);
            return View(vm);
        }

        /// <summary>
        /// Show logout page
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Logout(string logoutId)
        {
            var vm = await _account.BuildLogoutViewModelAsync(logoutId);

            if (vm.ShowLogoutPrompt == false)
            {
                // no need to show prompt
                return await Logout(vm);
            }

            return View(vm);
        }

        /// <summary>
        /// Handle logout page postback
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout(LogoutInputModel model)
        {
            var vm = await _account.BuildLoggedOutViewModelAsync(model.LogoutId);
            if (vm.TriggerExternalSignout)
            {
                string url = Url.Action("Logout", new { logoutId = vm.LogoutId });
                try
                {
                    // hack: try/catch to handle social providers that throw
                    await HttpContext.Authentication.SignOutAsync(vm.ExternalAuthenticationScheme,
                        new AuthenticationProperties { RedirectUri = url });
                }
                catch (NotSupportedException) // this is for the external providers that don't have signout
                {
                }
                catch (InvalidOperationException) // this is for Windows/Negotiate
                {
                }
            }

            // delete local authentication cookie
            await HttpContext.Authentication.SignOutAsync();

            return View("LoggedOut", vm);
        }

        /// <summary>
        /// initiate roundtrip to external authentication provider
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> ExternalLogin(string provider, string returnUrl)
        {
            returnUrl = Url.Action("ExternalLoginCallback", new { returnUrl = returnUrl });

            // windows authentication is modeled as external in the asp.net core authentication manager, so we need special handling
            if (AccountOptions.WindowsAuthenticationSchemes.Contains(provider))
            {
                // but they don't support the redirect uri, so this URL is re-triggered when we call challenge
                if (HttpContext.User is WindowsPrincipal)
                {
                    var props = new AuthenticationProperties();
                    props.Items.Add("scheme", HttpContext.User.Identity.AuthenticationType);

                    var id = new ClaimsIdentity(provider);
                    id.AddClaim(new Claim(ClaimTypes.NameIdentifier, HttpContext.User.Identity.Name));
                    id.AddClaim(new Claim(ClaimTypes.Name, HttpContext.User.Identity.Name));

                    await HttpContext.Authentication.SignInAsync(
                        IdentityServerConstants.ExternalCookieAuthenticationScheme, new ClaimsPrincipal(id), props);
                    return Redirect(returnUrl);
                }
                else
                {
                    // this triggers all of the windows auth schemes we're supporting so the browser can use what it supports
                    return new ChallengeResult(AccountOptions.WindowsAuthenticationSchemes);
                }
            }
            else
            {
                // start challenge and roundtrip the return URL
                var props = new AuthenticationProperties
                {
                    RedirectUri = returnUrl,
                    Items = { { "scheme", provider } }
                };
                return new ChallengeResult(provider, props);
            }
        }

        /// <summary>
        /// Post processing of external authentication
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> ExternalLoginCallback(string returnUrl)
        {
            // read external identity from the temporary cookie
            var info =
                await HttpContext.Authentication.GetAuthenticateInfoAsync(IdentityServerConstants
                    .ExternalCookieAuthenticationScheme);
            var tempUser = info?.Principal;
            if (tempUser == null)
            {
                throw new Exception("External authentication error");
            }

            // retrieve claims of the external user
            var claims = tempUser.Claims.ToList();

            // try to determine the unique id of the external user - the most common claim type for that are the sub claim and the NameIdentifier
            // depending on the external provider, some other claim type might be used
            var userIdClaim = claims.FirstOrDefault(x => x.Type == JwtClaimTypes.Subject);
            if (userIdClaim == null)
            {
                userIdClaim = claims.FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier);
            }
            if (userIdClaim == null)
            {
                throw new Exception("Unknown userid");
            }

            var emailClaim = claims.FirstOrDefault(x => x.Type == ClaimTypes.Email);


            string email = "";

            if (emailClaim != null)
            {
                email = emailClaim.Value;
            }

            string userName = "";
            Claim nameClaim = null;

            nameClaim = claims.FirstOrDefault(x => x.Type == ClaimTypes.Name);

            if (nameClaim != null)
            {
                userName = nameClaim.Value;
            }
            else
            {
                userName = email;
            }

            // remove the user id claim from the claims collection and move to the userId property
            // also set the name of the external authentication provider
            claims.Remove(userIdClaim);
            var provider = info.Properties.Items["scheme"];
            var userId = userIdClaim.Value;

            var temp = await _userManager.FindByEmailAsync(email);

            if (temp != null)
            {
                userId = temp.Id;
            }
            
            var result = await _signInManager.ExternalLoginSignInAsync(provider, userIdClaim.Value, true);

            // check if the external user is already provisioned
            IdentityUser user;
            if (!result.Succeeded)
            {
                // this sample simply auto-provisions new external user
                // another common approach is to start a registrations workflow first
                user = new IdentityUser() { UserName = email, Email = email,LockoutEnabled = false};
                

                foreach (var claim in claims)
                {
                    user.Claims.Add(new IdentityUserClaim(claim));
                }

                var Userresult = await _userManager.CreateAsync(user);
                
                var loginResult = await _userManager.AddLoginAsync(user,
                    new ExternalLoginInfo(info.Principal, provider, userIdClaim.Value, userName));

            }
            else
            {
                user = await _userManager.FindByEmailAsync(email);
            }

            var additionalClaims = new List<Claim>();

            // if the external system sent a session id claim, copy it over
            var sid = claims.FirstOrDefault(x => x.Type == JwtClaimTypes.SessionId);
            if (sid != null)
            {
                additionalClaims.Add(new Claim(JwtClaimTypes.SessionId, sid.Value));
            }

            // if the external provider issued an id_token, we'll keep it for signout
            AuthenticationProperties props = null;
            var id_token = info.Properties.GetTokenValue("id_token");
            if (id_token != null)
            {
                props = new AuthenticationProperties();
                props.StoreTokens(new[] { new AuthenticationToken { Name = "id_token", Value = id_token } });
            }

            // issue authentication cookie for user
            await HttpContext.Authentication.SignInAsync(userIdClaim.Value, user.UserName, provider, props,
                additionalClaims.ToArray());

            // delete temporary cookie used during external authentication
            await HttpContext.Authentication.SignOutAsync(IdentityServerConstants.ExternalCookieAuthenticationScheme);

            // validate return URL and redirect back to authorization endpoint
            if (_interaction.IsValidReturnUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }

            return Redirect("~/");
        }
    }
}