'// *******************************************************************************
'// Title               : IRC - IRC Handler
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

Imports System
Imports System.Net
Imports System.Net.Sockets
Imports System.Text

''' <summary>
''' This class handles all the functions that relates to the IRC connection
''' and its status. As well as some spesific event handlers to pick up
''' join/part messages from the channels.
''' </summary>
Public Class clsIRCHandler

    Private strTitle As String = "bIRCBot"
    Private strVersion As String = "0.1"
    Private strModified As String = "2006-07-07"
    Private strAuthor As String = "Kjell Arne Brudvik (brudvik@online.no)"

    ''' <summary>
    ''' Internal class to handle the data and buffers.
    ''' </summary>
    Public Class StateObject
        Public workSocket As Socket = Nothing
        Public BufferSize As Integer = 32767
        Public buffer(32767) As Byte
        Public sb As New StringBuilder()
    End Class

    ''' <summary>
    ''' Is raised when connection is successful.
    ''' </summary>
    Public Event onConnected()

    ''' <summary>
    ''' Is raised whenever an error occur.
    ''' </summary>
    Public Event onError(ByVal Description As String)

    ''' <summary>
    ''' Is raised when data is inbound to the IRC Bot.
    ''' </summary>
    Public Event onDataArrival(ByVal Data As Byte(), ByVal TotalBytes As Integer)

    ''' <summary>
    ''' Is raised when the IRC Bot is disconnected.
    ''' </summary>
    Public Event onDisconnect()

    ''' <summary>
    ''' Is raised when the sending of the data stream is completed.
    ''' </summary>
    Public Event onSendComplete(ByVal DataSize As Integer)

    Private Shared response As [String] = [String].Empty
    Private Shared port As Integer = 44
    Private Shared ipHostInfo As IPHostEntry = Dns.GetHostEntry("localhost")
    Private Shared ipAddress As ipAddress = ipHostInfo.AddressList(0)
    Private Shared client As New Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp)

    Private clsRandomNick As New clsIRCRandomNickHandler
    Private strCommandInterpreter As String = "!"
    Private strNickname As String = ""
    Private strUsername As String = ""

    ''' <summary>
    ''' Get the current version of this project.
    ''' </summary>
    Public ReadOnly Property Version() As String
        Get
            Return strVersion
        End Get
    End Property

    ''' <summary>
    ''' Get the current modification date of this project.
    ''' </summary>
    Public ReadOnly Property Modified() As String
        Get
            Return strModified
        End Get
    End Property

    ''' <summary>
    ''' Get the author of this project.
    ''' </summary>
    Public ReadOnly Property Author() As String
        Get
            Return strAuthor
        End Get
    End Property

    ''' <summary>
    ''' Get the title of this project.
    ''' </summary>
    Public ReadOnly Property Title() As String
        Get
            Return strTitle
        End Get
    End Property

    ''' <summary>
    ''' Gets or sets the nickname of the IRC Bot.
    ''' </summary>
    Public Property Nickname() As String
        Get
            Return strNickname
        End Get
        Set(ByVal value As String)
            strNickname = value
        End Set
    End Property

    ''' <summary>
    ''' Gets or sets the username of the IRC Bot.
    ''' </summary>
    Public Property Username() As String
        Get
            Return strUsername
        End Get
        Set(ByVal value As String)
            strUsername = value
        End Set
    End Property

    ''' <summary>
    ''' Gets or sets the command interpreter of the IRC Bot. This
    ''' is the interpreter the bot will look for to determine
    ''' if the incoming text might be a command or not. Default value
    ''' for this setting is '!' - which means it will react to
    ''' commands like !test.
    ''' </summary>
    Public Property CommandInterpreter() As String
        Get
            Return strCommandInterpreter
        End Get
        Set(ByVal value As String)
            strCommandInterpreter = value
        End Set
    End Property

    ''' <summary>
    ''' Sends the login sequence to the IRC server - here it sets its
    ''' nickname as well as username.
    ''' </summary>
    Public Sub Login()
        SendData(StringToBytes("NICK " & strNickname & vbCrLf))
        SendData(StringToBytes("USER " & strNickname & " localhost 0.0.0.0 :" & strUsername & "" & vbCrLf))
    End Sub

    ''' <summary>
    ''' Initiate the connection sequence, if connected it will
    ''' raise an onConnected event.
    ''' </summary>
    Public Sub Connect(ByVal RemoteHostName As String, ByVal RemotePort As Integer)
        Try
            client = New Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp)
            port = RemotePort
            ipHostInfo = Dns.GetHostEntry(RemoteHostName)
            ipAddress = ipHostInfo.AddressList(0)
            Dim remoteEP As New IPEndPoint(ipAddress, port)
            client.BeginConnect(remoteEP, AddressOf sockConnected, client)
        Catch
            RaiseEvent onError(Err.Description)
            Exit Sub
        End Try
    End Sub

    ''' <summary>
    ''' Send byte data to the IRC Server.
    ''' </summary>
    Public Sub SendData(ByVal Data() As Byte)
        Try
            Dim byteData As Byte() = Data
            client.BeginSend(byteData, 0, byteData.Length, 0, AddressOf sockSendEnd, client)
        Catch
            RaiseEvent onError(Err.Description)
            Exit Sub
        End Try
    End Sub

    ''' <summary>
    ''' Disconnect from the server.
    ''' </summary>
    Public Sub Disconnect()
        Try
            client.Shutdown(SocketShutdown.Both)
        Catch
        End Try
        client.Close()
    End Sub

    ''' <summary>
    ''' Convert a string to a byte array.
    ''' </summary>
    Public Function StringToBytes(ByVal Data As String) As Byte()
        StringToBytes = System.Text.ASCIIEncoding.ASCII.GetBytes(Data)
    End Function

    ''' <summary>
    ''' Convert a byte array to a string.
    ''' </summary>
    Public Function BytestoString(ByVal Data As Byte()) As String
        BytestoString = System.Text.ASCIIEncoding.ASCII.GetString(Data)
    End Function

    ''' <summary>
    ''' If the client manages to connect, then we'll raise the event
    ''' onConnected to signal that the connection was successful. If not
    ''' an error is raised.
    ''' </summary>
    Private Sub sockConnected(ByVal ar As IAsyncResult)
        Try
            If client.Connected = False Then RaiseEvent onError("Connection refused.") : Exit Sub
            Dim state As New StateObject()
            state.workSocket = client
            client.BeginReceive(state.buffer, 0, state.BufferSize, 0, AddressOf sockDataArrival, state)
            RaiseEvent onConnected()
        Catch
            RaiseEvent onError(Err.Description)
            Exit Sub
        End Try
    End Sub

    ''' <summary>
    ''' Read the incoming data from the client.
    ''' </summary>
    Private Sub sockDataArrival(ByVal ar As IAsyncResult)
        Dim state As StateObject = CType(ar.AsyncState, StateObject)
        Dim client As Socket = state.workSocket
        Dim bytesRead As Integer
        Try
            bytesRead = client.EndReceive(ar)
        Catch
            RaiseEvent onError(Err.Description)
            Exit Sub
        End Try
        Try
            Dim Data() As Byte = state.buffer
            If bytesRead = 0 Then
                client.Shutdown(SocketShutdown.Both)
                client.Close()
                RaiseEvent onDisconnect()
                Exit Sub
            End If
            ReDim state.buffer(32767)
            client.BeginReceive(state.buffer, 0, state.BufferSize, 0, AddressOf sockDataArrival, state)
            RaiseEvent onDataArrival(Data, bytesRead)
        Catch
            RaiseEvent onError(Err.Description)
            Exit Sub
        End Try
    End Sub

    ''' <summary>
    ''' Data has been sent successfully, raise an event to
    ''' signal that its been completed.
    ''' </summary>
    Private Sub sockSendEnd(ByVal ar As IAsyncResult)
        Try
            Dim client As Socket = CType(ar.AsyncState, Socket)
            Dim bytesSent As Integer = client.EndSend(ar)
            RaiseEvent onSendComplete(bytesSent)
        Catch
            RaiseEvent onError(Err.Description)
            Exit Sub
        End Try
    End Sub

    ''' <summary>
    ''' Returns whetever the IRC Bot is connected or not.
    ''' </summary>
    Public Function Connected() As Boolean
        Try
            Return client.Connected
        Catch
            RaiseEvent onError(Err.Description)
            Exit Function
        End Try
    End Function

    ''' <summary>
    ''' Handles the new generation of the IRC Bot handler. Upon creation a random
    ''' nickname is set, and will be used unless overrided by the user when
    ''' setting the values.
    ''' </summary>
    Public Sub New()
        clsRandomNick.Length = 8
        clsRandomNick.Letters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ"
        clsRandomNick.Numbers = "0123456789"
        strNickname = clsRandomNick.GenerateNickname
        strUsername = strTitle & " " & strVersion
    End Sub

End Class
