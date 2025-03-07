// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ESA_Terra_Argila.Areas.Identity.Pages.Account
{
    [AllowAnonymous]
    public class LockoutModel : PageModel
    {
        // Propriedade para a mensagem de bloqueio
        public string LockoutMessage { get; private set; }

        public void OnGet()
        {
            LockoutMessage = "A sua conta foi temporariamente bloqueada devido a várias tentativas de login falhadas. Tente novamente após 5 minutos ou contacte o suporte.";
        }
    }
}