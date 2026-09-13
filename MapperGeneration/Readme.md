Mapper Generation Readme
========================

This utility is not a dependency of BudgetAnalyser and shouldn't be considered part of BudgetAnalyser source code. It is only a tool that has been used to generate some of the code.
There is no intention for the generated code to be regenerated from some source DSL or similar. This tool simply provided a fast way to get started and has not been used since, its kept in case a reference is needed. 

This tool provides the scripting code that uses the TangyFruit Mapper library to generate conversion code from and to DTO's.  
https://www.nuget.org/packages/Rees.TangyFruitMapper
TangyFruit Mapper takes a business domain object and DTO and generates code to map from one to the other and back again. 