using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace GN.Library.Identity
{
	public interface ITokenService
	{
		string GenerateToken(ClaimsIdentity identity, int expiresAfterHours = 7*24);
		ClaimsPrincipal ValidateToken(string securityToken);
	}
	
	class TokenService : ITokenService , ISecurityTokenValidator
	{
		//private readonly TokenValidator _handler;
		private readonly JwtSecurityTokenHandler _handler;
		private readonly TokenOptions options;

		public TokenService(TokenOptions options)
		{
			this._handler = new JwtSecurityTokenHandler();
			this.options = options;
			this.options.Validate();
		}
		public SecurityKey GetSigningKey()
		{
			return new SymmetricSecurityKey(Encoding.ASCII.GetBytes(this.options.SigningKey));
		}
		public string GenerateToken(ClaimsIdentity identity, int expiresAfterHours = 7 * 24)
		{
			var tokenHandler = new JwtSecurityTokenHandler();
			var tokenDescriptor = new SecurityTokenDescriptor
			{
				Subject = identity,
				Expires = DateTime.UtcNow.AddHours(expiresAfterHours),
				SigningCredentials = new SigningCredentials(this.GetSigningKey(), SecurityAlgorithms.HmacSha256Signature)
			};
			var token = tokenHandler.CreateToken(tokenDescriptor);
			return tokenHandler.WriteToken(token);
		}
		public ClaimsPrincipal ValidateToken(string securityToken)
		{
			return this.ValidateToken(securityToken, new TokenValidationParameters(), out var a);
		}
		public int MaximumTokenSizeInBytes { get; set; }

        public bool CanValidateToken => throw new NotImplementedException();

        public bool CanReadToken(string securityToken)
		{
			return true;
		}

        public ClaimsPrincipal ValidateToken(string securityToken, TokenValidationParameters validationParameters, out SecurityToken validatedToken)
        {
			var result = this._handler.ValidateToken(securityToken, new TokenValidationParameters
			{
				ValidateAudience = false,
				IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(this.options.SigningKey)),
				ValidateIssuer = false,

			}, out validatedToken);
			var token = validatedToken as JwtSecurityToken;
			//token.Claims
			//var result = new ClaimsPrincipal(token.Claims);

			return result;
		}
    }
}
