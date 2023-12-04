Imports System.Data

Namespace Database
    ''' <summary>
    ''' All Database Type Services use this to provide a minimum amount of functionality. <br />
    ''' Each service will have specific functionality depending on the database type
    ''' </summary>
    Public Interface IRozeDatabaseCompliance
        Inherits IRozeCompliance
#Region "Properties"
        Property DatabaseName As String
        ''' <summary>
        ''' The schema the table is a part of. This is most common in MSSQL Databases
        ''' </summary>
        ''' <returns></returns>
        Property TableSchema As String
#End Region

#Region "Methods"
        ''' <summary>
        ''' Attach the Database to the server if the database does not already exist
        ''' </summary>
        ''' <param name="pDatabaseFile">The full path and name of the database file</param>
        ''' <returns>If the operation was successful</returns>
        Function AttachOrCreateDatabaseOnServer(pDatabaseFile As String) As Boolean

        ''' <summary>
        ''' Drops the table on the database using the column list passed in. Please be sure your <paramref name="pTableName"/> information is sql injection safe. 
        ''' This does not attempt to look for that.<br />
        ''' If table does not exist then does not try to drop
        ''' </summary>
        ''' <param name="pTableName">Name of the table to create</param>
        ''' <returns>if the operation was successful</returns>
        Function DropTableOnDatabase(pTableName As String) As Boolean

        ''' <summary>
        ''' Creates the table on the database using the column list passed in. Please be sure your <paramref name="pTableName"/> and <paramref name="pColumns"/> information is sql injection safe. 
        ''' This does not attempt to look for that.<br />
        ''' If table already exists. Does not try to create
        ''' </summary>
        ''' <param name="pTableName">Name of the table to create</param>
        ''' <param name="pColumns">structure of the table to create</param>
        ''' <returns>if the operation was successful</returns>
        Function CreateTableOnDatabase(pTableName As String, pColumns As List(Of DataColumn)) As Boolean

        ''' <summary>
        ''' Updates a table using the <paramref name="pSetDataValues"/> parameter to set the values. 
        ''' </summary>
        ''' <param name="pTableName">Name of the table to update</param>
        ''' <param name="pSetDataValues">Values to update in the table</param>
        ''' <param name="pWhereValues">Conditions to follow when updating the table</param>
        ''' <param name="pWhereAnd">Separate all <paramref name="pWhereValues"/> with an AND statement = true<br />Use False if you wish the where values to be separated by OR </param>
        ''' <returns>The number of rows updated</returns>
        Function UpdateTableData(pTableName As String, pSetDataValues As List(Of DatabaseParameter), pWhereValues As List(Of DatabaseParameter), pWhereAnd As Boolean) As Long

        ''' <summary>
        ''' Select data from the table using the sql statement
        ''' </summary>
        ''' <param name="pSqlStatement">Select the data from the table</param>
        ''' <returns>The data pulled by the sqlStatement</returns>
        Function SelectTableData(pSqlStatement As String) As DataTable

        ''' <summary>
        ''' Delete data from the table in the database
        ''' </summary>
        ''' <param name="pTableName">The table name to delete data from</param>
        ''' <param name="pWhereValues">Conditions to follow when updating the table</param>
        ''' <param name="pWhereAnd">Separate all <paramref name="pWhereValues"/> with an AND statement = true<br />Use False if you wish the where values to be separated by OR </param>
        ''' <returns>The number of rows deleted</returns>
        Function DeleteTableData(pTableName As String, pWhereValues As List(Of DatabaseParameter), pWhereAnd As Boolean) As Long

        ''' <summary>
        ''' Insert data into a table
        ''' </summary>
        ''' <param name="pTableName">The table name to delete data from</param>
        ''' <param name="pInsertValues">The values to insert as a new row</param>
        ''' <returns>Whether the operation was successful</returns>
        Function InsertTableData(pTableName As String, pInsertValues As List(Of DatabaseParameter)) As Boolean

        ''' <summary>
        ''' Update, Delete, Insert data in the table data on the database depending on what you did to the DataTable
        ''' </summary>
        ''' <param name="pDataTable">The table name to delete data from</param>
        ''' <returns>The number of rows updated, deleted and inserted</returns>
        Function UpdateInsertTableData(pDataTable As DataTable) As Integer
#End Region
    End Interface
End Namespace