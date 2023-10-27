Imports System.Data
Imports RozeComputing.Common.Models
Imports RozeComputing.Common.Models.Cleaning
''' <summary>
''' Works with <see cref="DataRow"/> You can: create, update, and delete information in <see cref="DataRow"/> using this service.<br />
''' You should not need to worry about exceptions rolling up into your program when using this.
''' TODO:
''' Get Values from DataRows
''' Update Values in DataRows
''' </summary>
Public Class DataRowService
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
    ''' Returns a <see cref="DataRow"/> that has been marked as new for the <see cref="DataTable"/>
    ''' </summary>
    ''' <param name="pDataTable"></param>
    ''' <returns>New <see cref="DataRow"/></returns>
    Public Shared Function GetNewDataRowFromDataTable(pDataTable As DataTable) As DataRow
        Return pDataTable.NewRow
    End Function

    ''' <summary>
    ''' Takes the <paramref name="pDataRow"/>, uses <paramref name="pColumnName"/> to get the value and tries to convert it to a <see cref="String"/>.<br />
    ''' Does call <see cref="SimpleCleaningService.GetCleanString(Object, String, Boolean, Boolean, Long, Boolean)"/><br />
    ''' Any errors hit will always be added to the Exceptions List to be viewed after returning the value
    ''' </summary>
    ''' <param name="pDataRow">The DataRow to look in the data for.</param>
    ''' <param name="pColumnName">The column to look in the <paramref name="pDataRow"/> for.</param>
    ''' <param name="pDefaultValue">Default Value to return if <paramref name="pValue"/> is invalid.</param>
    ''' <param name="pTrimLeftOfValue">Trims the left side of the return value</param>
    ''' <param name="pTrimRightOfValue">Trims the right side of the return value</param>
    ''' <param name="pLengthLimit">Option to limit the length of the returned <see cref="String"/>. <br />0=unlimited length</param>
    ''' <param name="pAllowExceptionRollUp">If = true Will allow errors to propagate through.<br />If = false will catch any errors.</param>
    ''' <returns>A clean valid <see cref="String"/> that can be used without fear of nulls</returns>
    Public Function GetStringFromDataRow(pDataRow As DataRow, pColumnName As String, pDefaultValue As String, Optional pTrimLeftOfValue As Boolean = False, Optional pTrimRightOfValue As Boolean = False, Optional pLengthLimit As Long = 0, Optional pAllowExceptionRollUp As Boolean = False) As String
        Dim rtnValue As String = String.Empty

        Try
            'Clear Exceptions
            If mCleaningSettings.ClearExceptionsAfterEachCall = True Then
                Exceptions.Clear()
            End If

            'Set defaultValue based off of settings
            If String.IsNullOrWhiteSpace(pDefaultValue) = False Then
                'Default value was set for function use it
            Else
                'Default value not set for function look at settings to know what to do
                If mCleaningSettings.DefaultValue = CleaningSettings.DefaultValueEnum.UseNullVal Then
                    pDefaultValue = Nothing
                Else
                    pDefaultValue = ""
                End If
            End If

            'Set rtnValue to default value before we start doing any checks
            rtnValue = pDefaultValue

            'Validate Parameters
            If IsNothing(pDataRow) Then
                Throw New ArgumentNullException(NameOf(pColumnName), "The parameter " & NameOf(pDataRow) & " is nothing")
            End If

            If IsNothing(pColumnName) Then
                Throw New ArgumentNullException(NameOf(pColumnName), "The parameter " & NameOf(pDataRow) & " is nothing")
            End If

            If pDataRow.Table.Columns.Contains(pColumnName) = False Then
                Throw New ArgumentOutOfRangeException(NameOf(pColumnName), pColumnName, "Column does not exist within Parameter (" & NameOf(pDataRow) & ")")
            End If

            rtnValue = mCleaningService.GetCleanString(pDataRow.Item(pColumnName), pDefaultValue, pTrimLeftOfValue, pTrimRightOfValue, pLengthLimit, pAllowExceptionRollUp)

            'Add any errors of using the cleaning service to this service
            Exceptions.AddRange(mCleaningService.Exceptions)
            mCleaningService.Exceptions.Clear() 'Clear so we don't add again to our list

        Catch ex As Exception
            Exceptions.Add(ex)
            If pAllowExceptionRollUp = True Then
                Throw
            End If
        End Try

        Return rtnValue
    End Function

    ''' <summary>
    ''' Takes the value and tries to convert it to a <see cref="String"/>.<br />
    ''' Does call <see cref="GetStringFromDataRow(DataRow, String, String, Boolean, Boolean, Long, Boolean)"/><br />
    ''' Any errors hit will always be added to the Exceptions List to be viewed after returning the value
    ''' </summary>
    ''' <param name="pDataRow">The DataRow to look in the data for.</param>
    ''' <param name="pColumnName">The column to look in the <paramref name="pDataRow"/> for.</param>
    ''' <param name="pTrimLeftOfValue">Trims the left side of the return value</param>
    ''' <param name="pTrimRightOfValue">Trims the right side of the return value</param>
    ''' <param name="pLengthLimit">Option to limit the length of the returned <see cref="String"/>. <br />0=unlimited length</param>
    ''' <returns>A clean valid <see cref="String"/> that can be used without fear of nulls</returns>
    Public Function GetStringFromDataRow(pDataRow As DataRow, pColumnName As String, Optional pTrimLeftOfValue As Boolean = False, Optional pTrimRightOfValue As Boolean = False, Optional pLengthLimit As Long = 0) As String
        Dim rtnValue As String = String.Empty

        Try
            'Set defaultValue based off of settings
            If mCleaningSettings.DefaultValue = CleaningSettings.DefaultValueEnum.UseNullVal Then
                Return GetStringFromDataRow(pDataRow, pColumnName, Nothing, pTrimLeftOfValue, pTrimRightOfValue, pLengthLimit, mCleaningSettings.AllowExceptionRollUp)
            Else
                Return GetStringFromDataRow(pDataRow, pColumnName, String.Empty, pTrimLeftOfValue, pTrimRightOfValue, pLengthLimit, mCleaningSettings.AllowExceptionRollUp)
            End If

        Catch ex As Exception
            Exceptions.Add(ex)
            If mCleaningSettings.AllowExceptionRollUp = True Then
                Throw
            End If
        End Try

        Return rtnValue
    End Function

    ''' <summary>
    ''' Takes the <paramref name="pDataRow"/>, uses <paramref name="pColumnName"/> to get the value and tries to convert it to a <see cref="Double"/>.<br />
    ''' Does call <see cref="GetDoubleFromDataRow(DataRow, String, Double, Boolean)"/><br />
    ''' Any errors hit will always be added to the Exceptions List to be viewed after returning the value
    ''' </summary>
    ''' <param name="pDataRow">The DataRow to look in the data for.</param>
    ''' <param name="pColumnName">The column to look in the <paramref name="pDataRow"/> for.</param>
    ''' <returns>A clean valid <see cref="Double"/> that can be used without fear of nulls</returns>
    Public Function GetDoubleFromDataRowFromDataRow(pDataRow As DataRow, pColumnName As String) As Double
        Dim rtnValue As Double = 0

        Try
            Select Case mCleaningSettings.DefaultValue
                Case CleaningSettings.DefaultValueEnum.UseMaxVal
                    rtnValue = Double.MaxValue
                    rtnValue = GetDoubleFromDataRow(pDataRow, pColumnName, Double.MaxValue, mCleaningSettings.AllowExceptionRollUp)

                Case CleaningSettings.DefaultValueEnum.UseZeroOrEmptyVal
                    rtnValue = 0
                    rtnValue = GetDoubleFromDataRow(pDataRow, pColumnName, 0, mCleaningSettings.AllowExceptionRollUp)

                Case CleaningSettings.DefaultValueEnum.UseMinVal
                    rtnValue = Double.MinValue
                    rtnValue = GetDoubleFromDataRow(pDataRow, pColumnName, Double.MinValue, mCleaningSettings.AllowExceptionRollUp)

                Case CleaningSettings.DefaultValueEnum.UseNullVal
                    rtnValue = Nothing
                    rtnValue = GetDoubleFromDataRow(pDataRow, pColumnName, Nothing, mCleaningSettings.AllowExceptionRollUp)

            End Select
        Catch ex As Exception
            Exceptions.Add(ex)
            If mCleaningSettings.AllowExceptionRollUp = True Then
                Throw
            End If
        End Try

        Return rtnValue
    End Function

    ''' <summary>
    ''' Takes the <paramref name="pDataRow"/>, uses <paramref name="pColumnName"/> to get the value and tries to convert it to a <see cref="Double"/>.<br />
    ''' Does call <see cref="SimpleCleaningService.GetCleanDouble(Object, Double, Boolean)"/><br />
    ''' Any errors hit will always be added to the Exceptions List to be viewed after returning the value
    ''' </summary>
    ''' <param name="pDataRow">The DataRow to look in the data for.</param>
    ''' <param name="pColumnName">The column to look in the <paramref name="pDataRow"/> for.</param>
    ''' <param name="pDefaultValue">Default Value to return if: <paramref name="pDataRow"/>, <paramref name="pColumnName"/>, or combination is invalid.</param>
    ''' <param name="pAllowExceptionRollUp">If = true Will allow errors to propagate through.<br />If = false will catch any errors.</param>
    ''' <returns>A clean valid <see cref="Double"/> that can be used without fear of nulls</returns>
    Public Function GetDoubleFromDataRow(pDataRow As DataRow, pColumnName As String, pDefaultValue As Double, pAllowExceptionRollUp As Boolean) As Double
        Dim rtnValue As Double = 0

        Try
            'Set defaultValue based off of settings
            If String.IsNullOrWhiteSpace(pDefaultValue) = False Then
                'Default value was set for function use it
            Else
                'Default value not set for function look at settings to know what to do
                If mCleaningSettings.DefaultValue = CleaningSettings.DefaultValueEnum.UseNullVal Then
                    pDefaultValue = Nothing
                Else
                    pDefaultValue = ""
                End If
            End If

            'Set rtnValue to default value before we start doing any checks
            rtnValue = pDefaultValue

            'Validate Parameters
            If IsNothing(pDataRow) Then
                Throw New ArgumentNullException(NameOf(pColumnName), "The parameter " & NameOf(pDataRow) & " is nothing")
            End If

            If IsNothing(pColumnName) Then
                Throw New ArgumentNullException(NameOf(pColumnName), "The parameter " & NameOf(pDataRow) & " is nothing")
            End If

            If pDataRow.Table.Columns.Contains(pColumnName) = False Then
                Throw New ArgumentOutOfRangeException(NameOf(pColumnName), pColumnName, "Column does not exist within Parameter (" & NameOf(pDataRow) & ")")
            End If

            rtnValue = mCleaningService.GetCleanDouble(pDataRow.Item(pColumnName), pDefaultValue, pAllowExceptionRollUp)

            'Add any errors of using the cleaning service to this service
            Exceptions.AddRange(mCleaningService.Exceptions)
            mCleaningService.Exceptions.Clear() 'Clear so we don't add again to our list


        Catch ex As Exception
            Exceptions.Add(ex)
            If pAllowExceptionRollUp = True Then
                Throw
            End If
        End Try

        Return rtnValue
    End Function


    ''' <summary>
    ''' Takes the <paramref name="pDataRow"/>, uses <paramref name="pColumnName"/> to get the value and tries to convert it to a <see cref="Integer"/>.<br />
    ''' Does call <see cref="GetIntegerFromDataRow(DataRow, String, Integer, Boolean)"/><br />
    ''' Any errors hit will always be added to the Exceptions List to be viewed after returning the value
    ''' </summary>
    ''' <param name="pDataRow">The DataRow to look in the data for.</param>
    ''' <param name="pColumnName">The column to look in the <paramref name="pDataRow"/> for.</param>
    ''' <returns>A clean valid <see cref="Integer"/> that can be used without fear of nulls</returns>
    Public Function GetIntegerFromDataRowFromDataRow(pDataRow As DataRow, pColumnName As String) As Integer
        Dim rtnValue As Integer = 0

        Try
            Select Case mCleaningSettings.DefaultValue
                Case CleaningSettings.DefaultValueEnum.UseMaxVal
                    rtnValue = Integer.MaxValue
                    rtnValue = GetIntegerFromDataRow(pDataRow, pColumnName, Integer.MaxValue, mCleaningSettings.AllowExceptionRollUp)

                Case CleaningSettings.DefaultValueEnum.UseZeroOrEmptyVal
                    rtnValue = 0
                    rtnValue = GetIntegerFromDataRow(pDataRow, pColumnName, 0, mCleaningSettings.AllowExceptionRollUp)

                Case CleaningSettings.DefaultValueEnum.UseMinVal
                    rtnValue = Integer.MinValue
                    rtnValue = GetIntegerFromDataRow(pDataRow, pColumnName, Integer.MinValue, mCleaningSettings.AllowExceptionRollUp)

                Case CleaningSettings.DefaultValueEnum.UseNullVal
                    rtnValue = Nothing
                    rtnValue = GetIntegerFromDataRow(pDataRow, pColumnName, Nothing, mCleaningSettings.AllowExceptionRollUp)

            End Select
        Catch ex As Exception
            Exceptions.Add(ex)
            If mCleaningSettings.AllowExceptionRollUp = True Then
                Throw
            End If
        End Try

        Return rtnValue
    End Function

    ''' <summary>
    ''' Takes the <paramref name="pDataRow"/>, uses <paramref name="pColumnName"/> to get the value and tries to convert it to a <see cref="Integer"/>.<br />
    ''' Does call <see cref="SimpleCleaningService.GetCleanInteger(Object, Integer, Boolean)"/><br />
    ''' Any errors hit will always be added to the Exceptions List to be viewed after returning the value
    ''' </summary>
    ''' <param name="pDataRow">The DataRow to look in the data for.</param>
    ''' <param name="pColumnName">The column to look in the <paramref name="pDataRow"/> for.</param>
    ''' <param name="pDefaultValue">Default Value to return if: <paramref name="pDataRow"/>, <paramref name="pColumnName"/>, or combination is invalid.</param>
    ''' <param name="pAllowExceptionRollUp">If = true Will allow errors to propagate through.<br />If = false will catch any errors.</param>
    ''' <returns>A clean valid <see cref="Integer"/> that can be used without fear of nulls</returns>
    Public Function GetIntegerFromDataRow(pDataRow As DataRow, pColumnName As String, pDefaultValue As Integer, pAllowExceptionRollUp As Boolean) As Integer
        Dim rtnValue As Integer = 0

        Try
            'Set defaultValue based off of settings
            If String.IsNullOrWhiteSpace(pDefaultValue) = False Then
                'Default value was set for function use it
            Else
                'Default value not set for function look at settings to know what to do
                If mCleaningSettings.DefaultValue = CleaningSettings.DefaultValueEnum.UseNullVal Then
                    pDefaultValue = Nothing
                Else
                    pDefaultValue = ""
                End If
            End If

            'Set rtnValue to default value before we start doing any checks
            rtnValue = pDefaultValue

            'Validate Parameters
            If IsNothing(pDataRow) Then
                Throw New ArgumentNullException(NameOf(pColumnName), "The parameter " & NameOf(pDataRow) & " is nothing")
            End If

            If IsNothing(pColumnName) Then
                Throw New ArgumentNullException(NameOf(pColumnName), "The parameter " & NameOf(pDataRow) & " is nothing")
            End If

            If pDataRow.Table.Columns.Contains(pColumnName) = False Then
                Throw New ArgumentOutOfRangeException(NameOf(pColumnName), pColumnName, "Column does not exist within Parameter (" & NameOf(pDataRow) & ")")
            End If

            rtnValue = mCleaningService.GetCleanInteger(pDataRow.Item(pColumnName), pDefaultValue, pAllowExceptionRollUp)

            'Add any errors of using the cleaning service to this service
            Exceptions.AddRange(mCleaningService.Exceptions)
            mCleaningService.Exceptions.Clear() 'Clear so we don't add again to our list

        Catch ex As Exception
            Exceptions.Add(ex)
            If pAllowExceptionRollUp = True Then
                Throw
            End If
        End Try

        Return rtnValue
    End Function


    ''' <summary>
    ''' Takes the <paramref name="pDataRow"/>, uses <paramref name="pColumnName"/> to get the value and tries to convert it to a <see cref="Long"/>.<br />
    ''' Does call <see cref="GetLongFromDataRow(DataRow, String, Long, Boolean)"/><br />
    ''' Any errors hit will always be added to the Exceptions List to be viewed after returning the value
    ''' </summary>
    ''' <param name="pDataRow">The DataRow to look in the data for.</param>
    ''' <param name="pColumnName">The column to look in the <paramref name="pDataRow"/> for.</param>
    ''' <returns>A clean valid <see cref="Long"/> that can be used without fear of nulls</returns>
    Public Function GetLongFromDataRowFromDataRow(pDataRow As DataRow, pColumnName As String) As Long
        Dim rtnValue As Long = 0

        Try
            Select Case mCleaningSettings.DefaultValue
                Case CleaningSettings.DefaultValueEnum.UseMaxVal
                    rtnValue = Long.MaxValue
                    rtnValue = GetLongFromDataRow(pDataRow, pColumnName, Long.MaxValue, mCleaningSettings.AllowExceptionRollUp)

                Case CleaningSettings.DefaultValueEnum.UseZeroOrEmptyVal
                    rtnValue = 0
                    rtnValue = GetLongFromDataRow(pDataRow, pColumnName, 0, mCleaningSettings.AllowExceptionRollUp)

                Case CleaningSettings.DefaultValueEnum.UseMinVal
                    rtnValue = Long.MinValue
                    rtnValue = GetLongFromDataRow(pDataRow, pColumnName, Long.MinValue, mCleaningSettings.AllowExceptionRollUp)

                Case CleaningSettings.DefaultValueEnum.UseNullVal
                    rtnValue = Nothing
                    rtnValue = GetLongFromDataRow(pDataRow, pColumnName, Nothing, mCleaningSettings.AllowExceptionRollUp)

            End Select
        Catch ex As Exception
            Exceptions.Add(ex)
            If mCleaningSettings.AllowExceptionRollUp = True Then
                Throw
            End If
        End Try

        Return rtnValue
    End Function

    ''' <summary>
    ''' Takes the <paramref name="pDataRow"/>, uses <paramref name="pColumnName"/> to get the value and tries to convert it to a <see cref="Long"/>.<br />
    ''' Does call <see cref="SimpleCleaningService.GetCleanLong(Object, Long, Boolean)"/><br />
    ''' Any errors hit will always be added to the Exceptions List to be viewed after returning the value
    ''' </summary>
    ''' <param name="pDataRow">The DataRow to look in the data for.</param>
    ''' <param name="pColumnName">The column to look in the <paramref name="pDataRow"/> for.</param>
    ''' <param name="pDefaultValue">Default Value to return if: <paramref name="pDataRow"/>, <paramref name="pColumnName"/>, or combination is invalid.</param>
    ''' <param name="pAllowExceptionRollUp">If = true Will allow errors to propagate through.<br />If = false will catch any errors.</param>
    ''' <returns>A clean valid <see cref="Long"/> that can be used without fear of nulls</returns>
    Public Function GetLongFromDataRow(pDataRow As DataRow, pColumnName As String, pDefaultValue As Long, pAllowExceptionRollUp As Boolean) As Long
        Dim rtnValue As Long = 0

        Try
            'Set defaultValue based off of settings
            If String.IsNullOrWhiteSpace(pDefaultValue) = False Then
                'Default value was set for function use it
            Else
                'Default value not set for function look at settings to know what to do
                If mCleaningSettings.DefaultValue = CleaningSettings.DefaultValueEnum.UseNullVal Then
                    pDefaultValue = Nothing
                Else
                    pDefaultValue = ""
                End If
            End If

            'Set rtnValue to default value before we start doing any checks
            rtnValue = pDefaultValue

            'Validate Parameters
            If IsNothing(pDataRow) Then
                Throw New ArgumentNullException(NameOf(pColumnName), "The parameter " & NameOf(pDataRow) & " is nothing")
            End If

            If IsNothing(pColumnName) Then
                Throw New ArgumentNullException(NameOf(pColumnName), "The parameter " & NameOf(pDataRow) & " is nothing")
            End If

            If pDataRow.Table.Columns.Contains(pColumnName) = False Then
                Throw New ArgumentOutOfRangeException(NameOf(pColumnName), pColumnName, "Column does not exist within Parameter (" & NameOf(pDataRow) & ")")
            End If

            rtnValue = mCleaningService.GetCleanLong(pDataRow.Item(pColumnName), pDefaultValue, pAllowExceptionRollUp)

            'Add any errors of using the cleaning service to this service
            Exceptions.AddRange(mCleaningService.Exceptions)
            mCleaningService.Exceptions.Clear() 'Clear so we don't add again to our list

        Catch ex As Exception
            Exceptions.Add(ex)
            If pAllowExceptionRollUp = True Then
                Throw
            End If
        End Try

        Return rtnValue
    End Function


    ''' <summary>
    ''' Takes the <paramref name="pDataRow"/>, uses <paramref name="pColumnName"/> to get the value and tries to convert it to a <see cref="Int16"/>.<br />
    ''' Does call <see cref="GetInt16FromDataRow(DataRow, String, Int16, Boolean)"/><br />
    ''' Any errors hit will always be added to the Exceptions List to be viewed after returning the value
    ''' </summary>
    ''' <param name="pDataRow">The DataRow to look in the data for.</param>
    ''' <param name="pColumnName">The column to look in the <paramref name="pDataRow"/> for.</param>
    ''' <returns>A clean valid <see cref="Int16"/> that can be used without fear of nulls</returns>
    Public Function GetInt16FromDataRowFromDataRow(pDataRow As DataRow, pColumnName As String) As Int16
        Dim rtnValue As Int16 = 0

        Try
            Select Case mCleaningSettings.DefaultValue
                Case CleaningSettings.DefaultValueEnum.UseMaxVal
                    rtnValue = Int16.MaxValue
                    rtnValue = GetInt16FromDataRow(pDataRow, pColumnName, Int16.MaxValue, mCleaningSettings.AllowExceptionRollUp)

                Case CleaningSettings.DefaultValueEnum.UseZeroOrEmptyVal
                    rtnValue = 0
                    rtnValue = GetInt16FromDataRow(pDataRow, pColumnName, 0, mCleaningSettings.AllowExceptionRollUp)

                Case CleaningSettings.DefaultValueEnum.UseMinVal
                    rtnValue = Int16.MinValue
                    rtnValue = GetInt16FromDataRow(pDataRow, pColumnName, Int16.MinValue, mCleaningSettings.AllowExceptionRollUp)

                Case CleaningSettings.DefaultValueEnum.UseNullVal
                    rtnValue = Nothing
                    rtnValue = GetInt16FromDataRow(pDataRow, pColumnName, Nothing, mCleaningSettings.AllowExceptionRollUp)

            End Select
        Catch ex As Exception
            Exceptions.Add(ex)
            If mCleaningSettings.AllowExceptionRollUp = True Then
                Throw
            End If
        End Try

        Return rtnValue
    End Function

    ''' <summary>
    ''' Takes the <paramref name="pDataRow"/>, uses <paramref name="pColumnName"/> to get the value and tries to convert it to a <see cref="Int16"/>.<br />
    ''' Does call <see cref="SimpleCleaningService.GetCleanInt16(Object, Int16, Boolean)"/><br />
    ''' Any errors hit will always be added to the Exceptions List to be viewed after returning the value
    ''' </summary>
    ''' <param name="pDataRow">The DataRow to look in the data for.</param>
    ''' <param name="pColumnName">The column to look in the <paramref name="pDataRow"/> for.</param>
    ''' <param name="pDefaultValue">Default Value to return if: <paramref name="pDataRow"/>, <paramref name="pColumnName"/>, or combination is invalid.</param>
    ''' <param name="pAllowExceptionRollUp">If = true Will allow errors to propagate through.<br />If = false will catch any errors.</param>
    ''' <returns>A clean valid <see cref="Int16"/> that can be used without fear of nulls</returns>
    Public Function GetInt16FromDataRow(pDataRow As DataRow, pColumnName As String, pDefaultValue As Int16, pAllowExceptionRollUp As Boolean) As Int16
        Dim rtnValue As Int16 = 0

        Try
            'Set defaultValue based off of settings
            If String.IsNullOrWhiteSpace(pDefaultValue) = False Then
                'Default value was set for function use it
            Else
                'Default value not set for function look at settings to know what to do
                If mCleaningSettings.DefaultValue = CleaningSettings.DefaultValueEnum.UseNullVal Then
                    pDefaultValue = Nothing
                Else
                    pDefaultValue = ""
                End If
            End If

            'Set rtnValue to default value before we start doing any checks
            rtnValue = pDefaultValue

            'Validate Parameters
            If IsNothing(pDataRow) Then
                Throw New ArgumentNullException(NameOf(pColumnName), "The parameter " & NameOf(pDataRow) & " is nothing")
            End If

            If IsNothing(pColumnName) Then
                Throw New ArgumentNullException(NameOf(pColumnName), "The parameter " & NameOf(pDataRow) & " is nothing")
            End If

            If pDataRow.Table.Columns.Contains(pColumnName) = False Then
                Throw New ArgumentOutOfRangeException(NameOf(pColumnName), pColumnName, "Column does not exist within Parameter (" & NameOf(pDataRow) & ")")
            End If

            rtnValue = mCleaningService.GetCleanInt16(pDataRow.Item(pColumnName), pDefaultValue, pAllowExceptionRollUp)

            'Add any errors of using the cleaning service to this service
            Exceptions.AddRange(mCleaningService.Exceptions)
            mCleaningService.Exceptions.Clear() 'Clear so we don't add again to our list

        Catch ex As Exception
            Exceptions.Add(ex)
            If pAllowExceptionRollUp = True Then
                Throw
            End If
        End Try

        Return rtnValue
    End Function


    ''' <summary>
    ''' Takes the <paramref name="pDataRow"/>, uses <paramref name="pColumnName"/> to get the value and tries to convert it to a <see cref="Int32"/>.<br />
    ''' Does call <see cref="GetInt32FromDataRow(DataRow, String, Int32, Boolean)"/><br />
    ''' Any errors hit will always be added to the Exceptions List to be viewed after returning the value
    ''' </summary>
    ''' <param name="pDataRow">The DataRow to look in the data for.</param>
    ''' <param name="pColumnName">The column to look in the <paramref name="pDataRow"/> for.</param>
    ''' <returns>A clean valid <see cref="Int32"/> that can be used without fear of nulls</returns>
    Public Function GetInt32FromDataRowFromDataRow(pDataRow As DataRow, pColumnName As String) As Int32
        Dim rtnValue As Int32 = 0

        Try
            Select Case mCleaningSettings.DefaultValue
                Case CleaningSettings.DefaultValueEnum.UseMaxVal
                    rtnValue = Int32.MaxValue
                    rtnValue = GetInt32FromDataRow(pDataRow, pColumnName, Int32.MaxValue, mCleaningSettings.AllowExceptionRollUp)

                Case CleaningSettings.DefaultValueEnum.UseZeroOrEmptyVal
                    rtnValue = 0
                    rtnValue = GetInt32FromDataRow(pDataRow, pColumnName, 0, mCleaningSettings.AllowExceptionRollUp)

                Case CleaningSettings.DefaultValueEnum.UseMinVal
                    rtnValue = Int32.MinValue
                    rtnValue = GetInt32FromDataRow(pDataRow, pColumnName, Int32.MinValue, mCleaningSettings.AllowExceptionRollUp)

                Case CleaningSettings.DefaultValueEnum.UseNullVal
                    rtnValue = Nothing
                    rtnValue = GetInt32FromDataRow(pDataRow, pColumnName, Nothing, mCleaningSettings.AllowExceptionRollUp)

            End Select
        Catch ex As Exception
            Exceptions.Add(ex)
            If mCleaningSettings.AllowExceptionRollUp = True Then
                Throw
            End If
        End Try

        Return rtnValue
    End Function

    ''' <summary>
    ''' Takes the <paramref name="pDataRow"/>, uses <paramref name="pColumnName"/> to get the value and tries to convert it to a <see cref="Int32"/>.<br />
    ''' Does call <see cref="SimpleCleaningService.GetCleanInt32(Object, Int32, Boolean)"/><br />
    ''' Any errors hit will always be added to the Exceptions List to be viewed after returning the value
    ''' </summary>
    ''' <param name="pDataRow">The DataRow to look in the data for.</param>
    ''' <param name="pColumnName">The column to look in the <paramref name="pDataRow"/> for.</param>
    ''' <param name="pDefaultValue">Default Value to return if: <paramref name="pDataRow"/>, <paramref name="pColumnName"/>, or combination is invalid.</param>
    ''' <param name="pAllowExceptionRollUp">If = true Will allow errors to propagate through.<br />If = false will catch any errors.</param>
    ''' <returns>A clean valid <see cref="Int32"/> that can be used without fear of nulls</returns>
    Public Function GetInt32FromDataRow(pDataRow As DataRow, pColumnName As String, pDefaultValue As Int32, pAllowExceptionRollUp As Boolean) As Int32
        Dim rtnValue As Int32 = 0

        Try
            'Set defaultValue based off of settings
            If String.IsNullOrWhiteSpace(pDefaultValue) = False Then
                'Default value was set for function use it
            Else
                'Default value not set for function look at settings to know what to do
                If mCleaningSettings.DefaultValue = CleaningSettings.DefaultValueEnum.UseNullVal Then
                    pDefaultValue = Nothing
                Else
                    pDefaultValue = ""
                End If
            End If

            'Set rtnValue to default value before we start doing any checks
            rtnValue = pDefaultValue

            'Validate Parameters
            If IsNothing(pDataRow) Then
                Throw New ArgumentNullException(NameOf(pColumnName), "The parameter " & NameOf(pDataRow) & " is nothing")
            End If

            If IsNothing(pColumnName) Then
                Throw New ArgumentNullException(NameOf(pColumnName), "The parameter " & NameOf(pDataRow) & " is nothing")
            End If

            If pDataRow.Table.Columns.Contains(pColumnName) = False Then
                Throw New ArgumentOutOfRangeException(NameOf(pColumnName), pColumnName, "Column does not exist within Parameter (" & NameOf(pDataRow) & ")")
            End If

            rtnValue = mCleaningService.GetCleanInt32(pDataRow.Item(pColumnName), pDefaultValue, pAllowExceptionRollUp)

            'Add any errors of using the cleaning service to this service
            Exceptions.AddRange(mCleaningService.Exceptions)
            mCleaningService.Exceptions.Clear() 'Clear so we don't add again to our list

        Catch ex As Exception
            Exceptions.Add(ex)
            If pAllowExceptionRollUp = True Then
                Throw
            End If
        End Try

        Return rtnValue
    End Function


    ''' <summary>
    ''' Takes the <paramref name="pDataRow"/>, uses <paramref name="pColumnName"/> to get the value and tries to convert it to a <see cref="Int64"/>.<br />
    ''' Does call <see cref="GetInt64FromDataRow(DataRow, String, Int64, Boolean)"/><br />
    ''' Any errors hit will always be added to the Exceptions List to be viewed after returning the value
    ''' </summary>
    ''' <param name="pDataRow">The DataRow to look in the data for.</param>
    ''' <param name="pColumnName">The column to look in the <paramref name="pDataRow"/> for.</param>
    ''' <returns>A clean valid <see cref="Int64"/> that can be used without fear of nulls</returns>
    Public Function GetInt64FromDataRowFromDataRow(pDataRow As DataRow, pColumnName As String) As Int64
        Dim rtnValue As Int64 = 0

        Try
            Select Case mCleaningSettings.DefaultValue
                Case CleaningSettings.DefaultValueEnum.UseMaxVal
                    rtnValue = Int64.MaxValue
                    rtnValue = GetInt64FromDataRow(pDataRow, pColumnName, Int64.MaxValue, mCleaningSettings.AllowExceptionRollUp)

                Case CleaningSettings.DefaultValueEnum.UseZeroOrEmptyVal
                    rtnValue = 0
                    rtnValue = GetInt64FromDataRow(pDataRow, pColumnName, 0, mCleaningSettings.AllowExceptionRollUp)

                Case CleaningSettings.DefaultValueEnum.UseMinVal
                    rtnValue = Int64.MinValue
                    rtnValue = GetInt64FromDataRow(pDataRow, pColumnName, Int64.MinValue, mCleaningSettings.AllowExceptionRollUp)

                Case CleaningSettings.DefaultValueEnum.UseNullVal
                    rtnValue = Nothing
                    rtnValue = GetInt64FromDataRow(pDataRow, pColumnName, Nothing, mCleaningSettings.AllowExceptionRollUp)

            End Select
        Catch ex As Exception
            Exceptions.Add(ex)
            If mCleaningSettings.AllowExceptionRollUp = True Then
                Throw
            End If
        End Try

        Return rtnValue
    End Function

    ''' <summary>
    ''' Takes the <paramref name="pDataRow"/>, uses <paramref name="pColumnName"/> to get the value and tries to convert it to a <see cref="Int64"/>.<br />
    ''' Does call <see cref="SimpleCleaningService.GetCleanInt64(Object, Int64, Boolean)"/><br />
    ''' Any errors hit will always be added to the Exceptions List to be viewed after returning the value
    ''' </summary>
    ''' <param name="pDataRow">The DataRow to look in the data for.</param>
    ''' <param name="pColumnName">The column to look in the <paramref name="pDataRow"/> for.</param>
    ''' <param name="pDefaultValue">Default Value to return if: <paramref name="pDataRow"/>, <paramref name="pColumnName"/>, or combination is invalid.</param>
    ''' <param name="pAllowExceptionRollUp">If = true Will allow errors to propagate through.<br />If = false will catch any errors.</param>
    ''' <returns>A clean valid <see cref="Int64"/> that can be used without fear of nulls</returns>
    Public Function GetInt64FromDataRow(pDataRow As DataRow, pColumnName As String, pDefaultValue As Int64, pAllowExceptionRollUp As Boolean) As Int64
        Dim rtnValue As Int64 = 0

        Try
            'Set defaultValue based off of settings
            If String.IsNullOrWhiteSpace(pDefaultValue) = False Then
                'Default value was set for function use it
            Else
                'Default value not set for function look at settings to know what to do
                If mCleaningSettings.DefaultValue = CleaningSettings.DefaultValueEnum.UseNullVal Then
                    pDefaultValue = Nothing
                Else
                    pDefaultValue = ""
                End If
            End If

            'Set rtnValue to default value before we start doing any checks
            rtnValue = pDefaultValue

            'Validate Parameters
            If IsNothing(pDataRow) Then
                Throw New ArgumentNullException(NameOf(pColumnName), "The parameter " & NameOf(pDataRow) & " is nothing")
            End If

            If IsNothing(pColumnName) Then
                Throw New ArgumentNullException(NameOf(pColumnName), "The parameter " & NameOf(pDataRow) & " is nothing")
            End If

            If pDataRow.Table.Columns.Contains(pColumnName) = False Then
                Throw New ArgumentOutOfRangeException(NameOf(pColumnName), pColumnName, "Column does not exist within Parameter (" & NameOf(pDataRow) & ")")
            End If

            rtnValue = mCleaningService.GetCleanInt64(pDataRow.Item(pColumnName), pDefaultValue, pAllowExceptionRollUp)

            'Add any errors of using the cleaning service to this service
            Exceptions.AddRange(mCleaningService.Exceptions)
            mCleaningService.Exceptions.Clear() 'Clear so we don't add again to our list

        Catch ex As Exception
            Exceptions.Add(ex)
            If pAllowExceptionRollUp = True Then
                Throw
            End If
        End Try

        Return rtnValue
    End Function

    ''' <summary>
    ''' Takes the <paramref name="pDataRow"/>, uses <paramref name="pColumnName"/> to get the value and tries to convert it to a <see cref="GUID"/>.<br />
    ''' Does call <see cref="GetGUIDFromDataRow(DataRow, String, GUID, Boolean)"/><br />
    ''' Any errors hit will always be added to the Exceptions List to be viewed after returning the value
    ''' </summary>
    ''' <param name="pDataRow">The DataRow to look in the data for.</param>
    ''' <param name="pColumnName">The column to look in the <paramref name="pDataRow"/> for.</param>
    ''' <returns>A clean valid <see cref="GUID"/> that can be used without fear of nulls</returns>
    Public Function GetGUIDFromDataRowFromDataRow(pDataRow As DataRow, pColumnName As String) As Guid
        Dim rtnValue As Guid = Guid.Empty

        Try
            Select Case mCleaningSettings.DefaultValue
                Case CleaningSettings.DefaultValueEnum.UseNullVal
                    rtnValue = Nothing
                    rtnValue = GetGUIDFromDataRow(pDataRow, pColumnName, Nothing, mCleaningSettings.AllowExceptionRollUp)
                Case Else
                    rtnValue = Guid.Empty
                    rtnValue = GetGUIDFromDataRow(pDataRow, pColumnName, Guid.Empty, mCleaningSettings.AllowExceptionRollUp)
            End Select
        Catch ex As Exception
            Exceptions.Add(ex)
            If mCleaningSettings.AllowExceptionRollUp = True Then
                Throw
            End If
        End Try

        Return rtnValue
    End Function

    ''' <summary>
    ''' Takes the <paramref name="pDataRow"/>, uses <paramref name="pColumnName"/> to get the value and tries to convert it to a <see cref="GUID"/>.<br />
    ''' Does call <see cref="SimpleCleaningService.GetCleanGUID(Object, GUID, Boolean)"/><br />
    ''' Any errors hit will always be added to the Exceptions List to be viewed after returning the value
    ''' </summary>
    ''' <param name="pDataRow">The DataRow to look in the data for.</param>
    ''' <param name="pColumnName">The column to look in the <paramref name="pDataRow"/> for.</param>
    ''' <param name="pDefaultValue">Default Value to return if: <paramref name="pDataRow"/>, <paramref name="pColumnName"/>, or combination is invalid.<br />Set = Null to let the setting handle default</param>
    ''' <param name="pAllowExceptionRollUp">If = true Will allow errors to propagate through.<br />If = false will catch any errors.</param>
    ''' <returns>A clean valid <see cref="GUID"/> that can be used without fear of nulls</returns>
    Public Function GetGUIDFromDataRow(pDataRow As DataRow, pColumnName As String, pDefaultValue As Guid, pAllowExceptionRollUp As Boolean) As Guid
        Dim rtnValue As Guid = Guid.Empty

        Try
            If IsNothing(pDefaultValue) = False Then
                'Default value was set for function use it
            Else
                'Default value not set for function look at settings to know what to do
                If mCleaningSettings.DefaultValue = CleaningSettings.DefaultValueEnum.UseNullVal Then
                    pDefaultValue = Nothing
                Else
                    pDefaultValue = Guid.Empty
                End If
            End If

            'Set rtnValue to default value before we start doing any checks
            rtnValue = pDefaultValue

            'Validate Parameters
            If IsNothing(pDataRow) Then
                Throw New ArgumentNullException(NameOf(pColumnName), "The parameter " & NameOf(pDataRow) & " is nothing")
            End If

            If IsNothing(pColumnName) Then
                Throw New ArgumentNullException(NameOf(pColumnName), "The parameter " & NameOf(pDataRow) & " is nothing")
            End If

            If pDataRow.Table.Columns.Contains(pColumnName) = False Then
                Throw New ArgumentOutOfRangeException(NameOf(pColumnName), pColumnName, "Column does not exist within Parameter (" & NameOf(pDataRow) & ")")
            End If

            rtnValue = mCleaningService.GetCleanGUID(pDataRow.Item(pColumnName), pDefaultValue, pAllowExceptionRollUp)

            'Add any errors of using the cleaning service to this service
            Exceptions.AddRange(mCleaningService.Exceptions)
            mCleaningService.Exceptions.Clear() 'Clear so we don't add again to our list

        Catch ex As Exception
            Exceptions.Add(ex)
            If pAllowExceptionRollUp = True Then
                Throw
            End If
        End Try

        Return rtnValue
    End Function



    ''' <summary>
    ''' Takes the <paramref name="pDataRow"/>, uses <paramref name="pColumnName"/> to get the value and tries to convert it to a <see cref="DateTime"/>.<br />
    ''' Does call <see cref="GetDateTimeFromDataRow(DataRow, String, DateTime, Boolean)"/><br />
    ''' Any errors hit will always be added to the Exceptions List to be viewed after returning the value
    ''' </summary>
    ''' <param name="pDataRow">The DataRow to look in the data for.</param>
    ''' <param name="pColumnName">The column to look in the <paramref name="pDataRow"/> for.</param>
    ''' <returns>A clean valid <see cref="DateTime"/> that can be used without fear of nulls</returns>
    Public Function GetDateTimeFromDataRowFromDataRow(pDataRow As DataRow, pColumnName As String) As DateTime
        Dim rtnValue As New DateTime(1900, 1, 1)

        Try
            Select Case mCleaningSettings.DefaultValue
                Case CleaningSettings.DefaultValueEnum.UseMaxVal
                    rtnValue = DateTime.MaxValue
                    rtnValue = GetDateTimeFromDataRow(pDataRow, pColumnName, DateTime.MaxValue, mCleaningSettings.AllowExceptionRollUp)

                Case CleaningSettings.DefaultValueEnum.UseZeroOrEmptyVal
                    rtnValue = New DateTime(1900, 1, 1)
                    rtnValue = GetDateTimeFromDataRow(pDataRow, pColumnName, New DateTime(1900, 1, 1), mCleaningSettings.AllowExceptionRollUp)

                Case CleaningSettings.DefaultValueEnum.UseMinVal
                    rtnValue = DateTime.MinValue
                    rtnValue = GetDateTimeFromDataRow(pDataRow, pColumnName, DateTime.MinValue, mCleaningSettings.AllowExceptionRollUp)

                Case CleaningSettings.DefaultValueEnum.UseNullVal
                    rtnValue = Nothing
                    rtnValue = GetDateTimeFromDataRow(pDataRow, pColumnName, Nothing, mCleaningSettings.AllowExceptionRollUp)

            End Select
        Catch ex As Exception
            Exceptions.Add(ex)
            If mCleaningSettings.AllowExceptionRollUp = True Then
                Throw
            End If
        End Try

        Return rtnValue
    End Function

    ''' <summary>
    ''' Takes the <paramref name="pDataRow"/>, uses <paramref name="pColumnName"/> to get the value and tries to convert it to a <see cref="DateTime"/>.<br />
    ''' Does call <see cref="SimpleCleaningService.GetCleanDateTime(Object, DateTime, Boolean)"/><br />
    ''' Any errors hit will always be added to the Exceptions List to be viewed after returning the value
    ''' </summary>
    ''' <param name="pDataRow">The DataRow to look in the data for.</param>
    ''' <param name="pColumnName">The column to look in the <paramref name="pDataRow"/> for.</param>
    ''' <param name="pDefaultValue">Default Value to return if: <paramref name="pDataRow"/>, <paramref name="pColumnName"/>, or combination is invalid.</param>
    ''' <param name="pAllowExceptionRollUp">If = true Will allow errors to propagate through.<br />If = false will catch any errors.</param>
    ''' <returns>A clean valid <see cref="DateTime"/> that can be used without fear of nulls</returns>
    Public Function GetDateTimeFromDataRow(pDataRow As DataRow, pColumnName As String, pDefaultValue As DateTime, pAllowExceptionRollUp As Boolean) As DateTime
        Dim rtnValue As New DateTime(1900, 1, 1)

        Try
            'Set defaultValue based off of settings
            If IsNothing(pDefaultValue) = False Then
                'Default value was set for function use it
            Else
                'Default value not set for function look at settings to know what to do
                If mCleaningSettings.DefaultValue = CleaningSettings.DefaultValueEnum.UseNullVal Then
                    pDefaultValue = Nothing
                Else
                    pDefaultValue = ""
                End If
            End If

            'Set rtnValue to default value before we start doing any checks
            rtnValue = pDefaultValue

            'Validate Parameters
            If IsNothing(pDataRow) Then
                Throw New ArgumentNullException(NameOf(pColumnName), "The parameter " & NameOf(pDataRow) & " is nothing")
            End If

            If IsNothing(pColumnName) Then
                Throw New ArgumentNullException(NameOf(pColumnName), "The parameter " & NameOf(pDataRow) & " is nothing")
            End If

            If pDataRow.Table.Columns.Contains(pColumnName) = False Then
                Throw New ArgumentOutOfRangeException(NameOf(pColumnName), pColumnName, "Column does not exist within Parameter (" & NameOf(pDataRow) & ")")
            End If

            rtnValue = mCleaningService.GetCleanDateTime(pDataRow.Item(pColumnName), pDefaultValue, pAllowExceptionRollUp)

            'Add any errors of using the cleaning service to this service
            Exceptions.AddRange(mCleaningService.Exceptions)
            mCleaningService.Exceptions.Clear() 'Clear so we don't add again to our list

        Catch ex As Exception
            Exceptions.Add(ex)
            If pAllowExceptionRollUp = True Then
                Throw
            End If
        End Try

        Return rtnValue
    End Function


    ''' <summary>
    ''' Takes the <paramref name="pDataRow"/>, uses <paramref name="pColumnName"/> to get the value and tries to convert it to a <see cref="DateTimeOffset"/>.<br />
    ''' Does call <see cref="GetDateTimeOffsetFromDataRow(DataRow, String, DateTimeOffset, Boolean)"/><br />
    ''' Any errors hit will always be added to the Exceptions List to be viewed after returning the value
    ''' </summary>
    ''' <param name="pDataRow">The DataRow to look in the data for.</param>
    ''' <param name="pColumnName">The column to look in the <paramref name="pDataRow"/> for.</param>
    ''' <returns>A clean valid <see cref="DateTimeOffset"/> that can be used without fear of nulls</returns>
    Public Function GetDateTimeOffsetFromDataRowFromDataRow(pDataRow As DataRow, pColumnName As String) As DateTimeOffset
        Dim rtnValue As DateTimeOffset = New DateTime(1900, 1, 1)

        Try
            Select Case mCleaningSettings.DefaultValue
                Case CleaningSettings.DefaultValueEnum.UseMaxVal
                    rtnValue = DateTimeOffset.MaxValue
                    rtnValue = GetDateTimeOffsetFromDataRow(pDataRow, pColumnName, DateTimeOffset.MaxValue, mCleaningSettings.AllowExceptionRollUp)

                Case CleaningSettings.DefaultValueEnum.UseZeroOrEmptyVal
                    rtnValue = New DateTime(1900, 1, 1)
                    rtnValue = GetDateTimeOffsetFromDataRow(pDataRow, pColumnName, New DateTime(1900, 1, 1), mCleaningSettings.AllowExceptionRollUp)

                Case CleaningSettings.DefaultValueEnum.UseMinVal
                    rtnValue = DateTimeOffset.MinValue
                    rtnValue = GetDateTimeOffsetFromDataRow(pDataRow, pColumnName, DateTimeOffset.MinValue, mCleaningSettings.AllowExceptionRollUp)

                Case CleaningSettings.DefaultValueEnum.UseNullVal
                    rtnValue = Nothing
                    rtnValue = GetDateTimeOffsetFromDataRow(pDataRow, pColumnName, Nothing, mCleaningSettings.AllowExceptionRollUp)

            End Select
        Catch ex As Exception
            Exceptions.Add(ex)
            If mCleaningSettings.AllowExceptionRollUp = True Then
                Throw
            End If
        End Try

        Return rtnValue
    End Function

    ''' <summary>
    ''' Takes the <paramref name="pDataRow"/>, uses <paramref name="pColumnName"/> to get the value and tries to convert it to a <see cref="DateTimeOffset"/>.<br />
    ''' Does call <see cref="SimpleCleaningService.GetCleanDateTimeOffset(Object, DateTimeOffset, Boolean)"/><br />
    ''' Any errors hit will always be added to the Exceptions List to be viewed after returning the value
    ''' </summary>
    ''' <param name="pDataRow">The DataRow to look in the data for.</param>
    ''' <param name="pColumnName">The column to look in the <paramref name="pDataRow"/> for.</param>
    ''' <param name="pDefaultValue">Default Value to return if: <paramref name="pDataRow"/>, <paramref name="pColumnName"/>, or combination is invalid.</param>
    ''' <param name="pAllowExceptionRollUp">If = true Will allow errors to propagate through.<br />If = false will catch any errors.</param>
    ''' <returns>A clean valid <see cref="DateTimeOffset"/> that can be used without fear of nulls</returns>
    Public Function GetDateTimeOffsetFromDataRow(pDataRow As DataRow, pColumnName As String, pDefaultValue As DateTimeOffset, pAllowExceptionRollUp As Boolean) As DateTimeOffset
        Dim rtnValue As DateTimeOffset = New DateTime(1900, 1, 1)

        Try
            'Set defaultValue based off of settings
            If IsNothing(pDefaultValue) = False Then
                'Default value was set for function use it
            Else
                'Default value not set for function look at settings to know what to do
                If mCleaningSettings.DefaultValue = CleaningSettings.DefaultValueEnum.UseNullVal Then
                    pDefaultValue = Nothing
                Else
                    pDefaultValue = ""
                End If
            End If

            'Set rtnValue to default value before we start doing any checks
            rtnValue = pDefaultValue

            'Validate Parameters
            If IsNothing(pDataRow) Then
                Throw New ArgumentNullException(NameOf(pColumnName), "The parameter " & NameOf(pDataRow) & " is nothing")
            End If

            If IsNothing(pColumnName) Then
                Throw New ArgumentNullException(NameOf(pColumnName), "The parameter " & NameOf(pDataRow) & " is nothing")
            End If

            If pDataRow.Table.Columns.Contains(pColumnName) = False Then
                Throw New ArgumentOutOfRangeException(NameOf(pColumnName), pColumnName, "Column does not exist within Parameter (" & NameOf(pDataRow) & ")")
            End If

            rtnValue = mCleaningService.GetCleanDateTimeOffset(pDataRow.Item(pColumnName), pDefaultValue, pAllowExceptionRollUp)

            'Add any errors of using the cleaning service to this service
            Exceptions.AddRange(mCleaningService.Exceptions)
            mCleaningService.Exceptions.Clear() 'Clear so we don't add again to our list

        Catch ex As Exception
            Exceptions.Add(ex)
            If pAllowExceptionRollUp = True Then
                Throw
            End If
        End Try

        Return rtnValue
    End Function



    ''' <summary>
    ''' Takes the <paramref name="pDataRow"/>, uses <paramref name="pColumnName"/> to get the value and tries to convert it to a <see cref="Boolean"/>.<br />
    ''' Defaults to <see cref="Boolean" langword=".False"/> if <see cref="CleaningSettings.DefaultValueEnum.UseNullVal"/> is NOT set<br />
    ''' Any errors hit will always be added to the Exceptions List to be viewed after returning the value
    ''' </summary>
    ''' <param name="pDataRow">The DataRow to look in the data for.</param>
    ''' <param name="pColumnName">The column to look in the <paramref name="pDataRow"/> for.</param>
    ''' <returns>A clean valid <see cref="Boolean"/> that can be used without fear of nulls</returns>
    Public Function GetBooleanFromDataRowFromDataRow(pDataRow As DataRow, pColumnName As String) As Boolean
        Dim rtnValue As Boolean = 0

        Try
            Select Case mCleaningSettings.DefaultValue

                Case CleaningSettings.DefaultValueEnum.UseNullVal
                    rtnValue = Nothing
                    rtnValue = GetBooleanFromDataRow(pDataRow, pColumnName, Nothing, mCleaningSettings.AllowExceptionRollUp)
                Case Else
                    rtnValue = Nothing
                    rtnValue = GetBooleanFromDataRow(pDataRow, pColumnName, False, mCleaningSettings.AllowExceptionRollUp)

            End Select
        Catch ex As Exception
            Exceptions.Add(ex)
            If mCleaningSettings.AllowExceptionRollUp = True Then
                Throw
            End If
        End Try

        Return rtnValue
    End Function

    ''' <summary>
    ''' Takes the <paramref name="pDataRow"/>, uses <paramref name="pColumnName"/> to get the value and tries to convert it to a <see cref="Boolean"/>.<br />
    ''' Does call <see cref="SimpleCleaningService.GetCleanBoolean(Object, Boolean, Boolean)"/><br />
    ''' Any errors hit will always be added to the Exceptions List to be viewed after returning the value
    ''' </summary>
    ''' <param name="pDataRow">The DataRow to look in the data for.</param>
    ''' <param name="pColumnName">The column to look in the <paramref name="pDataRow"/> for.</param>
    ''' <param name="pDefaultValue">Default Value to return if: <paramref name="pDataRow"/>, <paramref name="pColumnName"/>, or combination is invalid.</param>
    ''' <param name="pAllowExceptionRollUp">If = true Will allow errors to propagate through.<br />If = false will catch any errors.</param>
    ''' <returns>A clean valid <see cref="Boolean"/> that can be used without fear of nulls</returns>
    Public Function GetBooleanFromDataRow(pDataRow As DataRow, pColumnName As String, pDefaultValue As Boolean, pAllowExceptionRollUp As Boolean) As Boolean
        Dim rtnValue As Boolean = 0

        Try
            'Set rtnValue to default value before we start doing any checks
            rtnValue = pDefaultValue

            'Validate Parameters
            If IsNothing(pDataRow) Then
                Throw New ArgumentNullException(NameOf(pColumnName), "The parameter " & NameOf(pDataRow) & " is nothing")
            End If

            If IsNothing(pColumnName) Then
                Throw New ArgumentNullException(NameOf(pColumnName), "The parameter " & NameOf(pDataRow) & " is nothing")
            End If

            If pDataRow.Table.Columns.Contains(pColumnName) = False Then
                Throw New ArgumentOutOfRangeException(NameOf(pColumnName), pColumnName, "Column does not exist within Parameter (" & NameOf(pDataRow) & ")")
            End If

            rtnValue = mCleaningService.GetCleanBoolean(pDataRow.Item(pColumnName), pDefaultValue, pAllowExceptionRollUp)

            'Add any errors of using the cleaning service to this service
            Exceptions.AddRange(mCleaningService.Exceptions)
            mCleaningService.Exceptions.Clear() 'Clear so we don't add again to our list

        Catch ex As Exception
            Exceptions.Add(ex)
            If pAllowExceptionRollUp = True Then
                Throw
            End If
        End Try

        Return rtnValue
    End Function

End Class
