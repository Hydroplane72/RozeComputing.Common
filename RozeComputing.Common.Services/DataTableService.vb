Imports System.Data
Imports RozeComputing.Common.Models
Imports RozeComputing.Common.Models.Cleaning
Imports RozeComputing.Common.Models.Database
''' <summary>
''' Works with <see cref="DataTable"/>'s and does update/delete/selects from them.<br />
''' You should not need to worry about exceptions rolling up into your program when using this.
''' TODO:
''' Create select from table
''' Create new table
''' Update existing table
''' </summary>
Public Class DataTableService
    Implements IRozeCompliance

#Region "Variables"
    Private ReadOnly mCleaningSettings As New CleaningSettings

    Private ReadOnly mCleaningService As New SimpleCleaningService()
#End Region

#Region "Properties"
    ''' <summary>
    ''' If any errors are hit while cleaning. 
    ''' Exceptions will accumulate within this list. Log this list how you wish. 
    ''' This list is not persisted anywhere except memory.
    ''' </summary>
    ''' <returns>A list of Exceptions Handled</returns>
    Public Property Exceptions As List(Of Exception) = New List(Of Exception) Implements IRozeCompliance.Exceptions

#End Region

#Region "Constructor"
    ''' <summary>
    ''' 
    ''' </summary>
    Sub New()
        mCleaningSettings = New CleaningSettings()

        mCleaningService = New SimpleCleaningService(mCleaningSettings)
    End Sub

    ''' <summary>
    ''' Create the DataRow Helper with cleaning settings
    ''' </summary>
    ''' <param name="pCleaningSettings">Pass in any settings you wish to change from the default</param>
    Sub New(Optional pCleaningSettings As CleaningSettings = Nothing)
        If IsNothing(pCleaningSettings) Then
            pCleaningSettings = New CleaningSettings
        End If

        mCleaningSettings = pCleaningSettings

        mCleaningService = New SimpleCleaningService(mCleaningSettings)
    End Sub

#End Region

    ''' <summary>
    ''' Returns a new blank DataTable
    ''' </summary>
    Public Shared Function GetBlankDataTable() As DataTable
        Return New DataTable
    End Function

    ''' <summary>
    ''' Gets <see cref="DataRowCollection"/> from DataTable <br />
    ''' You should be able to iterate through the collection like a List in a for Each loop.<br />
    ''' For Example (In VB.Net): <br />
    ''' <code>For Each DataRow as  <see cref="DataRow"/> in  <see cref="DataRowCollection"/></code>
    ''' </summary>
    ''' <param name="pDataTable"></param>
    ''' <returns>The DataRows contained within the <see cref="DataTable"/></returns>
    Public Function GetDataRowsFromTable(pDataTable As DataTable, pAllowExceptionRollUp As Boolean) As DataRowCollection
        Dim dt As New DataTable

        Try
            Return pDataTable.Rows
        Catch ex As Exception
            Exceptions.Add(ex)
            If pAllowExceptionRollUp = True Then
                Throw
            End If
        End Try

        Return dt.Rows
    End Function

    ''' <summary>
    ''' Gets <see cref="DataRowCollection"/> from DataTable <br />
    ''' You should be able to iterate through the collection like a List in a for Each loop.<br />
    ''' For Example (In VB.Net): <br />
    ''' <code>For Each DataRow as  <see cref="DataRow"/> in  <see cref="DataRowCollection"/></code>
    ''' </summary>
    ''' <param name="pDataTable"></param>
    ''' <returns>The DataRows contained within the <see cref="DataTable"/></returns>
    Public Function GetDataRowsFromTable(pDataTable As DataTable) As DataRowCollection

        Dim DataRowCollection As DataRowCollection

        DataRowCollection = GetDataRowsFromTable(pDataTable, mCleaningSettings.AllowExceptionRollUp)

        Return pDataTable.Rows
    End Function

    ''' <summary>
    ''' Add a column to a table. Pass in the DataType you wish the column to be.
    ''' </summary>
    ''' <param name="pDataTable"></param>
    ''' <param name="pColumnName"></param>
    ''' <param name="pColumnType"></param>
    ''' <param name="pAllowExceptionRollUp"></param>
    Public Sub AddColumnToDataTable(ByRef pDataTable As DataTable, pColumnName As String, pColumnType As Type, pAllowExceptionRollUp As Boolean, Optional pMaxLength As Integer = -1, Optional pAllowNull As Boolean = True, Optional pDefaultValue As Object = Nothing, Optional pAutoIncrementSettings As DataTableAutoIncrementSettings = Nothing, Optional pUniqueValues As Boolean = False)

        Try
            If pDataTable.Columns.Contains(pColumnName) = True Then
                Throw New ArgumentException("Column (" & pColumnName & " ) already exists in provided data table.", NameOf(pColumnName))
            End If

            pDataTable.Columns.Add(pColumnName, pColumnType)
            If pMaxLength <> -1 Then
                pDataTable.Columns.Item(pColumnName).MaxLength = mCleaningService.GetCleanInteger(pMaxLength)
            End If
            pDataTable.Columns.Item(pColumnName).AllowDBNull = mCleaningService.GetCleanBoolean(pAllowNull)

            'See if passed in and if we care
            If IsNothing(pAutoIncrementSettings) = False AndAlso pAutoIncrementSettings.IsAutoIncrement = True Then
                pDataTable.Columns.Item(pColumnName).AutoIncrement = pAutoIncrementSettings.IsAutoIncrement
                pDataTable.Columns.Item(pColumnName).AutoIncrementSeed = pAutoIncrementSettings.AutoIncrementSeed
                pDataTable.Columns.Item(pColumnName).AutoIncrementStep = pAutoIncrementSettings.AutoIncrementStep
            End If

            If pAllowNull = False AndAlso IsNothing(pDefaultValue) Then
                Throw New ArgumentException(NameOf(pDefaultValue) & " is required when " & NameOf(pAllowNull) & " is set to false.")
            ElseIf pAllowNull = False AndAlso IsNothing(pDefaultValue) = False Then
                pDataTable.Columns.Item(pColumnName).DefaultValue = pDefaultValue
            End If
            pDataTable.Columns.Item(pColumnName).Unique = mCleaningService.GetCleanBoolean(pUniqueValues)

        Catch ex As Exception
            Exceptions.Add(ex)
            If pAllowExceptionRollUp = True Then
                Throw
            End If
        End Try
    End Sub
End Class
