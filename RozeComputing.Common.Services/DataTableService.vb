Imports System.Data
''' <summary>
''' Works with <see cref="DataTable"/>'s and does update/delete/selects from them.<br />
''' You should not need to worry about exceptions rolling up into your program when using this.
''' TODO:
''' Create select from table
''' Create new table
''' Update existing table
''' </summary>
Public Class DataTableService

    Public Function GetBlankDataTable() As DataTable
        Return New DataTable
    End Function
End Class
