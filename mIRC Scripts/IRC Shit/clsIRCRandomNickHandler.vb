'// *******************************************************************************
'// Title               : IRC - Random Nick Handler
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
'// Credits             : This is based on the password generation class that
'//                       Laith M. Zraikat (http://www.jeeran.com/) developed, the
'//                       idea then was used here to create the random nick
'//                       handler.
'// *******************************************************************************

Imports System.Text

''' <summary>
''' This class handles all the functions that relates to channel handling
''' and joining/parting of the IRC Bot.
''' </summary>
Public Class clsIRCRandomNickHandler

    Private strLetters As String = "abcdABCD"
    Private strNumbers As String = "1234"
    Private intChars As Integer = 8
    Private chrLetters As Char() = Nothing
    Private chrNumbers As Char() = Nothing

    ''' <summary>
    ''' Is raised when a random nickname has been generated.
    ''' </summary>
    Public Event onNicknameGeneration(ByVal nickname As String)

    ''' <summary>
    ''' Is raised if there is an error while generating a random nickname.
    ''' </summary>
    Public Event onError(ByVal description As String)

    ''' <summary>
    ''' Set or get the letters that will be used in the nickname generation.
    ''' </summary>
    Property Letters() As String
        Get
            Return strLetters
        End Get
        Set(ByVal value As String)
            strLetters = value
        End Set
    End Property

    ''' <summary>
    ''' Set or get the numbers that will be used in the nickname generation.
    ''' </summary>
    Property Numbers() As String
        Get
            Return strNumbers
        End Get
        Set(ByVal value As String)
            strNumbers = value
        End Set
    End Property

    ''' <summary>
    ''' Set or get the length of the nickname to be generated. Note that on most
    ''' IRC servers the maximum nickname length is 8 - so make sure you check this
    ''' before starting to experiment with the length.
    ''' </summary>
    Property Length() As Integer
        Get
            Return intChars
        End Get
        Set(ByVal value As Integer)
            intChars = value
        End Set
    End Property

    ''' <summary>
    ''' Return a random generated nickname based on letters and numbers.
    ''' </summary>
    Public Function GenerateNickname() As String
        Try
            Dim I As Long = 0
            Dim intRandom As Single = 0
            Dim intArrayIndex As Integer = 0
            Dim strTempString As New StringBuilder
            Dim strRandomLetter As String = ""
            chrLetters = strLetters.ToCharArray
            chrNumbers = strNumbers.ToCharArray
            For I = 1 To intChars Step 1
                Randomize()
                intRandom = Rnd()
                intArrayIndex = -1
                If (CType(intRandom * 111, Integer)) Mod 2 = 0 Then
                    Do While intArrayIndex < 0
                        intArrayIndex = Convert.ToInt16(chrLetters.GetUpperBound(0) * intRandom)
                    Loop
                    strRandomLetter = chrLetters(intArrayIndex)
                    If (CType(intArrayIndex * intRandom * 99, Integer)) Mod 2 <> 0 Then
                        strRandomLetter = chrLetters(intArrayIndex).ToString.ToUpper
                    End If
                    strTempString.Append(strRandomLetter)
                Else
                    Do While intArrayIndex < 0
                        intArrayIndex = Convert.ToInt16(chrNumbers.GetUpperBound(0) * intRandom)
                    Loop
                    strTempString.Append(chrNumbers(intArrayIndex))
                End If
            Next
            RaiseEvent onNicknameGeneration(strTempString.ToString)
            Return strTempString.ToString
        Catch ex As Exception
            RaiseEvent onError(ex.ToString)
            Return ""
        End Try
    End Function

End Class
