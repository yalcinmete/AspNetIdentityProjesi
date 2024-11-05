using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace AspNetIdentityProjesi.Identity
{
    public class CustomPasswordValidator : PasswordValidator
    {
        //RequireDigit,RequireLength,RequireLowercase.. lerin üstüne biz kendi koşulumuzu yazmak istersek ValidateAsync metodunu ezmemiz lazım.
        public override async Task<IdentityResult> ValidateAsync(string password)
        {
            //Sen normal sürecine devam et.Hatalar varsa resultin içine at ben üzerine eklemeler yapacağım demiş oluyoruz.
            var result= await base.ValidateAsync(password);

            if (password.Contains("12345")) 
            {
                var errors = result.Errors.ToList();
                errors.Add("Parola ardışık sayısal ifade içeremez");
                result = new IdentityResult(errors);
            }
            return result;
        }
    }
}