Imports System.Threading
Imports System.Net
Imports System.IO
Public Class Form1
    Inherits System.Windows.Forms.Form
    Private WithEvents clsIRC As New clsIRCHandler
    Private WithEvents clsIRCParser As New clsIRCParseHandler
    Private WithEvents clsIRCCommands As New clsIRCCommandHandler(clsIRC)
    Private WithEvents clsIRCMessages As New clsIRCMessageHandler(clsIRC)
    Private WithEvents clsIRCChannels As New clsIRCChannelHandler(clsIRC)
    Private WithEvents clsIRCBot As New clsIRCBotHandler(clsIRC)
    Private WithEvents clsIRCRandomNick As New clsIRCRandomNickHandler
    Private WebRequest As Net.HttpWebRequest
    Private WebResponse As Net.HttpWebResponse
    Dim Cookies As New Net.CookieContainer
    Dim GuessNumber As Integer
    Dim StatsScript As String
    Dim nickname, ident, hostname As String
    Dim StrHTML As String

    Private Sub ircClient_onConnected() Handles clsIRC.onConnected
        clsIRC.Login()
        Call UpdateStatus()
    End Sub

    Private Sub ircClient_onIncomingCommand(ByVal channel As String, ByVal nickname As String, ByVal ident As String, ByVal hostname As String, ByVal command As String, ByVal arguments As String) Handles clsIRCParser.onIncomingCommand
        clsIRCCommands.commandHandler(channel, nickname, ident, hostname, command, arguments)
    End Sub

    Public Function GetStringBetween(ByVal InputText As String, ByVal StartText As String, ByVal EndText As String, Optional ByVal StartPosition As Object = 1) As String

        Dim lnTextStart As Integer
        Dim lnTextEnd As Integer


        lnTextStart = InStr(StartPosition, InputText, StartText, CompareMethod.Text) + Len(StartText)
        lnTextEnd = InStr(lnTextStart, InputText, EndText, CompareMethod.Text)

        If lnTextStart >= (StartPosition + Len(StartText)) And lnTextEnd > lnTextStart Then
            GetStringBetween = Mid(InputText, lnTextStart, lnTextEnd - lnTextStart)
        Else
            GetStringBetween = ""
        End If

    End Function

    Private Function ReadStream(ByVal Stream As IO.Stream) As String

        Dim ResponseReader As IO.StreamReader
        Dim ResponseData As String
        ResponseReader = New IO.StreamReader(Stream)
        ResponseData = ResponseReader.ReadToEnd()
        ResponseReader.Close()

        Return ResponseData

    End Function

    Private Sub WiteStream(ByVal PostData As String)

        Dim RequestWriter As IO.StreamWriter
        RequestWriter = New IO.StreamWriter(WebRequest.GetRequestStream)
        RequestWriter.Write(PostData)
        RequestWriter.Close()

    End Sub

    Private Function httpGet(ByVal Address As String) As String
        On Error Resume Next

        WebRequest = Net.HttpWebRequest.Create(Address)
        'request.ContentType = "application/x-www-form-urlencoded" 
        WebRequest.Method = "GET"
        WebRequest.Referer = Address
        WebRequest.CookieContainer = New Net.CookieContainer()
        WebRequest.CookieContainer = Cookies

        WebResponse = WebRequest.GetResponse()
        Cookies.Add(WebResponse.Cookies)

        Return ReadStream(WebResponse.GetResponseStream())
    End Function

    Private Function httpPost(ByVal Address As String, ByVal PostData As String) As String

        WebRequest = Net.HttpWebRequest.Create(Address)
        WebRequest.UserAgent = "Mozilla/4.0 (compatible; MSIE 7.0; Windows NT 5.1; .NET CLR 1.1.4322; .NET CLR 2.0.50727; .NET CLR 3.0.04506.30)"
        WebRequest.ContentType = "application/x-www-form-urlencoded"
        WebRequest.Method = "POST"
        WebRequest.Referer = Address
        WebRequest.CookieContainer = New Net.CookieContainer()
        WebRequest.CookieContainer = cookies
        WiteStream(PostData)

        WebResponse = WebRequest.GetResponse()

        cookies.Add(WebResponse.Cookies)

        Return ReadStream(WebResponse.GetResponseStream())
    End Function

    Private Sub UpdateStatus()
        Status.Text = "Irc Connection State: " & clsIRC.Connected
        If clsIRC.Connected = True Then
            Me.txtNickname.Enabled = False
            Me.txtHostname.Enabled = False
            Me.txtPort.Enabled = False
            Me.txtUsername.Enabled = False
        Else
            Me.txtNickname.Enabled = True
            Me.txtHostname.Enabled = True
            Me.txtPort.Enabled = True
            Me.txtUsername.Enabled = True
        End If
    End Sub

    Private Sub GetFrame(ByVal GodName As String)
        Dim channel As String = txtChannel.Text
        Dim Stats() As String
        Dim strFrame As String
        strFrame = httpGet("http://owguru.com/Outwar/TimeFrames.php?server=quiver")
        strFrame = GetStringBetween(strFrame, "Time Left</th></tr><tr>", "                          </tr></table><h2>Update these gods")
        If InStr(strFrame, GodName) Then
            strFrame = GetStringBetween(strFrame, GodName, "in</td>")
            Stats = Split(strFrame, "<td>")
            clsIRCMessages.SendMessage(channel, "Last Killed: " & Replace(Stats(1), "</td>", Nothing))
            clsIRCMessages.SendMessage(channel, "Start Frame: " & Replace(Stats(2), "</td>", Nothing))
            clsIRCMessages.SendMessage(channel, "End Frame: " & Replace(Stats(3), "</td>", Nothing))
            clsIRCMessages.SendMessage(channel, "Time Left: " & Stats(4) & "in")
        Else
            clsIRCMessages.SendMessage(channel, "Could not find frame")
        End If

    End Sub

    Private Function RandomNumBetween(ByVal LowerLimit As Long, ByVal UpperLimit As Long) As Long
        Randomize()
        RandomNumBetween = Rnd() * (UpperLimit - LowerLimit) + LowerLimit
    End Function

    Private Sub Form1_Load(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Load
        NotifyIcon1.Icon = Me.Icon
        CheckForIllegalCrossThreadCalls = False
        txtUsername.Text = "DrankasSpotter"
        txtNickname.Text = "ImABot_" & RandomNumBetween(1, 9999)
    End Sub

    Private Function get_id(ByVal nickname As String, ByVal ident As String, ByVal hostname As String) As String
        Dim id As String = ident & "@" & hostname
        id += "::" & nickname
        Return id
    End Function

    Private Sub ircClient_onDataArrival(ByVal Data() As Byte, ByVal totBytes As Integer) Handles clsIRC.onDataArrival
        Dim strIncomingData As String = clsIRC.BytestoString(Data)
        Dim strLineData() As String = Split(strIncomingData, Chr(10))
        Dim I As Long = 0
        For I = 0 To UBound(strLineData) Step 1
            Dim strData As String = Replace(Replace(strLineData(I), Chr(10), ""), Chr(13), "")
            If Not Trim(strData) = "" Then
                clsIRCParser.parseIncomingReply(strLineData(I))
                txtLog.Text = txtLog.Text & vbCrLf & strData
            End If
        Next
    End Sub


    Private Sub btnConnect_LinkClicked(ByVal sender As System.Object, ByVal e As System.Windows.Forms.LinkLabelLinkClickedEventArgs) Handles btnConnect.LinkClicked
        If clsIRC.Connected = True Then
            MsgBox("Irc is already connected.", MsgBoxStyle.Information, "Error")
        Else
            Dim intPort As Integer = Integer.Parse(Me.txtPort.Text)
            clsIRC.Nickname = Me.txtNickname.Text
            clsIRC.Username = Me.txtUsername.Text
            clsIRC.Connect(Me.txtHostname.Text, intPort)
            Call UpdateStatus()
        End If
    End Sub

    Private Sub btnDisconnect_LinkClicked(ByVal sender As System.Object, ByVal e As System.Windows.Forms.LinkLabelLinkClickedEventArgs) Handles btnDisconnect.LinkClicked
        If clsIRC.Connected = True Then
            clsIRC.Disconnect()
            Call UpdateStatus()
        Else
            MsgBox("Irc Is not connected.", MsgBoxStyle.Exclamation, "Error")
        End If
    End Sub

    Private Sub btnJoinChannel_LinkClicked(ByVal sender As System.Object, ByVal e As System.Windows.Forms.LinkLabelLinkClickedEventArgs) Handles btnJoinChannel.LinkClicked
        If Not txtChannel.Text = "" And clsIRC.Connected = True And txtChannel.Text.Contains("#") = True Then
            clsIRCChannels.JoinChannel(txtChannel.Text)
        End If
    End Sub

    Private Sub btnPartChannel_LinkClicked(ByVal sender As System.Object, ByVal e As System.Windows.Forms.LinkLabelLinkClickedEventArgs) Handles btnPartChannel.LinkClicked
        If Not txtChannel.Text = "" And clsIRC.Connected = True And txtChannel.Text.Contains("#") = True Then
            clsIRCChannels.PartChannel(txtChannel.Text)
        End If
    End Sub

    Private Sub ircClient_onCommand(ByVal channel As String, ByVal nickname As String, ByVal ident As String, ByVal hostname As String, ByVal command As String, ByVal arguments As String) Handles clsIRCCommands.onCommand
        Dim Arcane1, Arcane2, Arcane3, Arcane4 As String
        Dim Shadow1, Shadow2, Shadow3, Shadow4 As String
        Dim Holy1, Holy2, Holy3, Holy4 As String
        Dim Fire1, Fire2, Fire3, Fire4 As String
        Dim Kin1, Kin2, Kin3, Kin4, Kin5, Kin6 As String
        Dim id As String = get_id(nickname, ident, hostname)

        Arcane1 = clsIRCMessages.GenerateMessagePart("The Arcane Alchemist - Room: 2147", True, True, clsIRCMessageHandler.strColors.ORANGE)
        Arcane2 = clsIRCMessages.GenerateMessagePart("Arcane Essence - Spectral Warrior", True, True, clsIRCMessageHandler.strColors.ORANGE)
        Arcane3 = clsIRCMessages.GenerateMessagePart("Magic Orb - Ancient Deserter", True, True, clsIRCMessageHandler.strColors.ORANGE)
        Arcane4 = clsIRCMessages.GenerateMessagePart("Fortified Soul - Bearded recluse", True, True, clsIRCMessageHandler.strColors.ORANGE)

        Shadow1 = clsIRCMessages.GenerateMessagePart("The Shadow Alchemist - Room: 2148", True, True, clsIRCMessageHandler.strColors.PURPLE)
        Shadow2 = clsIRCMessages.GenerateMessagePart("Dark Essence - Caustic Corpse", True, True, clsIRCMessageHandler.strColors.PURPLE)
        Shadow3 = clsIRCMessages.GenerateMessagePart("Evil Stone - Infuriated Savage", True, True, clsIRCMessageHandler.strColors.PURPLE)
        Shadow4 = clsIRCMessages.GenerateMessagePart("Consumed Soul - Choleric Ancient", True, True, clsIRCMessageHandler.strColors.PURPLE)

        Holy1 = clsIRCMessages.GenerateMessagePart("The Holy Alchemist - Room: 2149", True, True, clsIRCMessageHandler.strColors.DARKER_BLUE)
        Holy2 = clsIRCMessages.GenerateMessagePart("Holy Essence - Fallen Angel", True, True, clsIRCMessageHandler.strColors.DARKER_BLUE)
        Holy3 = clsIRCMessages.GenerateMessagePart("Consecrated Glyph - Apparitional Veteran", True, True, clsIRCMessageHandler.strColors.DARKER_BLUE)
        Holy4 = clsIRCMessages.GenerateMessagePart("Pure Soul - Rabid Wallabee", True, True, clsIRCMessageHandler.strColors.DARKER_BLUE)

        Fire1 = clsIRCMessages.GenerateMessagePart("The Fire Alchemist - Room: 2151", True, True, clsIRCMessageHandler.strColors.RED)
        Fire2 = clsIRCMessages.GenerateMessagePart("Fiery Essence - Haunter", True, True, clsIRCMessageHandler.strColors.RED)
        Fire3 = clsIRCMessages.GenerateMessagePart("Rock of Eternal Flame - Lost Demon", True, True, clsIRCMessageHandler.strColors.RED)
        Fire4 = clsIRCMessages.GenerateMessagePart("Conflagrated Soul - Forgotten Warrior", True, True, clsIRCMessageHandler.strColors.RED)

        Kin1 = clsIRCMessages.GenerateMessagePart("The Kinetic Alchemist - Room: 2152", True, True, clsIRCMessageHandler.strColors.DARK_GREEN)
        Kin2 = clsIRCMessages.GenerateMessagePart("Pure Black Jet - Poison Drake", True, True, clsIRCMessageHandler.strColors.DARK_GREEN)
        Kin3 = clsIRCMessages.GenerateMessagePart("Flame Scarred Ruby - Deadly Ripscale", True, True, clsIRCMessageHandler.strColors.DARK_GREEN)
        Kin4 = clsIRCMessages.GenerateMessagePart("Pure Kinetic Diamond - Earth Troll", True, True, clsIRCMessageHandler.strColors.DARK_GREEN)
        Kin5 = clsIRCMessages.GenerateMessagePart("Mysterious Sapphire - Evil Sherpa", True, True, clsIRCMessageHandler.strColors.DARK_GREEN)
        Kin6 = clsIRCMessages.GenerateMessagePart("Heavenly Emerald - Enraged Centaur", True, True, clsIRCMessageHandler.strColors.DARK_GREEN)

        If command = "!OUTWAR" Then
            Call Stats(arguments)
        End If

        If command = "!NEW" Then
            GuessNumber = RandomNumBetween(1, 25000)
            clsIRCMessages.SendMessage(channel, "New guess the number game started. The number can be any thing 1 - 25000, Use command !guess ### ")
        End If

        If command = "!GUESS" Then
            nickname = nickname
            Call NumberGuess(arguments, nickname)
        End If

        If command = "!FRAME" Then
            Dim Lvw As ListViewItem
            Lvw = Gods.FindItemWithText(arguments)
            If Lvw IsNot Nothing Then
                Call GetFrame(Lvw.Text)
            Else
                clsIRCMessages.SendMessage(channel, arguments & " not found")
            End If
        End If

    End Sub

    Private Sub NumberGuess(ByVal Number As Integer, ByVal nickname As String)
        Dim channel As String = txtChannel.Text

        If Number < GuessNumber Then
            clsIRCMessages.SendMessage(channel, nickname & " " & Number & " is to low")
        ElseIf Number > GuessNumber Then
            clsIRCMessages.SendMessage(channel, nickname & " " & Number & " is to high")
        ElseIf Number = GuessNumber Then
            clsIRCMessages.SendMessage(channel, nickname & " " & Number & " is correct! - If you want to play again type !new")
            Number = Nothing
        ElseIf Number = Nothing Then
            clsIRCMessages.SendMessage(channel, "No game active, type !new if you would like to start one.")
            clsIRCMessages.SendMessage(channel, "Error")
        End If
    End Sub

    Private Sub Stats(ByVal Info As String)
        Dim Fucker() As String
        Dim channel As String = txtChannel.Text
        Dim ServerName As String
        Dim Level As String
        Dim Exp As String
        Dim Growth As String
        Dim CharName As String
        Fucker = Split(Info, " ")
        ServerName = Fucker(0)
        CharName = Fucker(1)
        StatsScript = httpGet("http://" & ServerName & ".outwar.com/profile.php?transnick=" & CharName)
        If InStr(StatsScript, "PLAYER INFO") Then
            StatsScript = GetStringBetween(StatsScript, "PLAYER INFO", "crew_profile.php")
            Level = GetStringBetween(StatsScript, "CLASS</font></b></td>", "nt></b>")
            Level = GetStringBetween(Level, TextBox2.Text & "Level ", "</fo")
            Exp = GetStringBetween(StatsScript, "EXPERIENCE</font></b>", "nt></b>")
            Exp = GetStringBetween(Exp, TextBox2.Text, "</fo")
            Growth = GetStringBetween(StatsScript, "GROWTH YESTERDAY</font></b>", "nt></b>")
            Growth = GetStringBetween(Growth, TextBox2.Text, "</fo")

            clsIRCMessages.SendMessage(channel, "Stats For " & CharName & " On " & ServerName)
            clsIRCMessages.SendMessage(channel, "Level: " & Level)
            clsIRCMessages.SendMessage(channel, "Exp: " & Exp)
            clsIRCMessages.SendMessage(channel, "Growth Yesterday: " & Growth)
        Else
            clsIRCMessages.SendMessage(channel, "Error getting info.")
        End If
    End Sub

    Private Sub Form1_Resize(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Resize
        If Me.WindowState = FormWindowState.Minimized Then
            Me.Hide()
        End If
    End Sub

    Private Sub MenuItem1_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles MenuItem1.Click
        Me.Show()
        Me.WindowState = FormWindowState.Normal
    End Sub

    Private Sub MenuItem2_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles MenuItem2.Click
        End
    End Sub

    Private Sub NotifyIcon1_DoubleClick(ByVal sender As Object, ByVal e As System.EventArgs) Handles NotifyIcon1.DoubleClick
        Me.Show()
        Me.WindowState = FormWindowState.Normal
    End Sub

    Private Sub MenuItem3_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MenuItem3.Click
        Me.Hide()
    End Sub

    Private Sub MenuItem4_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MenuItem4.Click
        Me.Close()
    End Sub
End Class
