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
		And I set nhsnumber parameter for a Documents Search call to "patient2"
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
		And I set nhsnumber parameter for a Documents Search call to "patient4"
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
		And I set nhsnumber parameter for a Documents Search call to "patient3"
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
		And I set nhsnumber parameter for a Documents Search call to "patient2"
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
			And I set nhsnumber parameter for a Documents Search call to "patient2"
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
		And I set nhsnumber parameter for a Documents Search call to "patient2"
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
		And I set nhsnumber parameter for a Documents Search call to "patient2"
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
		And I set nhsnumber parameter for a Documents Search call to "patient2"
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
		And I set nhsnumber parameter for a Documents Search call to "patient2"
		And I set the required parameters for a Documents Search call
		And I set an invalid parameter for a Documents Search call
	When I make the "DocumentsSearch" request
		Then the response status code should be "422"
		And the response should be a OperationOutcome resource with error code "INVALID_PARAMETER"
	
Scenario: Search for Documents on a Patient that doesnt exist
	Given I configure the default "DocumentsSearch" request
		And I set the required parameters for a Documents Search call
		And I change the patient logical id to a non existent id
		And I set nhsnumber parameter for a Documents Search call to "invalidNHSnumber"
	When I make the "DocumentsSearch" request
		Then the response status code should be "404"
		And the response should be a OperationOutcome resource with error code "PATIENT_NOT_FOUND"

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
		And I set nhsnumber parameter for a Documents Search call to "patient2"
		And I set the required parameters for a Documents Search call
	When I make the "DocumentsSearch" request
		Then the response status code should indicate success
		And the response should be a Bundle resource of type "searchset"
		And I save a document url for retrieving later
	Given I configure the default "DocumentsRetrieve" request
	And I set nhsnumber parameter for a Documents Search call to "patient2"
		When I make the "DocumentsRetrieve" request
		Then the response status code should indicate success
		And I save the binary document from the retrieve
		And I Check the returned Binary Document is Valid
		And I Check the returned Binary Document Do Not Include Not In Use Fields
		
##########################################
#Documents Search/Find Patients Tests
##########################################
