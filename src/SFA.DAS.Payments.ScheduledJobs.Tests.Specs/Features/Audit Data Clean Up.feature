Feature: Audit Data Clean Up

As a service owner
When a training provider submits their learners to be paid for
And the training provider has already submitted in the current collection period
Then any previous submissions for that training provider are removed

Scenario: First submission in the collection period for training provider
	Given the training provider has submitted their learners to be paid for
	When the audit data cleanup job is triggered
	Then the audit data related to the current submission for that training provider is retained
	
Scenario: Subsequent submission in the collection period for training provider
	Given the training provider has submitted their learners to be paid for
	And the training provider has previously submitted their learners in the current collection period
	When the audit data cleanup job is triggered
	Then the audit data related to the previous submission is deleted
	And the audit data related to the current submission for that training provider is retained
	