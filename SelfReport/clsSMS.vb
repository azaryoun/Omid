Imports System.IO
Imports System.Net

Public Class clsSMS
    Public Function SendSMS_Batch(ByVal Message As String, ByVal DestinationAddress() As String, ByVal UserName As String, ByVal Password As String, ByVal Number As String, ByVal IPAddress As String, ByVal Company As String, ByVal IsFarsi As Boolean, ByVal SetPriority As Boolean, ByVal IsFlash As Boolean, ByVal BatchID As String) As String()

        Dim RetValue(1) As String
        RetValue(0) = "False"
        RetValue(1) = 0

        Try

            Dim vCellphone As String = ""
            Dim vMessage As String = Message
            Dim txt As String
            Dim ret As String = ""
            vMessage = vMessage.Replace(Chr(13), String.Empty)
            vMessage = IIf(IsFarsi, C2Unicode(vMessage), vMessage)

            txt = "<?xml version=""1.0"" encoding=""UTF-8""?>" & vbCrLf
            txt = txt & "<!DOCTYPE smsBatch PUBLIC '-//PERVASIVE//DTD CPAS 1.0//EN' 'http://www.rahyab.ir/dtd/Cpas.dtd' []>" & vbCrLf
            'txt = txt & "<!DOCTYPE smsBatch PUBLIC ""-//PERVASIVE//DTD CPAS 1.0//EN"" ""https://www.rahyab.ir/dtd/Cpas.dtd"">" & vbCrLf
            If SetPriority = True Then
                txt = txt & "<smsBatch company=""" & Company & " priority=""0" & """ batchID=""" & BatchID & """>" & vbCrLf
            Else
                txt = txt & "<smsBatch company=""" & Company & """ batchID=""" & BatchID & """>" & vbCrLf
            End If
            txt = txt & "<sms" & IIf(IsFlash, " msgClass=""0""", " msgClass=""1""") & IIf(IsFarsi, " binary=""true"" dcs=""8""", " binary=""false"" dcs=""0""") & ">" & vbCrLf
            For i As Integer = 0 To DestinationAddress.GetLength(0) - 1
                txt = txt & "<destAddr><![CDATA[" & correctNumber(DestinationAddress(i)) & "]]></destAddr>" & vbCrLf
            Next i

            txt = txt & "<origAddr><![CDATA[" & correctNumber(Number) & "]]></origAddr>" & vbCrLf
            txt = txt & "<message><![CDATA[" & vMessage & "]]></message>" & vbCrLf
            txt = txt & "</sms>" & vbCrLf
            txt = txt & "</smsBatch>"

            Dim vDataToPost As String = txt
            Dim vWebRequest As WebRequest
            Dim vRequestStream As Stream
            Dim vStreamWriter As StreamWriter
            Dim vWebResponse As WebResponse
            Dim vResponseStream As Stream
            Dim vStreamReader As StreamReader

            vWebRequest = WebRequest.Create(IPAddress)
            vWebRequest.Method = "POST"
            vWebRequest.ContentType = "text/xml"
            vWebRequest.Headers.Add("authorization", "Basic " & Base64Encode(UserName & ":" & Password))
            vRequestStream = vWebRequest.GetRequestStream()
            vStreamWriter = New StreamWriter(vRequestStream)
            vStreamWriter.Write(vDataToPost)
            vStreamWriter.Flush()
            vStreamWriter.Close()
            vRequestStream.Close()

            vWebResponse = vWebRequest.GetResponse()
            vResponseStream = vWebResponse.GetResponseStream()
            vStreamReader = New StreamReader(vResponseStream)
            txt = vStreamReader.ReadToEnd()
            vStreamReader.Close()
            vResponseStream.Close()
            vWebResponse.Close()

            If InStr(txt, "CHECK_OK", CompareMethod.Text) > 0 Then
                RetValue(0) = "CHECK_OK"
                RetValue(1) = BatchID
            Else
                Try
                    Dim Sindex, EIndex, MSG
                    Sindex = InStr(txt, "CDATA[", CompareMethod.Text)
                    EIndex = InStr(txt, "]]", CompareMethod.Text)
                    MSG = txt.Substring(Sindex + "CDATA[".Length - 1, EIndex - Sindex - "CDATA[".Length)
                    If (MSG.Contains("BLACK")) Then
                        MSG = "شماره گيرنده در ليست سياه مي باشد"
                    ElseIf (MSG.Contains("_DEST_")) Then
                        MSG = "شماره گيرنده نامعتبر مي باشد"
                    ElseIf (MSG.Contains("SOURCE")) Then
                        MSG = "شماره فرستنده نامعتبر مي باشد"
                    ElseIf (MSG.Contains("SUSPEND")) Then
                        MSG = "شماره فرستنده غیر فعال (تعلیق) می باشد"
                    ElseIf (MSG.Contains("Access")) Then
                        MSG = "نام كاربري يا رمز عبور شماره فرستنده اشتباه است"
                    ElseIf (MSG.Contains("Insufficient")) Then
                        MSG = "شارژ ارسال پيامك كافي نمي باشد"
                    ElseIf (MSG.Contains("Something")) Then
                        MSG = "فرمت درخواست ارسال پيامك داراي اشكال مي باشد"
                    ElseIf (MSG.Contains("Available")) Then
                        MSG = "در حال حاضر امكان ارسال پيامك وجود ندارد، لطفا مجددا سعي نماييد"
                    End If
                    RetValue(1) = MSG
                Catch ex As Exception
                    RetValue(1) = txt & "-" & ex.Message
                End Try
            End If

            Return RetValue

        Catch ex As Exception
            RetValue(1) = ex.Message.ToString()
            Return RetValue
        End Try
    End Function

    Public Function SendSMS_Single(ByVal Message As String, ByVal DestinationAddress As String, ByVal UserName As String, ByVal Password As String, ByVal Number As String, ByVal IPAddress As String, ByVal Company As String, ByVal IsFarsi As Boolean, ByVal SetPriority As Boolean, ByVal IsFlash As Boolean, ByVal BatchID As String) As String()

        Dim RetValue(1) As String
        RetValue(0) = "False"
        RetValue(1) = 0

        Try

            Dim vCellphone As String = ""
            Dim vMessage As String = Message
            Dim txt As String
            Dim ret As String = ""
            vMessage = vMessage.Replace(Chr(13), String.Empty)
            vMessage = IIf(IsFarsi, C2Unicode(vMessage), vMessage)

            txt = "<?xml version=""1.0"" encoding=""UTF-8""?>" & vbCrLf
            txt = txt & "<!DOCTYPE smsBatch PUBLIC '-//PERVASIVE//DTD CPAS 1.0//EN' 'http://www.rahyab.ir/dtd/Cpas.dtd' []>" & vbCrLf
            'txt = txt & "<!DOCTYPE smsBatch PUBLIC ""-//PERVASIVE//DTD CPAS 1.0//EN"" ""https://www.rahyab.ir/dtd/Cpas.dtd"">" & vbCrLf
            If SetPriority = True Then
                txt = txt & "<smsBatch company=""" & Company & " priority=""0" & """ batchID=""" & BatchID & """>" & vbCrLf
            Else
                txt = txt & "<smsBatch company=""" & Company & """ batchID=""" & BatchID & """>" & vbCrLf
            End If
            txt = txt & "<sms" & IIf(IsFlash, " msgClass=""0""", " msgClass=""1""") & IIf(IsFarsi, " binary=""true"" dcs=""8""", " binary=""false"" dcs=""0""") & ">" & vbCrLf
            txt = txt & "<destAddr><![CDATA[" & correctNumber(DestinationAddress) & "]]></destAddr>" & vbCrLf

            txt = txt & "<origAddr><![CDATA[" & correctNumber(Number) & "]]></origAddr>" & vbCrLf
            txt = txt & "<message><![CDATA[" & vMessage & "]]></message>" & vbCrLf
            txt = txt & "</sms>" & vbCrLf
            txt = txt & "</smsBatch>"

            Dim vDataToPost As String = txt
            Dim vWebRequest As WebRequest
            Dim vRequestStream As Stream
            Dim vStreamWriter As StreamWriter
            Dim vWebResponse As WebResponse
            Dim vResponseStream As Stream
            Dim vStreamReader As StreamReader

            vWebRequest = WebRequest.Create(IPAddress)
            vWebRequest.Method = "POST"
            vWebRequest.ContentType = "text/xml"
            vWebRequest.Headers.Add("authorization", "Basic " & Base64Encode(UserName & ":" & Password))
            vRequestStream = vWebRequest.GetRequestStream()
            vStreamWriter = New StreamWriter(vRequestStream)
            vStreamWriter.Write(vDataToPost)
            vStreamWriter.Flush()
            vStreamWriter.Close()
            vRequestStream.Close()

            vWebResponse = vWebRequest.GetResponse()
            vResponseStream = vWebResponse.GetResponseStream()
            vStreamReader = New StreamReader(vResponseStream)
            txt = vStreamReader.ReadToEnd()
            vStreamReader.Close()
            vResponseStream.Close()
            vWebResponse.Close()

            If InStr(txt, "CHECK_OK", CompareMethod.Text) > 0 Then
                RetValue(0) = "CHECK_OK"
                RetValue(1) = BatchID
            Else
                Try
                    Dim Sindex, EIndex, MSG
                    Sindex = InStr(txt, "CDATA[", CompareMethod.Text)
                    EIndex = InStr(txt, "]]", CompareMethod.Text)
                    MSG = txt.Substring(Sindex + "CDATA[".Length - 1, EIndex - Sindex - "CDATA[".Length)
                    If (MSG.Contains("BLACK")) Then
                        MSG = "شماره گيرنده در ليست سياه مي باشد"
                    ElseIf (MSG.Contains("_DEST_")) Then
                        MSG = "شماره گيرنده نامعتبر مي باشد"
                    ElseIf (MSG.Contains("SOURCE")) Then
                        MSG = "شماره فرستنده نامعتبر مي باشد"
                    ElseIf (MSG.Contains("SUSPEND")) Then
                        MSG = "شماره فرستنده غیر فعال (تعلیق) می باشد"
                    ElseIf (MSG.Contains("Access")) Then
                        MSG = "نام كاربري يا رمز عبور شماره فرستنده اشتباه است"
                    ElseIf (MSG.Contains("Insufficient")) Then
                        MSG = "شارژ ارسال پيامك كافي نمي باشد"
                    ElseIf (MSG.Contains("Something")) Then
                        MSG = "فرمت درخواست ارسال پيامك داراي اشكال مي باشد"
                    ElseIf (MSG.Contains("Available")) Then
                        MSG = "در حال حاضر امكان ارسال پيامك وجود ندارد، لطفا مجددا سعي نماييد"
                    End If
                    RetValue(1) = MSG
                Catch ex As Exception
                    RetValue(1) = txt & "-" & ex.Message
                End Try
            End If

            Return RetValue

        Catch ex As Exception
            RetValue(1) = ex.Message.ToString()
            Return RetValue
        End Try
    End Function

    Public Function C2Unicode(ByVal uMessage As String) As String

        Dim i As Integer
        Dim preUnicode_Number As Integer
        Dim preUnicode As String
        Dim ret As String = ""
        Dim strHex As String = ""
        For i = 0 To uMessage.Length - 1
            preUnicode_Number = 4 - String.Format("{0:X}", AscW(uMessage.Substring(i, 1))).Length
            preUnicode = String.Format("{0:D" & preUnicode_Number.ToString() & "}", 0)
            strHex = preUnicode & String.Format("{0:X}", AscW(uMessage.Substring(i, 1)))
            If strHex.Length = 4 Then
                ret &= strHex
            End If
        Next
        Return ret

    End Function

    Public Function correctNumber(ByVal uNumber As String) As String

        Dim ret As String = Trim(uNumber)
        If ret.Substring(0, 4) = "0098" Then ret = ret.Remove(0, 4)
        If ret.Substring(0, 3) = "098" Then ret = ret.Remove(0, 3)
        If ret.Substring(0, 3) = "+98" Then ret = ret.Remove(0, 3)
        If ret.Substring(0, 2) = "98" Then ret = ret.Remove(0, 2)
        If ret.Substring(0, 1) = "0" Then ret = ret.Remove(0, 1)
        'If ret.Substring(0, 2) = "91" Then ret = "0" + ret

        Return "+98" & ret

    End Function

    Private Function MyASC(ByVal OneChar As String) As Integer
        If OneChar = "" Then MyASC = 0 Else MyASC = Asc(OneChar)
    End Function

    Private Function Base64Encode(ByVal inData As String) As String
        'rfc1521
        '2001 Antonin Foller, Motobit Software, http://Motobit.cz
        Const Base64 As String = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789+/"
        Dim sOut As String = ""
        Dim I As Integer

        'For each group of 3 bytes
        For I = 1 To Len(inData) Step 3
            Dim nGroup, pOut As String

            'Create one long from this 3 bytes.
            nGroup = &H10000 * Asc(Mid(inData, I, 1)) + _
              &H100 * MyASC(Mid(inData, I + 1, 1)) + MyASC(Mid(inData, I + 2, 1))

            'Oct splits the long To 8 groups with 3 bits
            nGroup = Oct(nGroup)

            'Add leading zeros
            nGroup = StrDup(8 - Len(nGroup), "0") & nGroup

            'Convert To base64
            pOut = Mid(Base64, CLng("&o" & Mid(nGroup, 1, 2)) + 1, 1) + _
              Mid(Base64, CLng("&o" & Mid(nGroup, 3, 2)) + 1, 1) + _
              Mid(Base64, CLng("&o" & Mid(nGroup, 5, 2)) + 1, 1) + _
              Mid(Base64, CLng("&o" & Mid(nGroup, 7, 2)) + 1, 1)
            'Add the part To OutPut string
            sOut = sOut + pOut

            'Add a new line For Each 76 chars In dest (76*3/4 = 57)
            'If (I + 2) Mod 57 = 0 Then sOut = sOut + vbCrLf
        Next
        Select Case Len(inData) Mod 3
            Case 1 '8 bit final
                'sOut = Left(sOut, Len(sOut) - 2) + "=="
                sOut = sOut.Substring(0, Len(sOut) - 2) + "=="
            Case 2 '16 bit final
                'sOut = Left(sOut, Len(sOut) - 1) + "="
                sOut = sOut.Substring(0, Len(sOut) - 1) + "="
        End Select
        Base64Encode = sOut

    End Function

End Class
