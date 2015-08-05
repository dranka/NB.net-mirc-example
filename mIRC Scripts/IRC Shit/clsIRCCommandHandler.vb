'// *******************************************************************************
'// Title               : IRC - Command Handler
'// Author              : Kjell Arne Brudvik
'// Contact information : brudvik@online.no
'// Last modified       : 3th of July 2006
'// License             : This is released under the GNU-GPL license and are free
'//                       to be used by anyone that finds interrest in this project
'//                       the only thing I ask is that these notices are left
'//                       here without alterations. It would also be nice if you
'//                       write me a small email telling me about the project you
'//                       found this useful for, but that is merely becuase of my
'//                       own interrest in what people would use this for.
'// *******************************************************************************

''' <summary>
''' This class handles all the functions that relates to picking up commands
''' that are being sent to the bot. It will check if the incoming text
''' contains the command interpreter that has been set (default is !)
''' and if found, make a call to the event listener onCommand.
''' </summary>
Public Class clsIRCCommandHandler

    Private clsIRC As clsIRCHandler

    ''' <summary>
    ''' When a command is picked up, its directed to this event listener along
    ''' with attributes; channel, nickname, ident, hostname, command and
    ''' arguments.
    ''' </summary>
    Public Event onCommand(ByVal channel As String, ByVal nickname As String, ByVal ident As String, ByVal hostname As String, ByVal command As String, ByVal arguments As String)

    ''' <summary>
    ''' Creates a new instance of this class. Requires an instance of the
    ''' IRC handler to be supplied as an argument.
    ''' </summary>
    Public Sub New(ByVal ircHandler As clsIRCHandler)
        Me.clsIRC = ircHandler
    End Sub

    ''' <summary>
    ''' The command handler, it will check the incoming data and either pick up
    ''' the command issued, or if any pre-defined command like PING it will
    ''' act accordingly.
    ''' </summary>
    Public Sub commandHandler(ByVal channel As String, ByVal nickname As String, ByVal ident As String, ByVal hostname As String, ByVal command As String, ByVal arguments As String)
        Dim strCommand As String = UCase(command)
        If strCommand = "PING" Then
            clsIRC.SendData(clsIRC.StringToBytes("PONG :" & Mid(arguments, 2) & vbCrLf))
        ElseIf strCommand.Contains(clsIRC.CommandInterpreter) = True Then
            RaiseEvent onCommand(channel, nickname, ident, hostname, strCommand, arguments)
        End If
    End Sub

End Class
