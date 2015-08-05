'// *******************************************************************************
'// Title               : IRC - Message Handler
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
''' This class handles all the functions that relates to sending and generating
''' IRC messages both with and without formatting. This class should be used
''' when using the other classes, to ease the way of generating the PRIVMSG
''' data strings.
''' </summary>
Public Class clsIRCMessageHandler

    Private clsIRC As clsIRCHandler
    Private strBold As String = ""
    Private strColor As String = ""
    Private intMaxMessageLength As Integer = 300

    ''' <summary>
    ''' The enumeration list of colors that are available when generating
    ''' a message to be sent out with the IRC bot.
    ''' </summary>
    Public Enum strColors
        BLACK = 0
        GREY = 1
        DARK_BLUE = 2
        DARK_GREEN = 3
        RED = 4
        DARK_RED = 5
        PURPLE = 6
        ORANGE = 7
        BRIGHT_ORANGE = 8
        BRIGHT_GREEN = 9
        BLUE = 10
        BRIGHT_BLUE = 11
        DARKER_BLUE = 12
        DARK_PURPLE = 13
        DARK_GREY = 14
        DARKER_GREY = 15
    End Enum

    ''' <summary>
    ''' If any error occurs while either sending or generating messages then
    ''' it will be picked up by this event listener.
    ''' </summary>
    Public Event onError(ByVal description As String)

    ''' <summary>
    ''' When the message has been delivered in a single stream, then this
    ''' event listener will be called - with the destination and text
    ''' that was sent.
    ''' </summary>
    Public Event onMessageDelivered(ByVal destination As String, ByVal text As String)

    ''' <summary>
    ''' When the message has been delivered in several streams, then this
    ''' event listener will be called - with the destination, text and parts count
    ''' that was sent.
    ''' </summary>
    Public Event onMessageSplitDelivered(ByVal destination As String, ByVal text As String, ByVal parts As Integer)

    ''' <summary>
    ''' When a message has successfully been generated, then this event
    ''' listener will be called, with the text generated a long with any
    ''' formatting you have asked for.
    ''' </summary>
    Public Event onMessageGenerated(ByVal text As String, ByVal useBold As Boolean, ByVal useColor As Boolean, ByVal textcolor As strColors)

    ''' <summary>
    ''' Set the max message length the server accepts. This can be different
    ''' on the different IRC daemons, so make sure you check what this could
    ''' be on the server you want to operate this bot on. If you are unsure,
    ''' then leave it at the default setting (300 chars).
    ''' </summary>
    Public Property MaxMessageLength() As Integer
        Get
            Return intMaxMessageLength
        End Get
        Set(ByVal value As Integer)
            intMaxMessageLength = value
        End Set
    End Property

    ''' <summary>
    ''' Creates a new instance of this class. Requires an instance of the
    ''' IRC handler to be supplied as an argument.
    ''' </summary>
    Public Sub New(ByVal ircHandler As clsIRCHandler)
        Me.clsIRC = ircHandler
    End Sub

    ''' <summary>
    ''' Generate a message string, formatting the text with either bold or
    ''' colors if wanted. Default setting is plain text.
    ''' </summary>
    Public Function GenerateMessagePart(ByVal text As String, ByVal useBold As Boolean, ByVal useColor As Boolean, Optional ByVal textcolor As strColors = strColors.BLACK) As String
        Try
            Dim tmpString As String = text
            If useBold = True Then tmpString = strBold & text & strBold
            If useColor = True Then
                tmpString = strColor & textcolor & tmpString & strColor
            End If
            RaiseEvent onMessageGenerated(text, useBold, useColor, textcolor)
            Return tmpString
        Catch ex As Exception
            RaiseEvent onError(ex.ToString)
            Return ""
        End Try
    End Function

    ''' <summary>
    ''' Send a message to a destination (channel or nickname). The message will
    ''' be split over several lines if its longer than the max message length
    ''' that is defined (default 300 chars). It will attempt to split the string
    ''' in a smart way, by determining where the nearest space is located, and
    ''' then split it there.
    ''' </summary>
    Public Sub SendMessage(ByVal destination As String, ByVal text As String)
        Try
            If text.Length >= intMaxMessageLength Then
                Dim intTotalLength As Double = Math.Ceiling(text.Length / intMaxMessageLength)
                Dim intGetLength As Double = Math.Ceiling(intMaxMessageLength / intTotalLength)
                Dim intOffset As Integer = 1
                Dim strTempString As String = ""
                Dim I As Long = 0
                For I = 1 To intTotalLength Step 1
                    If Not I = intTotalLength Then
                        intGetLength = intMaxMessageLength
                        strTempString = Mid(text, intOffset, intGetLength)
                        Dim intSeparator As Integer = strTempString.LastIndexOf(" ")
                        intGetLength = intSeparator
                    End If
                    strTempString = Mid(text, intOffset, intGetLength)
                    clsIRC.SendData(clsIRC.StringToBytes("PRIVMSG " & destination & " :" & strTempString & vbCrLf))
                    intOffset += intGetLength
                Next
                RaiseEvent onMessageSplitDelivered(destination, text, intTotalLength)
            Else
                clsIRC.SendData(clsIRC.StringToBytes("PRIVMSG " & destination & " :" & text & vbCrLf))
                RaiseEvent onMessageDelivered(destination, text)
            End If
        Catch ex As Exception
            RaiseEvent onError(ex.ToString)
        End Try
    End Sub

End Class
