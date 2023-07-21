﻿namespace GPConnect.PatientFacingService.Provider.AcceptanceTests.Steps
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Cache;
    using Cache.ValueSet;
    using Builders.Patient;
    using Constants;
    using Context;
    using Enum;
    using Extensions;
    using Helpers;
    using Hl7.Fhir.Model;
    using Repository;
    using Shouldly;
    using TechTalk.SpecFlow;

    [Binding]
    public class RegisterPatientSteps : BaseSteps
    {
        private readonly HttpContext _httpContext;
        private readonly PatientSteps _patientSteps;
        private readonly HttpResponseSteps _httpResponseSteps;
        private readonly IFhirResourceRepository _fhirResourceRepository;
        private List<Patient> Patients => _httpContext.FhirResponse.Patients;

        public RegisterPatientSteps(HttpSteps httpSteps, HttpContext httpContext, PatientSteps patientSteps, HttpResponseSteps httpResponseSteps, IFhirResourceRepository fhirResourceRepository)
            : base(httpSteps)
        {
            _httpContext = httpContext;
            _patientSteps = patientSteps;
            _httpResponseSteps = httpResponseSteps;
            _fhirResourceRepository = fhirResourceRepository;
        }

        [Given(@"I create a Patient which does not exist on PDS and store it")]
        public void CreateAPatientWhichDoesNoteExistOnPDSAndStoreIt()
        {
            // amended github ref 115 (demonstrator)
            // RMB 24-10-2018
            //            var patientIdentifier = new Identifier(FhirConst.IdentifierSystems.kNHSNumber, "9019546082");
            var nhsNumber = GlobalContext.PatientNhsNumberMap["patient14"];
            var patientIdentifier = new Identifier(FhirConst.IdentifierSystems.kNHSNumber, nhsNumber);
            patientIdentifier.Extension.Add(new Extension
            {
                Url = FhirConst.StructureDefinitionSystems.kExtCcGpcNhsNumVerification,
                Value = new CodeableConcept(FhirConst.CodeSystems.kCcNhsNumVerification, "01", "Number present and verified", null)
            });

            var returnPatient = new Patient
            {
                Name = new List<HumanName>
                {
                    NameHelper.CreateOfficialName("GPConnectGivenName", "GPConnectFamilyName")
                },
                Gender = AdministrativeGender.Other,
                BirthDateElement = new Date("2017-05-05"),
                Identifier = new List<Identifier>
                {
                    patientIdentifier
                }
            };

            _fhirResourceRepository.Patient = returnPatient;
        }

        [Given(@"I set the Stored Patient Demographics to not match the NHS number")]
        public void SetTheStoredPatientDemographicsToNotMatchTheNhsNumber()
        {

            _fhirResourceRepository.Patient.Name = new List<HumanName>
            {
                NameHelper.CreateOfficialName("GPConnectGivenName", "GPConnectFamilyName")
            };

            _fhirResourceRepository.Patient.Gender = AdministrativeGender.Other;

            _fhirResourceRepository.Patient.BirthDateElement = new Date("2017-05-05");
        }

        [Given(@"I add ""(.*)"" Given Names to the Stored Patient Official Name")]
        public void AddGivenNamesToTheStoredPatientOfficialName(int extraGivenNames)
        {
            const string givenName = "GivenName";
            var givenNames = new List<FhirString>();

            for (var i = 1; i <= extraGivenNames; i++)
            {
                givenNames.Add(new FhirString($"{givenName}-{i}"));
            }

            var name = _fhirResourceRepository.Patient.Name.First(n => n.Use == HumanName.NameUse.Official);
            name.GivenElement.AddRange(givenNames);
        }

        [Given(@"I remove the Identifiers from the Stored Patient")]
        public void RemoveTheIdentifiersFromTheStoredPatient()
        {
            _fhirResourceRepository.Patient.Identifier = null;
        }


        [Given(@"I remove the Official Name from the Stored Patient")]
        public void RemoveTheOfficialNameFromTheStoredPatient()
        {
            _fhirResourceRepository.Patient.Name.ForEach(n =>
            {
                n.Use = HumanName.NameUse.Anonymous;
            });
        }

        [Given(@"I remove the Family Name from the Active Given Name for the Stored Patient")]
        public void RemoveTheFamilyNameFromTheActiveGivenNameForTheStoredPatient()
        { 
            foreach (var humanName in _fhirResourceRepository.Patient.Name.Where(IsActiveOfficialName))
            {
                humanName.Family = null;
            }
        }

        // github ref 110
        // RMB 23/10/2018		
        [Given(@"I change the Family Name from the Stored Patient")]
        public void ChangeTheFamilyNameFromTheStoredPatient()
        {
            _fhirResourceRepository.Patient.Name = null;
        }

        [Given(@"I remove the Gender from the Stored Patient")]
        public void RemoveTheGenderFromTheStoredPatient()
        {
            _fhirResourceRepository.Patient.Gender = null;
        }

        // github ref 110
        // RMB 23/10/2018		
        [Given(@"I change the Gender from the Stored Patient")]
        public void ChangeTheGenderFromTheStoredPatient()
        {
            _fhirResourceRepository.Patient.Gender = AdministrativeGender.Other;
        }

        [Given(@"I remove the Birth Date from the Stored Patient")]
        public void RemoveTheBirthDateFromTheStoredPatient()
        {
            _fhirResourceRepository.Patient.BirthDate = null;
        }

        // github ref 110
        // RMB 23/10/2018		
        [Given(@"I change the Birth Date from the Stored Patient")]
        public void ChangeTheBirthDateFromTheStoredPatient()
        {
            _fhirResourceRepository.Patient.BirthDate = "1958-07-21";
        }

        // github ref 110
        // RMB 23/10/2018		
        [Given(@"I change the NHSNo from the Stored Patient ""(.*)""")]
        public void ChangeTheNHNoFromTheStoredPatient(string nhsNumber)
        {
            var PatNHSNo = GlobalContext.PatientNhsNumberMap[nhsNumber];
            _fhirResourceRepository.Patient.Identifier[0].Value = PatNHSNo;
        }

        [Given(@"I add an Identifier with Value ""([^""]*)"" to the Stored Patient")]
        public void AddAnIdentifierWithValueToTheStoredPatient(string nhsNumber)
        {
            _fhirResourceRepository.Patient.Identifier.Add(new Identifier(FhirConst.IdentifierSystems.kNHSNumber, nhsNumber));
            _fhirResourceRepository.Patient.Identifier[0].Extension.Add(new Extension
            {
                Url = FhirConst.StructureDefinitionSystems.kExtCcGpcNhsNumVerification,
                Value = new CodeableConcept(FhirConst.CodeSystems.kCcNhsNumVerification, "01", "Number present and verified", null)
            });
        }

        [Given(@"I add the Stored Patient as a parameter")]
        public void AddTheStoredPatientAsAParameter()
        {
            _httpContext.HttpRequestConfiguration.BodyParameters.Add("registerPatient", _fhirResourceRepository.Patient);
        }

        [Given(@"I add the Stored Appointment as a parameter")]
        public void AddTheStoredAppointmentAsAParameter()
        {
            _httpContext.HttpRequestConfiguration.BodyParameters.Add("appointment", _fhirResourceRepository.Appointment);
        }

        [Given(@"I add the Stored Organization as a parameter")]
        public void AddTheStoredOrganizationAsAParameterWithName()
        {
            _httpContext.HttpRequestConfiguration.BodyParameters.Add("organization", _fhirResourceRepository.Organization);
        }

        [Given(@"I add the Stored Patient as a parameter with name ""([^""]*)""")]
        public void AddTheStoredPatientAsAParameterWithName(string parameterName)
        {
            _httpContext.HttpRequestConfiguration.BodyParameters.Add(parameterName, _fhirResourceRepository.Patient);
        }

        [Given(@"I add an Identifier with missing System to the Stored Patient")]
        public void AddAnIdentifierWithMissingSystemToTheStoredPatient()
        {
            var identifier = new Identifier
            {
                Value = "NewIdentifierNoSystem"
            };

            _fhirResourceRepository.Patient.Identifier.Add(identifier);
        }
        
        [Given(@"I add a generic Identifier to the Stored Patient")]
        public void AddAGenericIdentifierToTheStoredPatient()
        {
            var identifier = new Identifier
            {
                Value = "GenericIdentifierValue",
                System = "GenericIdentifierSystem"
            };

            _fhirResourceRepository.Patient.Identifier.Add(identifier);
        }

        [Given(@"I add a Active element to the Stored Patient")]
        public void AddAActiveElementToStoredPatient()
        {
            _fhirResourceRepository.Patient.Active = true;
        }

        [Given(@"I add a Address element to the Stored Patient")]
        public void AddAAddressElementToStoredPatient()
        {
            var address = new Address
            {
                CityElement = new FhirString("Leeds"),
                PostalCode = "LS1 6AE"
            };

            address.LineElement.Add(new FhirString("1 Trevelyan Square"));
            address.LineElement.Add(new FhirString("Boar Lane"));

            address.Use = Address.AddressUse.Temp;
            _fhirResourceRepository.Patient.Address.Add(address);
        }
// git hub ref 180
// RMB 4/2/19
        [Given(@"I add a Address element to the Stored Patient with Use ""([^""]*)""")]
        public void AddAAddressElementToStoredPatientWithUse(Address.AddressUse use)
        {
            var address = new Address
            {
                CityElement = new FhirString("Leeds"),
                PostalCode = "LS1 6AE"
            };

            address.LineElement.Add(new FhirString("1 Trevelyan Square"));
            address.LineElement.Add(new FhirString("Boar Lane"));

            address.Use = use;
            _fhirResourceRepository.Patient.Address.Add(address);
        }

        [Given(@"I add a Address element without temp to the Stored Patient")]
        public void AddAAddressElementWithoutTempToStoredPatient()
        {
            var address = new Address
            {
                CityElement = new FhirString("Leeds"),
                PostalCode = "LS1 6AE"
            };

            address.LineElement.Add(new FhirString("1 Trevelyan Square"));
            address.LineElement.Add(new FhirString("Boar Lane"));

            address.Use = Address.AddressUse.Work;
            _fhirResourceRepository.Patient.Address.Add(address);
        }

        [Given(@"I add a Animal element to the Stored Patient")]
        public void AddAAnimalElementToStoredPatient()
        {
            _fhirResourceRepository.Patient.Animal = new Patient.AnimalComponent
            {
                Species = new CodeableConcept("AllSpecies", "Human")
            };
        }

        [Given(@"I add a Births element to the Stored Patient")]
        public void AddABirthsElementToStoredPatient()
        {
            _fhirResourceRepository.Patient.MultipleBirth = new FhirBoolean(true);
        }

        [Given(@"I add a CareProvider element to the Stored Patient")]
        public void AddACareProviderElementToStoredPatient()
        {
            var reference = new ResourceReference
            {
                Display = "Test Care Provider"
            };

            _fhirResourceRepository.Patient.GeneralPractitioner.Add(reference);
        }

        [Given(@"I add a Communication element to the Stored Patient")]
        public void AddACommunicationElementToStoredPatient()
        {
            var com = new Patient.CommunicationComponent
            {
                Language = new CodeableConcept("https://tools.ietf.org/html/bcp47", "en")
            };

            _fhirResourceRepository.Patient.Communication.Add(com);
        }

        [Given(@"I add a Name element to the Stored Patient")]
        public void AddANameElementToStoredPatient()
        {
            _fhirResourceRepository.Patient.Name.Add(NameHelper.CreateName(HumanName.NameUse.Nickname, "AdditionalGiven", "AdditionalFamily"));
        }


        [Given(@"I add a Contact element to the Stored Patient")]
        public void AddAContactElementToStoredPatient()
        {
            var contact = new Patient.ContactComponent
            {
                Name = NameHelper.CreateName(HumanName.NameUse.Anonymous, "TestGiven", "TestFamily")
            };

            _fhirResourceRepository.Patient.Contact.Add(contact);
        }

        [Given(@"I add a Deceased element to the Stored Patient")]
        public void AddADeceasedElementToStoredPatient()
        {
            _fhirResourceRepository.Patient.Deceased = new FhirDateTime("2017-09-01T10:00:00");
        }

        [Given(@"I add a Link element to the Stored Patient")]
        public void AddALinkElementToStoredPatient()
        {
            var reference = new ResourceReference
            {
                Display = "Test Care Provider"
            };

            var link = new Patient.LinkComponent
            {
                Other = reference,
                Type = Patient.LinkType.Refer
            };

            _fhirResourceRepository.Patient.Link.Add(link);
        }

        [Given(@"I add a ManagingOrg element to the Stored Patient")]
        public void AddAManagingOrgElementToStoredPatient()
        {
            var reference = new ResourceReference
            {
                Display = "Test Managing Org"
            };

            _fhirResourceRepository.Patient.ManagingOrganization = reference;
        }

        [Given(@"I add a Marital element to the Stored Patient")]
        public void AddAMaritalElementToStoredPatient()
        {
            _fhirResourceRepository.Patient.MaritalStatus = new CodeableConcept(FhirConst.CodeSystems.kMaritalStatus, "M");
        }

        [Given(@"I add a Photo element to the Stored Patient")]
        public void AddAPhotoElementToStoredPatient()
        {
            var attachment = new Attachment
            {
                Url = "Test Photo Element"
            };

            _fhirResourceRepository.Patient.Photo.Add(attachment);
        }

        [Given(@"I add a Telecom element to the Stored Patient")]
        public void AddATelecomElementToStoredPatient()
        {
            _fhirResourceRepository.Patient.Telecom.Add(new ContactPoint(ContactPoint.ContactPointSystem.Phone, ContactPoint.ContactPointUse.Home, "01234567891"));
        }
        // github ref 138
        // RMB 12/11/2018

        [Given(@"I add a Telecom element use ""([^""]*)"" to the Stored Patient")]
        public void AddATelecomElementUseHomeToStoredPatient(string Use)
        {
            if (Use.Contains("Home"))
                {
                _fhirResourceRepository.Patient.Telecom.Add(new ContactPoint(ContactPoint.ContactPointSystem.Phone, ContactPoint.ContactPointUse.Home, "01234567891"));
                };

            if (Use.Contains("Work"))
            {
                _fhirResourceRepository.Patient.Telecom.Add(new ContactPoint(ContactPoint.ContactPointSystem.Phone, ContactPoint.ContactPointUse.Work, "01234567891"));
            };

            if (Use.Contains("Mobile"))
            {
                _fhirResourceRepository.Patient.Telecom.Add(new ContactPoint(ContactPoint.ContactPointSystem.Phone, ContactPoint.ContactPointUse.Mobile, "01234567891"));
            };

            if (Use.Contains("Temp"))
            {
                _fhirResourceRepository.Patient.Telecom.Add(new ContactPoint(ContactPoint.ContactPointSystem.Phone, ContactPoint.ContactPointUse.Temp, "01234567891"));
            };
            if (Use.Contains("Email"))
            {
                _fhirResourceRepository.Patient.Telecom.Add(new ContactPoint(ContactPoint.ContactPointSystem.Email, ContactPoint.ContactPointUse.Temp, "abc.d@mail.com"));
            };
        }

        [Given(@"I add a Telecom element use Mobile to the Stored Patient")]
        public void AddATelecomElementUseMobileToStoredPatient(string Use)
        {
            if (Use.Contains("Home"))
            {
                _fhirResourceRepository.Patient.Telecom.Add(new ContactPoint(ContactPoint.ContactPointSystem.Phone, ContactPoint.ContactPointUse.Home, "01234567891"));
            };

            if (Use.Contains("Work"))
            {
                _fhirResourceRepository.Patient.Telecom.Add(new ContactPoint(ContactPoint.ContactPointSystem.Phone, ContactPoint.ContactPointUse.Work, "01234567891"));
            };

            if (Use.Contains("Mobile"))
            {
                _fhirResourceRepository.Patient.Telecom.Add(new ContactPoint(ContactPoint.ContactPointSystem.Phone, ContactPoint.ContactPointUse.Mobile, "01234567891"));
            };

            if (Use.Contains("Temp"))
            {
                _fhirResourceRepository.Patient.Telecom.Add(new ContactPoint(ContactPoint.ContactPointSystem.Phone, ContactPoint.ContactPointUse.Temp, "01234567891"));
            };
            if (Use.Contains("Email"))
            {
                _fhirResourceRepository.Patient.Telecom.Add(new ContactPoint(ContactPoint.ContactPointSystem.Email, ContactPoint.ContactPointUse.Temp, "abc.d@mail.com"));
            };
        }
        [Given(@"I add a Telecom Home element to the Stored Patient")]
        public void AddATelecomHomeElementToStoredPatient()
        {
            _fhirResourceRepository.Patient.Telecom.Add(new ContactPoint(ContactPoint.ContactPointSystem.Phone, ContactPoint.ContactPointUse.Home, "01234567891"));
        }

        [Given(@"I add a Telecom Work element to the Stored Patient")]
        public void AddATelecomWorkElementToStoredPatient()
        {
            _fhirResourceRepository.Patient.Telecom.Add(new ContactPoint(ContactPoint.ContactPointSystem.Phone, ContactPoint.ContactPointUse.Work, "01132678901"));
        }
        [Given(@"I add a Telecom Mobile element to the Stored Patient")]
        public void AddATelecomMobileElementToStoredPatient()
        {
            _fhirResourceRepository.Patient.Telecom.Add(new ContactPoint(ContactPoint.ContactPointSystem.Phone, ContactPoint.ContactPointUse.Mobile, "0776789012"));
        }
        [Given(@"I add a Telecom Temp element to the Stored Patient")]
        public void AddATelecomTempElementToStoredPatient()
        {
            _fhirResourceRepository.Patient.Telecom.Add(new ContactPoint(ContactPoint.ContactPointSystem.Phone, ContactPoint.ContactPointUse.Temp, "01234567891"));
        }
        [Given(@"I add Multiple Telecom element to the Stored Patient")]
        public void AddMultipleTelecomElementToStoredPatient()
        {
            _fhirResourceRepository.Patient.Telecom.Add(new ContactPoint(ContactPoint.ContactPointSystem.Phone, ContactPoint.ContactPointUse.Work, "01132678901"));
        }
        [Given(@"I add Email element to the Stored Patient")]
        public void AddEmailElementToStoredPatient()
        {
            _fhirResourceRepository.Patient.Telecom.Add(new ContactPoint(ContactPoint.ContactPointSystem.Email, ContactPoint.ContactPointUse.Temp, "abc.d@mail.com"));
        }

        [Given(@"I add nhsCommunication extension to the Stored Patient")]
        public void AddnhsCommunicationExtensionToStoredPatient()
        {
            var coding = new Coding
            {
                System = "https://fhir.nhs.uk/STU3/CodeSystem/CareConnect-HumanLanguage-1",
                Code = "de",
                Display = "German"
            };

            var codableConcept = new CodeableConcept();
            codableConcept.Coding.Add(coding);

            var subExtension = new Extension
            {
                Url = "language",
                Value = codableConcept
            };

            var nhscextension = new Extension
            {
                Url = "https://fhir.nhs.uk/STU3/StructureDefinition/Extension-CareConnect-GPC-NHSCommunication-1",
            };

            nhscextension.Extension.Add(subExtension);

            var ir = new FhirBoolean(true);

            var irextension = new Extension
            {
                Url = "interpreterRequired",
                Value = ir
            };


            nhscextension.Extension.Add(irextension);
            _fhirResourceRepository.Patient.Extension.Add(nhscextension);

        }

        [Given(@"I add a Telecom element without temp to the Stored Patient")]
        public void AddATelecomElementWithoutTempToStoredPatient()
        {
            _fhirResourceRepository.Patient.Telecom.Add(new ContactPoint(ContactPoint.ContactPointSystem.Phone, ContactPoint.ContactPointUse.Work, "01234567891"));
        }

        [Then(@"the Patient Nhs Number Identifer should be valid")]
        public void ThePatientNhsNumberIdentiferShouldBeValid()
        {
            var storedPatient = _fhirResourceRepository.Patient;
            var storedPatientNhsNumber = storedPatient
                .Identifier
                .First(identifier => identifier.System == FhirConst.IdentifierSystems.kNHSNumber)
                .Value;

            Patients.ForEach(patient =>
            {
                var nhsNumberIdentifiers = patient
                    .Identifier
                    .Where(identifier => identifier.System == FhirConst.IdentifierSystems.kNHSNumber)
                    .ToList();

                nhsNumberIdentifiers.Count.ShouldBe(1, "The returned Patient Resource should contain a single NHS Number identifier");

                var nhsNumberIdentifier = nhsNumberIdentifiers.First();

                nhsNumberIdentifier.Value.ShouldNotBeNullOrEmpty("The NHS Number identifier must have a value element.");
                nhsNumberIdentifier.Value.ShouldBe(storedPatientNhsNumber, "The returned NHS Number does not match the sent NHS Number");

                var numberExtensions = nhsNumberIdentifier.Extension.Where(nne => nne.Url.Equals(FhirConst.StructureDefinitionSystems.kExtCcGpcNhsNumVerification)).ToList();

                numberExtensions.Count().ShouldBe(1,$"There can only be one extension on the NHS Number Identifer with a URL of {FhirConst.StructureDefinitionSystems.kExtCcGpcNhsNumVerification}");

                ValidateCodeConceptExtension(numberExtensions.First(), FhirConst.ValueSetSystems.kVsNhsNumVerification);

            });
        }

        [Then(@"the Patient should has a correct Address")]
        public void ThePatientShouldHasACorrectAddress()
        {
            var storedPatient = _fhirResourceRepository.Patient;
            var storedPatientAddress = storedPatient
                .Address
                .First().ToFhirJson();

            var responsePatientAddress = _httpContext.FhirResponse.Patients.First().Address.First().ToFhirJson();                

            responsePatientAddress.Equals(storedPatientAddress);
        }

        [Then(@"the Patient should has a correct Telecom")]
        public void ThePatientShouldHasACorrectTelecom()
        {
            var storedPatient = _fhirResourceRepository.Patient;
            var storedPatientTelecom = storedPatient
                .Telecom
                .First().ToFhirJson();

            var responsePatientTelecom = _httpContext.FhirResponse.Patients.First().Telecom.First().ToFhirJson();

            responsePatientTelecom.Equals(storedPatientTelecom);
        }

        [Then(@"the Patient should has a Telecom error")]
        public void ThePatientShouldHasTelecomError()
        {
            var errorMessage = ((OperationOutcome)_httpContext.FhirResponse.Resource).Issue.First().Details.Text;

            errorMessage.ShouldBe("The telecom use must be set to temp.", "The telecom use must be set to temp.");

        }

        [Then(@"the Patient should has a Address error")]
        public void ThePatientShouldHasAddressError()
        {
            var errorMessage = ((OperationOutcome)_httpContext.FhirResponse.Resource).Issue.First().Details.Text;

            errorMessage.ShouldBe("The address use must be set to temp.", "The address use must be set to temp.");

        }


        [Then(@"the Patient Registration Details Extension should be valid")]
        public void ThePatientRegistrationDetailsExtensioShouldBeValid()
        {
            Patients.ForEach(patient =>
            {
                var registrationDetailsExtensions = patient.Extension.Where(extension => extension.Url.Equals(FhirConst.StructureDefinitionSystems.kExtCcGpcRegDetails)).ToList();

                registrationDetailsExtensions.Count.ShouldBe(1, "Incorrect number of registration details extension have been returned. This should be 1.");

                var regDetailsExtension = registrationDetailsExtensions.First();
                var regExtensions = regDetailsExtension.Extension;

                ValidatePatientRegistrationType(regExtensions);
                ValidatePatientRegistrationStatus(regExtensions);
                ValidatePatientRegistrationPeriod(regExtensions);
                ValidatePatientPreferredBranchSurgery(regExtensions);

            });
        }

        private void ValidatePatientRegistrationType(List<Extension> extList)
        {

            var extensions = extList.Where(extension => extension.Url.Equals(FhirConst.StructureDefinitionSystems.kCCExtRegistrationType)).ToList();

            extensions.Count.ShouldBeLessThanOrEqualTo(1, "The patient resource should contain a registration type extension.");

            if (extensions.Any())
            {

                var codeList = ValueSetCache.Get(FhirConst.ValueSetSystems.kVsGpcRegistrationType).WithComposeIncludes().ToList();

                extensions.ForEach(registrationTypeExtension =>
                {
                    registrationTypeExtension.Value.ShouldNotBeNull("The registration type extension should have a value element.");
                    registrationTypeExtension.Value.ShouldBeOfType<CodeableConcept>("The registration type extension should be a CodeableConcept.");

                    var concept = (CodeableConcept)registrationTypeExtension.Value;

                    concept.Coding.ForEach(code =>
                    {
                        ShouldBeSingleCodingWhichIsInCodeList(code, codeList);
                    });
                });
            }
        }

        private void ValidatePatientRegistrationStatus(List<Extension> extList)
        {
            var extensions = extList.Where(extension => extension.Url.Equals(FhirConst.StructureDefinitionSystems.kCCExtRegistrationStatus)).ToList();

            extensions.Count.ShouldBe(0,"The patient resource should NOT contain a registration status extension.");

        }

        private void ValidatePatientRegistrationPeriod(List<Extension> extList)
        {
            var extensions = extList.Where(extension => extension.Url.Equals(FhirConst.StructureDefinitionSystems.kCCExtRegistrationPeriod)).ToList();

            extensions.Count.ShouldBeLessThanOrEqualTo(1, "The patient resource should contain a maximum of 1 registration period extension.");

            extensions.ForEach(registrationPeriodExtension =>
            {
                registrationPeriodExtension.Value.ShouldNotBeNull("The registration period extension should have a value element.");
                registrationPeriodExtension.Value.ShouldBeOfType<Period>("The registration period extension should be a Period.");
            });
        }

        private void ValidatePatientPreferredBranchSurgery(List<Extension> extList)
        {
            var extensions = extList.Where(extension => extension.Url.Equals(FhirConst.StructureDefinitionSystems.kCCExtPreferredBranchSurgery)).ToList();

            extensions.Count.ShouldBeLessThanOrEqualTo(1, "The patient resource should contain a maximum of 1 Preferred Branch Surgery extension.");

            extensions.ForEach(preferredBranchSurgeryExtension =>
            {
                preferredBranchSurgeryExtension.Value.ShouldNotBeNull("The Preferred Branch Surgery extension should have a value element.");
                preferredBranchSurgeryExtension.Value.ShouldBeOfType<ResourceReference>("The Preferred Branch Surgery extension should be a Period.");

                var reference = (ResourceReference)preferredBranchSurgeryExtension.Value;
                ValidateReferenceRequest(reference.Reference, GpConnectInteraction.LocationRead);
            });
        }

        [Then(@"the Patient Demographics should match the Stored Patient")]
        public void ThePatientDemographicsShouldMatchTheStoredPatient()
        {
            var storedPatient = _fhirResourceRepository.Patient;

            var storedPatientFamilyName = storedPatient
                .Name
                .First(x => x.Use == HumanName.NameUse.Official)
                .Family;

            Patients.ForEach(patient =>
            {
                patient.BirthDate.ShouldNotBeNull("The returned patient resource should contain a birthDate element.");
                patient.BirthDate.ShouldBe(storedPatient.BirthDate, "The returned patient DOB does not match the creted patient DOB");

                if (storedPatient.Gender != null)
                {
                    patient.Gender.ShouldBe(storedPatient.Gender, "The returned patient gender does not match the creted patient gender");
                }

                patient.Name.Count.ShouldBeGreaterThanOrEqualTo(1, "There should be at least one name element within the returned patient resource");

                var activeOfficialNames = patient.Name.Where(IsActiveOfficialName).ToList();

                var storedPatientActiveOfficialName = storedPatient.Name.Where(IsActiveOfficialName).First();
                var firstNamePatientReturned = patient.Name.Where(IsActiveOfficialName).First();

                activeOfficialNames.Count.ShouldBe(1, $"There should be a single Active Patient Name with a Use of Official, but found {activeOfficialNames.Count}.");
             
                var activeOfficialName = activeOfficialNames.First();

                //Given
              
                foreach (var given in storedPatientActiveOfficialName.Given)
                {
                    firstNamePatientReturned.Given.ShouldContain(given);
                }

                //Family
                activeOfficialName.Family.ShouldNotBeNull("There should be a family name in the returned patient resource.");
                activeOfficialName.Family.ShouldNotBeNullOrEmpty("The returned Patient Resource should contain a single family name");
                activeOfficialName.Family.ShouldBe(storedPatientFamilyName, "Returned patient family name does not match created patient family name", StringCompareShould.IgnoreCase);

            });

        }

        [Then(@"the Patient Optional Elements should be valid")]
        public void ThePatientOptionalElementsShouldBeValid()
        {
            Patients.ForEach(patient =>
            {
                //Would preferably check language as it has a binding strength of Required
                patient.Language?.IsLanguageFormat().ShouldBe(true, "A Patient Language has been provided but it does not conform to the required format. See: http://tools.ietf.org/html/bcp47");

                // EXTENSIONS
                var extensions = patient.Extension;
                ValidateMaxSingleCodeConceptExtension(extensions, FhirConst.StructureDefinitionSystems.kCCExtEthnicCategory, FhirConst.ValueSetSystems.kVsEthnicCategory);
                ValidateSingleExtension(extensions, FhirConst.StructureDefinitionSystems.kCcExtReligiousAffiliation); //Would preferably check codeable concept as it has a binding strength of Required
                ValidateSingleBooleanExtension(extensions, FhirConst.StructureDefinitionSystems.kCCExtPatientCadaver);
                ValidateMaxSingleCodeConceptExtension(extensions, FhirConst.StructureDefinitionSystems.kCCExtResidentialStatus, FhirConst.ValueSetSystems.kVsResidentialStatus);
                ValidateMaxSingleCodeConceptExtension(extensions, FhirConst.StructureDefinitionSystems.kCCExtTreatmentCategory, FhirConst.ValueSetSystems.kVsTreatmentCategory);
                ValidateNhsCommunicationExtension(extensions);


                //GENDER
                patient.Gender?.ShouldBeOfType<AdministrativeGender>();

                //NAMES
                patient.Name.ForEach(on =>
                {
                    on.Use.ShouldNotBeNull();
                    on.Use.ShouldBeOfType<HumanName.NameUse>();
                });

                //TELECOM
                ValidateTelecom(patient.Telecom, "Patient Telecom", true);

                //ADDRESS
                patient.Address.ForEach(add => ValidateAddress(add, "Patient Address"));

                //MARITAL STATUS
                if (patient.MaritalStatus != null)
                {
                    patient.MaritalStatus.Coding.Count.ShouldBe(1);
                    var vset = ValueSetCache.Get(FhirConst.ValueSetSystems.kVsMaritalStatus).WithComposeIncludes().ToList();
                    vset.AddRange(ValueSetCache.Get(FhirConst.ValueSetSystems.kVsNullFlavour).WithComposeIncludes().ToList());
                    ShouldBeSingleCodingWhichIsInCodeList(patient.MaritalStatus.Coding.First(), vset);
                }

                //CONTACT
                patient.Contact.ForEach(ValidateContact);

            });

        }
		
// git hub ref 153
// RMB 18/12/2018

	[Then(@"the Patient NhsCommunicationExtension should be valid")]
        public void ThePatientNhsCommunicationExtensionShouldBeValid()
        {
            Patients.ForEach(patient =>
            {
                var extensions = patient.Extension;

                ValidateRegPatientNhsCommunicationExtension(extensions);
            });
        }		

        [Given(@"I get the next Patient to register and store it")]
        public void GetTheNextPatientToRegisterAndStoreIt()
        {
            //Mimic PDS Trace
            var registerPatients = GlobalContext.RegisterPatients.OrderBy(a => Guid.NewGuid());

            foreach (var registerPatient in registerPatients.Where(rp => !rp.IsRegistered))
            {
                _patientSteps.GetThePatientForPatientNhsNumber(registerPatient.SPINE_NHS_NUMBER);

                var entries = _httpContext.FhirResponse.Entries;

                if (entries.Any())
                {
                    registerPatient.IsRegistered = true;
                }
                else
                {
                    var patientToRegister = new DefaultRegisterPatientBuilder(registerPatient).BuildPatient();

                    _fhirResourceRepository.Patient = patientToRegister;

                    return;
                }
            }
        }

        [Then(@"I store the patient in the register patient resource format")]
        public void GivenIStoreThePatientInTheRegisterPatientResourceFormat()
        {
            Patients.Count.ShouldBeGreaterThanOrEqualTo(1, "No patients were returned for the patient search.");

            var patient = Patients.First();

            var registerPatient = new Patient();

            var identifier = patient.Identifier.FirstOrDefault(x => x.System == FhirConst.IdentifierSystems.kNHSNumber);

            if (identifier != null)
            {
                registerPatient.Identifier.Add(new Identifier(identifier.System, identifier.Value));
            }

            var name = patient.Name.First();

            if (name != null)
            {
                registerPatient.Name.Add(name);
            }

            registerPatient.Gender = patient.Gender ?? AdministrativeGender.Unknown;
            registerPatient.BirthDateElement = patient.BirthDateElement ?? new Date();

            var patientMeta = new Meta();
            {
                IEnumerable<string> MetaProfile = new string[] { FhirConst.StructureDefinitionSystems.kPatient };
                patientMeta.Profile = MetaProfile;
            }
            registerPatient.Meta = patientMeta;

            registerPatient.Identifier = new List<Identifier>();
            var nhsNumber = GlobalContext.PatientNhsNumberMap["patient1"];

            var patientIdentifier = new Identifier(FhirConst.IdentifierSystems.kNHSNumber, nhsNumber);
            patientIdentifier.Extension.Add(new Extension
            {
                Url = FhirConst.StructureDefinitionSystems.kExtCcGpcNhsNumVerification,
                Value = new CodeableConcept(FhirConst.CodeSystems.kCcNhsNumVerification, "01", "Number present and verified", null)
            });

            registerPatient.Identifier.Add(patientIdentifier);

            _fhirResourceRepository.Patient = registerPatient;
        }

        [Given(@"I Set the Stored Patient Registration Details Extension")]
        public void GivenISetTheStoredPatientRegistrationDetailsExtension()
        {
            var extList = new List<Extension>
            {
                GetCodingExtension(FhirConst.StructureDefinitionSystems.kCCExtRegistrationStatus, FhirConst.CodeSystems.kCcGpcRegistrationStatus, "I", "Inactive")
            };

            var registrationDetails = new Extension
            {
                Url = FhirConst.StructureDefinitionSystems.kExtCcGpcRegDetails,
                Extension = extList
            };

            _fhirResourceRepository.Patient.Extension.Add(registrationDetails);
        }

        private void ValidateReferenceRequest(string reference, GpConnectInteraction interaction)
        {
            if (reference.StartsWith("Location/"))
            {
                var location = _httpSteps.GetResourceForRelativeUrl(GpConnectInteraction.LocationRead, reference);

                location.ShouldNotBeNull(
                    $"The Reference {reference} returned a null Location.");
                location.GetType()
                    .ShouldBe(typeof(Location),
                        $"The reference {reference} returned a {location.GetType().ToString()}.");
            }
            else if (reference.StartsWith("Organization/"))
            {
                var organization = _httpSteps.GetResourceForRelativeUrl(GpConnectInteraction.OrganizationRead, reference);

                organization.ShouldNotBeNull(
                    $"The Reference {reference} returned a null Location.");
                organization.GetType()
                    .ShouldBe(typeof(Location),
                        $"The reference {reference} returned a {organization.GetType().ToString()}.");
            }
            else if (reference.StartsWith("Practitioner/"))
            {
                var practitioner = _httpSteps.GetResourceForRelativeUrl(GpConnectInteraction.PractitionerRead, reference);

                practitioner.ShouldNotBeNull(
                    $"The Reference {reference} returned a null Location.");
                practitioner.GetType()
                    .ShouldBe(typeof(Location),
                        $"The reference {reference} returned a {practitioner.GetType().ToString()}.");
            }
        }

        private void StoreTheOrganization()
        {
            var organization = _httpContext.FhirResponse.Organizations.FirstOrDefault();
            if (organization != null)
            {
                _httpContext.HttpRequestConfiguration.GetRequestId = organization.Id;
                _fhirResourceRepository.Organization = organization;
            }
        }

        private void StoreThePractitioner()
        {
            var practitioner = _httpContext.FhirResponse.Practitioners.FirstOrDefault();
            if (practitioner != null)
            {
                _httpContext.HttpRequestConfiguration.GetRequestId = practitioner.Id;
                _fhirResourceRepository.Practitioner = practitioner;
            }
        }

        private void StoreTheLocation()
        {
            var location = _httpContext.FhirResponse.Locations.FirstOrDefault();
            if (location != null)
            {
                _httpContext.HttpRequestConfiguration.GetRequestId = location.Id;
                _fhirResourceRepository.Location = location;
            }
        }

        private void ValidateContact(Patient.ContactComponent contact)
        {
            if (contact != null)
            {
                contact.Extension.ForEach(ext => ext.Url.ShouldNotBeNullOrEmpty("Patient Contact has an invalid extension. Extensions must have a URL element."));

                var contactRelationship = contact.Relationship;

                contactRelationship?.ForEach(rel =>
                {
                    rel.Extension.ForEach(ext => ext.Url.ShouldNotBeNullOrEmpty("Patient Contact Relationship Code has an invalid extension. Extensions must have a URL element."));

                    rel.Coding.ForEach(cd =>
                    {
                        cd.Extension.ForEach(ext => ext.Url.ShouldNotBeNullOrEmpty("Patient Contact Relationship has an invalid extension. Extensions must have a URL element."));
                        cd.System.ShouldNotBeNullOrEmpty();
                        cd.Code.ShouldNotBeNullOrEmpty();
                        cd.Display.ShouldNotBeNullOrEmpty();
                    });
                });

                var contactName = contact.Name;

                contactName.ShouldNotBeNull();
                contactName.Use.ShouldNotBeNull();
                contactName.Use.ShouldBeOfType<HumanName.NameUse>($"Patient Contact Name Use is not a valid value within the value set {FhirConst.CodeSystems.kNameUse}");
                contactName.Family.Count().ShouldBeLessThanOrEqualTo(1, "Patient Contact Name Family Element should contain a maximum of 1.");

                ValidateTelecom(contact.Telecom, "Patient Contact Telecom");

                ValidateAddress(contact.Address, "Patient Contact Address");

                contact.Gender?.ShouldBeOfType<AdministrativeGender>();

                if (contact.Organization != null)
                {
                    contact.Organization.Reference.ShouldNotBeNullOrEmpty();
                    ValidateReferenceRequest(contact.Organization.Reference, GpConnectInteraction.OrganizationRead);
                }

            }
        }

        private void ValidateNhsCommunicationExtension(List<Extension> extensions)
        {
            var exts = extensions.Where(ece => ece.Url.Equals(FhirConst.StructureDefinitionSystems.kCCExtNhsCommunication)).ToList();
            exts.Count.ShouldBeLessThanOrEqualTo(1);

            if (exts.Any())
            {
                var ext = exts.First();

                var subExtensions = ext.Extension;

                ValidateExactSingleCodeConceptExtension(subExtensions, FhirConst.StructureDefinitionSystems.kCCExtCommLanguage, FhirConst.ValueSetSystems.kVsHumanLanguage);
                ValidateSingleBooleanExtension(subExtensions, FhirConst.StructureDefinitionSystems.kCCExtCommPreferred);
                ValidateMaxSingleCodeConceptExtension(subExtensions, FhirConst.StructureDefinitionSystems.kCCExtCommModeOfCommunication, FhirConst.ValueSetSystems.kVsLanguageAbilityMode);
                ValidateMaxSingleCodeConceptExtension(subExtensions, FhirConst.StructureDefinitionSystems.kCCExtCommCommProficiency, FhirConst.ValueSetSystems.kVsLanguageAbilityProficiency);
                ValidateSingleBooleanExtension(subExtensions, FhirConst.StructureDefinitionSystems.kCCExtCommInterpreterRequired);
            }
        }

        private void ValidateExactSingleCodeConceptExtension(List<Extension> extensions, string defUri, string vsetUri)
        {
            var exts = extensions.Where(ece => ece.Url.Equals(defUri)).ToList();
            exts.Count.ShouldBe(1);

            ValidateCodeConceptExtension(exts.First(), vsetUri);

        }

        private void ValidateMaxSingleCodeConceptExtension(List<Extension> extensions, string defUri, string vsetUri)
        {

            var exts = extensions.Where(
                    ece => ece.Url.Equals(defUri)).ToList();
            exts.Count.ShouldBeLessThanOrEqualTo(1);

            if (exts.Any())
            {
                ValidateCodeConceptExtension(exts.First(), vsetUri);
            }

        }

        private void ValidateSingleBooleanExtension(List<Extension> extensions, string defUri)
        {

            var exts = extensions.Where(
                ece => ece.Url.Equals(defUri)).ToList();
            exts.Count.ShouldBeLessThanOrEqualTo(1);

            if (exts.Any())
            {
                var val = exts.First();
                val.Value.ShouldNotBeNull();
                val.Value.ShouldBeOfType<FhirBoolean>();
            }

        }

        private void ValidateSingleExtension(List<Extension> extensions, string defUri)
        {

            var exts = extensions.Where(
                ece => ece.Url.Equals(defUri)).ToList();
            exts.Count.ShouldBeLessThanOrEqualTo(1);

            if (exts.Any())
            {
                var val = exts.First();
                val.Value.ShouldNotBeNull();
            }

        }
		
// git hub ref 153
// RMB 18/12/2018
       private void ValidateRegPatientNhsCommunicationExtension(List<Extension> extensions)
        {
            var exts = extensions.Where(ece => ece.Url.Equals(FhirConst.StructureDefinitionSystems.kCCExtNhsCommunication)).ToList();
            exts.Count.ShouldBe(1, "The patient resource should contain a nhsCommunication extension.");

            exts.ForEach(nhsCommunicationExtension =>
            {
                var val = nhsCommunicationExtension;
            });
            
        }		


        private static Extension GetCodingExtension(string extensionUrl, string codingUrl, string code, string display)
        {
            var coding = new Coding
            {
                Code = code,
                Display = display,
                System = codingUrl
            };

            var reason = new CodeableConcept();
            reason.Coding.Add(coding);

            return new Extension
            {
                Url = extensionUrl,
                Value = reason
            };
        }

        private static bool IsActiveOfficialName(HumanName name)
        {
            var endDateIsValid = name.Period?.End == null || DateTime.Parse(name.Period.End) >= DateTime.UtcNow;
            var startDateIsValid = name.Period?.Start == null || DateTime.Parse(name.Period.Start) <= DateTime.UtcNow;

            return name.Use == HumanName.NameUse.Official && endDateIsValid && startDateIsValid;
        }
    }
}
