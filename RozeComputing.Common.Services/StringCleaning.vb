Imports RozeComputing.Common.Models

''' <summary>
''' Allows for the passing in of values and will return a string. <br />
''' </summary>
Public Class StringCleaning
    Implements IRozeCompliance

#Region "Variables"
    Private mClearExceptionsBeforeEachCall As Boolean = False
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
    ''' Creates the string cleaning object
    ''' </summary>
    ''' <param name="pClearExceptionsBeforeEachCall">Set true if you don't care about looking at <see cref="Exceptions"/> for issues.</param>
    Sub New(Optional pClearExceptionsBeforeEachCall As Boolean = False)
        mClearExceptionsBeforeEachCall = pClearExceptionsBeforeEachCall
    End Sub

#End Region


    ''' <summary>
    ''' Takes the value and tries to convert it to a string.<br />
    ''' Any errors hit will always be added to the Exceptions List to be viewed after returning the value
    ''' </summary>
    ''' <param name="pValue">The Value to convert to a string</param>
    ''' <param name="pDefaultValue">Default Value to return if <paramref name="pValue"/> is invalid. <br /> WARNING: Will convert nulls to empty string.</param>
    ''' <param name="pTrimLeftOfValue">Trims the left side of the return value</param>
    ''' <param name="pTrimRightOfValue">Trims the right side of the return value</param>
    ''' <param name="pLengthLimit">Option to limit the length of the returned string. <br />0=unlimited length</param>
    ''' <param name="pThrowExceptions">If = true Will allow errors to propagate through.<br />If = false will catch any errors.</param>
    ''' <returns>A clean valid <see cref="String"/> that can be used without fear of nulls</returns>
    Public Function ReturnCleanString(pValue As Object, Optional pDefaultValue As String = "", Optional pTrimLeftOfValue As Boolean = False, Optional pTrimRightOfValue As Boolean = False, Optional pLengthLimit As Long = 0, Optional pThrowExceptions As Boolean = False) As String
        Dim rtnValue As String = String.Empty

        Try
            'Clear Exceptions
            If mClearExceptionsBeforeEachCall = True Then
                Exceptions.Clear()
            End If

            'Make sure default is good
            If IsNothing(pDefaultValue) Then
                pDefaultValue = String.Empty
            End If

            'Set rtnValue
            If IsNothing(pValue) Then
                Return pDefaultValue
            Else
                rtnValue = pValue.ToString()
            End If

            'Trim the value before we limit it

            'Limit the length of rtnValue if needed
            If pLengthLimit <> 0 AndAlso rtnValue.Length > pLengthLimit Then
                rtnValue = Left(rtnValue, pLengthLimit)
            End If

        Catch ex As Exception
            Exceptions.Add(ex)
            If pThrowExceptions = True Then
                Throw
            End If
        End Try

        Return rtnValue
    End Function


End Class
