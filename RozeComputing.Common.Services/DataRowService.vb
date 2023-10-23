Imports System.Data
''' <summary>
''' Works with <see cref="DataRow"/> You can: create, update, and delete information in <see cref="DataRow"/> using this service.<br />
''' You should not need to worry about exceptions rolling up into your program when using this.
''' TODO:
''' Get Values from DataRows
''' Update Values in DataRows
''' </summary>
Public Class DataRowService

    Public Function GetNewDataRowFromDataTable(pDataTable As DataTable) As DataRow
        Return pDataTable.NewRow
    End Function
End Class
