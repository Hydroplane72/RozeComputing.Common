Namespace Database
    Public Class DataTableAutoIncrementSettings

        ''' <summary>
        ''' Is the column supposed to auto increment<br />
        ''' Default: false
        ''' </summary>
        Private mIsAutoIncrement As Boolean = False
        Public Property IsAutoIncrement() As Boolean
            Get
                If IsNothing(mIsAutoIncrement) Then
                    mIsAutoIncrement = False
                End If

                Return mIsAutoIncrement
            End Get
            Set(ByVal value As Boolean)
                If IsNothing(value) Then
                    value = False
                End If

                mIsAutoIncrement = value
            End Set
        End Property

        Private mAutoIncrementSeed As Long = 0
        ''' <summary>
        ''' The starting point to increment at<br />
        ''' Default: 0
        ''' </summary>
        Public Property AutoIncrementSeed() As Long
            Get
                If IsNothing(mAutoIncrementSeed) Then
                    mAutoIncrementSeed = 0
                End If

                Return mAutoIncrementSeed
            End Get
            Set(ByVal value As Long)
                If IsNothing(value) Then
                    value = 0
                End If

                mAutoIncrementSeed = value
            End Set
        End Property


        Private mAutoIncrementSteps As Long = 1
        ''' <summary>
        ''' The amount to increment each new row<br />
        ''' Default: 1
        ''' </summary>
        Public Property AutoIncrementStep() As Long
            Get
                If IsNothing(mAutoIncrementSteps) Then
                    mAutoIncrementSteps = 1
                End If

                Return mAutoIncrementSteps
            End Get
            Set(ByVal value As Long)
                If IsNothing(value) Then
                    value = 0
                End If

                mAutoIncrementSteps = value
            End Set
        End Property
    End Class
End Namespace