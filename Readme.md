# Bulk Data Loader
Tool to create and load test data in bulk into a MySQL database.

## Operations
### Generate
```
-generate {configureation-name} {record-count} [-Append] [-Sql]
```
Generates CSV data (unless specified otherwise) based on the specified configuration file.

|Flag|Description|
|-|-|
|-Sql|Creates a SQL file intead of CSV data.|
|-Apend|If the output file already exists, the data is appended to the end of the file.|

### Load
```
-load {configuration-name} [-Sql]
```
Bulk loads the CSV data (unless specified otherwise) into MySQL.

|Flag|Description|
|-|-|
|-Sql|Executes SQL script. This is significantly slower than loading CSV data.|

### Create
```
-create {table-name} [-Overwrite]
```
Create a basic configuration file based on the table structure. Auto_Increment columns will not be included.

|Flag|Description|
|-|-|
|-Overwrite|Overwrite existing file.|

### MySQL 
To bulk load data into MySQL, the output file needs to be in a folder which the MySQL process has access to. 
By default, on development Windows machines this location is: 
```
C:\ProgramData\MySQL\MySQL Server 5.7\Uploads\
```

The location of the folder can be found by running the following query:
```
SHOW VARIABLES LIKE 'secure_file_priv'
```

### Local Configuration
There are two local application settings. Both can either be set in the appsettings.json file, or as an environment variable.
|Name|Description|
|-|-|
|DefaultConnection|Default database connection string. Required for -load action.|
|OutputFileLocation|Location where the output file will be generated. Required for -generate and -load actions.|