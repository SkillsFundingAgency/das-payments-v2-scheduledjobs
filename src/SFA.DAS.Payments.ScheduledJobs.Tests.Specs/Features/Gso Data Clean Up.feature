Feature: Gso Data Clean Up

As a service owner
When a provider submits duplicate GSO short course payments for the same learner and course
Then only the most recent payment for that learner and course is retained

Scenario: Duplicate GSO short course payment for a learner and course
	Given a GSO short course payment has been recorded for a learner and course
	And a more recent GSO short course payment has been recorded for the same learner and course
	When the GSO audit data cleanup job is triggered
	Then the audit data related to the most recent GSO payment for that learner and course is retained
	And the audit data related to the superseded GSO payment is deleted

Scenario: Multiple superseded GSO short course payments for a learner and course
	Given a GSO short course payment has been recorded for a learner and course
	And the learner and course has 2 superseded GSO short course payments
	When the GSO audit data cleanup job is triggered
	Then the audit data related to the most recent GSO payment for that learner and course is retained
	And the audit data related to the superseded GSO payments is deleted

Scenario: Single GSO short course payment for a learner and course
	Given a GSO short course payment has been recorded for a learner and course
	When the GSO audit data cleanup job is triggered
	Then the audit data related to the most recent GSO payment for that learner and course is retained
