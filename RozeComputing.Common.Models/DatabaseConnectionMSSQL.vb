Namespace Database
    Public Class DatabaseConnectionMSSQL
        Inherits DatabaseConnection

#Region "Properties"
        ''' <summary>
        ''' If the database is not yet attached to MSSQL setting this path and filename will attach the database to the server if <see cref="DatabaseName"/> does not yet exist
        ''' </summary>
        Public Property DBFileName As String = String.Empty

#End Region
    End Class
End Namespace