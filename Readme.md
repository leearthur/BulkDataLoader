﻿# Bulk Data Loader
Tool to create and load test data in bulk into a MySQL database.

## Operations
### Generate
```
-Generate {configureation-name} {record-count} [-Append] [-Sql]
```
Generates CSV data (unless specified otherwise) based on the specified configuration file.

| | |
|-|-|
|-Sql|Creates a SQL file intead of CSV data.|
|-Apend|If the output file already exists, the data is appended to the end of the file.|

### Load
```
-Load {configuration-name} [-Sql]
```
Bulk loads the CSV data (unless specified otherwise) into MySQL.

| | |
|-|-|
|-Sql|Executes SQL script. This is significantly slower than loading CSV data.|

### Create
```
-Create {table-name} [-Overwrite]
```
Create a basic configuration file based on the table structure.

| | |
|-|-|
|-Overwrite|Overwrite existing file.|

### Settings
```
-Settings
```
List application settings as definded in the app.config file.

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