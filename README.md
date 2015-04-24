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


The Dashboard
-------------
![Budget Monitoring Dashboard](https://github.com/Benrnz/BudgetAnalyser/blob/master/Screenshot1.png "The Budget Analyser Monitoring Dashboard")