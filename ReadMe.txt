Task:
To generate MS SQL scripts for each stored procedure/view/table/trigger... etc

The Microsoft Strikes Back: library SMO (Server Management Object) sucks big time, it's buggy and hard to work with!

It's quick and dirty project for now, but it works.

I have other very similar project under standard .Net 4.6.7 and it works better without falling into "exit with code 0" error.

Thank you
sam klok


Used with links:

1. using file appseeting.json
Microsoft.Extensions.Hosting
1.1 I used Microsoft.Extensions.Configuration 
    https://stackoverflow.com/questions/65110479/how-to-get-values-from-appsettings-json-in-a-console-application-using-net-core

1.2 I didn't use hosting: Creating and using configuration by using package

2. Main is how to get SQL scripts by using C#: 
SQL Server SMO (Server Management Object)

2.1 Generating sql code programmatically
https://stackoverflow.com/questions/12140422/generating-sql-code-programmatically

2.2 How do I generate SQL database script using C# or SQL?
https://stackoverflow.com/questions/36978473/how-do-i-generate-sql-database-script-using-c-sharp-or-sql

2.3
https://stackoverflow.com/questions/3159901/how-to-create-alter-scripts-instead-of-create-scripts-using-smo-server-manageme

3. Linq: Find Element in a Collection
https://stackoverflow.com/questions/8062954/linq-find-element-in-a-collection

4. SQL files:
https://github.com/sam-klok/BasicSqlJoins

