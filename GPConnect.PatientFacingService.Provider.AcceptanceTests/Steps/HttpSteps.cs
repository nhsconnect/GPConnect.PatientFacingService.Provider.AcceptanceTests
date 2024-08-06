﻿namespace GPConnect.PatientFacingService.Provider.AcceptanceTests.Steps
{
    using Constants;
    using Context;
    using Helpers;
    using Logger;
    using TechTalk.SpecFlow;
    using Hl7.Fhir.Model;
    using Enum;
    using Factories;
    using Http;
    using Repository;
    using System;
    using Extensions;

    [Binding]
    public class HttpSteps : Steps
    {
        private readonly HttpContext _httpContext;
        public JwtHelper jwtHelper;
        private readonly SecuritySteps _securitySteps;
        private readonly SecurityContext _securityContext;
        private readonly IFhirResourceRepository _fhirResourceRepository;
        private readonly HttpRequestConfiguration _httpRequestConfiguration;

        public HttpSteps(HttpContext httpContext, JwtHelper jwtHelper, SecuritySteps securitySteps, SecurityContext securityContext, IFhirResourceRepository fhirResourceRepository, HttpRequestConfiguration httpRequestConfiguration)
        {
            Log.WriteLine("HttpSteps() Constructor");
            _httpContext = httpContext;
            this.jwtHelper = jwtHelper;
            _securitySteps = securitySteps;
            _securityContext = securityContext;
            _fhirResourceRepository = fhirResourceRepository;
            _httpRequestConfiguration = httpRequestConfiguration;
        }

        public HttpRequestConfiguration GetHttpRequestConfiguration(GpConnectInteraction interaction, HttpRequestConfiguration httpRequestConfiguration = null)
        {
            if (httpRequestConfiguration == null)
            {
                httpRequestConfiguration = new HttpRequestConfiguration();
            }

            var httpRequestConfigurationFactory = new HttpRequestConfigurationFactory(interaction, httpRequestConfiguration);

            return httpRequestConfigurationFactory.GetHttpRequestConfiguration();
        }

        public JwtHelper GetJwtHelper(GpConnectInteraction interaction, JwtHelper jwtHelper = null)
        {
            if (jwtHelper == null)
            {
                jwtHelper = new JwtHelper();
            }

            var jwtFactory = new JwtFactory(interaction);

            jwtFactory.ConfigureJwt(jwtHelper);

            return jwtHelper;
        }

        public HttpRequestConfiguration GetRequestBody(GpConnectInteraction interaction, HttpRequestConfiguration httpRequestConfiguration = null)
        {
            if (httpRequestConfiguration == null)
            {
                httpRequestConfiguration = new HttpRequestConfiguration();
            }

            var requestFactory = new RequestFactory(interaction, _fhirResourceRepository);

            requestFactory.ConfigureBody(httpRequestConfiguration);

            return httpRequestConfiguration;
        }

        [Given(@"I configure the default ""(.*)"" request and ""(.*)"" patient")]
        public void ConfigureRequest(GpConnectInteraction interaction,string patient )
        {
            _httpRequestConfiguration.SetAuthorisationToken(patient);
            
            _httpContext.SetDefaults();

            _httpContext.HttpRequestConfiguration = GetHttpRequestConfiguration(interaction, _httpContext.HttpRequestConfiguration);
  
            jwtHelper = GetJwtHelper(interaction, jwtHelper);

            _securitySteps.ConfigureServerCertificatesAndSsl();
        }

        [Given(@"I configure the authorisation request for ""(.*)""")]
        public void ConfigureAuthorisationRequest(string patient)
        {

            _httpRequestConfiguration.SetAuthorisationToken(patient);
        }


        [Given(@"I configure the default ""(.*)"" request")]
        public void ConfigureRequest(GpConnectInteraction interaction)
        {

            _httpContext.SetDefaults();

            _httpContext.HttpRequestConfiguration = GetHttpRequestConfiguration(interaction, _httpContext.HttpRequestConfiguration);

            jwtHelper = GetJwtHelper(interaction, jwtHelper);

            _securitySteps.ConfigureServerCertificatesAndSsl();
        }

        [Given(@"I configure the default ""(.*)"" request with old URL")]
        public void ConfigureRequestDefaultMetaDataRequestWithOldURL(GpConnectInteraction interaction)
        {
            _httpContext.SetDefaults();

            _httpContext.HttpRequestConfiguration = GetHttpRequestConfiguration(interaction, _httpContext.HttpRequestConfiguration);
            jwtHelper = GetJwtHelper(interaction, jwtHelper);

            _securitySteps.ConfigureServerCertificatesAndSsl();
        }

        [When(@"I make the ""(.*)"" request")]
        public void MakeRequest(GpConnectInteraction interaction)
        {
            if (interaction.Equals(GpConnectInteraction.AppointmentCreate))
            {
                TeardownSteps.AppointmentCreated();
            }
            
            _httpContext.HttpRequestConfiguration = GetRequestBody(interaction, _httpContext.HttpRequestConfiguration);

           // _httpContext.HttpRequestConfiguration.RequestHeaders.ReplaceHeader(HttpConst.Headers.kAuthorization, _httpContext.HttpRequestConfiguration.Authorization);
           // _httpContext.HttpRequestConfiguration.RequestHeaders.ReplaceHeader(HttpConst.Headers.kXRequestID, _httpContext.HttpRequestConfiguration.X_Request_ID);

            //_httpContext.HttpRequestConfiguration.RequestHeaders.AddHeader("x-request-id", "60E0B220-8136-4CA5-AE46-1D97EF59D068");

            //_httpContext.HttpRequestConfiguration.RequestHeaders.RemoveHeader(HttpConst.Headers.kAuthorization);
            //_httpContext.HttpRequestConfiguration.RequestHeaders.RemoveHeader(HttpConst.Headers.kSspTraceId);
            // _httpContext.HttpRequestConfiguration.RequestHeaders.RemoveHeader(HttpConst.Headers.kSspInteractionId);


            var httpRequest = new HttpContextRequest(_httpContext, _securityContext);

            httpRequest.MakeRequest();
        }


        [When(@"I make the ""(.*)"" request with missing Header ""(.*)""")]
        public void MakeRequestWithMissingHeader(GpConnectInteraction interaction, string headerKey)
        {
            _httpContext.HttpRequestConfiguration = GetRequestBody(interaction, _httpContext.HttpRequestConfiguration);

            _httpContext.HttpRequestConfiguration.RequestHeaders.ReplaceHeader(HttpConst.Headers.kAuthorization, jwtHelper.GetBearerToken());

            _httpContext.HttpRequestConfiguration.RequestHeaders.RemoveHeader(headerKey);

            var httpRequest = new HttpContextRequest(_httpContext, _securityContext);

            httpRequest.MakeRequest();
        }

        [When(@"I make the ""(.*)"" request with an unencoded JWT Bearer Token")]
        public void MakeRequestWithAnUnencodedJwtBearerToken(GpConnectInteraction interaction)
        {
            var requestFactory = new RequestFactory(interaction, _fhirResourceRepository);

            requestFactory.ConfigureBody(_httpContext.HttpRequestConfiguration);

            _httpContext.HttpRequestConfiguration.RequestHeaders.ReplaceHeader(HttpConst.Headers.kAuthorization, jwtHelper.GetBearerTokenWithoutEncoding());

            var httpRequest = new HttpContextRequest(_httpContext, _securityContext);

            httpRequest.MakeRequest();
        }

        [When(@"I make the ""(.*)"" request with invalid Resource type")]
        public void MakeRequestWithInvalidResourceType(GpConnectInteraction interaction)
        {
            var requestFactory = new RequestFactory(interaction, _fhirResourceRepository);

            requestFactory.ConfigureBody(_httpContext.HttpRequestConfiguration);
            requestFactory.ConfigureInvalidResourceType(_httpContext.HttpRequestConfiguration);

            _httpContext.HttpRequestConfiguration.RequestHeaders.ReplaceHeader(HttpConst.Headers.kAuthorization, jwtHelper.GetBearerToken());

            var httpRequest = new HttpContextRequest(_httpContext, _securityContext);

            httpRequest.MakeRequest();
        }

        [When(@"I make the ""(.*)"" request with Invalid Additional Field in the Resource")]
        public void MakeRequestWithInvalidAdditionalFieldInTheResource(GpConnectInteraction interaction)
        {
            var requestFactory = new RequestFactory(interaction, _fhirResourceRepository);

            requestFactory.ConfigureBody(_httpContext.HttpRequestConfiguration);
            requestFactory.ConfigureAdditionalInvalidFieldInResource(_httpContext.HttpRequestConfiguration);

            _httpContext.HttpRequestConfiguration.RequestHeaders.ReplaceHeader(HttpConst.Headers.kAuthorization, jwtHelper.GetBearerToken());

            var httpRequest = new HttpContextRequest(_httpContext, _securityContext);

            httpRequest.MakeRequest();
        }

        [When(@"I make the ""(.*)"" request with invalid parameter Resource type")]
        public void MakeRequestWithInvalidParameterResourceType(GpConnectInteraction interaction)
        {
            var requestFactory = new RequestFactory(interaction, _fhirResourceRepository);
            requestFactory.ConfigureBody(_httpContext.HttpRequestConfiguration);
            requestFactory.ConfigureInvalidParameterResourceType(_httpContext.HttpRequestConfiguration);
            _httpContext.HttpRequestConfiguration.RequestHeaders.ReplaceHeader(HttpConst.Headers.kAuthorization, jwtHelper.GetBearerToken());

            var httpRequest = new HttpContextRequest(_httpContext, _securityContext);

            httpRequest.MakeRequest();
        }

        [When(@"I make the ""(.*)"" request with additional field in parameter Resource")]
        public void MakeRequestWithAdditionalFieldInParameterResource(GpConnectInteraction interaction)
        {
            var requestFactory = new RequestFactory(interaction, _fhirResourceRepository);
            requestFactory.ConfigureBody(_httpContext.HttpRequestConfiguration);
            requestFactory.ConfigureParameterResourceWithAdditionalField(_httpContext.HttpRequestConfiguration);
            _httpContext.HttpRequestConfiguration.RequestHeaders.ReplaceHeader(HttpConst.Headers.kAuthorization, jwtHelper.GetBearerToken());

            var httpRequest = new HttpContextRequest(_httpContext, _securityContext);

            httpRequest.MakeRequest();
        }

        [When(@"I make the ""(.*)"" request with depricated URLs")]
        public void MakeRequestWithDepricatedURLs(GpConnectInteraction interaction)
        {

            _httpContext.HttpRequestConfiguration = GetRequestBody(interaction, _httpContext.HttpRequestConfiguration);

            _httpContext.HttpRequestConfiguration.RequestHeaders.ReplaceHeader(HttpConst.Headers.kAuthorization, jwtHelper.GetBearerToken());

            var httpRequest = new HttpContextRequest(_httpContext, _securityContext);

            httpRequest.MakeRequest();
        }

        public Resource GetResourceForRelativeUrl(GpConnectInteraction gpConnectInteraction, string relativeUrl)
        {
            var httpRequestConfiguration = new HttpRequestConfiguration();

            var httpContextFactory = new HttpRequestConfigurationFactory(gpConnectInteraction, httpRequestConfiguration);

            httpContextFactory.GetHttpRequestConfiguration();
            
            var jwtHelper = new JwtHelper();
            var jwtFactory = new JwtFactory(gpConnectInteraction);

            jwtFactory.ConfigureJwt(jwtHelper);

            if (relativeUrl.Contains("Patient"))
            {
                var patient = relativeUrl.ToLower().Replace("/", string.Empty);
                try
                {
                    jwtHelper.RequestedPatientNHSNumber = GlobalContext.PatientNhsNumberMap[patient];
                }
                catch (Exception)
                {
                  
                    Patient patientResource = _fhirResourceRepository.Patient;
                    var nhsNumber = patientResource.Identifier[0].Value;
                    jwtHelper.RequestedPatientNHSNumber = nhsNumber;
                }
            }

            if (relativeUrl.Contains("Organization"))
            {
                var organizationId = relativeUrl.Split('/')[1];
                jwtHelper.RequestedOrganizationId = organizationId;
            }

            httpRequestConfiguration.RequestUrl = relativeUrl;

            _securitySteps.ConfigureServerCertificatesAndSsl();

            var requestFactory = new RequestFactory(gpConnectInteraction, _fhirResourceRepository);

            requestFactory.ConfigureBody(httpRequestConfiguration);

            httpRequestConfiguration.RequestHeaders.ReplaceHeader(HttpConst.Headers.kAuthorization, jwtHelper.GetBearerToken());

            var httpRequest = new HttpResourceRequest(httpRequestConfiguration, _securityContext);

            return httpRequest.MakeRequest().Resource;
        }
    }
}
