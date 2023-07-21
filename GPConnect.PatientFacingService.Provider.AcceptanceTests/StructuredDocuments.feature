@Structured @StructuredDocuments @PFS-Full-Pack
Feature: Documents

# These Tests are only Testing this Structured Area in isolation and Not with other Areas or Combinations of Include Parameters
# Tests around Multiple Structured Areas in one Request are tested in the MultipleRequests Feature

##########################################
#Search For Documents Tests
##########################################

Scenario: Search for Documents on a Patient with Documents
	Given I configure the default "PatientSearch" request
		And I set the Find Patient ID in the request to "patient2"
	When I make the "PatientSearch" request
		Then the response status code should indicate success
		Given I store the Patient
	Given I configure the default "DocumentsSearch" request
		And I set the required parameters for a Documents Search call
	When I make the "DocumentsSearch" request
		Then the response status code should indicate success
		And the response should be a Bundle resource of type "searchset"
		And The Bundle id should match the XRequestID
		And I Check the returned DocumentReference is Valid
		And I Check the returned DocumentReference Do Not Include Not In Use Fields



Scenario: Search for Documents on a Patient with Documents Over 5MB
	Given I configure the default "PatientSearch" request
		And I set the Find Patient ID in the request to "patient4"
	When I make the "PatientSearch" request
		Then the response status code should indicate success
		Given I store the Patient
	Given I configure the default "DocumentsSearch" request
		And I set the required parameters for a Documents Search call
	When I make the "DocumentsSearch" request
		Then the response status code should indicate success
		And the response should be a Bundle resource of type "searchset"
		And The Bundle id should match the XRequestID
		And I Check the returned DocumentReference is Valid
		And I Check the returned DocumentReference Content Doesnot Contain URL for over 5MB Attachment

Scenario: Search for Documents on a Patient with NO Documents
	Given I configure the default "PatientSearch" request
		And I set the Find Patient ID in the request to "patient3"
	When I make the "PatientSearch" request
		Then the response status code should indicate success
		Given I store the Patient
	Given I configure the default "DocumentsSearch" request
		And I set the required parameters for a Documents Search call
	When I make the "DocumentsSearch" request
		Then the response status code should indicate success
		And the response should be a Bundle resource of type "searchset"
		And The Bundle id should match the XRequestID
		And The Bundle should contain NO Documents
		
Scenario: Search for Documents without Mandatory include Params expect fail
	Given I configure the default "PatientSearch" request
		And I set the Find Patient ID in the request to "patient2"
	When I make the "PatientSearch" request
		Then the response status code should indicate success
		Given I store the Patient
	Given I configure the default "DocumentsSearch" request
	When I make the "DocumentsSearch" request
		Then the response status code should be "422"
		And the response should be a OperationOutcome resource with error code "INVALID_PARAMETER"

Scenario: Search for Documents using author parameter
	Given I configure the default "PatientSearch" request
		And I set the Find Patient ID in the request to "patient2"
	When I make the "PatientSearch" request
		Then the response status code should indicate success
		Given I store the Patient
	Given I configure the default "DocumentsSearch" request
		And I set the required parameters for a Documents Search call
		And I set the author parameters for a Documents Search call to "ORG1"
	When I make the "DocumentsSearch" request
		Then the response status code should indicate success
		And the response should be a Bundle resource of type "searchset"
		And The Bundle id should match the XRequestID
		And I Check the returned DocumentReference is Valid
		And I Check the returned DocumentReference Do Not Include Not In Use Fields

Scenario: Search for Documents using author parameter but with invalid identifier
	Given I configure the default "PatientSearch" request
		And I set the Find Patient ID in the request to "patient2"
	When I make the "PatientSearch" request
		Then the response status code should indicate success
		Given I store the Patient
	Given I configure the default "DocumentsSearch" request
		And I set the required parameters for a Documents Search call
		And I set the author parameters with an invalid identifier for a Documents Search call to "ORG1"
	When I make the "DocumentsSearch" request
		Then the response status code should be "422"
		And the response should be a OperationOutcome resource with error code "INVALID_PARAMETER"

Scenario: Search for Patient Documents created within a last 365 days
	Given I configure the default "PatientSearch" request
		And I set the Find Patient ID in the request to "patient2"
	When I make the "PatientSearch" request
		Then the response status code should indicate success
		Given I store the Patient
	Given I configure the default "DocumentsSearch" request
		And I set the required parameters for a Documents Search call
		Then I set the documents search parameters le to today and ge to 365 days ago
	When I make the "DocumentsSearch" request
		Then the response status code should indicate success
		And the response should be a Bundle resource of type "searchset"
		And The Bundle id should match the XRequestID

