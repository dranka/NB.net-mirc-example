'// *******************************************************************************
'// Title               : IRC - Channel Handler
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
''' This class handles all the functions that relates to bot handling.
''' </summary>
Public Class clsIRCBotHandler

    Private clsIRC As clsIRCHandler

    ''' <summary>
    ''' Creates a new instance of this class. Requires an instance of the
    ''' IRC handler to be supplied as an argument.
    ''' </summary>
    Public Sub New(ByVal ircHandler As clsIRCHandler)
        Me.clsIRC = ircHandler
    End Sub

    Public Event onNickChange(ByVal nickname As String)
    Public Event onNickChangeError(ByVal description As String)
    Public Event onError(ByVal description As String)

    Public Sub ChangeNickname(ByVal nickname As String)
        Try
            If nickname.Trim = "" Then
                RaiseEvent onNickChangeError("New nickname not supplied")
            Else
                clsIRC.SendData(clsIRC.StringToBytes("NICK " & nickname & vbCrLf))
                clsIRC.Nickname = nickname
                RaiseEvent onNickChange(nickname)
            End If
        Catch ex As Exception
            RaiseEvent onError(ex.ToString)
        End Try
    End Sub

End Class
