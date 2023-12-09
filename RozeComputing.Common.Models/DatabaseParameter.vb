Namespace Database
    Public Class DatabaseParameter
        ''' <summary>
        ''' Initialize the class with values instead of having to set each individually
        ''' </summary>
        ''' <param name="pColumnName"><see cref="ColumnName"/></param>
        ''' <param name="pValue"><see cref="Value"/></param>
        Public Sub New(pColumnName As String, pValue As Object, Optional pComparisonOperator As String = "=")
            ColumnName = pColumnName
            Value = pValue
            ComparisonOperator = pComparisonOperator
        End Sub

        'Enum ParameterDataType
        '    NotSet = 0
        '    IsString = 1
        '    IsDouble = 2
        '    IsDecimal = 3
        '    IsDateTime = 4
        '    IsBoolean = 5
        '    IsGUID = 6
        'End Enum

        ''' <summary>
        ''' Name of the Parameter in the sql statement. <br />
        ''' In the SQL Query the parameter name must start with @ to help differentiate between parameter and column names. 
        ''' For ex: Select * From tableName where [ColumnName] = <bold>@ParameterName</bold>
        ''' </summary>
        ''' <example></example>
        ''' <returns></returns>
        Public Property ColumnName As String = String.Empty

        ''' <summary>
        ''' The value of the Parameter. Most databases types are able to convert to the correct datatype. If have issues then set 
        ''' </summary>
        ''' <returns></returns>
        Public Property Value As Object = New Object

        ''' <summary>
        ''' Allows for different comparisons between ColumnName and value.<br />
        ''' </summary>
        ''' <returns>Defaults to "="</returns>
        Public Property ComparisonOperator As String = "="
    End Class
End Namespace