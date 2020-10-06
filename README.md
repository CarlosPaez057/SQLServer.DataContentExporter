# Notes 
Wanted to develop a Web Service to export data behind a fire-wall (assuming I know the data strurctures).  
The attached sample was created to connect to an SQL Server and execute queries to pull data.  Very similar to the export facility but you don't have to be a DBA. 
You just need to know the tables or dataset groups

# Topics 

C#
WebServices
DB Connectivity - via OLE
Reflection - Class Factory
Excel - Data Reader
Dependency Injection
JSON - simple configuration

# SQL Requirements   ( SQL Server )

Setup a database : SampleCodeDB
	
Setup
	Create asp-net-membership tables
	
	https://www.c-sharpcorner.com/blogs/create-install-asp-net-membership-database
	
	
Run SQL in Solution

# Application Settings  ( appsettings.json )

JSON changes required

SERVER-HOST\\SQLEXPRESS01   =>  yourhost\\yourSQLServer