@patient @Find-Patient @PFS-Full-Pack
Feature: PatientSearch

Scenario: Returned patients should contain a logical identifier
	Given I configure the default "PatientSearch" request
		And I set the Find Patient ID in the request to "patient2"
	When I make the "PatientSearch" request
	Then the response status code should indicate success
		And the response should be a Bundle resource of type "searchset"
		And the response bundle should contain "1" entries
		And the Patient Id should be valid
		And the patient resource in the bundle should contain meta data profile and version id

Scenario: When a patient is not found on the provider system an empty bundle should be returned
	Given I configure the default "PatientSearch" request
		And I set the Find Patient ID in the request to "patientNotInSystem"
	When I make the "PatientSearch" request
	Then the response status code should indicate success
		And the response should be a Bundle resource of type "searchset"
		And the response bundle should contain "0" entries

Scenario: The response should be an error if nhs nuber is not provided
	Given I configure the default "PatientSearch" request
	When I make the "PatientSearch" request
	 Then the response status code should be "422"
		And the response should be a OperationOutcome resource with error code "INVALID_PARAMETER"

Scenario: The response should be an error if invalid nhs nuber is sent
	Given I configure the default "PatientSearch" request
		#And I add the parameter "identifier" with the value "https://fhir.nhs.uk/Id/nhs-number|"
		And I set the Find Patient ID in the request to "invalidNHSnumber"
	When I make the "PatientSearch" request
	Then the response status code should be "400"
		And the response should be a OperationOutcome resource with error code "INVALID_NHS_NUMBER"
	#Then the response status code should be "422"
	#	And the response should be a OperationOutcome resource with error code "INVALID_PARAMETER"

Scenario Outline: The patient search endpoint should accept the accept header
	Given I configure the default "PatientSearch" request
		And I set the Accept header to "<AcceptHeader>"
	When I make the "PatientSearch" request
	Then the response status code should indicate success
		And the response body should be FHIR <ResultFormat>
		And the response should be a Bundle resource of type "searchset"
		And the response bundle should contain "1" entries
		And the Patient Id should be valid
		And the Patient Identifiers should be valid for Patient "patient2"
	Examples:
		| AcceptHeader          | ResultFormat |
		| application/fhir+xml  | XML          |
		| application/fhir+json | JSON         |

#Scenario Outline: The patient search endpoint should accept the format parameter
#	 Given I configure the default "PatientSearch" request
#		And I add the parameter "_format" with the value "<FormatParam>"
#		And I set the Find Patient ID in the request to "patient2"
#	When I make the "PatientSearch" request
#	Then the response status code should indicate success
#		And the response body should be FHIR <ResultFormat>
#		And the response should be a Bundle resource of type "searchset"
#		And the response bundle should contain "1" entries
#		And the Patient Id should be valid
#		And the Patient Identifiers should be valid for Patient "patient2"
#	Examples:
#		| FormatParam           | ResultFormat |
#		| application/fhir+xml  | XML          |
#		| application/fhir+json | JSON         |

#Scenario Outline: The patient search endpoint should accept the format parameter after the identifier parameter
Scenario Outline: The patient search endpoint should accept the format parameter after the patient id
	 Given I configure the default "PatientSearch" request
		And I set the Accept header to "<AcceptHeader>"
		And I set the Find Patient ID in the request to "patient2"
	#	And I add a Patient Identifier parameter with default System and Value "patient2"
		And I add the parameter "_format" with the value "<FormatParam>"
	When I make the "PatientSearch" request
	Then the response status code should indicate success
		And the response body should be FHIR <ResultFormat>
		And the response should be a Bundle resource of type "searchset"
		And the response bundle should contain "1" entries
		And the Patient Id should be valid
		And the Patient Identifiers should be valid for Patient "patient2"
	Examples:
		| AcceptHeader          | FormatParam           | ResultFormat |
		| application/fhir+xml  | application/fhir+xml  | XML          |
		| application/fhir+json | application/fhir+xml  | XML          |
		| application/fhir+json | application/fhir+json | JSON         |
		| application/fhir+xml  | application/fhir+json | JSON         |

#Scenario Outline: The patient search endpoint should accept the format parameter before the identifier parameter
#Scenario Outline: The patient search endpoint should accept the format parameter before the patient id
#	Given I configure the default "PatientSearch" request
#		And I set the Accept header to "<AcceptHeader>"
#		And I add the parameter "_format" with the value "<FormatParam>"
#		And I set the Find Patient ID in the request to "patient2"
#	#	And I add a Patient Identifier parameter with default System and Value "patient2"
#	When I make the "PatientSearch" request
#	Then the response status code should indicate success
#		And the response body should be FHIR <ResultFormat>
#		And the response should be a Bundle resource of type "searchset"
#		And the response bundle should contain "1" entries
#		And the Patient Id should be valid
#		And the Patient Identifiers should be valid for Patient "patient2"
#	Examples:
#		| AcceptHeader          | FormatParam           | ResultFormat |
#		| application/fhir+xml  | application/fhir+xml  | XML          |
#		| application/fhir+json | application/fhir+xml  | XML          |
#		| application/fhir+json | application/fhir+json | JSON         |
#		| application/fhir+xml  | application/fhir+json | JSON         |

