Namespace Database
    Public MustInherit Class DatabaseConnection
#Region "Properties"
        ''' <summary>
        ''' The name of the database to connect to on the <see cref="ServerName"/>
        ''' </summary>
        Public Property DatabaseName As String = String.Empty

        ''' <summary>
        ''' The server name too connect to
        ''' </summary>
        Public Property ServerName As String = String.Empty

        ''' <summary>
        ''' The port to connect to for the server. <br />
        ''' Leave this empty most of the time.
        ''' </summary>
        Public Property ServerPort As String = String.Empty

        ''' <summary>
        ''' The User name credentials to connect to the server
        ''' </summary>
        Public Property UserName As String = String.Empty

        ''' <summary>
        ''' The password credentials to connect to the server
        ''' </summary>
        Public Property Password As String = String.Empty

        ''' <summary>
        ''' If set to false then will use integrated/default security for the database. <br />
        ''' It is not recommended that you use integrated security unless you are using a separate process (Like SSO) to validate connections.
        ''' </summary>
        ''' <returns></returns>
        Public Property CredentialsNeeded As Boolean = True
#End Region
    End Class
End Namespace