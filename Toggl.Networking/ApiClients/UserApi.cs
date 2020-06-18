﻿using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Toggl.Networking.Exceptions;
using Toggl.Networking.Helpers;
using Toggl.Networking.Models;
using Toggl.Networking.Network;
using Toggl.Networking.Serialization;
using Toggl.Networking.Serialization.Converters;
using Toggl.Shared;
using Toggl.Shared.Extensions;
using Toggl.Shared.Models;
using NotImplementedException = System.NotImplementedException;

namespace Toggl.Networking.ApiClients
{
    internal sealed class UserApi : BaseApi, IUserApi
    {
        private const string userAlreadyExistsApiErrorMessage = "user with this email already exists";
        private const string googleProvider = "google";
        private const string appleProvider = "apple";

        private readonly UserEndpoints endPoints;
        private readonly IJsonSerializer serializer;

        public UserApi(Endpoints endPoints, IApiClient apiClient, IJsonSerializer serializer,
            Credentials credentials)
            : base(apiClient, serializer, credentials, endPoints.LoggedIn)
        {
            this.endPoints = endPoints.User;
            this.serializer = serializer;
        }

        public async Task<IUser> Get()
            => await SendRequest<User>(endPoints.Get, AuthHeader)
                .ConfigureAwait(false);

        public async Task<IUser> GetWithGoogle()
            => await SendRequest<User>(endPoints.GetWithGoogle, AuthHeader)
                .ConfigureAwait(false);

        public async Task<IUser> GetWithApple(string clientId)
        {
            var headers = new[] {AuthHeader, HttpHeader.Referer(clientId) };
            return await SendRequest<User>(endPoints.Get, headers)
                .ConfigureAwait(false);
        }

        public async Task<IUser> Update(IUser user)
            => await SendRequest(endPoints.Put, AuthHeader, user as User ?? new User(user), SerializationReason.Post)
                .ConfigureAwait(false);

        public Task<string> ResetPassword(Email email)
        {
            var json = $"{{\"email\":\"{email}\"}}";
            return SendRequest(endPoints.ResetPassword, new HttpHeader[0], json)
                .ContinueWith(t => t.Result.Trim('"'));
        }

        public async Task<IUser> SignUp(
            Email email,
            Password password,
            bool termsAccepted,
            int countryId,
            string timezone
        )
        {
            if (!email.IsValid)
                throw new ArgumentException(nameof(email));

            var dto = new SignUpParameters
            {
                Email = email,
                Password = password,
                Workspace = new WorkspaceParameters
                {
                    InitialPricingPlan = PricingPlans.Free
                },
                TermsAccepted = termsAccepted,
                CountryId = countryId,
                Timezone = timezone
            };
            var json = serializer.Serialize(dto, SerializationReason.Post);
            try
            {
                var user = await SendRequest<User>(endPoints.Post, new HttpHeader[0], json)
                    .ConfigureAwait(false);
                return user;
            }
            catch (BadRequestException ex)
            when (ex.LocalizedApiErrorMessage == userAlreadyExistsApiErrorMessage)
            {
                throw new EmailIsAlreadyUsedException(ex);
            }
        }

        public async Task<IUser> SignUpWithGoogle(string googleToken, bool termsAccepted, int countryId, string timezone)
        {
            Ensure.Argument.IsNotNull(googleToken, nameof(googleToken));
            var parameters = new ThirdPartySignUpParameters
            {
                Provider = googleProvider,
                Token = googleToken,
                Workspace = new WorkspaceParameters
                {
                    InitialPricingPlan = PricingPlans.Free
                },
                TermsAccepted = termsAccepted,
                CountryId = countryId,
                Timezone = timezone
            };

            var json = serializer.Serialize(parameters, SerializationReason.Post);
            return await SendRequest<User>(endPoints.PostWithGoogle, new HttpHeader[0], json)
                .ConfigureAwait(false);
        }

        public async Task<IUser> SignUpWithApple(string clientId, string appleToken, string fullname, bool termsAccepted, int countryId, string timezone)
        {
            Ensure.Argument.IsNotNull(appleToken, nameof(appleToken));
            var parameters = new ThirdPartySignUpParameters
            {
                Provider = appleProvider,
                Token = appleToken,
                Fullname = fullname,
                Workspace = new WorkspaceParameters
                {
                    InitialPricingPlan = PricingPlans.Free
                },
                TermsAccepted = termsAccepted,
                CountryId = countryId,
                Timezone = timezone
            };

            var headers = new[] { HttpHeader.Referer(clientId) };
            var json = serializer.Serialize(parameters, SerializationReason.Post);
            return await SendRequest<User>(endPoints.Post, headers, json)
                .ConfigureAwait(false);
        }

        public async Task<string> LinkSso(Email email, string confirmationCode)
        {
            var endpoint = endPoints.EnableSso;

            var json = serializer.Serialize(new SsoLinkParameters
            {
                ConfirmationCode = confirmationCode
            }, SerializationReason.Post);

            return await SendRequest(endpoint, AuthHeader, json)
                .ConfigureAwait(false);
        }

        protected override async Task<Exception> GetExceptionFor(IRequest request, IResponse response,
            IEnumerable<HttpHeader> headers)
        {
            if (response.StatusCode == HttpStatusCode.Forbidden)
            {
                return new UnauthorizedException(request, response);
            }

            return await base.GetExceptionFor(request, response, headers);
        }

        [Preserve(AllMembers = true)]
        internal class WorkspaceParameters
        {
            public string Name { get; set; } = null;

            public PricingPlans InitialPricingPlan { get; set; }
        }

        [Preserve(AllMembers = true)]
        private class SignUpParameters
        {
            [JsonConverter(typeof(EmailConverter))]
            public Email Email { get; set; }

            [JsonConverter(typeof(PasswordConverter))]
            public Password Password { get; set; }

            public WorkspaceParameters Workspace { get; set; }

            [JsonProperty("tos_accepted")]
            public bool TermsAccepted { get; set; }

            public int CountryId { get; set; }

            public string Timezone { get; set; }
        }

        [Preserve(AllMembers = true)]
        private class ThirdPartySignUpParameters
        {
            public string Token { get; set; }

            public string Provider { get; set; }

            [JsonProperty("full_name", NullValueHandling = NullValueHandling.Ignore)]
            public string? Fullname { get; set; }

            public WorkspaceParameters Workspace { get; set; }

            [JsonProperty("tos_accepted")]
            public bool TermsAccepted { get; set; }

            public int CountryId { get; set; }

            public string Timezone { get; set; }
        }

        [Preserve(AllMembers = true)]
        private class SsoLinkParameters
        {
            public string ConfirmationCode { get; set; }
        }
    }
}