Scenario Outline: Patient resource should contain NHS number identifier returned as XML
	Given I configure the default "PatientSearch" request
		And I set the Accept header to "application/fhir+xml"
		And I set the Find Patient ID in the request to "<Patient>"
	When I make the "PatientSearch" request
	Then the response status code should indicate success
		And the response body should be FHIR XML
		And the response should be a Bundle resource of type "searchset"
		And the response bundle should contain "1" entries
		And the Patient Identifiers should be valid for Patient "<Patient>"
	Examples:
		| Patient  |
		| patient1 |
		| patient2 |
		| patient3 |

Scenario Outline: Patient search response conforms with the GPConnect specification
	Given I configure the default "PatientSearch" request
		And I set the Find Patient ID in the request to "<Patient>"
	When I make the "PatientSearch" request
	Then the response status code should indicate success
		And the response body should be FHIR JSON
		And the response should be a Bundle resource of type "searchset"
		And the response bundle should contain "1" entries
		And the Patient Name should be valid
		And the Patient Use should be valid
		And the Patient Communication should be valid
		And the Patient Contact should be valid
		And the Patient MultipleBirth should be valid
		And the Patient MaritalStatus should be valid
		And the Patient Deceased should be valid
		And the Patient Telecom should be valid
		# spoke Ed and he mentined we might not need these tests.
		#And the Patient ManagingOrganization Organization should be valid and resolvable
		#And the Patient GeneralPractitioner Practitioner should be valid and resolvable
		And the Patient should exclude disallowed fields
		And the Patient Link should be valid and resolvable
		And the Patient Contact Telecom use should be valid
		And the Patient Not In Use should be valid

	Examples:
		| Patient   |
		| patient1  |
		| patient2  |
		| patient3  |
		| patient4  |
		| patient5  |
		| patient6  |

Scenario: Patient search response does not return deceased patient
	Given I configure the default "PatientSearch" request
		And I set the Find Patient ID in the request to "patient18"
	When I make the "PatientSearch" request
	Then the response status code should indicate success
	And the response body should be FHIR JSON
	And the response should be a Bundle resource of type "searchset"
	And the response bundle should contain "0" entries

Scenario Outline: System should return error if multiple patient ID are sent
	 Given I configure the default "PatientSearch" request
		And I set the Find Patient ID in the request to "<PatientOne>" and "<PatientTwo>"
	When I make the "PatientSearch" request
	Then the response status code should be "400"
		And the response should be a OperationOutcome resource with error code "BAD_REQUEST"
	Examples:
		| PatientOne | PatientTwo |
		| patient2   | patient2   |
		| patient1   | patient2   |
		| patient2   | patient1   |


Scenario: Patient search valid response check caching headers exist
	Given I configure the default "PatientSearch" request
		And I set the Find Patient ID in the request to "patient2"
	When I make the "PatientSearch" request
	Then the response status code should indicate success
		And the response should be a Bundle resource of type "searchset"
		And the response bundle should contain "1" entries
		And the Patient Id should be valid
		And the required cacheing headers should be present in the response

Scenario: Returned patients should contain a preferred branch
	Given I configure the default "PatientSearch" request
		And I set the Find Patient ID in the request to "patient2"
	When I make the "PatientSearch" request
	Then the response status code should indicate success
		And the response should be a Bundle resource of type "searchset"
		And the response bundle should contain "1" entries
		And the Patient RegistrationDetails should include preferredBranchSurgery

Scenario: When a patient on the provider system has sensitive flag
# github ref 103
# RMB 22/10/2018
	Given I configure the default "PatientSearch" request
		And I set the Find Patient ID in the request to "patient9"
	When I make the "PatientSearch" request
	Then the response status code should indicate success
		And the response should be a Bundle resource of type "searchset"
		And the response bundle should contain "0" entries

Scenario: When a patient on the provider system has inactive flag
# github ref 107 demonstrator 115
# RMB 22/10/2018
	Given I configure the default "PatientSearch" request
		And I set the Find Patient ID in the request to "patient21"
	When I make the "PatientSearch" request
	Then the response status code should indicate success
		And the response should be a Bundle resource of type "searchset"
		And the response bundle should contain "0" entries


	Scenario: No Consent Patient search gets a valid response
	Given I configure the default "PatientSearch" request
		And I set the Find Patient ID in the request to "patient15"
	When I make the "PatientSearch" request
	Then the response status code should indicate success
		And the response should be a Bundle resource of type "searchset"
		And the response bundle should contain "1" entries
		And the Patient Id should be valid
		And the required cacheing headers should be present in the response