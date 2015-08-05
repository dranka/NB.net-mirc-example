'// *******************************************************************************
'// Title               : IRC - Parse Handler
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
''' This class handles all the functions that relates to the parsing of
''' IRC data. To determine what is being sent to and from the IRC Bot.
''' </summary>
Public Class clsIRCParseHandler

    ''' <summary>
    ''' List of current supported server/command replies.
    ''' </summary>
    ''' <remarks></remarks>
    Public Enum strCommandErrors
        SERVER_WELCOME = 1
        SERVER_HOST = 2
        SERVER_CREATED = 3
        SERVER_INFORMATION = 4
        SERVER_FULL = 5
        WAIT_AND_TRY_AGAIN = 263
        MARKED_AS_AWAY = 305
        MARKED_AS_BACK = 306
        NO_TOPIC_SET = 331
        CHANNEL_TOPIC = 332
        SERVER_DETAILS = 351
        INVITE_SUCCESSFUL = 341
        NO_SUCH_NICNAME = 401
        NO_SUCH_SERVER = 402
        NO_SUCH_CHANNEL = 403
        CANNOT_SEND_TO_CHANNEL = 404
        TOO_MANY_CHANNELS = 405
        WAS_NO_SUCH_USER = 406
        TOO_MANY_TARGETS = 407
        NO_SUCH_SERVICE = 408
        NO_NICKNAME_GIVEN = 431
        ERRONEUSE_NICNAME = 432
        NICKNAME_IN_USE = 433
        NICKNAME_CHANNEL_TEMPORARY_UNAVAILABLE = 437
        YOU_ARE_NOT_ON_THAT_CHANNEL = 442
        USER_ALREADY_ON_CHANNEL = 443
        USER_COULD_NOT_LOGIN = 444
        YOU_ARE_BANNED_FROM_SERVER = 465
        CHANNEL_KEY_ALREADY_SET = 467
        CANNOT_JOIN_FULL_CHANNEL = 471
        CANNOT_JOIN_INVITE_CHANNEL = 473
        CANNOT_JOIN_BANNED_FROM_CHANNEL = 474
        CANNOT_JOIN_KEYED_CHANNEL = 475
        NOT_IRC_OPERATOR = 481
    End Enum

    ''' <summary>
    ''' Is raised when the parse routine finds that the incoming text could
    ''' very well be a command.
    ''' </summary>
    Public Event onIncomingCommand(ByVal channel As String, ByVal nickname As String, ByVal ident As String, ByVal hostname As String, ByVal command As String, ByVal arguments As String)

    ''' <summary>
    ''' Is raised when the parse routine finds a error related feedback.
    ''' </summary>
    Public Event onCommandError(ByVal source As String, ByVal nickname As String, ByVal reply As String, ByVal replyinfo As strCommandErrors, ByVal replycode As String)

    ''' <summary>
    ''' Is raised when a server reply is caught (not errors, those are handled in onCommandError)
    ''' </summary>
    Public Event onServerReply(ByVal source As String, ByVal nickname As String, ByVal reply As String, ByVal replyinfo As strCommandErrors, ByVal replycode As String)

    ''' <summary>
    ''' Is raised when the bot or someone else parts a channel.
    ''' </summary>
    Public Event onPart(ByVal channel As String, ByVal nickname As String, ByVal ident As String, ByVal hostname As String)

    ''' <summary>
    ''' Is raised when the bot or someone else joins a channel.
    ''' </summary>
    Public Event onJoin(ByVal channel As String, ByVal nickname As String, ByVal ident As String, ByVal hostname As String)

    ''' <summary>
    ''' Is raised when an error occurs in this class.
    ''' </summary>
    Public Event onError(ByVal Description As String)

    ''' <summary>
    ''' Go through the incoming data and check if we find any useful data
    ''' in there. The parse routine will pick up elements like PING, JOIN,
    ''' PART and PRIVMSG amongst others and raise events to signal them
    ''' different commands.
    ''' </summary>
    Public Sub parseIncomingReply(ByVal incomingData As String)
        Try
            Dim strLineDataEntries() As String = Split(incomingData, " ")
            Dim strLineDataEntry As String
            Dim I As Long = 0
            For I = 0 To UBound(strLineDataEntries) - 1 Step 1
                strLineDataEntry = strLineDataEntries(I)
                If UCase(strLineDataEntry) = "PING" Then
                    Dim strHostname As String = strLineDataEntries(I + 1)
                    RaiseEvent onIncomingCommand("", "", "", "", "PING", strHostname)
                End If
                If UCase(strLineDataEntry) = "JOIN" Then
                    Dim strUserdata As String = strLineDataEntries(I - 1)
                    Dim strChannel As String = stripLineBreaks(strLineDataEntries(I + 1))
                    If strChannel.Contains(":") = True Then strChannel = Mid(strChannel, 2, Len(strChannel))
                    RaiseEvent onJoin(strChannel, getNickname(strUserdata), getUserIdent(strUserdata), getUserHostname(strUserdata))
                End If
                If UCase(strLineDataEntry) = "PART" Then
                    Dim strUserdata As String = strLineDataEntries(I - 1)
                    Dim strChannel As String = stripLineBreaks(strLineDataEntries(I + 1))
                    If strChannel.Contains(":") = True Then strChannel = Mid(strChannel, 2, Len(strChannel))
                    RaiseEvent onPart(strChannel, getNickname(strUserdata), getUserIdent(strUserdata), getUserHostname(strUserdata))
                End If
                If UCase(strLineDataEntry) = "PRIVMSG" Then
                    Dim strUserdata As String = strLineDataEntries(I - 1)
                    Dim strNickname As String = getNickname(strUserdata)
                    Dim strUserIdent As String = getUserIdent(strUserdata)
                    Dim strUserHostname As String = getUserHostname(strUserdata)
                    Dim strChannel As String = strLineDataEntries(I + 1)
                    If Not strChannel.Contains("#") Then strChannel = strNickname
                    Dim strCommand As String = stripLineBreaks(strLineDataEntries(I + 2))
                    strCommand = Mid(strCommand, 2, Len(strCommand))
                    Dim strArguments() As String = getArguments(strLineDataEntries, I + 3)
                    Dim strLineArguments As String = getLineArguments(strArguments)
                    RaiseEvent onIncomingCommand(strChannel, strNickname, strUserIdent, strUserHostname, strCommand, strLineArguments)
                End If
                Call getCommandReplies(strLineDataEntry, stripLineBreaks(incomingData.Trim), stripLineBreaks(strLineDataEntries(I + 1)), stripLineBreaks(strLineDataEntries(I + 2)))
            Next
        Catch ex As Exception
            RaiseEvent onError(ex.ToString)
        End Try
    End Sub

    ''' <summary>
    ''' Check what the commands/server replies.
    ''' </summary>
    Private Sub getCommandReplies(ByVal replycode As String, ByVal incomingdata As String, ByVal source As String, ByVal nickname As String)
        Try
            If source.Contains(":") = True Then source = Mid(source, 2, Len(source))
            Dim intReplyStart As Long = incomingdata.LastIndexOf(":")
            Dim strReplyMessages() = Trim(incomingdata).Split(Chr(13))
            If strReplyMessages.Length > 0 Then incomingdata = strReplyMessages(0)
            Dim strReplyMessage = stripLineBreaks(Mid(Trim(incomingdata), intReplyStart + 2, Len(incomingdata)))

            If replycode = "001" Then
                RaiseEvent onServerReply(source, nickname, strReplyMessage, strCommandErrors.SERVER_WELCOME, replycode)
            ElseIf replycode = "002" Then
                RaiseEvent onServerReply(source, nickname, strReplyMessage, strCommandErrors.SERVER_HOST, replycode)
            ElseIf replycode = "003" Then
                RaiseEvent onServerReply(source, nickname, strReplyMessage, strCommandErrors.SERVER_CREATED, replycode)
            ElseIf replycode = "004" Then
                RaiseEvent onServerReply(source, nickname, strReplyMessage, strCommandErrors.SERVER_INFORMATION, replycode)
            ElseIf replycode = "005" Then
                RaiseEvent onServerReply(source, nickname, strReplyMessage, strCommandErrors.SERVER_FULL, replycode)
            ElseIf replycode = "263" Then
                RaiseEvent onCommandError(source, nickname, strReplyMessage, strCommandErrors.WAIT_AND_TRY_AGAIN, replycode)
            ElseIf replycode = "433" Then
                RaiseEvent onCommandError(source, nickname, strReplyMessage, strCommandErrors.NICKNAME_IN_USE, replycode)
            End If

        Catch ex As Exception
        End Try
    End Sub

    ''' <summary>
    ''' Returns the nickname from the incoming text. 
    ''' The incoming text needs to be in the format: nick!ident@hostname
    ''' </summary>
    Private Function getNickname(ByVal incomingText As String) As String
        Try
            Dim intLength As Long = InStr(incomingText, "!", CompareMethod.Text)
            Dim strNickname As String = Mid(incomingText, 2, intLength - 2)
            Return strNickname
        Catch ex As Exception
            Return ""
        End Try
    End Function

    ''' <summary>
    ''' Returns the ident from the incoming text. 
    ''' The incoming text needs to be in the format: nick!ident@hostname
    ''' </summary>
    Private Function getUserIdent(ByVal incomingText As String) As String
        Try
            Dim intStart As Long = InStr(incomingText, "!", CompareMethod.Text)
            Dim strUserInformation As String = Mid(incomingText, intStart + 1, Len(incomingText))
            Dim strUserInformationElements() As String = Split(strUserInformation, "@")
            Return strUserInformationElements(0)
        Catch ex As Exception
            Return ""
        End Try
    End Function

    ''' <summary>
    ''' Returns the hostname from the incoming text. 
    ''' The incoming text needs to be in the format: nick!ident@hostname
    ''' </summary>
    Private Function getUserHostname(ByVal incomingText As String) As String
        Try
            Dim intStart As Long = InStr(incomingText, "!", CompareMethod.Text)
            Dim strUserInformation As String = Mid(incomingText, intStart + 1, Len(incomingText))
            Dim strUserInformationElements() As String = Split(strUserInformation, "@")
            Return strUserInformationElements(1)
        Catch ex As Exception
            Return ""
        End Try
    End Function

    ''' <summary>
    ''' Returns the list of arguments as a string array.
    ''' </summary>
    Private Function getArguments(ByVal incomingText() As String, ByVal offset As Integer) As String()
        Try
            Dim strArguments() As String = Nothing
            Dim I As Long = 0
            Dim X As Long = 0
            For I = offset To UBound(incomingText) Step 1
                ReDim Preserve strArguments(20)
                strArguments(X) = stripLineBreaks(incomingText(I))
                X += 1
            Next
            Return strArguments
        Catch ex As Exception
            Dim strNoArguments() As String = {""}
            Return strNoArguments
        End Try
    End Function

    ''' <summary>
    ''' Returns the list of arguments as a string.
    ''' </summary>
    Private Function getLineArguments(ByVal incomingText() As String) As String
        Try
            Dim I As Long = 0
            Dim strArguments = ""
            For I = 0 To UBound(incomingText) Step 1
                If Not Trim(incomingText(I)) = "" Then
                    If strArguments = "" Then
                        strArguments = incomingText(I)
                    Else
                        strArguments = strArguments & " " & incomingText(I)
                    End If
                End If
            Next
            Return strArguments
        Catch ex As Exception
            Return ""
        End Try
    End Function

    ''' <summary>
    ''' Remove any line breaks from the supplied text.
    ''' </summary>
    Private Function stripLineBreaks(ByVal incomingText As String) As String
        Return Replace(Replace(incomingText, Chr(10), ""), Chr(13), "")
    End Function

End Class
