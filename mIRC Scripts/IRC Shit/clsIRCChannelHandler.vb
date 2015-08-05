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
''' This class handles all the functions that relates to channel handling
''' and joining/parting of the IRC Bot.
''' </summary>
Public Class clsIRCChannelHandler

    Private clsIRC As clsIRCHandler
    Private strRemoveOperator As String = "0"

    ''' <summary>
    ''' List of channel modes currently supported.
    ''' </summary>
    Public Enum channelModesList
        OPUSER = 1
        DEOPUSER = 2
        MODERATECHANNEL = 3
        INVITEONLY = 4
        NOEXTERNALMESSAGES = 5
        TOPICBYOPSONLY = 6
        KEY = 7
        LIMIT = 8
        PRIVATECHANNEL = 9
        SECRETCHANNEL = 10
        KEYREMOVE = 11
    End Enum

    ''' <summary>
    ''' Is raised whenever you make the bot join a channel.
    ''' </summary>
    Public Event onJoinChannel(ByVal channel As String)

    ''' <summary>
    ''' Is raised whenever you make the bot part a channel.
    ''' </summary>
    Public Event onPartChannel(ByVal channel As String)

    ''' <summary>
    ''' Is raised whenever an error occur.
    ''' </summary>
    Public Event onError(ByVal Description As String)

    ''' <summary>
    ''' Is raised when a channel key is set.
    ''' </summary>
    Public Event onChannelGotKey(ByVal channel As String, ByVal key As String)

    ''' <summary>
    ''' Is raised when a channel key is removed.
    ''' </summary>
    Public Event onChannelRemovedKey(ByVal channel As String, ByVal key As String)

    ''' <summary>
    ''' Is raised when a channel limitation is set.
    ''' </summary>
    Public Event onChannelGotLimitations(ByVal channel As String, ByVal limit As String)

    ''' <summary>
    ''' Is raised when a channel limitation is removed.
    ''' </summary>
    Public Event onChannelRemovedLimitations(ByVal channel As String)

    ''' <summary>
    ''' Is raised when a channel is moderated.
    ''' </summary>
    Public Event onChannelGotModeration(ByVal channel As String)

    ''' <summary>
    ''' Is raised when a channel is no longer moderated.
    ''' </summary>
    Public Event onChannelRemovedModeration(ByVal channel As String)

    ''' <summary>
    ''' Is raised when a channel enforce that no external messages can be
    ''' sent to the channel.
    ''' </summary>
    Public Event onChannelEnforceNoExternalMessages(ByVal channel As String)

    ''' <summary>
    ''' Is raised when a channel allows external messages to be sent to the channel.
    ''' </summary>
    Public Event onChannelNoLongerEnforceNoExternalMessages(ByVal channel As String)

    ''' <summary>
    ''' Is raised when a channel is marked as private.
    ''' </summary>
    Public Event onChannelIsPrivate(ByVal channel As String)

    ''' <summary>
    ''' Is raised when a channel is no longer marked as private.
    ''' </summary>
    Public Event onChannelIsNoLongerPrivate(ByVal channel As String)

    ''' <summary>
    ''' Is raised when a channel is marked as secret.
    ''' </summary>
    Public Event onChannelIsSecret(ByVal channel As String)

    ''' <summary>
    ''' Is raised when a channel is no longer marked as secret.
    ''' </summary>
    Public Event onChannelIsNoLongerSecret(ByVal channel As String)

    ''' <summary>
    ''' Is raised when a channel topic can be set by operators only.
    ''' </summary>
    Public Event onChannelTopicByOperatorsOnly(ByVal channel As String)

    ''' <summary>
    ''' Is raised when a channel topic can be set by everyone.
    ''' </summary>
    Public Event onChannelTopicByEveryone(ByVal channel As String)

    ''' <summary>
    ''' Is raised when a channel is marked as invitation only.
    ''' </summary>
    Public Event onChannelInviteOnly(ByVal channel As String)

    ''' <summary>
    ''' Is raised when a channel is no longer invitation only.
    ''' </summary>
    Public Event onChannelNotInviteOnly(ByVal channel As String)

    ''' <summary>
    ''' Is raised when a user recieves operator status on a channel.
    ''' </summary>
    Public Event onUserMarkedAsOperator(ByVal channel As String, ByVal nickname As String)

    ''' <summary>
    ''' Is raised when a user on a channel is no longer operator there.
    ''' </summary>
    Public Event onUserRemovedAsOperator(ByVal channel As String, ByVal nickname As String)

    ''' <summary>
    ''' Creates a new instance of this class. Requires an instance of the
    ''' IRC handler to be supplied as an argument.
    ''' </summary>
    Public Sub New(ByVal ircHandler As clsIRCHandler)
        Me.clsIRC = ircHandler
    End Sub

    ''' <summary>
    ''' Sets and gets the operator that is used when removing a channel flag.
    ''' Default this is set to '0' which means you call a command and supply
    ''' '0' as the argument.
    ''' </summary>
    Public Property RemoveOperator() As String
        Get
            Return strRemoveOperator
        End Get
        Set(ByVal value As String)
            strRemoveOperator = value
        End Set
    End Property

    ''' <summary>
    ''' Make the bot join a spesific channel.
    ''' </summary>
    Public Sub JoinChannel(ByVal channel As String)
        Try
            channel = verifyChannelName(channel)
            clsIRC.SendData(clsIRC.StringToBytes("JOIN " & channel & vbCrLf))
            RaiseEvent onJoinChannel(channel)
        Catch ex As Exception
            RaiseEvent onError(ex.ToString)
        End Try
    End Sub

    ''' <summary>
    ''' Make the bot part a spesific channel.
    ''' </summary>
    Public Sub PartChannel(ByVal channel As String)
        Try
            channel = verifyChannelName(channel)
            clsIRC.SendData(clsIRC.StringToBytes("PART " & channel & vbCrLf))
            RaiseEvent onPartChannel(channel)
        Catch ex As Exception
            RaiseEvent onError(ex.ToString)
        End Try
    End Sub

    ''' <summary>
    ''' Set or remove a user as a operator of a given channel.
    ''' </summary>
    Private Sub setUserAsOperator(ByVal channel As String, ByVal nickname As String, Optional ByVal remove As Boolean = False)
        Try
            If remove = False Then
                clsIRC.SendData(clsIRC.StringToBytes("MODE " & channel & " +o " & nickname & vbCrLf))
                RaiseEvent onUserMarkedAsOperator(channel, nickname)
            Else
                clsIRC.SendData(clsIRC.StringToBytes("MODE " & channel & " -o " & nickname & vbCrLf))
                RaiseEvent onUserRemovedAsOperator(channel, nickname)
            End If
        Catch ex As Exception
            RaiseEvent onError(ex.ToString)
        End Try
    End Sub

    ''' <summary>
    ''' Set or remove if a channel should be invitation only.
    ''' </summary>
    Private Sub setChannelInviteOnly(ByVal channel As String, Optional ByVal remove As Boolean = False)
        Try
            If remove = False Then
                clsIRC.SendData(clsIRC.StringToBytes("MODE " & channel & " +i" & vbCrLf))
                RaiseEvent onChannelInviteOnly(channel)
            Else
                clsIRC.SendData(clsIRC.StringToBytes("MODE " & channel & " -i" & vbCrLf))
                RaiseEvent onChannelNotInviteOnly(channel)
            End If
        Catch ex As Exception
            RaiseEvent onError(ex.ToString)
        End Try
    End Sub

    ''' <summary>
    ''' Set or remove if a channel should be key protected.
    ''' </summary>
    Private Sub setChannelKey(ByVal channel As String, ByVal key As String, Optional ByVal remove As Boolean = False)
        Try
            If remove = False Then
                clsIRC.SendData(clsIRC.StringToBytes("MODE " & channel & " +k " & key & vbCrLf))
                RaiseEvent onChannelGotKey(channel, key)
            Else
                clsIRC.SendData(clsIRC.StringToBytes("MODE " & channel & " -k " & key & vbCrLf))
                RaiseEvent onChannelRemovedKey(channel, key)
            End If
        Catch ex As Exception
            RaiseEvent onError(ex.ToString)
        End Try
    End Sub

    ''' <summary>
    ''' Set or remove if a channel should have a spesific limitation of users.
    ''' </summary>
    Private Sub setChannelLimitations(ByVal channel As String, ByVal limit As String, Optional ByVal remove As Boolean = False)
        Try
            If remove = False Then
                clsIRC.SendData(clsIRC.StringToBytes("MODE " & channel & " +l " & limit & vbCrLf))
                RaiseEvent onChannelGotLimitations(channel, limit)
            Else
                clsIRC.SendData(clsIRC.StringToBytes("MODE " & channel & " -l" & vbCrLf))
                RaiseEvent onChannelRemovedLimitations(channel)
            End If
        Catch ex As Exception
            RaiseEvent onError(ex.ToString)
        End Try
    End Sub

    ''' <summary>
    ''' Set or remove if a channel should be moderated (only voiced and operators can speak).
    ''' </summary>
    Private Sub setChannelModerated(ByVal channel As String, Optional ByVal remove As Boolean = False)
        Try
            If remove = False Then
                clsIRC.SendData(clsIRC.StringToBytes("MODE " & channel & " +m" & vbCrLf))
                RaiseEvent onChannelGotModeration(channel)
            Else
                clsIRC.SendData(clsIRC.StringToBytes("MODE " & channel & " -m" & vbCrLf))
                RaiseEvent onChannelRemovedModeration(channel)
            End If
        Catch ex As Exception
            RaiseEvent onError(ex.ToString)
        End Try
    End Sub

    ''' <summary>
    ''' Set or remove if people outside the channel can post messages to the channel.
    ''' </summary>
    Private Sub setChannelNoExternalMessages(ByVal channel As String, Optional ByVal remove As Boolean = False)
        Try
            If remove = False Then
                clsIRC.SendData(clsIRC.StringToBytes("MODE " & channel & " +n" & vbCrLf))
                RaiseEvent onChannelEnforceNoExternalMessages(channel)
            Else
                clsIRC.SendData(clsIRC.StringToBytes("MODE " & channel & " -n" & vbCrLf))
                RaiseEvent onChannelNoLongerEnforceNoExternalMessages(channel)
            End If
        Catch ex As Exception
            RaiseEvent onError(ex.ToString)
        End Try
    End Sub

    ''' <summary>
    ''' Set or remove if a channel should be marked as private (more common to set the channel
    ''' as secret).
    ''' </summary>
    Private Sub setChannelPrivate(ByVal channel As String, Optional ByVal remove As Boolean = False)
        Try
            If remove = False Then
                clsIRC.SendData(clsIRC.StringToBytes("MODE " & channel & " +p" & vbCrLf))
                RaiseEvent onChannelIsPrivate(channel)
            Else
                clsIRC.SendData(clsIRC.StringToBytes("MODE " & channel & " -p" & vbCrLf))
                RaiseEvent onChannelIsNoLongerPrivate(channel)
            End If
        Catch ex As Exception
            RaiseEvent onError(ex.ToString)
        End Try
    End Sub

    ''' <summary>
    ''' Set or remove if a channel should be marked as secret.
    ''' </summary>
    Private Sub setChannelSecret(ByVal channel As String, Optional ByVal remove As Boolean = False)
        Try
            If remove = False Then
                clsIRC.SendData(clsIRC.StringToBytes("MODE " & channel & " +s" & vbCrLf))
                RaiseEvent onChannelIsSecret(channel)
            Else
                clsIRC.SendData(clsIRC.StringToBytes("MODE " & channel & " -s" & vbCrLf))
                RaiseEvent onChannelIsNoLongerSecret(channel)
            End If
        Catch ex As Exception
            RaiseEvent onError(ex.ToString)
        End Try
    End Sub

    ''' <summary>
    ''' Set or remove if a channels topic can be altered by others than the operators.
    ''' </summary>
    Private Sub setChannelTopicOnlyByOperators(ByVal channel As String, Optional ByVal remove As Boolean = False)
        Try
            If remove = False Then
                clsIRC.SendData(clsIRC.StringToBytes("MODE " & channel & " +t" & vbCrLf))
                RaiseEvent onChannelTopicByOperatorsOnly(channel)
            Else
                clsIRC.SendData(clsIRC.StringToBytes("MODE " & channel & " -t" & vbCrLf))
                RaiseEvent onChannelTopicByEveryone(channel)
            End If
        Catch ex As Exception
            RaiseEvent onError(ex.ToString)
        End Try
    End Sub

    ''' <summary>
    ''' Set the different channelmodes.
    ''' </summary>
    Public Sub ChannelMode(ByVal channel As String, ByVal mode As channelModesList, Optional ByVal argument As String = "")
        Try
            channel = verifyChannelName(channel)
            If mode = channelModesList.DEOPUSER And Not argument = "" Then Call setUserAsOperator(channel, argument, True)
            If mode = channelModesList.OPUSER And Not argument = "" Then Call setUserAsOperator(channel, argument)
            If mode = channelModesList.INVITEONLY And argument = "" Then Call setChannelInviteOnly(channel)
            If mode = channelModesList.INVITEONLY And argument = strRemoveOperator Then Call setChannelInviteOnly(channel, True)
            If mode = channelModesList.KEY And Not argument = "" And Not argument = strRemoveOperator Then Call setChannelKey(channel, argument)
            If mode = channelModesList.KEYREMOVE And Not argument = "" Then Call setChannelKey(channel, argument, True)
            If mode = channelModesList.LIMIT And Not argument = "" Then Call setChannelLimitations(channel, argument)
            If mode = channelModesList.LIMIT And argument = strRemoveOperator Then Call setChannelLimitations(channel, argument, True)
            If mode = channelModesList.MODERATECHANNEL And argument = "" Then Call setChannelModerated(channel)
            If mode = channelModesList.MODERATECHANNEL And argument = strRemoveOperator Then Call setChannelModerated(channel, True)
            If mode = channelModesList.NOEXTERNALMESSAGES And argument = "" Then Call setChannelNoExternalMessages(channel)
            If mode = channelModesList.NOEXTERNALMESSAGES And argument = strRemoveOperator Then Call setChannelNoExternalMessages(channel, True)
            If mode = channelModesList.PRIVATECHANNEL And argument = "" Then Call setChannelPrivate(channel)
            If mode = channelModesList.PRIVATECHANNEL And argument = strRemoveOperator Then Call setChannelPrivate(channel, True)
            If mode = channelModesList.SECRETCHANNEL And argument = "" Then Call setChannelSecret(channel)
            If mode = channelModesList.SECRETCHANNEL And argument = strRemoveOperator Then Call setChannelSecret(channel, True)
            If mode = channelModesList.TOPICBYOPSONLY And argument = "" Then Call setChannelTopicOnlyByOperators(channel)
            If mode = channelModesList.TOPICBYOPSONLY And argument = strRemoveOperator Then Call setChannelTopicOnlyByOperators(channel, True)
        Catch ex As Exception
            RaiseEvent onError(ex.ToString)
        End Try
    End Sub

    ''' <summary>
    ''' Check if the channel name is correctly given, meaning it should
    ''' contain a # - if not, then append it.
    ''' </summary>
    Private Function verifyChannelName(ByVal channel As String) As String
        If channel.Contains("#") Then
            Return channel
        Else
            Return "#" & channel
        End If
    End Function

End Class
