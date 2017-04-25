﻿using System.Threading.Tasks;
using AbcLeaves.Core;

namespace AbcLeaves.Api.Services
{
    public interface IGoogleOAuthService
    {
        Task<ExchangeAuthCodeResult> ExchangeAuthCode(string code, string redirectUrl);
        Task<ExchangeRefreshTokenResult> ExchangeRefreshToken(string refreshToken);
        Task<VerifyAccessResult> ValidateRefreshTokenAsync(string refreshToken);
    }
}