
Imports System.Configuration
Imports NLog
Imports NLog.Layouts
Imports NLog.Targets

Public Class LoggingDatabaseService
    Implements IDisposable

#Region "IDisposable"


    Private disposedValue As Boolean

    Protected Overridable Sub Dispose(disposing As Boolean)
        If Not disposedValue Then
            If disposing Then
                ' TODO: dispose managed state (managed objects)
            End If

            ' TODO: free unmanaged resources (unmanaged objects) and override finalizer
            ' TODO: set large fields to null
            disposedValue = True
        End If
    End Sub

    ' ' TODO: override finalizer only if 'Dispose(disposing As Boolean)' has code to free unmanaged resources
    ' Protected Overrides Sub Finalize()
    '     ' Do not change this code. Put cleanup code in 'Dispose(disposing As Boolean)' method
    '     Dispose(disposing:=False)
    '     MyBase.Finalize()
    ' End Sub

    Public Sub Dispose() Implements IDisposable.Dispose
        ' Do not change this code. Put cleanup code in 'Dispose(disposing As Boolean)' method
        Dispose(disposing:=True)
        GC.SuppressFinalize(Me)
    End Sub

#End Region

#Region "Properties and Variables"
    Public Property Logger As Logger = NLog.LogManager.GetCurrentClassLogger()

    Public Enum DBProviderEnum
        MySql_Data = 1
        MySqlConnector = 2
        System_Data_SQLite = 3
        Microsoft_Data_SqlClient = 4
        Microsoft_Data_Sqlite = 5
        Npgsql = 6
        Oracle_ManagedDataAccess = 7
        Mono_Data_Sqlite = 8
    End Enum
#End Region

#Region "Constructor"
    Sub New(Optional pLogName As String = "")
        If String.IsNullOrWhiteSpace(pLogName) = False Then
            Logger = LogManager.GetLogger(pLogName)
        End If
    End Sub
#End Region

#Region "Method"
    Public Sub AddMSSQLTarget(pTargetName As String, pDBProvider As DBProviderEnum, pConnectionString As String, Optional pInsertCommand As String = "INSERT INTO Logs(CreatedOn,Message,Level,Exception,StackTrace,Logger,Url) VALUES (@datetime,@msg,@level,@exception,@trace,@logger,@url)", Optional pCommandParameters As List(Of DatabaseParameterInfo) = Nothing)


        Dim dbProviderStr As String

        Select Case pDBProvider

            Case DBProviderEnum.MySql_Data
                dbProviderStr = "MySql.Data.MySqlClient.MySqlConnection, MySql.Data"
            Case DBProviderEnum.MySqlConnector
                dbProviderStr = "MySqlConnector.MySqlConnection, MySqlConnector"
            Case DBProviderEnum.System_Data_SQLite
                dbProviderStr = "System.Data.SQLite.SQLiteConnection, System.Data.SQLite"
            Case DBProviderEnum.Microsoft_Data_SqlClient
                dbProviderStr = "Microsoft.Data.SqlClient.SqlConnection, Microsoft.Data.SqlClient"
            Case DBProviderEnum.Microsoft_Data_Sqlite
                dbProviderStr = "Microsoft.Data.Sqlite.SqliteConnection, Microsoft.Data.Sqlite"
            Case DBProviderEnum.Npgsql
                dbProviderStr = "Npgsql.NpgsqlConnection, Npgsql"
            Case DBProviderEnum.Oracle_ManagedDataAccess
                dbProviderStr = "Oracle.ManagedDataAccess.Client.OracleConnection, Oracle.ManagedDataAccess"
            Case DBProviderEnum.Mono_Data_Sqlite
                dbProviderStr = "Mono.Data.Sqlite.SqliteConnection, Mono.Data.Sqlite"
            Case Else
                Throw New Exception("Unknown DB Provider")
        End Select
        Dim logFile = New Targets.DatabaseTarget(pTargetName) With {
            .Name = pTargetName,
            .ConnectionString = pConnectionString,
            .DBProvider = dbProviderStr,
            .CommandText = pInsertCommand
        }

        If IsNothing(pCommandParameters) Then
            logFile.Parameters.Add(New DatabaseParameterInfo("@datetime", "${date}"))
            logFile.Parameters.Add(New DatabaseParameterInfo("@msg", "${message}"))
            logFile.Parameters.Add(New DatabaseParameterInfo("@level", "${level}"))
            logFile.Parameters.Add(New DatabaseParameterInfo("@exception", "${exception}"))
            logFile.Parameters.Add(New DatabaseParameterInfo("@trace", "${stacktrace}"))
            logFile.Parameters.Add(New DatabaseParameterInfo("@logger", "${logger}"))
            logFile.Parameters.Add(New DatabaseParameterInfo("@url", "${aspnet-request-url}"))
        Else
            For Each parameter In pCommandParameters
                logFile.Parameters.Add(parameter)
            Next
        End If

        'AddMSSQLTarget(logFile, LogLevel.Trace, LogLevel.Fatal)
    End Sub
    Public Sub AddMSSQLTarget(pTargetName As String, pDBProvider As String, pConnectionString As String, Optional pInsertCommand As String = "INSERT INTO Logs(CreatedOn,Message,Level,Exception,StackTrace,Logger,Url) VALUES (@datetime,@msg,@level,@exception,@trace,@logger,@url)", Optional pCommandParameters As List(Of DatabaseParameterInfo) = Nothing)

        Dim logFile = New Targets.DatabaseTarget(pTargetName) With {
            .Name = pTargetName,
            .ConnectionString = pConnectionString,
            .DBProvider = pDBProvider,
            .CommandText = pInsertCommand
        }

        If IsNothing(pCommandParameters) Then
            logFile.Parameters.Add(New DatabaseParameterInfo("@datetime", "${date}"))
            logFile.Parameters.Add(New DatabaseParameterInfo("@msg", "${message}"))
            logFile.Parameters.Add(New DatabaseParameterInfo("@level", "${level}"))
            logFile.Parameters.Add(New DatabaseParameterInfo("@exception", "${exception}"))
            logFile.Parameters.Add(New DatabaseParameterInfo("@trace", "${stacktrace}"))
            logFile.Parameters.Add(New DatabaseParameterInfo("@logger", "${logger}"))
            logFile.Parameters.Add(New DatabaseParameterInfo("@url", "${aspnet-request-url}"))
        Else
            For Each parameter In pCommandParameters
                logFile.Parameters.Add(parameter)
            Next
        End If

        'AddMSSQLTarget(logFile, LogLevel.Trace, LogLevel.Fatal)
    End Sub


    Public Sub AddMSSQLTarget(pFileTarget As FileTarget, pMinLogLevel As LogLevel, pMaxLogLevel As LogLevel)
        Dim config = New NLog.Config.LoggingConfiguration()

        If IsNothing(NLog.LogManager.Configuration) = False Then
            config = LogManager.Configuration
        End If

        config.AddRule(pMinLogLevel, pMaxLogLevel, pFileTarget)

        LogManager.Configuration = config
    End Sub

#End Region
End Class
