Namespace Cleaning
    Public Class CleaningSettings

        ''' <summary>
        ''' Allows for the cleaner to know what spectrum to default values to without needing to do so for every call
        ''' </summary>
        Enum DefaultValueEnum
            ''' <summary>
            ''' Tells the cleaner to use the absolute minimum value possible for the object type.
            ''' </summary>
            UseMinVal = 1

            ''' <summary>
            ''' Tells the cleaner to use the absolute maximum value possible for the object type.
            ''' </summary>
            UseMaxVal = 2

            ''' <summary>
            ''' Tells the cleaner to use the zero default for the object type <br />
            ''' <see cref="String"/> = String.Empty <br />
            ''' <see cref="Double"/> = 0 <br />
            ''' <see cref="Integer"/> = 0 <br />
            ''' Etc.
            ''' </summary>
            UseZeroOrEmptyVal = 3

            ''' <summary>
            ''' Tells the cleaner to use null as the default value for the object type. <br />
            ''' NOT RECOMMENDED: Choosing this option allows for null values to be returned from cleaning.
            ''' </summary>
            UseNullVal = 4
        End Enum


        Private mDefaultValue As DefaultValueEnum = DefaultValueEnum.UseZeroOrEmptyVal
        ''' <summary>
        ''' Tells the cleaner what default value to use if you do not set for the function. Some return types do not use have viable min or max values.
        ''' For example: <br />
        ''' <see cref="String"/> does not have a viable min or max value. The only settings payed attention to will be <see cref="DefaultValueEnum.UseZeroOrEmptyVal"/> and <see cref="DefaultValueEnum.UseNullVal"/>
        ''' </summary>
        Public Property DefaultValue() As DefaultValueEnum
            Get
                If IsNothing(mDefaultValue) Then
                    mDefaultValue = DefaultValueEnum.UseZeroOrEmptyVal
                End If
                Return mDefaultValue
            End Get
            Set(ByVal value As DefaultValueEnum)
                If IsNothing(value) Then
                    value = DefaultValueEnum.UseZeroOrEmptyVal
                End If
                mDefaultValue = value
            End Set
        End Property


        Private mAllowExceptionRollUp As Boolean = False
        ''' <summary>
        ''' Defaults to false <br />
        ''' Set to true if you wish any exceptions to roll out instead of just being logged.
        ''' </summary>
        ''' <returns></returns>
        Public Property AllowExceptionRollUp() As Boolean
            Get
                If IsNothing(mAllowExceptionRollUp) Then
                    mAllowExceptionRollUp = False
                End If

                Return mAllowExceptionRollUp
            End Get
            Set(ByVal value As Boolean)
                If IsNothing(value) Then
                    value = False
                End If

                mAllowExceptionRollUp = value
            End Set
        End Property

        Private mClearExceptionsAfterEachCall As Boolean = True
        ''' <summary>
        ''' Defaults to true <br />
        ''' Set to false if you wish to not clear the Exception List after each call
        ''' </summary>
        ''' <returns></returns>
        Public Property ClearExceptionsAfterEachCall() As Boolean
            Get
                If IsNothing(mClearExceptionsAfterEachCall) Then
                    mClearExceptionsAfterEachCall = True
                End If

                Return mClearExceptionsAfterEachCall
            End Get
            Set(ByVal value As Boolean)
                If IsNothing(value) Then
                    value = True
                End If

                mClearExceptionsAfterEachCall = value
            End Set
        End Property


    End Class
End Namespace