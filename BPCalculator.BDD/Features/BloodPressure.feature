Feature: Blood Pressure Category
  In order to know my health status
  As a user
  I want to be told my blood pressure category based on my systolic and diastolic readings

Scenario Outline: Determine Blood Pressure Category
  Given the systolic pressure is <systolic>
  And the diastolic pressure is <diastolic>
  When the blood pressure category is calculated
  Then the category should be <category>

  Examples:
    | systolic | diastolic | category |
    | 80       | 50        | Low      |
    | 110      | 70        | Ideal    |
    | 130      | 85        | PreHigh  |
    | 150      | 95        | High     |
