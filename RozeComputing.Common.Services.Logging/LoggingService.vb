Imports NLog
Imports NLog.Layouts
Imports NLog.Targets
Imports RozeComputing.Common.Models

''' <summary>
''' Helps with implementation of NLog. <br />
''' Use the Property <see cref="Logger"/> to actually log info straight through NLog <br />
''' Use the functions if you wish for an easy to use solution. <br />
''' It can be expensive to rebuild the NLog configuration every time your program needs it. <br />
''' It is recommended to use this in a static variable throughout your code. <br />
''' An example NLog.Config is available to download from git hub if you wish to use a .config file instead of creating a target.
''' </summary>
Public Class LoggingService
    Implements IDisposable, IRozeCompliance

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
    ''' <summary>
    ''' Instantiate the class. Then call this property to actually log information
    ''' </summary>
    ''' <returns></returns>
    Public Property Logger As Logger = NLog.LogManager.GetCurrentClassLogger()

    Public Property Exceptions As New List(Of Exception) Implements IRozeCompliance.Exceptions

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

#Region "Methods"

    ''' <summary>
    ''' Creates a target file based on a couple of basic configurations
    ''' </summary>
    ''' <param name="pFileName"></param>
    ''' <param name="pTargetName"></param>
    Public Sub AddFileTarget(pFileName As String, pTargetName As String)
        Try

            Dim config = New NLog.Config.LoggingConfiguration()

            If IsNothing(NLog.LogManager.Configuration) = False Then
                config = LogManager.Configuration
            End If

            Dim logFile = New Targets.FileTarget(pTargetName) With {
            .FileName = pFileName,
            .KeepFileOpen = False,
            .Name = pTargetName,
            .ReplaceFileContentsOnEachWrite = False,
            .ArchiveAboveSize = 2000,
            .ArchiveEvery = FileArchivePeriod.Day,
            .ArchiveNumbering = ArchiveNumberingMode.Sequence,
            .CreateDirs = True,
            .DeleteOldFileOnStartup = False,
            .MaxArchiveFiles = 30,
            .ConcurrentWrites = True,
            .EnableFileDelete = True,
            .Layout = New SimpleLayout() With {.Text = "${longdate} - ${level:uppercase=true}: ${message}${onexception:${newline}EXCEPTION\: ${exception:format=ToString}}"}}

            AddFileTarget(logFile, LogLevel.Trace, LogLevel.Fatal)

        Catch ex As Exception
            Exceptions.Add(ex)
        End Try
    End Sub

    ''' <summary>
    ''' Fill out the FileTarget information on your own and pass it in to add it to the configuration.
    ''' </summary>
    ''' <param name="pFileTarget">The place where file logs will go</param>
    ''' <param name="pMinLogLevel">Min level you wish to log to this target</param>
    ''' <param name="pMaxLogLevel">Max level you wish to log to this target.</param>
    Public Sub AddFileTarget(pFileTarget As FileTarget, pMinLogLevel As LogLevel, pMaxLogLevel As LogLevel)
        Try

            Dim config = New NLog.Config.LoggingConfiguration()

            If IsNothing(NLog.LogManager.Configuration) = False Then
                config = LogManager.Configuration
            End If

            config.AddRule(pMinLogLevel, pMaxLogLevel, pFileTarget)

            LogManager.Configuration = config

        Catch ex As Exception
            Exceptions.Add(ex)
        End Try
    End Sub

    ''' <summary>
    ''' Adds an NLog Database target. <br /> 
    ''' PLEASE NOTE: <br />
    ''' Optional Parameters are set to default to MSSQL database. Will need to over write if using a different type of database.
    ''' </summary>
    ''' <param name="pTargetName">The Name of the target</param>
    ''' <param name="pDBProvider">The DB Provider. If the option is not available use the overloaded function to pass in the db provider as a string instead.</param>
    ''' <param name="pConnectionString">The Connection string to connect to the DB</param>
    ''' <param name="pInsertCommand">The command NLog should execute to talk to your database</param>
    ''' <param name="pCommandParameters">The command parameters NLog will need to use to fill out your insert command</param>
    Public Sub AddDatabaseTarget(pTargetName As String, pDBProvider As DBProviderEnum, pConnectionString As String, Optional pInsertCommand As String = "INSERT INTO Logs(CreatedOn,Message,Level,Exception,StackTrace,Logger,Url) VALUES (@datetime,@msg,@level,@exception,@trace,@logger,@url)", Optional pCommandParameters As List(Of DatabaseParameterInfo) = Nothing)

        Try
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
                .CommandText = pInsertCommand,
                .IsolationLevel = Data.IsolationLevel.ReadCommitted
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

            AddDatabaseTarget(logFile, LogLevel.Trace, LogLevel.Fatal)

        Catch ex As Exception
            Exceptions.Add(ex)
        End Try
    End Sub

    ''' <summary>
    ''' Adds an NLog Database target. <br /> 
    ''' PLEASE NOTE: <br />
    ''' Optional Parameters are set to default to MSSQL database. Will need to over write if using a different type of database.
    ''' </summary>
    ''' <param name="pTargetName">The Name of the target</param>
    ''' <param name="pDBProvider">The DB Provider. Click <see href="https://github.com/NLog/NLog/wiki/Database-target#dbprovider-examples">Code examples</see> for examples</param>
    ''' <param name="pConnectionString">The Connection string to connect to the DB</param>
    ''' <param name="pInsertCommand">The command NLog should execute to talk to your database</param>
    ''' <param name="pCommandParameters">The command parameters NLog will need to use to fill out your insert command</param>
    Public Sub AddDatabaseTarget(pTargetName As String, pDBProvider As String, pConnectionString As String, Optional pInsertCommand As String = "INSERT INTO Logs(CreatedOn,Message,Level,Exception,StackTrace,Logger,Url) VALUES (@datetime,@msg,@level,@exception,@trace,@logger,@url)", Optional pCommandParameters As List(Of DatabaseParameterInfo) = Nothing)
        Try
            Dim logFile = New Targets.DatabaseTarget(pTargetName) With {
            .Name = pTargetName,
            .ConnectionString = pConnectionString,
            .DBProvider = pDBProvider,
            .CommandText = pInsertCommand,
            .IsolationLevel = Data.IsolationLevel.ReadCommitted
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

            AddDatabaseTarget(logFile, LogLevel.Trace, LogLevel.Fatal)

        Catch ex As Exception
            Exceptions.Add(ex)
        End Try
    End Sub

    ''' <summary>
    ''' Fill out the FileTarget information on your own and pass it in to add it to the configuration.
    ''' </summary>
    ''' <param name="pDatabaseTarget">The place where file logs will go</param>
    ''' <param name="pMinLogLevel">Min level you wish to log to this target</param>
    ''' <param name="pMaxLogLevel">Max level you wish to log to this target.</param>
    Public Sub AddDatabaseTarget(pDatabaseTarget As DatabaseTarget, pMinLogLevel As LogLevel, pMaxLogLevel As LogLevel)
        Try
            Dim config = New NLog.Config.LoggingConfiguration()

            If IsNothing(NLog.LogManager.Configuration) = False Then
                config = LogManager.Configuration
            End If
            For Each existingtarget In config.AllTargets
                If existingtarget.Name = pDatabaseTarget.Name Then
                    Throw New Exception("Stopped from adding multiple targets with the same name")
                End If
            Next
            config.AddRule(pMinLogLevel, pMaxLogLevel, pDatabaseTarget)

            LogManager.Configuration = config

        Catch ex As Exception
            Exceptions.Add(ex)
        End Try
    End Sub

#End Region
End Class
