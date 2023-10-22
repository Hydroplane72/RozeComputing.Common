''' <summary>
''' All valid Roze Service classes must use this to provide a uniform method of use.
''' </summary>
Public Interface IRozeCompliance

    ''' <summary>
    ''' If any errors are hit while cleaning. 
    ''' Exceptions will accumulate within this list. Log this list how you wish. 
    ''' This list is not persisted anywhere except memory.
    ''' </summary>
    ''' <returns>A list of Exceptions Handled</returns>
    Property Exceptions As List(Of Exception)
End Interface
