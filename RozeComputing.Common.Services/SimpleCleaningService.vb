Imports RozeComputing.Common.Models
Imports RozeComputing.Common.Models.Cleaning

''' <summary>
''' Allows for the passing in of values and will clean/convert the Value or return the default if not convertible. <br />
''' </summary>
Public Class SimpleCleaningService
    Implements IRozeCompliance

#Region "Variables"
    Private ReadOnly mCleaningSettings As New CleaningSettings
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
    ''' Creates the cleaning object.
    ''' </summary>
    ''' <param name="pCleaningSettings">Pass in any settings you wish to change from the default of <see cref="CleaningSettings"/></param>
    Sub New(pCleaningSettings As CleaningSettings)
        If IsNothing(pCleaningSettings) Then
            pCleaningSettings = New CleaningSettings
        End If

        mCleaningSettings = pCleaningSettings
    End Sub
    ''' <summary>
    ''' Creates the cleaning object with default <see cref="CleaningSettings"/>
    ''' </summary>
    Sub New()
        mCleaningSettings = New CleaningSettings
    End Sub

#End Region

    ''' <summary>
    ''' Takes the value and tries to convert it to a <see cref="String"/>.<br />
    ''' Any errors hit will always be added to the Exceptions List to be viewed after returning the value
    ''' </summary>
    ''' <param name="pValue">The Value to convert to a <see cref="String"/></param>
    ''' <param name="pDefaultValue">Default Value to return if <paramref name="pValue"/> is invalid.</param>
    ''' <param name="pTrimLeftOfValue">Trims the left side of the return value</param>
    ''' <param name="pTrimRightOfValue">Trims the right side of the return value</param>
    ''' <param name="pLengthLimit">Option to limit the length of the returned <see cref="String"/>. <br />0=unlimited length</param>
    ''' <param name="pAllowExceptionRollUp">If = true Will allow errors to propagate through.<br />If = false will catch any errors.</param>
    ''' <returns>A clean valid <see cref="String"/> that can be used without fear of nulls</returns>
    Public Function GetCleanString(pValue As Object, pDefaultValue As String, Optional pTrimLeftOfValue As Boolean = False, Optional pTrimRightOfValue As Boolean = False, Optional pLengthLimit As Long = 0, Optional pAllowExceptionRollUp As Boolean = False) As String
        Dim rtnValue As String = String.Empty

        Try
            'Clear Exceptions
            If mCleaningSettings.ClearExceptionsAfterEachCall = True Then
                Exceptions.Clear()
            End If

            'Set rtnValue
            If IsNothing(pValue) Then
                Return pDefaultValue
            Else
                rtnValue = pValue.ToString()
            End If

            'Trim the value before we limit it
            If pTrimLeftOfValue = True Then
                rtnValue = rtnValue.TrimStart
            End If
            If pTrimRightOfValue = True Then
                rtnValue = rtnValue.TrimEnd
            End If

            'Limit the length of rtnValue if needed
            If pLengthLimit <> 0 AndAlso rtnValue.Length > pLengthLimit Then
                rtnValue = Left(rtnValue, pLengthLimit)
            End If

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
    ''' Any errors hit will always be added to the Exceptions List to be viewed after returning the value
    ''' </summary>
    ''' <param name="pValue">The Value to convert to a <see cref="String"/></param>
    ''' <param name="pTrimLeftOfValue">Trims the left side of the return value</param>
    ''' <param name="pTrimRightOfValue">Trims the right side of the return value</param>
    ''' <param name="pLengthLimit">Option to limit the length of the returned <see cref="String"/>. <br />0=unlimited length</param>
    ''' <returns>A clean valid <see cref="String"/> that can be used without fear of nulls</returns>
    Public Function GetCleanString(pValue As Object, Optional pTrimLeftOfValue As Boolean = False, Optional pTrimRightOfValue As Boolean = False, Optional pLengthLimit As Long = 0) As String
        Dim rtnValue As String = String.Empty

        Try
            'Set defaultValue based off of settings
            If mCleaningSettings.DefaultValue = CleaningSettings.DefaultValueEnum.UseNullVal Then
                Return GetCleanString(pValue, Nothing, pTrimLeftOfValue, pTrimRightOfValue, pLengthLimit, mCleaningSettings.AllowExceptionRollUp)
            Else
                Return GetCleanString(pValue, String.Empty, pTrimLeftOfValue, pTrimRightOfValue, pLengthLimit, mCleaningSettings.AllowExceptionRollUp)
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
    ''' Takes the Value and tries to convert it to an <see cref="Double"/><br />
    ''' Any errors hit will always be added to the Exceptions List to be viewed after returning the value
    ''' </summary>
    ''' <param name="pValue">The Value to convert to a <see cref="Double"/></param>
    ''' <returns>A clean valid <see cref="Double"/> that can be used without fear of nulls</returns>
    Public Function GetCleanDouble(pValue As Object) As Double
        Dim rtnValue As Double = 0

        Try
            Select Case mCleaningSettings.DefaultValue
                Case CleaningSettings.DefaultValueEnum.UseMaxVal
                    rtnValue = Double.MaxValue
                    rtnValue = GetCleanDouble(pValue, Double.MaxValue, mCleaningSettings.AllowExceptionRollUp)

                Case CleaningSettings.DefaultValueEnum.UseZeroOrEmptyVal
                    rtnValue = 0
                    rtnValue = GetCleanDouble(pValue, 0, mCleaningSettings.AllowExceptionRollUp)

                Case CleaningSettings.DefaultValueEnum.UseMinVal
                    rtnValue = Double.MinValue
                    rtnValue = GetCleanDouble(pValue, Double.MinValue, mCleaningSettings.AllowExceptionRollUp)

                Case CleaningSettings.DefaultValueEnum.UseNullVal
                    rtnValue = Nothing
                    rtnValue = GetCleanDouble(pValue, Nothing, mCleaningSettings.AllowExceptionRollUp)

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
    ''' Takes the Value and tries to convert it to an <see cref="Double"/><br />
    ''' Any errors hit will always be added to the Exceptions List to be viewed after returning the value
    ''' </summary>
    ''' <param name="pValue">The Value to convert to a <see cref="Double"/></param>
    ''' <param name="pDefaultValue">Default Value to return if <paramref name="pValue"/> is invalid. <br /> WARNING: Will convert nulls to MinValue of <see cref="Double"/>.</param>
    ''' <param name="pAllowExceptionRollUp">If = true Will allow errors to roll out.<br />If = false will catch any errors and not cause issues in your system.</param>
    ''' <returns>A clean valid <see cref="Double"/> that can be used without fear of nulls</returns>
    Public Function GetCleanDouble(pValue As Object, pDefaultValue As Double, pAllowExceptionRollUp As Boolean) As Double
        Dim rtnValue As Double = 0

        Try
            Dim pValueCleaned = GetCleanString(pValue, pDefaultValue, True, True,, pAllowExceptionRollUp)

            'Try converting to Double
            If String.IsNullOrWhiteSpace(pValueCleaned) Then
                rtnValue = pDefaultValue
            Else
                If Double.TryParse(pValueCleaned, rtnValue) = False Then
                    rtnValue = pDefaultValue
                    Throw New ArgumentException("Parameter " & NameOf(pValue) & " is not parse able with value: " & pValue & vbCrLf & "Returning default value (" & pDefaultValue & ")")
                End If
            End If

        Catch ex As Exception
            Exceptions.Add(ex)
            If pAllowExceptionRollUp = True Then
                Throw
            End If
        End Try

        Return rtnValue
    End Function

    ''' <summary>
    ''' Takes the Value and tries to convert it to an <see cref="Integer"/><br />
    ''' Any errors hit will always be added to the Exceptions List to be viewed after returning the value
    ''' </summary>
    ''' <param name="pValue">The Value to convert to a <see cref="Integer"/></param>
    ''' <returns>A clean valid <see cref="Integer"/> that can be used without fear of nulls</returns>
    Public Function GetCleanInteger(pValue As Object) As Integer
        Dim rtnValue As Integer = 0

        Try
            Select Case mCleaningSettings.DefaultValue
                Case CleaningSettings.DefaultValueEnum.UseMaxVal
                    rtnValue = Integer.MaxValue
                    rtnValue = GetCleanInteger(pValue, Integer.MaxValue, mCleaningSettings.AllowExceptionRollUp)

                Case CleaningSettings.DefaultValueEnum.UseZeroOrEmptyVal
                    rtnValue = 0
                    rtnValue = GetCleanInteger(pValue, 0, mCleaningSettings.AllowExceptionRollUp)

                Case CleaningSettings.DefaultValueEnum.UseMinVal
                    rtnValue = Integer.MinValue
                    rtnValue = GetCleanInteger(pValue, Integer.MinValue, mCleaningSettings.AllowExceptionRollUp)

                Case CleaningSettings.DefaultValueEnum.UseNullVal
                    rtnValue = Nothing
                    rtnValue = GetCleanInteger(pValue, Nothing, mCleaningSettings.AllowExceptionRollUp)

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
    ''' Takes the Value and tries to convert it to an <see cref="Integer"/><br />
    ''' Any errors hit will always be added to the Exceptions List to be viewed after returning the value
    ''' </summary>
    ''' <param name="pValue">The Value to convert to a <see cref="Integer"/></param>
    ''' <param name="pDefaultValue">Default Value to return if <paramref name="pValue"/> is invalid. <br /> WARNING: Will convert nulls to MinValue of <see cref="Integer"/>.</param>
    ''' <param name="pAllowExceptionRollUp">If = true Will allow errors to propagate through.<br />If = false will catch any errors.</param>
    ''' <returns>A clean valid <see cref="Integer"/> that can be used without fear of nulls</returns>
    Public Function GetCleanInteger(pValue As Object, Optional pDefaultValue As Integer = Integer.MinValue, Optional pAllowExceptionRollUp As Boolean = False) As Integer
        Dim rtnValue As Integer = 0

        Try
            Dim pValueCleaned = GetCleanString(pValue, pDefaultValue, True, True,, pAllowExceptionRollUp)

            'Try converting to integer
            If String.IsNullOrWhiteSpace(pValueCleaned) Then
                rtnValue = pDefaultValue
            Else
                If Integer.TryParse(pValueCleaned, rtnValue) = False Then
                    rtnValue = pDefaultValue
                    Throw New ArgumentException("Parameter " & NameOf(pValue) & " is not parse able with value: " & pValue & vbCrLf & "Returning default value (" & pDefaultValue & ")")
                End If
            End If

        Catch ex As Exception
            Exceptions.Add(ex)
            If pAllowExceptionRollUp = True Then
                Throw
            End If
        End Try

        Return rtnValue
    End Function

    ''' <summary>
    ''' Takes the Value and tries to convert it to an <see cref="Long"/><br />
    ''' Any errors hit will always be added to the Exceptions List to be viewed after returning the value
    ''' </summary>
    ''' <param name="pValue">The Value to convert to a <see cref="Long"/></param>
    ''' <returns>A clean valid <see cref="Long"/> that can be used without fear of nulls</returns>
    Public Function GetCleanLong(pValue As Object) As Long
        Dim rtnValue As Long = 0

        Try
            Select Case mCleaningSettings.DefaultValue
                Case CleaningSettings.DefaultValueEnum.UseMaxVal
                    rtnValue = Long.MaxValue
                    rtnValue = GetCleanLong(pValue, Long.MaxValue, mCleaningSettings.AllowExceptionRollUp)

                Case CleaningSettings.DefaultValueEnum.UseZeroOrEmptyVal
                    rtnValue = 0
                    rtnValue = GetCleanLong(pValue, 0, mCleaningSettings.AllowExceptionRollUp)

                Case CleaningSettings.DefaultValueEnum.UseMinVal
                    rtnValue = Long.MinValue
                    rtnValue = GetCleanLong(pValue, Long.MinValue, mCleaningSettings.AllowExceptionRollUp)

                Case CleaningSettings.DefaultValueEnum.UseNullVal
                    rtnValue = Nothing
                    rtnValue = GetCleanLong(pValue, Nothing, mCleaningSettings.AllowExceptionRollUp)

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
    ''' Takes the Value and tries to convert it to an <see cref="Long"/><br />
    ''' Any errors hit will always be added to the Exceptions List to be viewed after returning the value
    ''' </summary>
    ''' <param name="pValue">The Value to convert to a <see cref="Long"/></param>
    ''' <param name="pDefaultValue">Default Value to return if <paramref name="pValue"/> is invalid. <br /> WARNING: Will convert nulls to MinValue of <see cref="Long"/>.</param>
    ''' <param name="pAllowExceptionRollUp">If = true Will allow errors to propagate through.<br />If = false will catch any errors.</param>
    ''' <returns>A clean valid <see cref="Long"/> that can be used without fear of nulls</returns>
    Public Function GetCleanLong(pValue As Object, Optional pDefaultValue As Long = Long.MinValue, Optional pAllowExceptionRollUp As Boolean = False) As Long
        Dim rtnValue As Long = 0

        Try
            Dim pValueCleaned = GetCleanString(pValue, pDefaultValue, True, True,, pAllowExceptionRollUp)


            'Try converting to Long
            If String.IsNullOrWhiteSpace(pValueCleaned) Then
                rtnValue = pDefaultValue
            Else
                If Long.TryParse(pValueCleaned, rtnValue) = False Then
                    rtnValue = pDefaultValue
                    Throw New ArgumentException("Parameter " & NameOf(pValue) & " is not parse able with value: " & pValue & vbCrLf & "Returning default value (" & pDefaultValue & ")")
                End If
            End If

        Catch ex As Exception
            Exceptions.Add(ex)
            If pAllowExceptionRollUp = True Then
                Throw
            End If
        End Try

        Return rtnValue
    End Function

    ''' <summary>
    ''' Takes the Value and tries to convert it to an <see cref="Int16"/><br />
    ''' Any errors hit will always be added to the Exceptions List to be viewed after returning the value
    ''' </summary>
    ''' <param name="pValue">The Value to convert to a <see cref="Int16"/></param>
    ''' <returns>A clean valid <see cref="Int16"/> that can be used without fear of nulls</returns>
    Public Function GetCleanInt16(pValue As Object) As Int16
        Dim rtnValue As Int16 = 0

        Try
            Select Case mCleaningSettings.DefaultValue
                Case CleaningSettings.DefaultValueEnum.UseMaxVal
                    rtnValue = Int16.MaxValue
                    rtnValue = GetCleanInt16(pValue, Int16.MaxValue, mCleaningSettings.AllowExceptionRollUp)

                Case CleaningSettings.DefaultValueEnum.UseZeroOrEmptyVal
                    rtnValue = 0
                    rtnValue = GetCleanInt16(pValue, 0, mCleaningSettings.AllowExceptionRollUp)

                Case CleaningSettings.DefaultValueEnum.UseMinVal
                    rtnValue = Int16.MinValue
                    rtnValue = GetCleanInt16(pValue, Int16.MinValue, mCleaningSettings.AllowExceptionRollUp)

                Case CleaningSettings.DefaultValueEnum.UseNullVal
                    rtnValue = Nothing
                    rtnValue = GetCleanInt16(pValue, Nothing, mCleaningSettings.AllowExceptionRollUp)

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
    ''' Takes the Value and tries to convert it to an <see cref="Int16"/><br />
    ''' Any errors hit will always be added to the Exceptions List to be viewed after returning the value
    ''' </summary>
    ''' <param name="pValue">The Value to convert to a <see cref="Int16"/></param>
    ''' <param name="pDefaultValue">Default Value to return if <paramref name="pValue"/> is invalid. <br /> WARNING: Will convert nulls to MinValue of <see cref="Int16"/>.</param>
    ''' <param name="pAllowExceptionRollUp">If = true Will allow errors to propagate through.<br />If = false will catch any errors.</param>
    ''' <returns>A clean valid <see cref="Int16"/> that can be used without fear of nulls</returns>
    Public Function GetCleanInt16(pValue As Object, Optional pDefaultValue As Int16 = Int16.MinValue, Optional pAllowExceptionRollUp As Boolean = False) As Int16
        Dim rtnValue As Int16 = 0

        Try
            Dim pValueCleaned = GetCleanString(pValue, pDefaultValue, True, True,, pAllowExceptionRollUp)

            'Try converting to Int16
            If String.IsNullOrWhiteSpace(pValueCleaned) Then
                rtnValue = pDefaultValue
            Else
                If Int16.TryParse(pValueCleaned, rtnValue) = False Then
                    rtnValue = pDefaultValue
                    Throw New ArgumentException("Parameter " & NameOf(pValue) & " is not parse able with value: " & pValue & vbCrLf & "Returning default value (" & pDefaultValue & ")")
                End If
            End If

        Catch ex As Exception
            Exceptions.Add(ex)
            If pAllowExceptionRollUp = True Then
                Throw
            End If
        End Try

        Return rtnValue
    End Function

    ''' <summary>
    ''' Takes the Value and tries to convert it to an <see cref="Int16"/><br />
    ''' Any errors hit will always be added to the Exceptions List to be viewed after returning the value
    ''' </summary>
    ''' <param name="pValue">The Value to convert to a <see cref="Int16"/></param>
    ''' <returns>A clean valid <see cref="Int16"/> that can be used without fear of nulls</returns>
    Public Function GetCleanInt32(pValue As Object) As Int32
        Dim rtnValue As Int32 = 0

        Try
            Select Case mCleaningSettings.DefaultValue
                Case CleaningSettings.DefaultValueEnum.UseMaxVal
                    rtnValue = Int32.MaxValue
                    rtnValue = GetCleanInt32(pValue, Int32.MaxValue, mCleaningSettings.AllowExceptionRollUp)

                Case CleaningSettings.DefaultValueEnum.UseZeroOrEmptyVal
                    rtnValue = 0
                    rtnValue = GetCleanInt32(pValue, 0, mCleaningSettings.AllowExceptionRollUp)

                Case CleaningSettings.DefaultValueEnum.UseMinVal
                    rtnValue = Int32.MinValue
                    rtnValue = GetCleanInt32(pValue, Int32.MinValue, mCleaningSettings.AllowExceptionRollUp)

                Case CleaningSettings.DefaultValueEnum.UseNullVal
                    rtnValue = Nothing
                    rtnValue = GetCleanInt32(pValue, Nothing, mCleaningSettings.AllowExceptionRollUp)

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
    ''' Takes the Value and tries to convert it to an <see cref="Int32"/><br />
    ''' Any errors hit will always be added to the Exceptions List to be viewed after returning the value
    ''' </summary>
    ''' <param name="pValue">The Value to convert to a <see cref="Int32"/></param>
    ''' <param name="pDefaultValue">Default Value to return if <paramref name="pValue"/> is invalid. <br /> WARNING: Will convert nulls to MinValue of <see cref="Int32"/>.</param>
    ''' <param name="pAllowExceptionRollUp">If = true Will allow errors to propagate through.<br />If = false will catch any errors.</param>
    ''' <returns>A clean valid <see cref="Int32"/> that can be used without fear of nulls</returns>
    Public Function GetCleanInt32(pValue As Object, Optional pDefaultValue As Int32 = Int32.MinValue, Optional pAllowExceptionRollUp As Boolean = False) As Int32
        Dim rtnValue As Int32 = 0

        Try
            Dim pValueCleaned = GetCleanString(pValue, pDefaultValue, True, True,, pAllowExceptionRollUp)

            'Try converting to Int32
            If String.IsNullOrWhiteSpace(pValueCleaned) Then
                rtnValue = pDefaultValue
            Else
                If Int32.TryParse(pValueCleaned, rtnValue) = False Then
                    rtnValue = pDefaultValue
                    Throw New ArgumentException("Parameter " & NameOf(pValue) & " is not parse able with value: " & pValue & vbCrLf & "Returning default value (" & pDefaultValue & ")")
                End If
            End If

        Catch ex As Exception
            Exceptions.Add(ex)
            If pAllowExceptionRollUp = True Then
                Throw
            End If
        End Try

        Return rtnValue
    End Function

    ''' <summary>
    ''' Takes the Value and tries to convert it to an <see cref="Int64"/><br />
    ''' Any errors hit will always be added to the Exceptions List to be viewed after returning the value
    ''' </summary>
    ''' <param name="pValue">The Value to convert to a <see cref="Int64"/></param>
    ''' <returns>A clean valid <see cref="Int64"/> that can be used without fear of nulls</returns>
    Public Function GetCleanInt64(pValue As Object) As Int64
        Dim rtnValue As Int64 = 0

        Try
            Select Case mCleaningSettings.DefaultValue
                Case CleaningSettings.DefaultValueEnum.UseMaxVal
                    rtnValue = Int64.MaxValue
                    rtnValue = GetCleanInt64(pValue, Int64.MaxValue, mCleaningSettings.AllowExceptionRollUp)

                Case CleaningSettings.DefaultValueEnum.UseZeroOrEmptyVal
                    rtnValue = 0
                    rtnValue = GetCleanInt64(pValue, 0, mCleaningSettings.AllowExceptionRollUp)

                Case CleaningSettings.DefaultValueEnum.UseMinVal
                    rtnValue = Int64.MinValue
                    rtnValue = GetCleanInt64(pValue, Int64.MinValue, mCleaningSettings.AllowExceptionRollUp)

                Case CleaningSettings.DefaultValueEnum.UseNullVal
                    rtnValue = Nothing
                    rtnValue = GetCleanInt64(pValue, Nothing, mCleaningSettings.AllowExceptionRollUp)

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
    ''' Takes the Value and tries to convert it to an <see cref="Int64"/><br />
    ''' Any errors hit will always be added to the Exceptions List to be viewed after returning the value
    ''' </summary>
    ''' <param name="pValue">The Value to convert to a <see cref="Int64"/></param>
    ''' <param name="pDefaultValue">Default Value to return if <paramref name="pValue"/> is invalid. <br /> WARNING: Will convert nulls to MinValue of <see cref="Int64"/>.</param>
    ''' <param name="pAllowExceptionRollUp">If = true Will allow errors to propagate through.<br />If = false will catch any errors.</param>
    ''' <returns>A clean valid <see cref="Int64"/> that can be used without fear of nulls</returns>
    Public Function GetCleanInt64(pValue As Object, Optional pDefaultValue As Int64 = Int64.MinValue, Optional pAllowExceptionRollUp As Boolean = False) As Int64
        Dim rtnValue As Int64 = 0

        Try
            Dim pValueCleaned = GetCleanString(pValue, pDefaultValue, True, True,, pAllowExceptionRollUp)

            'Try converting to Int64
            If String.IsNullOrWhiteSpace(pValueCleaned) Then
                rtnValue = pDefaultValue
            Else
                If Int64.TryParse(pValueCleaned, rtnValue) = False Then
                    rtnValue = pDefaultValue
                    Throw New ArgumentException("Parameter " & NameOf(pValue) & " is not parse able with value: " & pValue & vbCrLf & "Returning default value (" & pDefaultValue & ")")
                End If
            End If

        Catch ex As Exception
            Exceptions.Add(ex)
            If pAllowExceptionRollUp = True Then
                Throw
            End If
        End Try

        Return rtnValue
    End Function

    ''' <summary>
    ''' Takes the Value and tries to convert it to a <see cref="Guid"/><br />
    ''' The following <see cref="CleaningSettings.DefaultValueEnum"/> values default to <see cref="Guid.Empty"/><br />
    ''' (<see cref="CleaningSettings.DefaultValueEnum.UseMaxVal"/>, <see cref="CleaningSettings.DefaultValueEnum.UseZeroOrEmptyVal"/>, <see cref="CleaningSettings.DefaultValueEnum.UseMinVal"/>)
    ''' Any errors hit will always be added to the Exceptions List to be viewed after returning the value
    ''' </summary>
    ''' <param name="pValue">The Value to convert to a <see cref="Guid"/></param>
    ''' <returns>A clean valid <see cref="Guid"/> that can be used without fear of nulls</returns>
    Public Function GetCleanGUID(pValue As Object) As Guid
        Dim rtnValue As Guid = Guid.Empty

        Try
            Select Case mCleaningSettings.DefaultValue

                Case CleaningSettings.DefaultValueEnum.UseNullVal
                    rtnValue = Nothing
                    rtnValue = GetCleanGUID(pValue, Nothing, mCleaningSettings.AllowExceptionRollUp)
                Case Else

                    rtnValue = Guid.Empty
                    rtnValue = GetCleanGUID(pValue, Guid.Empty, mCleaningSettings.AllowExceptionRollUp)
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
    ''' Takes the Value and tries to convert it to an <see cref="GUID"/><br />
    ''' The following <see cref="CleaningSettings.DefaultValueEnum"/> values default to <see cref="Guid.Empty"/><br />
    ''' (<see cref="CleaningSettings.DefaultValueEnum.UseMaxVal"/>, <see cref="CleaningSettings.DefaultValueEnum.UseZeroOrEmptyVal"/>, <see cref="CleaningSettings.DefaultValueEnum.UseMinVal"/>)
    ''' Any errors hit will always be added to the Exceptions List to be viewed after returning the value
    ''' </summary>
    ''' <param name="pValue">The Value to convert to a <see cref="GUID"/></param>
    ''' <param name="pDefaultValue">Default Value to return if <paramref name="pValue"/> is invalid. <br /> WARNING: Will convert nulls to MinValue of <see cref="GUID"/>.</param>
    ''' <param name="pAllowExceptionRollUp">If = true Will allow errors to propagate through.<br />If = false will catch any errors.</param>
    ''' <param name="pForceDefaultValue"><see cref="Guid"/> will not allow setting the default of <see cref="Guid"/>  = to anything but null in an optional parameter. Only ever need to set this parameter if you are trying to set the default = to null</param>
    ''' <returns>A clean valid <see cref="GUID"/> that can be used without fear of nulls</returns>
    Public Function GetCleanGUID(pValue As Object, Optional pDefaultValue As Guid = Nothing, Optional pAllowExceptionRollUp As Boolean = False, Optional pForceDefaultValue As Boolean = False) As Guid
        Dim rtnValue As Guid = Guid.Empty

        Try
            Dim pValueCleaned As String = String.Empty

            'Get value cleaned
            If IsNothing(pDefaultValue) Then
                If pForceDefaultValue Then
                    pValueCleaned = GetCleanString(pValue, Nothing, True, True,, pAllowExceptionRollUp)
                Else
                    pValueCleaned = GetCleanString(pValue, Guid.Empty.ToString, True, True,, pAllowExceptionRollUp)
                End If
            Else
                pValueCleaned = GetCleanString(pValue, pDefaultValue.ToString, True, True,, pAllowExceptionRollUp)
            End If

            'Try converting to GUID
            If String.IsNullOrWhiteSpace(pValueCleaned) Then
                rtnValue = pDefaultValue
            Else
                If Guid.TryParse(pValueCleaned, rtnValue) = False Then
                    rtnValue = pDefaultValue
                    Throw New ArgumentException("Parameter " & NameOf(pValue) & " is not parse able with value: " & pValue & vbCrLf & "Returning default value (" & pDefaultValue & ")")
                End If
            End If

        Catch ex As Exception
            Exceptions.Add(ex)
            If pAllowExceptionRollUp = True Then
                Throw
            End If
        End Try

        Return rtnValue
    End Function

    ''' <summary>
    ''' Takes the Value and tries to convert it to an <see cref="DateTime"/><br />
    ''' Any errors hit will always be added to the Exceptions List to be viewed after returning the value
    ''' </summary>
    ''' <param name="pValue">The Value to convert to a <see cref="DateTime"/></param>
    ''' <returns>A clean valid <see cref="DateTime"/> that can be used without fear of nulls</returns>
    Public Function GetCleanDateTime(pValue As Object) As DateTime
        Dim rtnValue As DateTime = DateTime.Now

        Try
            Select Case mCleaningSettings.DefaultValue
                Case CleaningSettings.DefaultValueEnum.UseMaxVal
                    rtnValue = DateTime.MaxValue
                    rtnValue = GetCleanDateTime(pValue, DateTime.MaxValue, mCleaningSettings.AllowExceptionRollUp)

                Case CleaningSettings.DefaultValueEnum.UseZeroOrEmptyVal
                    rtnValue = New DateTime(1900, 1, 1)
                    rtnValue = GetCleanDateTime(pValue, DateTime.Now, mCleaningSettings.AllowExceptionRollUp)

                Case CleaningSettings.DefaultValueEnum.UseMinVal
                    rtnValue = DateTime.MinValue
                    rtnValue = GetCleanDateTime(pValue, DateTime.MinValue, mCleaningSettings.AllowExceptionRollUp)

                Case CleaningSettings.DefaultValueEnum.UseNullVal
                    rtnValue = Nothing
                    rtnValue = GetCleanDateTime(pValue, Nothing, mCleaningSettings.AllowExceptionRollUp)

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
    ''' Takes the Value and tries to convert it to an <see cref="DateTime"/><br />
    ''' Any errors hit will always be added to the Exceptions List to be viewed after returning the value
    ''' </summary>
    ''' <param name="pValue">The Value to convert to a <see cref="DateTime"/></param>
    ''' <param name="pDefaultValue">Default Value to return if <paramref name="pValue"/> is invalid. If you want the default to equal nothing please set <paramref name="pForceDefaultValue"/> = to true</param>
    ''' <param name="pAllowExceptionRollUp">If = true Will allow errors to propagate through.<br />If = false will catch any errors.</param>
    ''' <param name="pForceDefaultValue"><see cref="DateTime"/> will not allow setting the default of <see cref="DateTime"/> = to anything but null in an optional parameter. Only ever need to set this parameter if you are trying to set the default = to null</param>
    ''' <returns>A clean valid <see cref="DateTime"/> that can be used without fear of nulls</returns>
    Public Function GetCleanDateTime(pValue As Object, Optional pDefaultValue As DateTime = Nothing, Optional pAllowExceptionRollUp As Boolean = False, Optional pForceDefaultValue As Boolean = False) As DateTime
        Dim rtnValue As New DateTime(1900, 1, 1)

        Try
            If IsNothing(pDefaultValue) AndAlso pForceDefaultValue = False Then
                pDefaultValue = New DateTime(1900, 1, 1)
            End If

            Dim pValueCleaned = GetCleanString(pValue, pDefaultValue, True, True,, pAllowExceptionRollUp)

            'Try converting to DateTime
            If String.IsNullOrWhiteSpace(pValueCleaned) Then
                rtnValue = pDefaultValue
            Else
                If DateTime.TryParse(pValueCleaned, rtnValue) = False Then
                    rtnValue = pDefaultValue
                    Throw New ArgumentException("Parameter " & NameOf(pValue) & " is not parse able with value: " & pValue & vbCrLf & "Returning default value (" & pDefaultValue & ")")
                End If
            End If

        Catch ex As Exception
            Exceptions.Add(ex)
            If pAllowExceptionRollUp = True Then
                Throw
            End If
        End Try

        Return rtnValue
    End Function

    ''' <summary>
    ''' Takes the Value and tries to convert it to an <see cref="DateTimeOffset"/><br />
    ''' Any errors hit will always be added to the Exceptions List to be viewed after returning the value
    ''' </summary>
    ''' <param name="pValue">The Value to convert to a <see cref="DateTimeOffset"/></param>
    ''' <returns>A clean valid <see cref="DateTimeOffset"/> that can be used without fear of nulls</returns>
    Public Function GetCleanDateTimeOffset(pValue As Object) As DateTimeOffset
        Dim rtnValue As DateTimeOffset = DateTimeOffset.Now

        Try
            Select Case mCleaningSettings.DefaultValue
                Case CleaningSettings.DefaultValueEnum.UseMaxVal
                    rtnValue = DateTimeOffset.MaxValue
                    rtnValue = GetCleanDateTimeOffset(pValue, DateTimeOffset.MaxValue, mCleaningSettings.AllowExceptionRollUp)

                Case CleaningSettings.DefaultValueEnum.UseZeroOrEmptyVal
                    rtnValue = New DateTime(1900, 1, 1)
                    rtnValue = GetCleanDateTimeOffset(pValue, DateTimeOffset.Now, mCleaningSettings.AllowExceptionRollUp)

                Case CleaningSettings.DefaultValueEnum.UseMinVal
                    rtnValue = DateTimeOffset.MinValue
                    rtnValue = GetCleanDateTimeOffset(pValue, DateTimeOffset.MinValue, mCleaningSettings.AllowExceptionRollUp)

                Case CleaningSettings.DefaultValueEnum.UseNullVal
                    rtnValue = Nothing
                    rtnValue = GetCleanDateTimeOffset(pValue, Nothing, mCleaningSettings.AllowExceptionRollUp)

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
    ''' Takes the Value and tries to convert it to an <see cref="DateTimeOffset"/><br />
    ''' Any errors hit will always be added to the Exceptions List to be viewed after returning the value
    ''' </summary>
    ''' <param name="pValue">The Value to convert to a <see cref="DateTimeOffset"/></param>
    ''' <param name="pDefaultValue">Default Value to return if <paramref name="pValue"/> is invalid. <br /> WARNING: Will convert nulls to DateTimeOffset of 1/1/1900.</param>
    ''' <param name="pAllowExceptionRollUp">If = true Will allow errors to propagate through.<br />If = false will catch any errors.</param>
    ''' <param name="pForceDefaultValue"><see cref="DateTimeOffset"/>  will not allow setting the default of <see cref="DateTimeOffset"/>  = to anything but null in an optional parameter. Only ever need to set this parameter if you are trying to set the default = to null</param>
    ''' <returns>A clean valid <see cref="DateTimeOffset"/> that can be used without fear of nulls</returns>
    Public Function GetCleanDateTimeOffset(pValue As Object, Optional pDefaultValue As DateTimeOffset = Nothing, Optional pAllowExceptionRollUp As Boolean = False, Optional pForceDefaultValue As Boolean = False) As DateTimeOffset
        Dim rtnValue As DateTimeOffset = New DateTime(1900, 1, 1)

        Try
            If IsNothing(pDefaultValue) AndAlso pForceDefaultValue = False Then
                pDefaultValue = New DateTime(1900, 1, 1)
            End If
            Dim pValueCleaned = GetCleanString(pValue, pDefaultValue.ToString, True, True,, pAllowExceptionRollUp)

            'Try converting to DateTimeOffset
            If String.IsNullOrWhiteSpace(pValueCleaned) Then
                rtnValue = pDefaultValue
            Else
                If DateTimeOffset.TryParse(pValueCleaned, rtnValue) = False Then
                    rtnValue = pDefaultValue
                    Throw New ArgumentException("Parameter " & NameOf(pValue) & " is not parse able with value: " & pValue & vbCrLf & "Returning default value (" & pDefaultValue & ")")
                End If
            End If

        Catch ex As Exception
            Exceptions.Add(ex)
            If pAllowExceptionRollUp = True Then
                Throw
            End If
        End Try

        Return rtnValue
    End Function


    ''' <summary>
    ''' Takes the Value and tries to convert it to an <see cref="Boolean"/><br />
    ''' Defaults to <see cref="Boolean" langword=".False"/> if <see cref="CleaningSettings.DefaultValueEnum.UseNullVal"/> is NOT set<br />
    ''' Any errors hit will always be added to the Exceptions List to be viewed after returning the value
    ''' </summary>
    ''' <param name="pValue">The Value to convert to a <see cref="Boolean"/></param>
    ''' <returns>A clean valid <see cref="Boolean"/> that can be used without fear of nulls</returns>
    Public Function GetCleanBoolean(pValue As Object) As Boolean
        Dim rtnValue As Boolean = 0

        Try
            Select Case mCleaningSettings.DefaultValue
                Case CleaningSettings.DefaultValueEnum.UseNullVal
                    rtnValue = Nothing
                    rtnValue = GetCleanBoolean(pValue, Nothing, mCleaningSettings.AllowExceptionRollUp)
                Case Else
                    rtnValue = False
                    rtnValue = GetCleanBoolean(pValue, False, mCleaningSettings.AllowExceptionRollUp)

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
    ''' Takes the Value and tries to convert it to an <see cref="Boolean"/><br />
    ''' Defaults to <see cref="Boolean" langword=".False"/><br />
    ''' Any errors hit will always be added to the Exceptions List to be viewed after returning the value
    ''' </summary>
    ''' <param name="pValue">The Value to convert to a <see cref="Boolean"/></param>
    ''' <param name="pDefaultValue">Default Value to return if <paramref name="pValue"/> is invalid. <br />Defaults to <see cref="Boolean" langword=".False" />.</param>
    ''' <param name="pAllowExceptionRollUp">If = true Will allow errors to propagate through.<br />If = false will catch any errors.</param>
    ''' <returns>A clean valid <see cref="Boolean"/> that can be used without fear of nulls</returns>
    Public Function GetCleanBoolean(pValue As Object, Optional pDefaultValue As Boolean = False, Optional pAllowExceptionRollUp As Boolean = False) As Boolean
        Dim rtnValue As Boolean = 0

        Try
            'Clean and put to upper to make job easier down below
            Dim pValueCleaned = GetCleanString(pValue, pDefaultValue, True, True,, pAllowExceptionRollUp).ToUpper

            'Try converting to Boolean
            If String.IsNullOrWhiteSpace(pValueCleaned) Then
                rtnValue = pDefaultValue
            Else
                If Boolean.TryParse(pValueCleaned, rtnValue) = False Then
                    'Direct convert did not work try some different combinations
                    If pValueCleaned.StartsWith("F") OrElse pValueCleaned.StartsWith("N") Then
                        Return False
                    ElseIf pValueCleaned.StartsWith("T") OrElse pValueCleaned.StartsWith("Y") Then
                        Return True
                    ElseIf pValueCleaned = "1" OrElse pValueCleaned = "-1" Then
                        Return True
                    ElseIf pValueCleaned = "0" Then
                        Return False
                    End If

                    'No direct parse set and log it.
                    rtnValue = pDefaultValue
                    Throw New ArgumentException("Parameter " & NameOf(pValue) & " is not parse able with value: " & pValue & vbCrLf & "Returning default value (" & pDefaultValue & ")")
                End If
            End If

        Catch ex As Exception
            Exceptions.Add(ex)
            If pAllowExceptionRollUp = True Then
                Throw
            End If
        End Try

        Return rtnValue
    End Function

End Class
