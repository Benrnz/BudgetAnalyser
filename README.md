[![benrnz MyGet Build Status](https://www.myget.org/BuildSource/Badge/benrnz?identifier=eaa804ca-ad35-41da-a864-eb490be52974)](https://www.myget.org/)
## Budget Analyser ##
=====================


What is it?
-----------
It's a simple, low time investment budgeting app. It helps you keep to a monthly budget, see how much you can spend during the month and analyse past data to improve your budget.

Why?
----
Some budgeting apps are complicated or time consuming and aren't very visual.  I wanted something that I can spend minutes with per month and to store data in a neutral manner (imported statement data is stored in CSV format and is Excel friendly).  This code is open source, so if it doesn't exactly suit you're budgeting style, then fork it, and change it.

Features
--------
 - Automatically match transactions with budget categories aka "Buckets".
 - Import bank statements in CSV format and only store them locally; ie not uploaded anywhere.
 - Specify a monthly budget and monitor how you track against imported statements.
 - Visual graphs for tracking performance.
 - Analyse any period of time to find averages and improve your monthly budget.
 - Plan for long term goals and ad-hoc/annual bills with "saved-up-for" budget categories.
 - Figure out how much surplus you have with actual bank statement data while still ensuring you don't spend money set aside for longer term goals/bills.
  
 
Overview
--------
It's a simple 2 tier application at the moment with all business logic residing in the engine assembly. The UI is currently WPF, but the intention is to create others as well. All data is saved only locally, no data is uploaded anywhere. This project has a fundamental goal not to upload any user's personal budgeting or bank statement data online.
No databases are currently used, data is saved in XML format.  It is the user's responsibility to save their data in a secure location (ie: NTFS permissions or a PGP/TrueCrypt file or drive).


Getting Started
---------------
No binaries are currently uploaded, so you will need to build the solution yourself.


Terminology
-----------
* _Bucket_

A Bucket is a metaphor used to describe a container that contains a set amount of funds ready to be spent.  When the bucket is empty, there is no more money to spend.  Buckets cannot have an overdrawn (negative) balance. Any overspending is automatically taken from the Surplus bucket, if there's no Surplus remaining, get ready to bounce.
There are a few different kinds of buckets: A Spent-Monthly-Bucket is a bucket that will automatically empty any remaining funds into Surplus at the end of the month, if all funds are not spent. A Saved-Up-For-Bucket will not empty itself at the end of the month, so if all funds are not spent the bucket will get bigger over time. This is useful for expenses that are paid annually or ad-hoc, ie not paid on a regular date each month. 
When setting up a Budget you create Buckets and set a monthly amount you wish to allocate from your salary.  

The Dashboard
-------------
![Budget Monitoring Dashboard](https://github.com/Benrnz/BudgetAnalyser/blob/master/Screenshot1.png "The Budget Analyser Monitoring Dashboard")
The dashboard is used to show quick reference information and alerts.  Most functionality on this page is only avaialble when the global filter is set to show one month's worth of data.

Overview Tiles
* _Create New, Open, Load Demo Tiles_

These tiles provide ability to load and create new budget analyser files.
* _Days Since Last Import Tile_

Shows the number of days since bank statement data was loaded into the analyser.
* _Overspent Tile_

Shows the number of buckets that have been overspent this month.
* _Surprise Payment Warning Tile_

This is a configurable tile that can monitor a single bucket that was weekly or fortnightly payments. Sometimes there will be three fortnightly payments or five weekly payments in one month. This tile will warn when the next "surprise" payment will be.

Global Filter Tiles
* _Account Filter Tile_

Provides the ability to filter by single account.
* _Date Filter Tile_

Provides a date range filter.  This should usually be set to one month's worth of data, except when running reports looking for trends over long periods. This filter controls all date range filtering throughout the application, including reporting.

Monthly Tracking
* _Bucket Monitoring Tiles_

These are configurable tiles that can be optionally added per bucket.  They show a bar graph indicating remaining funds left in the bucket for the month.
* _Bank Surplus_

This is a bar graph showing the remaining funds left in the Surplus bucket. This monitors the actual bank Surplus amount from the previous Ledger Book Reconciliation. Essentially this is the remaining funds available for general spending (ie spending that doesn't fall into any other Budgeted Bucket category).
* _Budgeted Surplus_

This is a bar graph showing the remaining funds for Surplus compared to the budgeted surplus amount.  This is sometimes different to the real surplus funds in the bank when incomes exceed expected budget amounts, or other budget buckets have exceeded their budgeted amounts. For comparison purposes only.

Projects
* _Fixed Amount Project Tiles_

These are configurable tiles where you have an on-going project that you do not want to spend more than a certain amount on.  For example the MASTERBED project is a renovation project to repaint and decorate a master bedroom. The screen shot shows the spending to date has reached the budgeted amount. The PS3 Project hasn't had any spending allocated against it yet, so its bar graph is showing a full bucket.
