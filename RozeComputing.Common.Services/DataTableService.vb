Imports System.Data
Imports RozeComputing.Common.Models
Imports RozeComputing.Common.Models.Cleaning
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
End Class
