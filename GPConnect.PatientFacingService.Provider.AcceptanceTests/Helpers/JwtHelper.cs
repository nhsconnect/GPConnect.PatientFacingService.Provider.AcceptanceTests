﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using GPConnect.PatientFacingService.Provider.AcceptanceTests.Constants;
using GPConnect.PatientFacingService.Provider.AcceptanceTests.Extensions;
using GPConnect.PatientFacingService.Provider.AcceptanceTests.Logger;
using Microsoft.IdentityModel.Tokens;

// ReSharper disable ClassNeverInstantiated.Global

namespace GPConnect.PatientFacingService.Provider.AcceptanceTests.Helpers
{
    public class JwtHelper
    {
        private const string Bearer = "Bearer ";
        private const int MaxExpiryTimeInMinutes = 5;

        public DateTime? CreationTime { get; set; }
        public DateTime? ExpiryTime { get; set; }
        public string ReasonForRequest { get; set; }
        public string AuthTokenURL { get; set; }
        public string AuthTokenURLFoundationsAndAppmts { get; set; }
        public string AuthTokenURLStructured { get; set; }
        public string AuthTokenURLDocuments { get; set; }
        public string RequestingDevice { get; set; }
        public string RequestingOrganization { get; set; }
        public string RequestingIdentity { get; set; }
        public string RequestingIdentityId { get; set; }
        public string RequestedScope { get; set; }
        public string RequestedPatientNHSNumber { get; set; }
        public string RequestedOrganizationODSCode { get; set; }
        public string RequestedOrganizationId { get; set; }
        public string RequestingSystemUrl { get; set; }

        public JwtHelper()
        {
            Log.WriteLine("JwtHelper() Constructor");
            SetDefaultValues();
        }

        public void SetDefaultValues()
        {
            CreationTime = DateTime.UtcNow;
            ExpiryTime = CreationTime.Value.AddMinutes(MaxExpiryTimeInMinutes);
            SetCreationTimeSecondsPast(2);
            SetExpiryTimeInSecondsPast(2);
            ReasonForRequest = JwtConst.Values.kDirectCare;
            AuthTokenURL = "";
            //AuthTokenURLFoundationsAndAppmts = AppSettingsHelper.JwtAudValueFoundationsAndAppmts;
            //AuthTokenURLStructured = AppSettingsHelper.JwtAudValueStructured;
            //AuthTokenURLDocuments = AppSettingsHelper.JwtAudValueDocuments;
            RequestingDevice = FhirHelper.GetDefaultDevice().ToFhirJson();
            RequestingOrganization = FhirHelper.GetDefaultOrganization().ToFhirJson();
            RequestingIdentityId = FhirHelper.GetDefaultPractitioner().Id;
            RequestingIdentity = FhirHelper.GetDefaultPractitioner().ToFhirJson();
            RequestedScope = JwtConst.Scope.kOrganizationRead;
            // TODO Check We're Using The Correct Scope For Metadata vs. GetCareRecord
            RequestedPatientNHSNumber = null;
            // TODO Move Dummy Data Out Into App.Config Or Somewhere Else
         

            RequestingSystemUrl = "https://ConsumerSystemURL";
        }

        private static string BuildEncodedHeader()
        {
            return new JwtHeader().Base64UrlEncode();
        }

        private string BuildEncodedPayload()
        {
            return BuildPayload().Base64UrlEncode();
        }

        private JwtPayload BuildPayload()
        {
            var claims = new List<Claim>();

            if (RequestingSystemUrl != null)
                claims.Add(new Claim(JwtConst.Claims.kRequestingSystemUrl, RequestingSystemUrl, ClaimValueTypes.String));
            if (RequestingIdentityId != null)
                claims.Add(new Claim(JwtConst.Claims.kPractitionerId, RequestingIdentityId, ClaimValueTypes.String));
            if (AuthTokenURL != null)
                claims.Add(new Claim(JwtConst.Claims.kAuthTokenURL, AuthTokenURL, ClaimValueTypes.String));
            if (ExpiryTime != null)
                claims.Add(new Claim(JwtConst.Claims.kExpiryTime, EpochTime.GetIntDate(ExpiryTime.Value).ToString(), ClaimValueTypes.Integer64));
            if (CreationTime != null)
                claims.Add(new Claim(JwtConst.Claims.kCreationTime, EpochTime.GetIntDate(CreationTime.Value).ToString(), ClaimValueTypes.Integer64));
            if (ReasonForRequest != null)
                claims.Add(new Claim(JwtConst.Claims.kReasonForRequest, ReasonForRequest, ClaimValueTypes.String));
            if (RequestingDevice != null)
                claims.Add(new Claim(JwtConst.Claims.kRequestingDevice, RequestingDevice, JsonClaimValueTypes.Json));
            if (RequestingOrganization != null)
                claims.Add(new Claim(JwtConst.Claims.kRequestingOrganization, RequestingOrganization, JsonClaimValueTypes.Json));
            if (RequestingIdentity != null)
                claims.Add(new Claim(JwtConst.Claims.kRequestingPractitioner, RequestingIdentity, JsonClaimValueTypes.Json));
            if (RequestedScope != null)
                claims.Add(new Claim(JwtConst.Claims.kRequestedScope, RequestedScope, ClaimValueTypes.String));
            
            return new JwtPayload(claims);
        }

        public string GetBearerTokenWithoutEncoding()
        {
            return Bearer + BuildBearerTokenOrgResourceWithoutEncoding();
        }

        private string BuildBearerTokenOrgResourceWithoutEncoding()
        {
            return new JwtHeader().SerializeToJson() + "." + BuildPayload().SerializeToJson() + ".";
        }

        public string GetBearerToken()
        {
            return Bearer + BuildBearerTokenOrgResource();
        }

        private string BuildBearerTokenOrgResource()
        {
            return BuildEncodedHeader() + "." + BuildEncodedPayload() + ".";
        }

        public void SetExpiryTimeInSeconds(double seconds)
        {
            Debug.Assert(CreationTime != null, "_jwtCreationTime != null");
            ExpiryTime = CreationTime.Value.AddSeconds(seconds);
        }

        public void SetCreationTimeSeconds(double seconds)
        {
            CreationTime = DateTime.UtcNow.AddSeconds(seconds);
            ExpiryTime = CreationTime.Value.AddMinutes(MaxExpiryTimeInMinutes);
        }

        public void SetExpiryTimeInSecondsPast(double seconds)
        {
            Debug.Assert(CreationTime != null, "_jwtCreationTime != null");
            ExpiryTime = CreationTime.Value.AddMinutes(MaxExpiryTimeInMinutes);
            ExpiryTime.Value.AddSeconds(-seconds);
        }

        public void SetCreationTimeSecondsPast(double seconds)
        {
            CreationTime = DateTime.UtcNow.AddSeconds(-seconds);
            ExpiryTime = CreationTime.Value.AddMinutes(MaxExpiryTimeInMinutes);
        }

        public void SetRequestingPractitioner(string practitionerId, string practitionerJson)
        {
            // TODO Make The RequestingPractitionerId Use The Business Identifier And Not The Logical Identifier 
            RequestingIdentityId = practitionerId;
            RequestingIdentity = practitionerJson;
        }
    }
}
