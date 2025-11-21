Feature: Blood Pressure Categorization
  In order to help users understand their blood pressure
  As a clinician
  I want the system to categorize blood pressure readings correctly

  Scenario Outline: Categorize blood pressure
    Given a systolic reading of <systolic> and diastolic reading of <diastolic>
    When the blood pressure is evaluated
    Then the category should be <category>

    Examples:
      | systolic | diastolic | category |
      | 115      | 75        | Ideal    |
      | 85       | 55        | Low      |
      | 130      | 85        | PreHigh  |
      | 150      | 95        | High     |
