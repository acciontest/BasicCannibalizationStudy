Feature: BasicCannibalizationStudy
	This sample app demonstrates a basic cannibalization analysis between two stores.

Background:
  Given alteryx running at" http://gallery.alteryx.com/"
  And I am logged in using "deepak.manoharan@accionlabs.com" and "P@ssw0rd"
  #And I publish the application "twitter tracker"
  #And I check if the application is "Valid"


Scenario Outline:run the app to calculate the distance
When I run the App "<app>" with <Potential Store Drivetime> and predefined Existing Store Drivetime
Then I see the  amount of cannibalized sales in the output "<amount>"

Examples: 
| app                         | Potential Store Drivetime | amount      |
| Basic Cannibalization Study | 5                         | 16259138 |