Scenario Outline: Search for Patient Documents created less than a date
	Given I configure the default "PatientSearch" request
		And I set the Find Patient ID in the request to "patient2"
	When I make the "PatientSearch" request
		Then the response status code should indicate success
		Given I store the Patient
	Given I configure the default "DocumentsSearch" request
		And I set the required parameters for a Documents Search call		
		Then I set the created search parameter to less than "<Days>" days ago
	When I make the "DocumentsSearch" request
		Then the response status code should indicate success
		And the response should be a Bundle resource of type "searchset"
		And The Bundle id should match the XRequestID
Examples:
		| Days	|
		| 2		|

Scenario Outline: Search for Patient Documents created greater than a date
	Given I configure the default "PatientSearch" request
		And I set the Find Patient ID in the request to "patient2"
	When I make the "PatientSearch" request
		Then the response status code should indicate success
		Given I store the Patient
	Given I configure the default "DocumentsSearch" request
		And I set the required parameters for a Documents Search call		
		Then I set the created search parameter to greater than "<Days>" days ago
	When I make the "DocumentsSearch" request
		Then the response status code should indicate success
		And the response should be a Bundle resource of type "searchset"
		And The Bundle id should match the XRequestID
Examples:
		| Days |
		| 365  |

Scenario: Search for Documents with an invalid parameter
	Given I configure the default "PatientSearch" request
		And I set the Find Patient ID in the request to "patient2"
	When I make the "PatientSearch" request
		Then the response status code should indicate success
		Given I store the Patient
	Given I configure the default "DocumentsSearch" request
		And I set the required parameters for a Documents Search call
		And I set an invalid parameter for a Documents Search call
	When I make the "DocumentsSearch" request
		Then the response status code should be "422"
		And the response should be a OperationOutcome resource with error code "INVALID_PARAMETER"
	
Scenario: Search for Documents on a Patient that doesnt exist
	Given I configure the default "DocumentsSearch" request
		And I set the required parameters for a Documents Search call
		And I change the patient logical id to a non existent id
	When I make the "DocumentsSearch" request
		Then the response status code should be "404"
		And the response should be a OperationOutcome resource with error code "PATIENT_NOT_FOUND"

# SU:  skipping this test for now as SU need to confirm with program if Temporary patient test is needed. 04/07/2023
#Scenario: Search for Documents on a patient which exists on the system as a temporary patient
#	Given I get the next Patient to register and store it
#	Given I configure the default "RegisterPatient" request
#		And I add the Stored Patient as a parameter
#	When I make the "RegisterPatient" request
#	Then the response status code should indicate success
#		And the response should be a Bundle resource of type "searchset"
#		And the response bundle should contain a single Patient resource
#	Given I store the Patient
#	Given I configure the default "DocumentsSearch" request
#		And I set the required parameters for a Documents Search call
#	When I make the "DocumentsSearch" request
#	Then the response status code should be "404"
#		And the response should be a OperationOutcome resource with error code "PATIENT_NOT_FOUND"

Scenario: Search for Documents on a Patient with deceased date not older than 28 days
	Given I configure the default "PatientSearch" request
		And I set the Find Patient ID in the request to "patient18"
	When I make the "PatientSearch" request
		Then the response status code should indicate success
		And the response body should be FHIR JSON
		And the response should be a Bundle resource of type "searchset"
		And the response bundle contains a deceased date not older than allowed access period "28" days
		
##########################################
#Retrieve  Documents Tests
##########################################
	
Scenario: Retrieve a Document for Patient2
	Given I configure the default "PatientSearch" request
		And I set the Find Patient ID in the request to "patient2"
	When I make the "PatientSearch" request
		Then the response status code should indicate success
		Given I store the Patient
	Given I configure the default "DocumentsSearch" request
		And I set the required parameters for a Documents Search call
	When I make the "DocumentsSearch" request
		Then the response status code should indicate success
		And the response should be a Bundle resource of type "searchset"
		And I save a document url for retrieving later
	Given I configure the default "DocumentsRetrieve" request
		When I make the "DocumentsRetrieve" request
		Then the response status code should indicate success
		And I save the binary document from the retrieve
		And I Check the returned Binary Document is Valid
		And I Check the returned Binary Document Do Not Include Not In Use Fields


#Removed as URL should not be sent back for a doc over 5mb Test
#Scenario: Retrieve a Document Over 5mb for Patient4 expect Fail
#	Given I configure the default "DocumentsPatientSearch" request
#		And I add a Patient Identifier parameter with default System and Value "patient4"
#		When I make the "DocumentsPatientSearch" request
#		Then the response status code should indicate success
#		Given I store the Patient
#	Given I configure the default "DocumentsSearch" request
#		And I set the required parameters for a Documents Search call
#	When I make the "DocumentsSearch" request
#		Then the response status code should indicate success
#		And the response should be a Bundle resource of type "searchset"
#		And I save a document url for retrieving later
#	Given I configure the default "DocumentsRetrieve" request
#		When I make the "DocumentsRetrieve" request
#		Then the response status code should be "404"
#		And the response should be a OperationOutcome resource with error code "NO_RECORD_FOUND"
#		And I clear the saved document url
		
##########################################
#Documents Search/Find Patients Tests
##########################################
