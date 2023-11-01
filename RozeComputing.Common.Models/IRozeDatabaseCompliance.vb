Imports System.Data

Namespace Database

    Public Interface IRozeDatabaseCompliance
        Inherits IRozeCompliance


#Region "Methods"
        ''' <summary>
        ''' Attach the Database to the server if the database does not already exist
        ''' </summary>
        ''' <param name="pDatabaseFile">The full path and name of the database file</param>
        ''' <param name="pDatabaseName">The name of the database</param>
        ''' <returns>If the operation was successful</returns>
        Function AttachOrCreateDatabaseOnServer(pDatabaseFile As String, pDatabaseName As String) As Boolean

        Function CreateOrAlterTableOnDatabase(pDatabaseName As String, pTableName As String) As Boolean

        Function UpdateTableData(pDatabaseName As String, pTableName As String, pSetDataValues As List(Of DatabaseParameter), pWhereValues As List(Of DatabaseParameter)) As Long

        Function SelectTableData(pDatabaseName As String, pTableName As String, pSqlStatement As String, pParameters As List(Of DatabaseParameter)) As DataTable

        Function DeleteTableData(pDatabaseName As String, pTableName As String, pWhereValues As List(Of DatabaseParameter)) As DataTable

        Function InsertTableData(pDatabaseName As String, pTableName As String, pInsertValues As List(Of DatabaseParameter)) As DataTable

        Function ExecuteStoredProcedure(pDatabaseName As String, pProcedureName As String, pProcedureParameters As List(Of DatabaseParameter)) As DataTable

#End Region
    End Interface
End Namespace